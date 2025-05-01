using System;
using System.Collections.Generic;
using System.Linq;
using GenerationService.Models;
using GenerationService.Data;
using System.Threading.Tasks;
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

        public async Task<List<Schedule>> GetScheduleByClass(int classId){
            // Сначала пробуем получить расписание из БД
            var dbSchedules = await _db.Schedules.Where(s => s.ClassId == classId).ToListAsync();
            if (dbSchedules.Any())
                return dbSchedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber).ToList();
            // Если в БД пусто — генерируем и сохраняем
            var generated = GenerateSchedule();
            if (generated.Count == 0) return [];

            _db.Schedules.AddRange(generated);
            _db.SaveChanges();
            return generated.Where(s => s.ClassId == classId).OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber)
                .ToList();
        }

        public List<Schedule> GetScheduleByTeacher(int teacherId){
            var dbSchedules = _db.Schedules.Where(s => s.TeacherId == teacherId).ToList();
            if (dbSchedules.Any())
                return dbSchedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber).ToList();
            var generated = GenerateSchedule();
            if (generated.Count == 0) return [];

            _db.Schedules.AddRange(generated);
            _db.SaveChanges();
            return generated.Where(s => s.TeacherId == teacherId).OrderBy(s => s.DayOfWeek).ThenBy(s => s.LessonNumber)
                .ToList();
        }

        public byte[] GetSchedulePdfByClass(int classId){
            var schedule = GetScheduleByClass(classId);
            var classDict = _db.Classes.ToDictionary(c => c.Id, c => c.Name);
            var className = classDict.TryGetValue(classId, out var cname) ? cname : classId.ToString();
            return GeneratePdf(schedule.Result, $"Расписание для класса {className}");
        }

        public byte[] GetSchedulePdfByTeacher(int teacherId){
            var schedule = GetScheduleByTeacher(teacherId);
            var teacherDict = _db.Teachers.ToDictionary(t => t.Id, t => t.FullName);
            var teacherName = teacherDict.TryGetValue(teacherId, out var tName) ? tName : teacherId.ToString();
            return GeneratePdf(schedule, $"Расписание для учителя {teacherName}");
        }

        private byte[] GeneratePdf(List<Schedule> schedule, string title){
            // Получаем справочники для отображения имен
            var subjectDict = _db.Subjects.ToDictionary(s => s.Id, s => s.Name);
            var teacherDict = _db.Teachers.ToDictionary(t => t.Id, t => t.FullName);
            var classDict = _db.Classes.ToDictionary(c => c.Id, c => c.Name);

            using var ms = new MemoryStream();
            var document = new PdfSharpCore.Pdf.PdfDocument();
            var page = document.AddPage();
            var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
            var font = new PdfSharpCore.Drawing.XFont("Arial", 10);
            var boldFont = new PdfSharpCore.Drawing.XFont("Arial", 14, PdfSharpCore.Drawing.XFontStyle.Bold);

            double y = 40;
            gfx.DrawString(title, boldFont, PdfSharpCore.Drawing.XBrushes.Black,
                new PdfSharpCore.Drawing.XRect(0, y, page.Width, 20), PdfSharpCore.Drawing.XStringFormats.TopCenter);
            y += 30;

            // Заголовок таблицы
            string[] headers = ["День", "№", "Время", "Класс", "Предмет", "Учитель"];
            double[] colWidths = [80, 30, 80, 40, 150, 150];
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
                    item.DayOfWeek.ToString(),
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

        public List<Schedule> GenerateSchedule(){
            // Если расписание уже есть в БД — не пересоздаём
            var version = _db.ScheduleVersions.OrderByDescending(v => v.Id).FirstOrDefault();
            if (_db.Schedules.Any() && version != null)
                return _db.Schedules.Where(s => s.ScheduleVersionId == version.Id).ToList();

            return GenerateScheduleWithVersion();
        }

        public List<Schedule> RegenerateSchedule(){
            // Очистить старое расписание и создать новую версию
            var newVersion = new ScheduleVersion {GeneratedAt = DateTime.UtcNow};
            _db.ScheduleVersions.Add(newVersion);
            _db.Schedules.RemoveRange(_db.Schedules);
            _db.SaveChanges();
            var schedules = GenerateScheduleWithVersion(newVersion);
            _db.Schedules.AddRange(schedules);
            _db.SaveChanges();
            return schedules;
        }

        private List<Schedule> GenerateScheduleWithVersion(ScheduleVersion? version = null){
            version ??= new ScheduleVersion {GeneratedAt = DateTime.UtcNow};
            if (version.Id == 0) {
                _db.ScheduleVersions.Add(version);
                _db.SaveChanges();
            }

            // Пример базовой логики генерации расписания (без учета времени)
            var schedules = new List<Schedule>();
            var daysOfWeek = new[]
                {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday};

            var classLessons = _db.ClassLessons.ToList();
            var teacherSubjects = _db.TeacherSubjects.ToList();
            var classes = _db.Classes.Where(c => c.Active).ToList();
            var subjects = _db.Subjects.Where(s => s.Active).ToList();
            var teachers = _db.Teachers.Where(t => t.Active).ToList();

            // Для каждого класса и предмета ищем закрепленного учителя
            var teacherByClassSubject = new Dictionary<(int classId, int subjectId), int>();
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
            int maxLessonsPerDay = 6;
            var classScheduleMap = new Dictionary<(int classId, DayOfWeek day, int lessonNumber), bool>();
            var teacherScheduleMap = new Dictionary<(int teacherId, DayOfWeek day, int lessonNumber), bool>();

            foreach (var cl in classes) {
                var lessons = classLessons.Where(l => l.ClassId == cl.Id).ToList();
                var subjectHoursLeft = lessons.ToDictionary(l => l.SubjectId, l => l.HoursPerWeek);
                var daySubjects = daysOfWeek.ToDictionary(d => d, d => new HashSet<int>());
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

                        schedules.Add(new Schedule {
                            ClassId = cl.Id,
                            TeacherId = teacherId,
                            SubjectId = subjectId,
                            DayOfWeek = day,
                            LessonNumber = lessonNumber,
                            StartTime = TimeSpan.FromHours(8 + lessonNumber - 1),
                            EndTime = TimeSpan.FromHours(9 + lessonNumber - 1),
                            ScheduleVersionId = version.Id
                        });
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

            // Сохранять расписание в БД по необходимости
            return schedules;
        }

        List<Schedule> IScheduleService.GetScheduleByClass(int classId){
            throw new NotImplementedException();
        }
    }
}