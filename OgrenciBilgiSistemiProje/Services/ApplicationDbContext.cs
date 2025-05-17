using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Models;

namespace OgrenciBilgiSistemiProje.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet'ler
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<AdminAcc> Admins { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<CourseList> CourseList { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<StudentMessage> StudentMessages { get; set; }
        public DbSet<StudentLesson> StudentLessons { get; set; }
        public DbSet<Attendance> Attendance { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student yapılandırması
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasOne(s => s.Department)
                      .WithMany(d => d.Students)
                      .HasForeignKey(s => s.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Lesson yapılandırması
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasOne(l => l.Department)
                      .WithMany(d => d.Lessons)
                      .HasForeignKey(l => l.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Teacher)
                      .WithMany(t => t.Lessons)
                      .HasForeignKey(l => l.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Grade yapılandırması
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasOne(g => g.Student)
                      .WithMany(s => s.Grades)
                      .HasForeignKey(g => g.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Lesson)
                      .WithMany(l => l.Grades)
                      .HasForeignKey(g => g.LessonId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Quiz)
                      .WithMany() // Quiz sınıfında Grades listesi yoksa
                      .HasForeignKey(g => g.QuizId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 🔥 ÖNEMLİ: Eski unique constraint kaldırılıyor
                // entity.HasIndex(g => new { g.StudentId, g.LessonId }).IsUnique(); // ❌ ARTIK YOK

                // ✅ Doğru benzersizlik kuralı:
                entity.HasIndex(g => new { g.StudentId, g.QuizId }).IsUnique();
            });


            // CourseList yapılandırması
            modelBuilder.Entity<CourseList>(entity =>
            {
                entity.HasOne(cl => cl.Lesson) // burada LessonId ile CourseList arasındaki ilişkiyi tanımlıyoruz
                      .WithMany() // CourseList ile Lesson arasında bire çok ilişki var
                      .HasForeignKey(cl => cl.LessonId) // CourseList tablosundaki LessonId alanını Lesson tablosundaki LessonId ile ilişkilendiriyoruz
                      .OnDelete(DeleteBehavior.Restrict); // Silme işlemi sırasında kısıtlama getiriyoruz

                entity.HasOne(cl => cl.Department)
                      .WithMany()
                      .HasForeignKey(cl => cl.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // Attendance için benzersiz index
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.LessonId, a.AttendanceDate })
                .IsUnique();

            // StudentLesson için benzersiz index
            modelBuilder.Entity<StudentLesson>()
                .HasIndex(sl => new { sl.StudentId, sl.LessonId })
                .IsUnique();

            // Veritabanı performans optimizasyonları
            modelBuilder.Entity<Student>().HasIndex(s => s.StudentEmail).IsUnique();
            modelBuilder.Entity<Teacher>().HasIndex(t => t.TeacherMail).IsUnique();

            

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=OgrenciBilgiSistemi;Trusted_Connection=True;");
            }

            // Geliştirme ortamında detaylı loglama
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine);
#endif
        }
    }
}