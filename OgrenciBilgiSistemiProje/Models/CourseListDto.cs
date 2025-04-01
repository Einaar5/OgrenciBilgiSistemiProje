using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class CourseListDto
    {
        public int Id { get; set; }

        [Required]
        public int CourseDay { get; set; }


        public string CourseTime { get; set; } = "";


        [Required]
        public int DepartmentId { get; set; }


        [Required]
        public int LessonId { get; set; }
    }
}
