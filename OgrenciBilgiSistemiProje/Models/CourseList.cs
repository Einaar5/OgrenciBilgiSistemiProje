using System.ComponentModel.DataAnnotations;

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

       
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

    }
}
