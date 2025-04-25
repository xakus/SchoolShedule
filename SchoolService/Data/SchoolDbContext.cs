using Microsoft.EntityFrameworkCore;
using SchoolService.Models;

namespace SchoolService.Data
{
    /// <summary>
    /// Контекст базы данных для SchoolService
    /// </summary>
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options) { }

        public DbSet<School> Schools => Set<School>();
        public DbSet<Class> Classes => Set<Class>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<ClassLessonse> ClassLessons => Set<ClassLessonse>();
        public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    }
}
