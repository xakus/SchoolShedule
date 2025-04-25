using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolService.Models
{
    /// <summary>
    /// Модель школы
    /// </summary>
    [Table("school_t")]
    public class School
    {
        
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = null!;
        [Column("max_lessons_day")]
        public int MaxLessonsDay { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("create_date_time")]
        public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;
        [Column("active")]
        public bool Active { get; set; } = true;
        public List<Class> Classes { get; set; } = new();
        public List<Teacher> Teachers { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
    }
}
