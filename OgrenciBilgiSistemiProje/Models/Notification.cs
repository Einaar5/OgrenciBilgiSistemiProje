using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgrenciBilgiSistemiProje.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [MaxLength(100)]
        public string NotificationTitle { get; set; } = "";

        [MaxLength(500)]
        public string NotificationContent { get; set; } = "";

        public DateTime NotificationDate { get; set; } = DateTime.Now; // Varsayılan tarih

        public Teacher Teacher { get; set; } = new Teacher();

        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }

        public Department Department { get; set; } = new Department();

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }


    }
}
