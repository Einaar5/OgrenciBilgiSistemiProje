using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgrenciBilgiSistemiProje.Models
{
    public class Quiz
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string QuizName { get; set; } = "";

        // Quiz ağırlığı

        [Required]
        public float QuizWeight { get; set; } = 0;

        public int Score { get; set; } = 0;

        public int lessonId { get; set; }

        [ForeignKey("lessonId")]
        public Lesson Lesson { get; set; } = new Lesson();

        public int teacherId { get; set; }
        public Teacher Teacher { get; set; } = new Teacher();


    }
}
