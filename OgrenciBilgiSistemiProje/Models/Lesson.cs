using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace OgrenciBilgiSistemiProje.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [MaxLength(100)]
        public string LessonName { get; set; } = "";

        public int? Credit { get; set; } // Dersin kredisi (örneğin, 3 kredi)

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        [ForeignKey("Teacher")]
        public int? TeacherId { get; set; } // Bir ders bir öğretmen tarafından verilir
        public virtual Teacher Teacher { get; set; }

        // Dersin notlarıyla ilişki
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
