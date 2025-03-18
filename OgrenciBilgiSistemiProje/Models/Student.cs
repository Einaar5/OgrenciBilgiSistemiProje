using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
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
        public string Password { get; set; } = "";

        [MaxLength(100)]
        public string StudentPhone { get; set; } = "";

        [MaxLength(250)]
        public string StudentAddress { get; set; } = "";

        [MaxLength(100)]
        public string StudentGender { get; set; } = "";

        public DateTime StudentRegisterDate { get; set; }

       

        [MaxLength(100)]
        public string ImageFileName { get; set; } = "";

        // Department ile ilişki
        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        // Notlarla ilişki
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
