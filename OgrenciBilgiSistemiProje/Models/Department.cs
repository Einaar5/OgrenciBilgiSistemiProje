using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = "";

        public int? Quota { get; set; }

        // Bölümün dersleriyle ilişki
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        // Bölümün öğrencileriyle ilişki
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
