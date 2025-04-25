using Microsoft.EntityFrameworkCore;
using SchoolService.Data;
using SchoolService.Models;
using StackExchange.Redis;
using SchoolService.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace SchoolService.Services
{
    public class TeacherService
    {
        private readonly SchoolDbContext _db;
        private readonly IDatabase _redis;
        public TeacherService(SchoolDbContext db, IConnectionMultiplexer redis)
        {
            _db = db;
            _redis = redis.GetDatabase();
        }

        public async Task<TeacherResponse?> AddTeacherWithDtoAsync(CreateTeacherRequest request, int userId)
        {
            if (!CheckUser(request.SchoolId, userId).Result) return null;
            var teacher = new Teacher
            {
                FullName = request.FullName,
                SchoolId = request.SchoolId,
                CreateDateTime = DateTime.UtcNow,
                Active = true
            };
            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync();
            string key = $"teacher:{teacher.Id}";
            string value = System.Text.Json.JsonSerializer.Serialize(teacher);
            await _redis.StringSetAsync(key, value);
            return new TeacherResponse
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                SchoolId = teacher.SchoolId,
                Active = teacher.Active,
                CreateDateTime = teacher.CreateDateTime
            };
        }

        public async Task<List<TeacherResponse>> GetTeachersBySchoolDtoAsync(int schoolId, int userId)
        {
            if (!CheckUser(schoolId, userId).Result) return [];
            var teachers = await _db.Teachers.Where(t => t.SchoolId == schoolId && t.Active == true).ToListAsync();
            return teachers.Select(t => new TeacherResponse
            {
                Id = t.Id,
                FullName = t.FullName,
                SchoolId = t.SchoolId,
                Active = t.Active,
                CreateDateTime = t.CreateDateTime
            }).ToList();
        }

        public async Task<TeacherResponse?> GetTeacherDtoAsync(int teacherId, int userId)
        {
            string key = $"teacher:{teacherId}";
            var cached = await _redis.StringGetAsync(key);
            if (!cached.IsNullOrEmpty)
            {
                var t = System.Text.Json.JsonSerializer.Deserialize<TeacherResponse>(cached!);
                if (t == null || !CheckUser(t.SchoolId, userId).Result) return null;
                return t;
            }
            var teacher = await _db.Teachers.FindAsync(teacherId);
            if (teacher == null) return null;
            if (!CheckUser(teacher.SchoolId, userId).Result) return null;
            var dto = new TeacherResponse
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                SchoolId = teacher.SchoolId,
                Active = teacher.Active,
                CreateDateTime = teacher.CreateDateTime
            };
            await _redis.StringSetAsync(key, System.Text.Json.JsonSerializer.Serialize(dto));
            return dto;
        }

        public async Task<bool> DeleteTeacherAsync(int teacherId, int userId)
        {
            var teacher = await _db.Teachers.FindAsync(teacherId);
            if (teacher == null) return false;
            if (!CheckUser(teacher.SchoolId, userId).Result) return false;
            teacher.Active = false;
            teacher.CreateDateTime = DateTime.SpecifyKind(teacher.CreateDateTime, DateTimeKind.Utc);
            _db.Teachers.Update(teacher);
            await _db.SaveChangesAsync();
            string key = $"teacher:{teacherId}";
            await _redis.KeyDeleteAsync(key);
            return true;
        }
        // --- Присваивание предметов учителю ---
        public async Task AddSubjectToTeacherAsync(int teacherId, int subjectId, int userId)
        {
            var t = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId && t.Active == true);
            var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId && s.Active == true);
            if (t == null || subject == null) return;
            if (!CheckUser(t.SchoolId, userId).Result) return;
            if (!CheckUser(subject.SchoolId, userId).Result) return;

            _db.TeacherSubjects.Add(new TeacherSubject { TeacherId = teacherId, SubjectId = subjectId });
            await _db.SaveChangesAsync();
            // Можно обновить кеш учителя, если нужно
        }

        public async Task RemoveSubjectFromTeacherAsync(int teacherSubjectId, int userId)
        {
            var ts = await _db.TeacherSubjects.FirstOrDefaultAsync(x => x.Id == teacherSubjectId);
            if (ts == null) return;
            var t = await _db.Teachers.FindAsync(ts.TeacherId);
            if (t == null) return;
            if (!CheckUser(t.SchoolId, userId).Result) return;

            _db.TeacherSubjects.Remove(ts);
            await _db.SaveChangesAsync();
            // Можно обновить кеш учителя, если нужно

        }
        private async Task<bool> CheckUser(int schoolId, int userId)
        {
            var subject = await _db.Schools.FindAsync(schoolId);
            if (subject == null) return false;
            if (subject.UserId != userId) return false;
            return true;
        }

        public async Task<TeacherResponse?> EditTeacherNameWithDtoAsync(EditTeacherNameRequest request, int userId)
        {
            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == request.Id && t.Active == true);
            if (teacher == null) return null;
            if (!CheckUser(teacher.SchoolId, userId).Result) return null;
            string key = $"teacher:{teacher.Id}";
            await _redis.KeyDeleteAsync(key);
            teacher.FullName = request.FullName;
            teacher.CreateDateTime = DateTime.SpecifyKind(teacher.CreateDateTime, DateTimeKind.Utc);
            _db.Teachers.Update(teacher);
            await _db.SaveChangesAsync();
            string value = System.Text.Json.JsonSerializer.Serialize(teacher);
            await _redis.StringSetAsync(key, value);
            return new TeacherResponse
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                SchoolId = teacher.SchoolId,
                Active = teacher.Active,
                CreateDateTime = teacher.CreateDateTime
            };
        }

        public async Task<List<TeacherSubject>> GetTeacherSubjectsByTheacherIdDtoAsync(int teacherId, int userId)
        {
            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId && t.Active == true);
            if (teacher == null) return [];
            if (!CheckUser(teacher.SchoolId, userId).Result) return [];
            var teacherSubjects = await _db.TeacherSubjects.Where(ts=> ts.TeacherId == teacherId).ToListAsync();
            if (teacherSubjects.IsNullOrEmpty()) return [];
            return teacherSubjects;
        }
    }
}
