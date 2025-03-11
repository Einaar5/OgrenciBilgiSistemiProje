using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class TeacherDto
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
        public string TeacherPhone { get; set; } = "";
        [MaxLength(100)]
        public string TeacherAdrress { get; set; } = "";
        [MaxLength(100)]
        public string TeacherGender { get; set; } = "";
        public DateTime TeacherRegisterDate { get; set; }
        public IFormFile? ImageFile { get; set; }
        [MaxLength(100)]
        public string TeacherBrans { get; set; } = "";
    }
}
