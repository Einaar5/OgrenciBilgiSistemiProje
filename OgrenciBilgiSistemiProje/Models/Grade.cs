using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        [ForeignKey("Lesson")]
        public int LessonId { get; set; }
        public virtual Lesson Lesson { get; set; }


        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; } // İlişkilendirilmiş Quiz nesnesi

        public int? Midterm { get; set; } // Vize notu
        public int? Final { get; set; }   // Final notu
        public float? Average { get; set; } // Ortalama (isteğe bağlı, hesaplanabilir)
    }
}
