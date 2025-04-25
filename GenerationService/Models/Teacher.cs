using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GenerationService.Models
{
    /// <summary>
    /// Модель учителя
    /// </summary>
    [Table("teacher_t")]
    public class Teacher
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("full_name")]
        public string FullName { get; set; } = null!;
        [Column("school_t_id")]
        public int SchoolId { get; set; }
        [Column("active")]
        public bool Active { get; set; } = true;
        [Column("create_date_time")]
        public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;
        public List<TeacherSubject> TeacherSubjects { get; set; } = new();
    }
}
