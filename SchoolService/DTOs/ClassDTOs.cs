namespace SchoolService.DTOs
{
    // DTO для создания класса
    public class CreateClassRequest
    {
        public string Name { get; set; } = null!;
        public int SchoolId { get; set; }
        public int ActiveDayWeek { get; set; }
    }
    
    // DTO для создания класса
    public class EditNameClassRequest
    {
        public string Name { get; set; } = null!;
        public int ClassId { get; set; }
    }

    // DTO для создания класса
    public class EditDayWeekClassRequest
    {
        public int ActiveDayWeek { get; set; }
        public int ClassId { get; set; }
    }

    // DTO для ответа по классу
    public class ClassResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int SchoolId { get; set; }
        public int ActiveDayWeek { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    // DTO для привязки/отвязки предмета к классу
    public class ClassSubjectRequest
    {
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int HoursPerWeek { get; set; }
    }
}
