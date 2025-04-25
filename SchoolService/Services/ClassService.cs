using Microsoft.EntityFrameworkCore;
using SchoolService.Data;
using SchoolService.Models;
using StackExchange.Redis;
using SchoolService.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace SchoolService.Services
{
    public class ClassService
    {
        private readonly SchoolDbContext _db;
        private readonly IDatabase _redis;
        public ClassService(SchoolDbContext db, IConnectionMultiplexer redis)
        {
            _db = db;
            _redis = redis.GetDatabase();
        }

        public async Task<ClassResponse?> AddClassWithDtoAsync(CreateClassRequest request, int userId)
        {
            if (!CheckUser(request.SchoolId, userId).Result) return null;
            var c = new Class
            {
                Name = request.Name,
                SchoolId = request.SchoolId,
                ActiveDayWeek = request.ActiveDayWeek,
                CreateDateTime = DateTime.UtcNow,
                Active = true
            };
            _db.Classes.Add(c);
            await _db.SaveChangesAsync();
            string key = $"class:{c.Id}";
            string value = System.Text.Json.JsonSerializer.Serialize(c);
            await _redis.StringSetAsync(key, value);
            var clazz = new ClassResponse
            {
                Id = c.Id,
                Name = c.Name,
                SchoolId = c.SchoolId,
                ActiveDayWeek = c.ActiveDayWeek,
                CreateDateTime = c.CreateDateTime
            };
            return clazz;
        }
        public async Task<ClassResponse?> EditClassNameWithDtoAsync(EditNameClassRequest request,int userId)
        {

            var c = await _db.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId && c.Active == true);
            if (c == null) return null;
            if (!CheckUser(c.SchoolId, userId).Result) return null;
            c.Name = request.Name;
            c.CreateDateTime = DateTime.SpecifyKind(c.CreateDateTime, DateTimeKind.Utc);
            _db.Classes.Update(c);
            await _db.SaveChangesAsync();
            string key = $"class:{c.Id}";
            
            await _redis.KeyDeleteAsync(key);
            var clazz = new ClassResponse
            {
                Id = c.Id,
                Name = c.Name,
                SchoolId = c.SchoolId,
                ActiveDayWeek = c.ActiveDayWeek,
                CreateDateTime = c.CreateDateTime
            };
            string value = System.Text.Json.JsonSerializer.Serialize(c);
            await _redis.SetAddAsync(key, value);
            return clazz;
        }
        public async Task<ClassResponse?> EditClassDayWeekWithDtoAsync(EditDayWeekClassRequest request, int userId)
        {

            var c = await _db.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId && c.Active == true);
            if (c == null) return null;
            if (!CheckUser(c.SchoolId, userId).Result) return null;
            c.ActiveDayWeek = request.ActiveDayWeek;
            c.CreateDateTime = DateTime.SpecifyKind(c.CreateDateTime, DateTimeKind.Utc);
            _db.Classes.Update(c);
            await _db.SaveChangesAsync();
            string key = $"class:{c.Id}";

            await _redis.KeyDeleteAsync(key);
            var clazz = new ClassResponse
            {
                Id = c.Id,
                Name = c.Name,
                SchoolId = c.SchoolId,
                ActiveDayWeek = c.ActiveDayWeek,
                CreateDateTime = c.CreateDateTime
            };
            string value = System.Text.Json.JsonSerializer.Serialize(c);
            await _redis.SetAddAsync(key, value);
            return clazz;
        }

        public async Task<List<ClassResponse>> GetClassesBySchoolDtoAsync(int schoolId, int userId)
        {
            if (!CheckUser(schoolId, userId).Result) return new List<ClassResponse>();

            var classes = await _db.Classes.Where(c => c.SchoolId == schoolId && c.Active == true).ToListAsync();
            return classes.Select(c => new ClassResponse
            {
                Id = c.Id,
                Name = c.Name,
                SchoolId = c.SchoolId,
                ActiveDayWeek = c.ActiveDayWeek,
                CreateDateTime = c.CreateDateTime
            }).ToList();
        }

        public async Task<ClassResponse?> GetClassDtoAsync(int id, int userId)
        {
            string key = $"class:{id}";
            var cached = await _redis.StringGetAsync(key);

            if (!cached.IsNullOrEmpty)
            {
                var clss = System.Text.Json.JsonSerializer.Deserialize<ClassResponse>(cached!);
                if (clss == null) return null;
                if (!CheckUser(clss.SchoolId, userId).Result) return null;
                return clss;
            }
            var c = await _db.Classes.FindAsync(id);
            if (c == null) return null;
            if (!CheckUser(c.SchoolId, userId).Result) return null;
            var dto = new ClassResponse
            {
                Id = c.Id,
                Name = c.Name,
                SchoolId = c.SchoolId,
                ActiveDayWeek = c.ActiveDayWeek,
                CreateDateTime = c.CreateDateTime
            };
            await _redis.StringSetAsync(key, System.Text.Json.JsonSerializer.Serialize(dto));
            return dto;
        }


        public async Task<bool> DeleteClassAsync(int id, int userId)
        {
            var c = await _db.Classes.FindAsync(id);
            if (c == null) return false;
            if (!CheckUser(c.SchoolId, userId).Result) return false;
            c.Active = false;
            c.CreateDateTime = DateTime.SpecifyKind(c.CreateDateTime, DateTimeKind.Utc);
            _db.Classes.Update(c);
            await _db.SaveChangesAsync();
            string key = $"class:{id}";
            await _redis.KeyDeleteAsync(key);
            return true;
        }
        // --- Присваивание предметов к классу с количеством часов ---
        public async Task AddSubjectToClassAsync(int classId, int subjectId, int hoursPerWeek, int userId)
        {
            var c = await _db.Classes.FindAsync(classId);
            var subject = await _db.Subjects.FindAsync(subjectId);
            if (c == null || subject == null) return;
            if (!CheckUser(c.SchoolId, userId).Result) return;
            if (!CheckUser(subject.SchoolId, userId).Result) return;
            _db.ClassLessons.Add(new ClassLessonse { ClassId = classId, SubjectId = subjectId, HoursPerWeek = hoursPerWeek });
            await _db.SaveChangesAsync();
            // Можно обновить кеш класса, если нужно
        }

        public async Task RemoveSubjectFromClassAsync(int classLessonsId, int userId)
        {

            var cl = await _db.ClassLessons.FirstOrDefaultAsync(x => x.Id == classLessonsId);
            if (cl == null) return;
            var c = await _db.Classes.FindAsync(cl.ClassId);
            var subject = await _db.Subjects.FindAsync(cl.SubjectId);
            if (c == null || subject == null) return;
            if (!CheckUser(c.SchoolId, userId).Result) return;
            if (!CheckUser(subject.SchoolId, userId).Result) return;

            _db.ClassLessons.Remove(cl);
            await _db.SaveChangesAsync();
            // Можно обновить кеш класса, если нужно

        }

        // Получить все предметы для класса
        public async Task<List<ClassLessonse>> GetClassSubjectsAsync(int classId)
        {
            var c = await _db.Classes.FindAsync(classId);
            if (c == null) return [];
            return await _db.ClassLessons.Where(cl => cl.ClassId == classId).ToListAsync();
        }

        private async Task<bool> CheckUser(int schoolId, int userId)
        {
            var subject = await _db.Schools.FindAsync(schoolId);
            if (subject == null) return false;
            if (subject.UserId != userId) return false;
            return true;
        }

        internal async Task<List<ClassLessonse>> GetClassesSubjectsByClassIdDtoAsync(int classId, int userId)
        {
            var c = await _db.Classes.FindAsync(classId);
            if (c == null) return [];
            if (!CheckUser(c.SchoolId, userId).Result) return [];
            var cl = await _db.ClassLessons.Where(lessons => lessons.ClassId == classId).ToListAsync();
            if (cl.IsNullOrEmpty()) return [];

            return cl;
        }
    }
}
