using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class LessonDto
    {
        public int LessonId { get; set; }

        [MaxLength(100)]
        public string LessonName { get; set; } = "";

        public int? Credit { get; set; }

        public int DepartmentId { get; set; }

        public int? TeacherId { get; set; }
    }
}
