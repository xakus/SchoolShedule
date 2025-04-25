namespace SchoolService.DTOs
{
    // DTO для создания предмета
    public class CreateSubjectRequest
    {
        public string Name { get; set; } = null!;
        public int SchoolId { get; set; }
    }

    // DTO для изменения имени предмета
    public class EditSubjectNameRequest
    {
        public string Name { get; set; } = null!;
        public int SubjectId { get; set; }
    }

    // DTO для ответа по предмету
    public class SubjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int SchoolId { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
