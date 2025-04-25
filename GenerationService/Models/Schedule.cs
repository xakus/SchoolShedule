using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenerationService.Models
{
    /// <summary>
    /// Модель для хранения расписания уроков
    /// </summary>
    [Table("schedule")]
    public class Schedule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("calss_id")]
        public int ClassId { get; set; }
        [Column("teacher_id")]
        public int TeacherId { get; set; }
        [Column("subject_id")]
        public int SubjectId { get; set; }
        [Column("day_of_week")]
        public DayOfWeek DayOfWeek { get; set; }
        [Column("lessons_number")]
        public int LessonNumber { get; set; } // Порядковый номер урока в дне
        [Column("start_time")]
        public TimeSpan StartTime { get; set; }
        [Column("end_time")]
        public TimeSpan EndTime { get; set; }
        [Column("shedule_version_id")]
        public int ScheduleVersionId { get; set; }
    }
}
