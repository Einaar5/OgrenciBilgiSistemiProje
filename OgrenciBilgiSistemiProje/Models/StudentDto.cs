using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class StudentDto
    {
        public int StudentId { get; set; }

        [MaxLength(100)]
        public string StudentName { get; set; } = "";

        [MaxLength(100)]
        public string StudentSurname { get; set; } = "";

        [MaxLength(100)]
        public string StudentEmail { get; set; } = "";

        [MaxLength(100)]
        public string Password { get; set; } = "";

        [MaxLength(100)]
        public string StudentPhone { get; set; } = "";

        [MaxLength(250)]
        public string StudentAddress { get; set; } = "";

        [MaxLength(100)]
        public string StudentGender { get; set; } = "";

        public DateTime StudentRegisterDate { get; set; }

        public IFormFile? ImageFile { get; set; }

        public int DepartmentId { get; set; } // DepartmentName yerine DepartmentId
    }
}
