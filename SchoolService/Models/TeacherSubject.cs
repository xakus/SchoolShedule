using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolService.Models
{
    /// <summary>
    /// Связь учителя и предмета
    /// </summary>
    [Table("teacher_subject")]
    public class TeacherSubject
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("teacher_t_id")]
        public int TeacherId { get; set; }

        [Column("subject_t_id")]
        public int SubjectId { get; set; }

    }
}
