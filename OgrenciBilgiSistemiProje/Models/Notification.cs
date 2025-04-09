using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgrenciBilgiSistemiProje.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string NotificationTitle { get; set; } = "";

        [Required]
        [MaxLength(500)]
        public string NotificationContent { get; set; } = "";

        public DateTime NotificationDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        [Required]
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }

}