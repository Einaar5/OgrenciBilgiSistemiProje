using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace OgrenciBilgiSistemiProje.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [MaxLength(100)]
        public string StudentName { get; set; } = "";

        [MaxLength(100)]
        public string StudentSurname { get; set; } = "";

        [MaxLength(100)]
        public string StudentEmail { get; set; } = "";

        [MaxLength(100)]
        public string StudentPhone { get; set; } = "";

        [MaxLength(250)]
        public string StudentAddress { get; set; } = "";

        [MaxLength(100)]
        public string StudentGender { get; set; } = "";

        public DateTime StudentRegisterDate { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; } = "";

        [MaxLength(100)]
        public string DepartmentName { get; set; } = ""; // ="" ile boş değer atadık. yoksa null hatası alırız.






    }
}
