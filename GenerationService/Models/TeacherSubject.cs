using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GenerationService.Models
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
