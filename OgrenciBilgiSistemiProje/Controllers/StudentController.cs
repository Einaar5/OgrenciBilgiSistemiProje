using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Models;
using OgrenciBilgiSistemiProje.Services;

namespace OgrenciBilgiSistemiProje.Controllers
{
   
public class StudentController : BaseController
    {
        private readonly ApplicationDbContext context; // Veritabanı bağlantısı
        private readonly IWebHostEnvironment environment;

        public StudentController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            // Öğrenciyi bul ve Department bilgisini de dahil et
            var student = context.Students
                .Include(s => s.Department) // Department bilgisini dahil et
                .FirstOrDefault(x => x.StudentEmail == HttpContext.Session.GetString("username"));

            if (student != null)
            {
                ViewData["ImageFileName"] = student.ImageFileName;

                // DepartmentName'i ViewData'ya ekleyebilirsiniz
                ViewData["DepartmentName"] = student.Department.Name;
            }

            return View(student);
        }

        public IActionResult Edit()
        {
            var student = context.Students.FirstOrDefault(x => x.StudentEmail == HttpContext.Session.GetString("username"));

            if (student == null)
            {
                return RedirectToAction("StuTeaLog","Account");
            }
            var studentDto = new StudentDto
            {
                StudentName = student.StudentName,
                StudentSurname = student.StudentSurname,
                StudentEmail = student.StudentEmail,
                StudentPhone = student.StudentPhone,
                StudentAddress = student.StudentAddress,
                StudentGender = student.StudentGender,
                DepartmentId = student.DepartmentId,
                Password = student.Password

            };
            
            var departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.           
            ViewData["Departments"] = departments; // Bölümleri view'a gönderiyoruz.
            ViewData["ImageFileName"] = student.ImageFileName; // Resim dosya adını view'a gönderiyoruz.


            return View(studentDto);
        }


        [HttpPost]
        public IActionResult Edit(StudentDto studentDto)
        {
            var student = context.Students.FirstOrDefault(x => x.StudentEmail == HttpContext.Session.GetString("username"));
            if (student == null)
            {
                return RedirectToAction("Index", "Student");
            }
            if (!ModelState.IsValid) // Eğer model doğrulama başarısız olursa
            {
                return View(studentDto); // Hata varsa formu tekrar göster
            }
            string newFileName = student.ImageFileName; // Eğer yeni bir resim yüklenmediyse eski resim dosya adını kullan
            if (studentDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // burada dosya adı oluşturuluyor Örnek : 20211231235959999
                newFileName += Path.GetExtension(studentDto.ImageFile!.FileName); // dosya adının sonuna uzantı ekleniyor Örnek : 20211231235959999.jpg

                string imageFullPath = environment.WebRootPath + "/img/" + newFileName; // Resmin yükleneceği yolu belirt ve dosya adını ekle
                using (var stream = System.IO.File.Create(imageFullPath)) // Dosyayı oluşturuyoruz. Çünkü burada dosyayı sunucuya kaydedeceğiz. Stream sınıfı, dosya işlemleri yapmamızı sağlar.
                {
                    studentDto.ImageFile.CopyTo(stream); // Dosyayı yeni dosyaya kopyalıyoruz.
                }

                string oldImagePath = environment.WebRootPath + "/img/" + student.ImageFileName; // Eski resmin dosya yolunu belirt
                System.IO.File.Delete(oldImagePath); // Eski resmi siliyoruz çünkü artık kullanılmayacak

            }

            student.StudentEmail = studentDto.StudentEmail;
            student.StudentPhone = studentDto.StudentPhone;
            student.StudentAddress = studentDto.StudentAddress;
            student.ImageFileName = newFileName;
            student.Password = studentDto.Password;


            context.SaveChanges();
            return RedirectToAction("Index", "Student");
        }
    }
}
