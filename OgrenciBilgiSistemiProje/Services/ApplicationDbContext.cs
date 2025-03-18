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
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // İlişkisel yapılandırmalar           
            modelBuilder.Entity<Student>() // burada student tablosu için ilişkisel yapılandırmalar yapılıyor
                .HasOne(s => s.Department) // bir öğrenci bir bölüme aittir
                .WithMany(d => d.Students) // bir bölümde birden fazla öğrenci olabilir
                .HasForeignKey(s => s.DepartmentId); // öğrenci tablosunda bölüm id'si tutuluyor

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Department)
                .WithMany(d => d.Lessons)
                .HasForeignKey(l => l.DepartmentId);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Teacher)
                .WithMany(t => t.Lessons)
                .HasForeignKey(l => l.TeacherId);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentId);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Lesson)
                .WithMany(l => l.Grades)
                .HasForeignKey(g => g.LessonId);
        }
    }
    
}
