namespace OgrenciBilgiSistemiProje.Models
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string QuizName { get; set; } = "";
        public int LessonId { get; set; }
        public float QuizWeight { get; set; }
        public int teacherId { get; set; }

    }
}
