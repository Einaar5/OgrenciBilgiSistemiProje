using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Models;
using OgrenciBilgiSistemiProje.Services;

namespace OgrenciBilgiSistemiProje.Controllers
{
    [Authorize(Roles = "Teacher")] // Sadece öğretmen rolündeki kullanıcılar bu controller'a erişebilir
    public class TeacherController : Controller
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
            var teacherUsername = HttpContext.User.Identity.Name; // Kullanıcı adını alıyoruz.
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername); // Kullanıcı adına göre öğrenciyi buluyoruz.
            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(teacher); // Öğrenciyi view'a gönderiyoruz.
        }

        public IActionResult Edit()
        {
            var teacherUsername = HttpContext.User.Identity.Name; // Kullanıcı adını alıyoruz.
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername); // Kullanıcı adına göre öğrenciyi buluyoruz.
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
            var lesson = context.Lessons.Include(x => x.Teacher).FirstOrDefault(x => x.Teacher.TeacherMail == teacherUsername); // Öğretmenin derslerini buluyoruz.

            ViewBag.LessonName = lesson.LessonName; // Ders adını view'a gönderiyoruz.
            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName; // Resim dosya adını view'a gönderiyoruz.
            return View(teacherDto); // ÖğrenciDto'yu view'a gönderiyoruz.
        }

        [HttpPost]
        public IActionResult Edit(TeacherDto teacherDto)
        {
            var teacherUsername = HttpContext.User.Identity.Name; // Kullanıcı adını alıyoruz.
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);
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



        public IActionResult Grades()
        {
            var teacherUsername = HttpContext.User.Identity.Name; // Kullanıcı adını alıyoruz.
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername); // Öğretmeni buluyoruz.
            if (teacher == null)
            {
                return RedirectToAction("StuTeaLog", "Account"); // Eğer öğretmen yoksa login sayfasına yönlendiriyoruz.
            }


            // Öğretmenin derslerini alıyoruz.
            var lessons = context.Lessons // burada dersler tablosundan öğretmenin derslerini alıyoruz
                .Include(x => x.Teacher)
                .Where(x => x.Teacher.TeacherMail == teacherUsername)
                .ToList();

            // Öğretmenin derslerine ait notları alıyoruz.
            var grades = context.Grades
                .Include(x => x.Student)
                .Include(x => x.Lesson)
                .Where(x => lessons.Select(l => l.LessonId).Contains(x.LessonId))
                .ToList();


            // Öğretmenin derslerine kayıtlı öğrencileri alıyoruz.
            var studentIds = grades.Select(g => g.StudentId).Distinct().ToList();
            ViewBag.Students = context.Students
                .Where(s => studentIds.Contains(s.StudentId)) // Sadece notu olan öğrencileri al
                .ToList();

            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            ViewBag.TeacherLessons = teacher?.Lessons?.ToList() ?? new List<Lesson>();
            return View(grades); // Notları view'a gönderiyoruz.
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public IActionResult Grades(GradeDto gradeDto)
        {
            // 1. Öğretmen doğrulama
            var teacherUsername = HttpContext.User.Identity.Name;
            var teacher = context.Teachers
                .Include(t => t.Lessons)
                .FirstOrDefault(t => t.TeacherMail == teacherUsername);

            if (teacher == null)
            {
                return Unauthorized(); // 401 hatası daha uygun
            }

            // 2. Seçilen dersin öğretmene ait olup olmadığını kontrol
            var selectedLesson = teacher.Lessons.FirstOrDefault(l => l.LessonId == gradeDto.LessonId);
            if (selectedLesson == null)
            {
                ModelState.AddModelError("LessonId", "Bu dersi vermiyorsunuz");
                return View(gradeDto); // Aynı sayfaya hata mesajıyla dön
            }

            // 3. Öğrenci kontrolü
            var student = context.Students
                .FirstOrDefault(s => s.StudentId == gradeDto.StudentId);
            if (student == null)
            {
                ModelState.AddModelError("StudentId", "Öğrenci bulunamadı");
                return View(gradeDto);
            }

            // 4. Not işlemleri (sadece seçilen ders için)
            var grade = context.Grades
                .FirstOrDefault(g => g.StudentId == gradeDto.StudentId
                                 && g.LessonId == gradeDto.LessonId);

            // Ortalama hesaplama düzeltildi
            var average = (gradeDto.Midterm * 0.4) + (gradeDto.Final * 0.6);

            if (grade == null)
            {
                context.Grades.Add(new Grade
                {
                    StudentId = gradeDto.StudentId,
                    LessonId = gradeDto.LessonId,
                    Midterm = gradeDto.Midterm,
                    Final = gradeDto.Final,
                    Average = (float?)average
                });
            }
            else
            {
                grade.Midterm = gradeDto.Midterm;
                grade.Final = gradeDto.Final;
                grade.Average = (float?)average;
            }

            try
            {
                context.SaveChanges();
                TempData["SuccessMessage"] = "Notlar başarıyla kaydedildi";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kayıt sırasında hata oluştu";
                // Loglama yapılabilir
            }

            return RedirectToAction("Grades");
        }
    }
}
