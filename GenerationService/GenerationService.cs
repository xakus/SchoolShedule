using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenerationService
{
    public interface IGenerationService
    {
        Task GenerateWeeklyScheduleAsync();
        Task<List<LessonSchedule>> GetScheduleByClassAsync(int classId);
        Task<List<LessonSchedule>> GetScheduleByTeacherAsync(int teacherId);
        Task<byte[]> GetSchedulePdfByClassAsync(int classId);
        Task<byte[]> GetSchedulePdfByTeacherAsync(int teacherId);
    }

    public class GenerationService : IGenerationService
    {
        public async Task GenerateWeeklyScheduleAsync()
        {
            // TODO: Реализовать алгоритм генерации расписания без коллизий
            throw new NotImplementedException();
        }

        public async Task<List<LessonSchedule>> GetScheduleByClassAsync(int classId)
        {
            // TODO: Получить расписание по классу
            throw new NotImplementedException();
        }

        public async Task<List<LessonSchedule>> GetScheduleByTeacherAsync(int teacherId)
        {
            // TODO: Получить расписание по учителю
            throw new NotImplementedException();
        }

        public async Task<byte[]> GetSchedulePdfByClassAsync(int classId)
        {
            // TODO: Сгенерировать PDF по классу
            throw new NotImplementedException();
        }

        public async Task<byte[]> GetSchedulePdfByTeacherAsync(int teacherId)
        {
            // TODO: Сгенерировать PDF по учителю
            throw new NotImplementedException();
        }
    }

    // Вынесите сюда модель LessonSchedule или добавьте ссылку на проект с моделями
    public class LessonSchedule
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
