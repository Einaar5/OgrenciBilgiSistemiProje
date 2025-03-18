using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string TeacherName { get; set; } = "";

        [MaxLength(100)]
        public string TeacherSurname { get; set; } = "";

        [MaxLength(100)]
        public string TeacherMail { get; set; } = "";

        [MaxLength(100)]
        public string TeacherPassword { get; set; } = "";

        [MaxLength(100)]
        public string TeacherPhone { get; set; } = "";

        [MaxLength(100)]
        public string TeacherAddress { get; set; } = "";

        [MaxLength(100)]
        public string TeacherGender { get; set; } = "";

        public DateTime TeacherRegisterDate { get; set; }

        

        [MaxLength(100)]
        public string ImageFileName { get; set; } = "";

       
        // Öğretmenin verdiği derslerle ilişki
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
