using Microsoft.EntityFrameworkCore;
using SchoolService.Data;
using SchoolService.Models;
using StackExchange.Redis;
using SchoolService.DTOs;
using static SchoolService.Helpers.UserContextHelper;
using System.Text.Json;

namespace SchoolService.Services
{
    public class SchoolSer
    {
        private readonly SchoolDbContext _db;
        private readonly IDatabase _redis;
        public SchoolSer(SchoolDbContext db, IConnectionMultiplexer redis)
        {
            _db = db;
            _redis = redis.GetDatabase();
        }

        public async Task<SchoolResponse?> AddSchoolWithDtoAsync(CreateSchoolRequest request, UserInfo user)
        {
            var school = new School
            {
                Name = request.Name,
                UserId = user.Id,
                MaxLessonsDay = request.MaxLessonsDay,
                CreateDateTime = DateTime.UtcNow,
                Active = true
            };
            _db.Schools.Add(school);
            await _db.SaveChangesAsync();
            string key = $"school:{school.Id}";
            string value = JsonSerializer.Serialize(school);
            await _redis.StringSetAsync(key, value);
            var dto = new SchoolResponse
            {
                Id = school.Id,
                Name = school.Name,
                MaxLessonsDay = school.MaxLessonsDay,
                UserId = school.UserId,
                CreateDateTime = school.CreateDateTime,

            };
            return dto;
        }

        public async Task<List<SchoolResponse>> GetSchoolsByUserDtoAsync(int userId)
        {
            var schools = await _db.Schools.Where(s => s.UserId == userId && s.Active == true).ToListAsync();
            return schools.Select(s => new SchoolResponse
            {
                Id = s.Id,
                Name = s.Name,
                MaxLessonsDay = s.MaxLessonsDay,
                UserId = s.UserId,
                CreateDateTime = s.CreateDateTime
            }).ToList();
        }

        public async Task<SchoolResponse?> GetSchoolDtoAsync(int id, int userId)
        {
            string key = $"school:{id}";
            var cached = await _redis.StringGetAsync(key);
            if (!cached.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<SchoolResponse>(cached!);
            }
            var school = await _db.Schools.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && s.Active == true);
            if (school == null) return null;
            var dto = new SchoolResponse
            {
                Id = school.Id,
                Name = school.Name,
                MaxLessonsDay = school.MaxLessonsDay,
                UserId = school.UserId,
                CreateDateTime = school.CreateDateTime,
                
            };
            await _redis.StringSetAsync(key, JsonSerializer.Serialize(dto));
            return dto;
        }

        public async Task<bool> DeleteSchoolAsync(int id, int userId)
        {
            var school = await _db.Schools.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (school == null) return false;
            school.Active = false;
            school.CreateDateTime = DateTime.SpecifyKind(school.CreateDateTime, DateTimeKind.Utc);
            _db.Schools.Update(school);
            await _db.SaveChangesAsync();
            string key = $"school:{id}";
            await _redis.KeyDeleteAsync(key);
            return true;
        }

        public async Task<SchoolResponse?> EditSchoolNameAsync(EditSchoolName request, int userId)
        {
            string key = $"school:{request.SchoolId}";
            await _redis.KeyDeleteAsync(key);
           
            var school = await _db.Schools.FirstOrDefaultAsync(s => s.Id == request.SchoolId && s.UserId == userId);
            if (school == null) return null;
            school.Name = request.Name;
            school.CreateDateTime = DateTime.UtcNow;
            _db.Schools.Update(school);
            await _db.SaveChangesAsync();
            var dto = new SchoolResponse
            {
                Id = school.Id,
                Name = school.Name,
                MaxLessonsDay = school.MaxLessonsDay,
                UserId = school.UserId,
                CreateDateTime = school.CreateDateTime,

            };
            await _redis.StringSetAsync(key, JsonSerializer.Serialize(dto));
            return dto;
        }
        public async Task<SchoolResponse?> EditSchoolMaxLessosnsDayAsync(EditSchoolMaxLessonsDay request, int userId)
        {
            string key = $"school:{request.SchoolId}";
            await _redis.KeyDeleteAsync(key);

            var school = await _db.Schools.FirstOrDefaultAsync(s => s.Id == request.SchoolId && s.UserId == userId);
            if (school == null) return null;
            school.MaxLessonsDay = request.MaxLessosnsDay;
            _db.Schools.Update(school);
            await _db.SaveChangesAsync();
            var dto = new SchoolResponse
            {
                Id = school.Id,
                Name = school.Name,
                MaxLessonsDay = school.MaxLessonsDay,
                UserId = school.UserId,
                CreateDateTime = school.CreateDateTime,

            };
            await _redis.StringSetAsync(key, JsonSerializer.Serialize(dto));
            return dto;
        }

    }
}
