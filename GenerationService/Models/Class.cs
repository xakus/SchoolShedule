using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GenerationService.Models
{
    /// <summary>
    /// ������ ������ (Class)
    /// </summary>
    [Table("class_t")]
    public class Class
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = null!;
        [Column("school_t_id")]
        public int SchoolId { get; set; }
        [Column("active_day_week")]
        public int ActiveDayWeek { get; set; }
        [Column("active")]
        public bool Active { get; set; } = true;
        [Column("create_date_time")]
        public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;
        public List<ClassLessonse> ClassLessons { get; set; } = new();
    }
}
