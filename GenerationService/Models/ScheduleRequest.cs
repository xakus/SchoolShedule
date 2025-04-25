using System.Collections.Generic;

namespace GenerationService.Models
{
    /// <summary>
    /// DTO для генерации расписания (например, список классов, учителей, предметов и т.д.)
    /// </summary>
    public class ScheduleRequest
    {
        public List<int> ClassIds { get; set; } = new();
        public List<int> TeacherIds { get; set; } = new();
        public List<int> SubjectIds { get; set; } = new();
        // Можно добавить дополнительные параметры, если потребуется
    }
}
