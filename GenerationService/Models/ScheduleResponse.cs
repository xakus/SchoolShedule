namespace GenerationService.Models;

public class ScheduleResponse {
    public int Id{get; set;}
    public string ClassName{get; set;} = string.Empty;
    public string TeacherName{get; set;} = string.Empty;
    public string SubjectName{get; set;} = string.Empty;
    public string DayWeek {get; set;} = string.Empty;
    public int LessonsNumber {get; set;} = 0;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}