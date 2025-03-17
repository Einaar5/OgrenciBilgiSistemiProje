namespace OgrenciBilgiSistemiProje.Models
{
    public class GradeDto
    {
        public int GradeId { get; set; }

        public int StudentId { get; set; }

        public int LessonId { get; set; }

        public int? Midterm { get; set; }

        public int? Final { get; set; }
    }
}
