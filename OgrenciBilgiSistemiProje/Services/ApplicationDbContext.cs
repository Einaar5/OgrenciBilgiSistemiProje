using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Models;

namespace OgrenciBilgiSistemiProje.Services
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<AdminAcc> Admins { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<CourseList> CourseList { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Student-Department ilişkisi
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lesson-Department ilişkisi
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Department)
                .WithMany(d => d.Lessons)
                .HasForeignKey(l => l.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lesson-Teacher ilişkisi
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Teacher)
                .WithMany(t => t.Lessons)
                .HasForeignKey(l => l.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Grade-Student ilişkisi
            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Grade-Lesson ilişkisi
            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Lesson)
                .WithMany(l => l.Grades)
                .HasForeignKey(g => g.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseList-Lesson ilişkisi
            modelBuilder.Entity<CourseList>()
                .HasOne(cl => cl.Lesson)
                .WithMany()
                .HasForeignKey(cl => cl.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

           
            // CourseList-Department ilişkisi
            modelBuilder.Entity<CourseList>()
                .HasOne(cl => cl.Department)
                .WithMany()
                .HasForeignKey(cl => cl.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
