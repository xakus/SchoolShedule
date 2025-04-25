using System;

namespace GenerationService.Models
{
    /// <summary>
    /// Модель для связи учителя, класса и предмета (какой учитель ведёт какой предмет в каком классе)
    /// </summary>
    public class ScheduleTeacherSubject
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
    }
}
