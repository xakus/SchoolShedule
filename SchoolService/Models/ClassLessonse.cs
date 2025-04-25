using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolService.Models
{
    /// <summary>
    /// Связь класса и предмета с количеством часов в неделю
    /// </summary>
    [Table("class_lessons")]
    public class ClassLessonse
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("class_t_id")]
        public int ClassId { get; set; }
        [Column("subject_t_id")]
        public int SubjectId { get; set; }
        [Column("hours_per_week")]
        public int HoursPerWeek { get; set; }
    }
}
