using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemiProje.Models
{
    public class DepartmentDto
    {
        
        public int Id { get; set; }  // Primary Key

        [MaxLength(100)]
        public string Name { get; set; } = "";  // Bölüm Adı

        public int Quota { get; set; }  // Kontenjan
    }
}
