using GenerationService.Models;
using GenerationService.Data;
using Microsoft.EntityFrameworkCore;

namespace GenerationService.Services {
    /// <summary>
    /// Сервис для генерации и получения расписания
    /// </summary>
    public class ScheduleService : IScheduleService {
        // Здесь должна быть ссылка на DbContext или другой источник данных
        // Пример: private readonly SchoolDbContext _db;
        private readonly GenerationDbContext _db;

        public ScheduleService(GenerationDbContext db){
            _db = db;
        }

        public List<Schedule> GetScheduleByClass(long schoolId, int classId){
            // Сначала пробуем получить расписание из БД
            var dbSchedules = _db.Schedules.Where(s => s.ClassId == classId && s.SchoolId == schoolId).ToList();
            if (dbSchedules.Count != 0)
                return dbSchedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber).ToList();
            // Если в БД пусто — генерируем и сохраняем
            var generated = GenerateSchedule(schoolId).Result;
            if (generated.Count == 0) return [];

            _db.Schedules.AddRange(generated);
            _db.SaveChangesAsync();
            return generated.Where(s => s.ClassId == classId).OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber)
                .ToList();
        }

        public List<Schedule> GetScheduleByTeacher(long schoolId, int teacherId){
            var dbSchedules = _db.Schedules.Where(s => s.TeacherId == teacherId && s.SchoolId == schoolId).ToList();
            if (dbSchedules.Count != 0)
                return dbSchedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber).ToList();
            var generated = GenerateSchedule(schoolId).Result;
            if (generated.Count == 0) return [];

            _db.Schedules.AddRange(generated);
            _db.SaveChangesAsync();
            return generated.Where(s => s.TeacherId == teacherId).OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber)
                .ToList();
        }

        public async Task<byte[]> GetSchedulePdfByClass(long schoolId, int classId){
            var schedule = GetScheduleByClass(schoolId, classId);
            var classDb = _db.Classes.FirstOrDefault(c => c.Id == classId)!;
            return await GeneratePdf(schedule, $"Расписание для \"{classDb.Name}\" класса");
        }

        public async Task<byte[]> GetSchedulePdfByTeacher(long schoolId, int teacherId){
            var schedule = GetScheduleByTeacher(schoolId, teacherId);
            var teacher = _db.Teachers.FirstOrDefault(t => t.Id == teacherId)!;
            return await GeneratePdf(schedule, $"Расписание для учителя {teacher.FullName}");
        }

        private async Task<byte[]> GeneratePdf(List<Schedule> schedule, string title){
            // Получаем справочники для отображения имен
            var subjectDict = await _db.Subjects.ToDictionaryAsync(s => s.Id, s => s.Name);
            var teacherDict = await _db.Teachers.ToDictionaryAsync(t => t.Id, t => t.FullName);
            var classDict = await _db.Classes.ToDictionaryAsync(c => c.Id, c => c.Name);

            using var ms = new MemoryStream();
            var document = new PdfSharpCore.Pdf.PdfDocument();
            var page = document.AddPage();
            var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
            var font = new PdfSharpCore.Drawing.XFont("Arial", 10);
            var boldFont = new PdfSharpCore.Drawing.XFont("Spendthrift", 12, PdfSharpCore.Drawing.XFontStyle.Bold);

            double y = 40;
            gfx.DrawString(title, boldFont, PdfSharpCore.Drawing.XBrushes.Black,
                new PdfSharpCore.Drawing.XRect(0, y, page.Width, 20), PdfSharpCore.Drawing.XStringFormats.TopCenter);
            y += 30;

            // Заголовок таблицы
            string[] headers = ["День", "№", "Время", "Класс", "Предмет", "Учитель"];
            double[] colWidths = [80, 30, 80, 70, 120, 150];
            double x = 40;
            for (var i = 0; i < headers.Length; i++) {
                gfx.DrawString(headers[i], boldFont, PdfSharpCore.Drawing.XBrushes.Black,
                    new PdfSharpCore.Drawing.XRect(x, y, colWidths[i], 20),
                    PdfSharpCore.Drawing.XStringFormats.TopLeft);
                x += colWidths[i];
            }

            y += 22;

            // Сортируем по дню, номеру урока, классу
            var sorted = schedule.OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber).ThenBy(s => s.ClassId).ToList();
            DayOfWeek? prevDay = null;
            foreach (var item in sorted) {
                // Разделитель между днями
                if (prevDay != null && prevDay != item.DayOfWeek) {
                    gfx.DrawLine(PdfSharpCore.Drawing.XPens.Gray, 40, y, page.Width - 40, y);
                    y += 10;
                }

                prevDay = item.DayOfWeek;
                x = 40;
                var subjName = subjectDict.TryGetValue(item.SubjectId, out var sname)
                    ? sname
                    : item.SubjectId.ToString();
                var teacherName = teacherDict.TryGetValue(item.TeacherId, out var tname)
                    ? tname
                    : item.TeacherId.ToString();
                var className = classDict.TryGetValue(item.ClassId, out var cname) ? cname : item.ClassId.ToString();
                string[] row = [
                    DayOfWeekRus(item.DayOfWeek),
                    item.LessonNumber.ToString(),
                    $@"{item.StartTime:hh\:mm}-{item.EndTime:hh\:mm}",
                    " " + className,
                    subjName,
                    teacherName
                ];
                for (var i = 0; i < row.Length; i++) {
                    gfx.DrawString(row[i], font, PdfSharpCore.Drawing.XBrushes.Black,
                        new PdfSharpCore.Drawing.XRect(x, y, colWidths[i], 20),
                        PdfSharpCore.Drawing.XStringFormats.TopLeft);
                    x += colWidths[i];
                }

                y += 20;
                if (!(y > page.Height - 40)) continue;
                page = document.AddPage();
                gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                y = 40;
            }

            document.Save(ms, false);
            return ms.ToArray();
        }

        public Task<List<Schedule>> GenerateSchedule(long schoolId){
            // Если расписание уже есть в БД — не пересоздаём
            var version = _db.ScheduleVersions.OrderByDescending(v => v.Id).FirstOrDefault();
            List<Schedule> schedules;
            if (_db.Schedules.Any() && version != null)
                schedules = _db.Schedules.Where(s => s.ScheduleVersionId == version.Id).ToList();
            else {
                schedules = GenerateScheduleWithVersion(schoolId);
            }

            return Task.FromResult(schedules);
        }

        public Task<List<ScheduleResponse>> GenerateScheduleResponse(long schoolId){
            // Если расписание уже есть в БД — не пересоздаём
            var version = _db.ScheduleVersions.OrderByDescending(v => v.Id).FirstOrDefault();
            List<Schedule> schedules;
            if (_db.Schedules.Any() && version != null)
                schedules = _db.Schedules.Where(s => s.ScheduleVersionId == version.Id).ToList();
            else {
                schedules = GenerateScheduleWithVersion(schoolId);
            }

            var teachers = _db.Teachers.Where(t => t.Active).ToDictionary(t => t.Id, t => t.FullName);
            var classes = _db.Classes.Where(c => c.Active).ToDictionary(c => c.Id, c => c.Name);
            var subjects = _db.Subjects.Where(s => s.Active).ToDictionary(s => s.Id, s => s.Name);
            var scheduleResponse = schedules.Select(s => new ScheduleResponse {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                LessonsNumber = s.LessonNumber,
                SubjectName = subjects.TryGetValue(s.SubjectId, out var subject) ? subject : string.Empty,
                DayWeek = DayOfWeekRus(s.DayOfWeek),
                TeacherName = teachers.TryGetValue(s.TeacherId, out var teacher) ? teacher : string.Empty,
                ClassName = classes.TryGetValue(s.ClassId, out var className) ? className : string.Empty,
            }).ToList();
            return Task.FromResult(scheduleResponse);
        }

        public Task<List<ScheduleResponse>> RegenerateSchedule(long schoolId){
            // Очистить старое расписание и создать новую версию
            var newVersion = new ScheduleVersion {GeneratedAt = DateTime.UtcNow};
            _db.ScheduleVersions.Add(newVersion);
            _db.Schedules.RemoveRange(_db.Schedules.Where(s=>s.SchoolId == schoolId));
            _db.SaveChanges();
            var schedules = GenerateScheduleWithVersion(schoolId, newVersion);
            var teachers = _db.Teachers.Where(t => t.Active).ToDictionary(t => t.Id, t => t.FullName);
            var classes = _db.Classes.Where(c => c.Active).ToDictionary(c => c.Id, c => c.Name);
            var subjects = _db.Subjects.Where(s => s.Active).ToDictionary(s => s.Id, s => s.Name);
            var scheduleResponse = schedules.Select(s => new ScheduleResponse {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                LessonsNumber = s.LessonNumber,
                SubjectName = subjects.TryGetValue(s.SubjectId, out var subject) ? subject : string.Empty,
                DayWeek = DayOfWeekRus(s.DayOfWeek),
                TeacherName = teachers.TryGetValue(s.TeacherId, out var teacher) ? teacher : string.Empty,
                ClassName = classes.TryGetValue(s.ClassId, out var className) ? className : string.Empty,
            }).ToList();
            return  Task.FromResult(scheduleResponse);
        }

        private List<Schedule> GenerateScheduleWithVersion(long schoolId, ScheduleVersion? version = null){
            version ??= new ScheduleVersion {GeneratedAt = DateTime.UtcNow};
            if (version.Id == 0) {
                _db.ScheduleVersions.Add(version);
                _db.SaveChanges();
            }

            // Пример базовой логики генерации расписания (без учета времени)
            var schedules = new List<Schedule>();
            var daysOfWeek = new[]
                {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday};
            var school = _db.Schools.Where(s => s.Active).OrderBy(s => s.Id).FirstOrDefault(s => s.Id == schoolId);
            var classes = _db.Classes.Where(c => c.Active && c.SchoolId == schoolId).ToList();
            var classIds = classes.Select(c => c.Id).ToList();
            var classLessons = _db.ClassLessons.Where(cl => classIds.Contains(cl.ClassId)).ToList();
            var teachers = _db.Teachers.Where(t => t.Active && t.SchoolId == schoolId).ToList();
            var teacherIds = teachers.Select(t => t.Id).ToList();
            var teacherSubjects = _db.TeacherSubjects.Where(subject => teacherIds.Contains(subject.TeacherId)).ToList();


            // Для каждого класса и предмета ищем закрепленного учителя
            var teacherByClassSubject = new Dictionary<(int classId, long subjectId), int>();
            foreach (var cl in classes) {
                foreach (var lesson in classLessons.Where(l => l.ClassId == cl.Id)) {
                    // Найти учителя, который ведет этот предмет
                    var matchingTeachers = teacherSubjects
                        .Where(ts => ts.SubjectId == lesson.SubjectId)
                        .ToList();

                    var random = new Random();
                    var teacher = matchingTeachers.Count > 0
                        ? matchingTeachers[random.Next(matchingTeachers.Count)]
                        : null;
                    if (teacher != null) {
                        teacherByClassSubject[(cl.Id, lesson.SubjectId)] = teacher.TeacherId;
                    }
                }
            }

            // Распределяем уроки по дням и номерам (без коллизии по времени)
            var maxLessonsPerDay = school?.MaxLessonsDay ?? 0;
            var classScheduleMap = new Dictionary<(int classId, DayOfWeek day, int lessonNumber), bool>();
            var teacherScheduleMap = new Dictionary<(int teacherId, DayOfWeek day, int lessonNumber), bool>();

            foreach (var cl in classes) {
                var lessons = classLessons.Where(l => l.ClassId == cl.Id).ToList();
                var subjectHoursLeft = lessons.ToDictionary(l => l.SubjectId, l => l.HoursPerWeek);
                var daySubjects = daysOfWeek.ToDictionary(d => d, _ => new HashSet<int>());
                var totalLessons = lessons.Sum(l => l.HoursPerWeek);
                var maxWeeklyLessons = Math.Min(35, totalLessons);
                var placedLessons = 0;
                // Для каждого дня недели
                foreach (var day in daysOfWeek) {
                    var lessonsInDay = 0;
                    while (lessonsInDay < maxLessonsPerDay && subjectHoursLeft.Values.Any(h => h > 0)) {
                        // Предметы, которые не были сегодня и у которых остались часы
                        var availableSubjects = subjectHoursLeft
                            .Where(kv => kv.Value > 0 && !daySubjects[day].Contains(kv.Key))
                            .Select(kv => kv.Key)
                            .ToList();
                        if (!availableSubjects.Any())
                            break;
                        var subjectId = availableSubjects.First(); // Можно рандомизировать
                        var teacherId = teacherByClassSubject.TryGetValue((cl.Id, subjectId), out var tid) ? tid : 0;
                        if (teacherId == 0) {
                            subjectHoursLeft[subjectId]--;
                            continue;
                        }

                        var lessonNumber = lessonsInDay + 1;
                        if (classScheduleMap.ContainsKey((cl.Id, day, lessonNumber))) {
                            lessonsInDay++;
                            continue;
                        }

                        if (teacherScheduleMap.ContainsKey((teacherId, day, lessonNumber))) {
                            lessonsInDay++;
                            continue;
                        }

                        ///////////////////
                        var startTime = TimeSpan.FromHours(8) + TimeSpan.FromMinutes(30);
                        TimeSpan[] peremena = [
                            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5),
                            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5),
                            TimeSpan.FromMinutes(5)
                        ];
                        ///////////////////
                        var schl = new Schedule {
                            SchoolId = schoolId,
                            ClassId = cl.Id,
                            TeacherId = teacherId,
                            SubjectId = subjectId,
                            DayOfWeek = day,
                            LessonNumber = lessonNumber,
                            StartTime = LessonT(startTime, lessonNumber, maxLessonsPerDay, peremena,
                                LessonTime.StartTime),
                            EndTime = LessonT(startTime, lessonNumber, maxLessonsPerDay, peremena, LessonTime.EndTime),
                            ScheduleVersionId = version.Id
                        };
                        schedules.Add(schl);
                        _db.Schedules.AddRange(schl);
                        classScheduleMap[(cl.Id, day, lessonNumber)] = true;
                        teacherScheduleMap[(teacherId, day, lessonNumber)] = true;
                        daySubjects[day].Add(subjectId);
                        subjectHoursLeft[subjectId]--;
                        lessonsInDay++;
                        placedLessons++;
                        if (placedLessons >= maxWeeklyLessons)
                            break;
                    }

                    if (placedLessons >= maxWeeklyLessons)
                        break;
                }
            }

            _db.SaveChanges();
            // Сохранять расписание в БД по необходимости
            return schedules;
        }

        private static TimeSpan LessonT(TimeSpan startTime, int lessonNumber, int maxLessons, TimeSpan[] peremena,
            LessonTime p2){
            var startSpans = new TimeSpan[maxLessons];
            var endSpans = new TimeSpan[maxLessons];
            for (var i = 0; i < maxLessons; i++) {
                if (i == 0) startSpans[i] = startTime;
                else
                    startSpans[i] = endSpans[i - 1] + (peremena == null || peremena.Length - 1 < i
                        ? TimeSpan.FromMinutes(5)
                        : peremena[i - 1]);
                endSpans[i] = startSpans[i] + TimeSpan.FromMinutes(45);
            }

            return p2 switch {
                LessonTime.StartTime => startSpans[lessonNumber - 1],
                LessonTime.EndTime => endSpans[lessonNumber - 1],
                _ => TimeSpan.Zero
            };
        }

        private enum LessonTime {
            StartTime,
            EndTime,
        }

        private string DayOfWeekRus(DayOfWeek dayOfWeek){
            return dayOfWeek switch {
                DayOfWeek.Sunday => "Воскресенье",
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                _ => ""
            };
        }
    }
}