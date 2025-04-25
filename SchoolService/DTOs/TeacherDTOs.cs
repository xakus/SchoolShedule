namespace SchoolService.DTOs
{
    // DTO для создания учителя
    public class CreateTeacherRequest
    {
        public string FullName { get; set; } = null!;
        public int SchoolId { get; set; }
    }

    // DTO для ответа по учителю
    public class TeacherResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public int SchoolId { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    // DTO для ответа по учителю
    public class EditTeacherNameRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
    }

    // DTO для привязки/отвязки предмета к учителю
    public class TeacherSubjectRequest
    {
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
    }
}
