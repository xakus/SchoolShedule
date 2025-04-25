namespace SchoolService.DTOs
{
    // DTO для создания школы
    public class CreateSchoolRequest
    {
        public string Name { get; set; } = null!;
        public int MaxLessonsDay { get; set; }
    }

    // DTO для ответа по школе
    public class SchoolResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int MaxLessonsDay { get; set; }
        public int UserId { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class EditSchoolName
    {
        public string Name { get; set; } = null!;
        public int SchoolId { get; set; }
    }
    public class EditSchoolMaxLessonsDay
    {
        public int MaxLessosnsDay { get; set; }
        public int SchoolId { get; set; }
    }
}
