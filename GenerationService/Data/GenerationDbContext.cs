using Microsoft.EntityFrameworkCore;
using GenerationService.Models;

namespace GenerationService.Data
{
    public class GenerationDbContext : DbContext
    {
        public GenerationDbContext(DbContextOptions<GenerationDbContext> options) : base(options) { }

        public DbSet<Class> Classes => Set<Class>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<ClassLessonse> ClassLessons => Set<ClassLessonse>();
        public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
        public DbSet<Schedule> Schedules => Set<Schedule>();
        public DbSet<ScheduleTeacherSubject> ScheduleTeacherSubjects => Set<ScheduleTeacherSubject>();
        public DbSet<ScheduleVersion> ScheduleVersions => Set<ScheduleVersion>();
    }
}
