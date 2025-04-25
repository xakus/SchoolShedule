using Microsoft.EntityFrameworkCore;
using SchoolService.Data;
using SchoolService.Models;
using StackExchange.Redis;
using SchoolService.DTOs;
using System.ComponentModel;

namespace SchoolService.Services
{
    public class SubjectService
    {
        private readonly SchoolDbContext _db;
        private readonly IDatabase _redis;
        public SubjectService(SchoolDbContext db, IConnectionMultiplexer redis)
        {
            _db = db;
            _redis = redis.GetDatabase();
        }

        public async Task<SubjectResponse?> AddSubjectWithDtoAsync(CreateSubjectRequest request, int userId)
        {
            if (!CheckUser(request.SchoolId, userId).Result) return null;
            var subject = new Subject
            {
                Name = request.Name,
                SchoolId = request.SchoolId,
                CreateDateTime = DateTime.UtcNow,
                Active = true
            };
            _db.Subjects.Add(subject);
            await _db.SaveChangesAsync();
            string key = $"subject:{subject.Id}";
            string value = System.Text.Json.JsonSerializer.Serialize(subject);
            await _redis.StringSetAsync(key, value);
            var dto = new SubjectResponse
            {
                Id = subject.Id,
                Name = subject.Name,
                CreateDateTime = subject.CreateDateTime,
                SchoolId = subject.SchoolId
            };
            return dto;
        }

        public async Task<List<SubjectResponse>> GetSubjectsBySchoolDtoAsync(int schoolId, int userId)
        {
            if (!CheckUser(schoolId, userId).Result) return new List<SubjectResponse>();
            var subjects = await _db.Subjects.Where(s => s.SchoolId == schoolId && s.Active == true).ToListAsync();
            return subjects.Select(s => new SubjectResponse
            {
                Id = s.Id,
                Name = s.Name,
                SchoolId = s.SchoolId,
                CreateDateTime = s.CreateDateTime
            }).ToList();
        }

        public async Task<bool> DeleteSubjectAsync(int subjectId, int userId)
        {
            var subject = await _db.Subjects.FindAsync(subjectId);
            if (subject == null) return false;
            if (!CheckUser(subject.SchoolId, userId).Result) return false;
            subject.Active = false;
            subject.CreateDateTime = DateTime.SpecifyKind(subject.CreateDateTime, DateTimeKind.Utc);
            _db.Subjects.Update(subject);
            await _db.SaveChangesAsync();
            string key = $"subject:{subjectId}";
            await _redis.KeyDeleteAsync(key);
            return true;
        }

        public async Task<SubjectResponse?> EditNameSubjectAsync(EditSubjectNameRequest request, int userId)
        {
            var subject = await _db.Subjects.FindAsync(request.SubjectId);
            if (subject == null) return null;
            string key = $"subject:{subject.Id}";
            await _redis.KeyDeleteAsync(key);
            if (!CheckUser(subject.SchoolId, userId).Result) return null;
            subject.Name = request.Name;
            subject.CreateDateTime = DateTime.SpecifyKind(subject.CreateDateTime, DateTimeKind.Utc);
            _db.Subjects.Update(subject);
            await _db.SaveChangesAsync();
            string value = System.Text.Json.JsonSerializer.Serialize(subject);
            await _redis.StringSetAsync(key, value);
            var dto = new SubjectResponse
            {
                Id = subject.Id,
                Name = subject.Name,
                CreateDateTime = subject.CreateDateTime,
                SchoolId = subject.SchoolId
            };
            return dto;

        }

        private async Task<bool> CheckUser(int schoolId, int userId)
        {
            var subject = await _db.Schools.FindAsync(schoolId);
            if (subject == null) return false;
            if (subject.UserId != userId) return false;
            return true;
        }
    }
}