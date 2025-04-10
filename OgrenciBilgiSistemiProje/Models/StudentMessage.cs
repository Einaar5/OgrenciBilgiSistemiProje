using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class StudentMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = "";

        [Required]
        [MaxLength(500)]
        public string Content { get; set; } = "";

        public DateTime SentDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public int SenderStudentId { get; set; }    // öğrencinin kimliğini (ID’sini) tutan bir alan
        [ForeignKey("SenderStudentId")]     // Students tablosundaki StudentId ile ilişkilendirilecek.  Kimden geldiğini gösterir.
        public Student Sender { get; set; }

        public int ReceiverTeacherId { get; set; }      // öğretmenin kimliğini (ID’sini) tutuyor.
        [ForeignKey("ReceiverTeacherId")]       // Teachers tablosundaki Id ile ilişkilendirilecek. Kime gittiği gösterir.
        public Teacher Receiver { get; set; }
    }
}
