using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Models;
using OgrenciBilgiSistemiProje.Services;

namespace OgrenciBilgiSistemiProje.Controllers
{
    
    public class TeacherController : BaseController
    {
        private readonly ApplicationDbContext context; // Veritabanı bağlantısı
        private readonly IWebHostEnvironment environment;

        public TeacherController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == HttpContext.Session.GetString("username")); // Kullanıcı adına göre öğrenciyi buluyoruz.
            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(teacher); // Öğrenciyi view'a gönderiyoruz.
        }

        public IActionResult Edit()
        {
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == HttpContext.Session.GetString("username")); // Kullanıcı adına göre öğrenciyi buluyoruz.
            if (teacher == null)
            {
                return RedirectToAction("StuTeaLog", "Account"); // Eğer öğrenci yoksa login sayfasına yönlendiriyoruz.
            }
            var teacherDto = new TeacherDto // Öğrenciyi öğrenciDto'ya çeviriyoruz.
            {
                TeacherName = teacher.TeacherName,
                TeacherSurname = teacher.TeacherSurname,
                TeacherMail = teacher.TeacherMail,
                TeacherPhone = teacher.TeacherPhone,
                TeacherAddress = teacher.TeacherAddress,
                TeacherGender = teacher.TeacherGender,
                TeacherPassword = teacher.TeacherPassword

            };

            // Öğretmenin derslerini buluyoruz.
            var lesson = context.Lessons.Include(x => x.Teacher).FirstOrDefault(x => x.Teacher.TeacherMail == HttpContext.Session.GetString("username")); // Öğretmenin derslerini buluyoruz.

            ViewBag.LessonName = lesson.LessonName; // Ders adını view'a gönderiyoruz.
            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName; // Resim dosya adını view'a gönderiyoruz.
            return View(teacherDto); // ÖğrenciDto'yu view'a gönderiyoruz.
        }

        [HttpPost]
        public IActionResult Edit(TeacherDto teacherDto)
        {
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == HttpContext.Session.GetString("username"));
            if (teacher == null)
            {
                return RedirectToAction("StuTeaLog", "Account");
            }
            if(!ModelState.IsValid) //eğer model doğrulama başarısız olursa
            {
                return View(teacherDto); // öğrenciDto'yu view'a geri gönder
            }

            string newFileName = teacher.ImageFileName; // Eğer yeni bir resim yüklenmediyse eski resim dosya adını kullan
            if (teacherDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // burada dosya adı oluşturuluyor Örnek : 20211231235959999
                newFileName += Path.GetExtension(teacherDto.ImageFile!.FileName); // dosya adının sonuna uzantı ekleniyor Örnek : 20211231235959999.jpg

                string imageFullPath = environment.WebRootPath + "/img/" + newFileName; // Resmin yükleneceği yolu belirt ve dosya adını ekle
                using (var stream = System.IO.File.Create(imageFullPath)) // Dosyayı oluşturuyoruz. Çünkü burada dosyayı sunucuya kaydedeceğiz. Stream sınıfı, dosya işlemleri yapmamızı sağlar.
                {
                    teacherDto.ImageFile.CopyTo(stream); // Dosyayı yeni dosyaya kopyalıyoruz.
                }

                string oldImagePath = environment.WebRootPath + "/img/" + teacher.ImageFileName; // Eski resmin dosya yolunu belirt
                System.IO.File.Delete(oldImagePath); // Eski resmi siliyoruz çünkü artık kullanılmayacak

            }

            teacher.TeacherMail = teacherDto.TeacherMail;
            teacher.TeacherPhone = teacherDto.TeacherPhone;
            teacher.TeacherAddress = teacherDto.TeacherAddress;
            teacher.ImageFileName = newFileName;
            teacher.TeacherPassword = teacherDto.TeacherPassword;

            context.SaveChanges(); // Değişiklikleri kaydet
            return RedirectToAction("Index"); // Index sayfasına yönlendir

        }
    }
}
