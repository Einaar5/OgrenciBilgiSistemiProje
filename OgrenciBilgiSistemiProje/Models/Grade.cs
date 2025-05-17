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

        public int Score { get; set; } = 0;
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; } // İlişkilendirilmiş Quiz nesnesi

        
        public float? Average { get; set; } // Ortalama (isteğe bağlı, hesaplanabilir)
    }
}
