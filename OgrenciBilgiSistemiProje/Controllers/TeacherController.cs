using System;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
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
            var teacherUsername = HttpContext.User.Identity?.Name; // Kullanıcı adını alıyoruz.
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername); // Kullanıcı adına göre öğrenciyi buluyoruz.
            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(teacher); // Öğrenciyi view'a gönderiyoruz.
        }

        #region Öğretmen Bilgi Edit
        public IActionResult Edit()
        {
            var teacherUsername = HttpContext.User.Identity?.Name; // Kullanıcı adını alıyoruz.
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
            var teacherUsername = HttpContext.User.Identity?.Name; // Kullanıcı adını alıyoruz.
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
        #endregion


        #region Notlandırma
        public IActionResult Grades()
        {
            var teacherUsername = HttpContext.User.Identity?.Name; // Kullanıcı adını alıyoruz.
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
            var teacherUsername = HttpContext.User.Identity?.Name;
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
        #endregion


        #region Courses
        public IActionResult Courses()
        {
            var teacherUsername = HttpContext.User.Identity?.Name; // Kullanıcı adına göre id alıyoruz
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);
            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            var teacherId = context.Teachers
                .Where(x => x.TeacherMail == teacherUsername)
                .Select(x => x.Id)
                .FirstOrDefault();

            // Öğretmenin derslerini günlere göre grupla
            var courses = context.CourseList
                .Where(c => c.Lesson.TeacherId == teacherId)
                .Include(c => c.Lesson)
                .Include(c=>c.Department)
                .OrderBy(c => c.CourseDay)
                .ThenBy(c => c.CourseTime)
                .ToList();

            return View(courses);

        }
        #endregion


        #region Duyuru Sistemi

        // Öğretmende Duyuru Listeleme
        public IActionResult ListNotifications()
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(teacherUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = context.Teachers
                .FirstOrDefault(x => x.TeacherMail == teacherUsername);

            if (teacher == null)
            {
                return NotFound("Öğretmen bulunamadı.");
            }

            var notifications = context.Notifications
                .Include(n => n.Department)     // Duyurunun ait olduğu bölümü dahil et. Department ile bir foreign key ilişkisi ve Department tablosundaki veritabanını çeker.
                .Where(n => n.TeacherId == teacher.Id)      // Notifications tablosundaki her satırın (n) TeacherId sütununun, teacher nesnesinin Id özelliğiyle eşleşip eşleşmediğini kontrol eder.
                .OrderByDescending(n => n.NotificationDate) // belirli bir kritere göre sıralar.
                .ToList();  // Listeler

            ViewBag.TeacherImageFileName = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(notifications);
        }


        // Öğretmen Duyuru Oluşturma


        public IActionResult CreateNotification()
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(teacherUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = context.Teachers
                .Include(t => t.Lessons)    // Öğretmeni çekerken, onun derslerini de getir.
                .ThenInclude(l => l.Department)     // Dersleri çekerken, her dersin bölümünü de getir.
                .FirstOrDefault(x => x.TeacherMail == teacherUsername); // E-postası teacherUsername ile eşleşen ilk öğretmeni bul. Eğer yoksa, null (hiçbir şey) döndür."

            if (teacher == null)
            {
                return NotFound("Öğretmen bulunamadı.");
            }

            // Bir öğretmenin (teacher) verdiği derslerle bağlantılı olan bölümleri (Department) buluyor.
            var departments = teacher.Lessons
                .Select(l => l.Department)  // listedeki her bir eleman için bir şey seçmemizi sağlar. Burada, her ders (l) için o dersin bağlı olduğu bölümü (Department) seçiyoruz. Her dersin bölümünü al diyoruz.
                .Distinct() // listedeki tekrar eden elemanları temizler.
                .ToList();

            if (!departments.Any())
            {
                departments = context.Departments.ToList();
            }

            // Notification adında bir nesne oluşuyor. Bu nesne, bir bildirim kaydı içeriyor ve şu bilgilerle doluruluyor
            var notification = new Notification
            {
                TeacherId = teacher.Id, // Bu bildirim, teacher adlı öğretmen için. Onun ID’sini bildirime yaz.
                NotificationDate = DateTime.Now,
                IsRead = false
            };

            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            ViewBag.TeacherImageFileName = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;

            return View(notification);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNotification(Notification notification)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(teacherUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = context.Teachers
                .Include(t => t.Lessons)    // Öğretmeni alırken, onun derslerini de yanına ekle.
                .ThenInclude(l => l.Department) // Dersleri çekerken, her dersin hangi bölüme (Department) ait olduğunu da getir.
                .FirstOrDefault(x => x.TeacherMail == teacherUsername); // E-postası teacherUsername ile eşleşen ilk öğretmeni bul. Eğer yoksa, null (hiçbir şey) döndür.

            if (teacher == null)
            {
                return NotFound("Öğretmen bulunamadı.");
            }

            // Navigation property'leri ModelState'den çıkaralım
            // Formdaki her alanı kontrol ettim, şimdi durumlarını tutuyorum.
            ModelState.Remove("Teacher");   // Teacher alanını doğrulama kontrolünden çıkar
            ModelState.Remove("Department");    // Department alanını da doğrulama kontrolünden çıkar

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                ViewBag.Errors = errors;

                var departments = teacher.Lessons   // teacher nesnesinden Dersleri tablosunu alırız.
                    .Select(l => l.Department)  // her dersin bağlı olduğu bölümü alırız
                    .Distinct()     // Aynı olan değerleri temizler.
                    .ToList();  // Ve Listeleriz.

                if (!departments.Any())
                {
                    departments = context.Departments.ToList();     // eğer öğretmenin hiç dersi yoksa (veya derslere bağlı bölüm yoksa), tüm bölümleri getiriyor.
                }

                /*
                    bir dropdown menüsü (HTML <select>) oluşturmak için kullanılan bir yapı.
                    departments: Dropdown’da gösterilecek bölümler.
                    "Id": Her bölümün benzersiz kimliği (ID’si).
                    "Name": Kullanıcının gördüğü bölüm adı (mesela, “Matematik Bölümü”).
                    notification.DepartmentId: Varsayılan olarak seçili olacak bölümün ID’si (mesela, kullanıcı daha önce bir bölüm seçtiyse, o seçili kalsın).
                    Yani, “Bölüm listesini al, dropdown için hazırla ve varsa önceki seçimi işaretle” diyoruz.
                 */
                ViewBag.Departments = new SelectList(departments, "Id", "Name", notification.DepartmentId);
                ViewBag.TeacherImageFileName = teacher.ImageFileName;
                return View(notification);
            }

            // Notification nesnesini tamamla
            notification.TeacherId = teacher.Id;
            notification.NotificationDate = DateTime.Now;
            notification.IsRead = false;

            // Veritabanına kaydet
            try
            {
                context.Notifications.Add(notification);
                context.SaveChanges();
                return RedirectToAction("ListNotifications");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Duyuru kaydedilirken bir hata oluştu: " + ex.Message;
                var departments = teacher.Lessons
                    .Select(l => l.Department)
                    .Distinct()
                    .ToList();
                if (!departments.Any())
                {
                    departments = context.Departments.ToList();
                }
                ViewBag.Departments = new SelectList(departments, "Id", "Name", notification.DepartmentId);
                ViewBag.TeacherImageFileName = teacher.ImageFileName;
                return View(notification);
            }
        }



        // Öğretmen Gelen Duyuruları Görme İşlemi

        public IActionResult ListMessages()
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            if(string.IsNullOrEmpty(teacherUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);

            if(teacher == null)
            {
                return NotFound("Öğretmen bulunamadı.");
            }

            var messages = context.StudentMessages  // StudentMessages tablosunu alıyoruz
                .Include(m => m.Sender) // İçerisinden kimin gönderdiği değeri alırız.
                .Where(m => m.ReceiverTeacherId == teacher.Id)  // Koşulluyoruz. Eğer Gönderilen öğretmenin Id'si, teacher.Id'ye eşitse
                .OrderByDescending(m => m.SentDate) // Ve Gönderilen tarihine göre sıralıyoruz.
                .ToList();  // Listele

            ViewBag.TeacherImageFileName = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(messages);
        }

        

        #endregion

    }
}
