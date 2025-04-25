using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenerationService.Models
{
    [Table("schedule_version")]
    public class ScheduleVersion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("generate_at")]
        public DateTime GeneratedAt { get; set; }
    }
}
