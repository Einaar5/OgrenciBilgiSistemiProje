using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgrenciBilgiSistemiProje.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [Required]
        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public bool IsComeHour1 { get; set; } = false; // 1. saat katılım durumu

        [Required]
        public bool IsComeHour2 { get; set; } = false; // 2. saat katılım durumu

        [Required]
        public bool IsComeHour3 { get; set; } = false; // 3. saat katılım durumu

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}