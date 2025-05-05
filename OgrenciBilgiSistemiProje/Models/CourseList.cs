using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgrenciBilgiSistemiProje.Models
{
    public class CourseList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseDay {  get; set; }


        public string CourseTime { get; set; } = "";

        public string CourseClass { get; set; } = "";

        public int? StudentLessonId { get; set; }
        [ForeignKey("StudentLessonId")]
        public virtual StudentLesson? StudentLesson { get; set; }

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

    }
}
