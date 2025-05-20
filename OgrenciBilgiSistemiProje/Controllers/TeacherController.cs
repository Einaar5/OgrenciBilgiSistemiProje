using System;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using OgrenciBilgiSistemiProje.Migrations;
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
            var teacher = context.Teachers.Include(l => l.Lessons).FirstOrDefault(x => x.TeacherMail == teacherUsername); // Kullanıcı adına göre öğrenciyi buluyoruz.
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

            ViewBag.LessonName = lesson?.LessonName; // Ders adını view'a gönderiyoruz.
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
            if (!ModelState.IsValid) //eğer model doğrulama başarısız olursa
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
            var teacher = context.Teachers.Include(l => l.Lessons).FirstOrDefault(x => x.TeacherMail == teacherUsername); // Kullanıcı adına göre öğrenciyi buluyoruz.
            ViewData["ImageFileName"] = teacher.ImageFileName;

            var teacherId = context.Teachers
               .Where(x => x.TeacherMail == teacherUsername)
               .Select(x => x.Id)
               .FirstOrDefault();
            var lessons = context.Lessons
                .Include(x => x.Teacher)
                .Include(x => x.Department)
                .Where(x => x.TeacherId == teacherId).ToList();

            return View(lessons);
        }

        public IActionResult QuizList(int id)
        {
            var teacherUsername = HttpContext.User.Identity?.Name; // Kullanıcı adını alıyoruz.
            var teacher = context.Teachers.Include(l => l.Lessons).FirstOrDefault(x => x.TeacherMail == teacherUsername); // Kullanıcı adına göre öğrenciyi buluyoruz.
            ViewData["ImageFileName"] = teacher.ImageFileName;
            var quizs = context.Quizzes
               .Include(q => q.Lesson)
               .Where(q => q.Lesson.LessonId == id)
               .ToList();
            if (quizs == null)
            {
                return RedirectToAction("Grades");
            }
           int lessonId = id; // LessonId'yi view'a gönderiyoruz.

            ViewBag.LessonId = lessonId.ToString(); // LessonId'yi view'a gönderiyoruz.

            return View(quizs);
        }

        public IActionResult CreateQuiz(int id)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            var teacher = context.Teachers.Include(l => l.Lessons).FirstOrDefault(x => x.TeacherMail == teacherUsername);
            ViewData["ImageFileName"] = teacher?.ImageFileName;

            var lesson = context.Lessons.FirstOrDefault(x => x.LessonId == id);
            if (lesson == null) return RedirectToAction("Grades");

            // O dersin mevcut sınavlarını ve toplam ağırlığını al
            var quizzes = context.Quizzes
                .Where(q => q.LessonId == lesson.LessonId)
                .ToList();

            var totalWeight = quizzes.Sum(q => q.QuizWeight);
            var remainingWeight = 100 - totalWeight; // Kalan ağırlığı hesapla

            var quizDto = new QuizDto
            {
                LessonId = lesson.LessonId,
            };

            // ViewBag değerlerini her durumda ata
            ViewBag.TotalWeight = totalWeight;
            ViewBag.RemainingWeight = remainingWeight; // BU SATIR EKSİKTİ!
            ViewBag.LessonName = lesson.LessonName;

            // Sınav isimleri
            string[] quizNames = { "Vize", "Final", "Quiz", "Proje", "Ödev" };
            ViewBag.QuizNames = quizNames;

            return View(quizDto);
        }


        [HttpPost]
        public IActionResult CreateQuiz(QuizDto quizDto)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            var teacher = context.Teachers.Include(l => l.Lessons)
                                         .FirstOrDefault(x => x.TeacherMail == teacherUsername);
            var lesson = context.Lessons.FirstOrDefault(l => l.LessonId == quizDto.LessonId);

            if (lesson == null || teacher == null)
            {
                ViewBag.ErrorMessage = "Ders veya öğretmen bulunamadı.";
                ViewBag.Lessons = context.Lessons.ToList();
                // ViewBag.QuizNames'i burada tekrar set etmeye gerek yok, çünkü GET metodundan geliyor
                ViewBag.LessonName = lesson?.LessonName;
                ViewBag.TotalWeight = 0;
                ViewBag.RemainingWeight = 100;
                return View(quizDto);
            }

            // O dersin sınavlarının ağırlıklarını alıyoruz
            var quizzes = context.Quizzes
                .Where(q => q.LessonId == lesson.LessonId)
                .ToList();

            var totalWeight = quizzes.Sum(q => q.QuizWeight);
            var newTotalWeight = totalWeight + quizDto.QuizWeight;

            // Toplam ağırlık 100'ü geçiyorsa hata ver
            if (newTotalWeight > 100)
            {
                ViewBag.ErrorMessage = $"Toplam ağırlık %100'ü geçemez. Kalan ağırlık: {100 - totalWeight}%";
                ViewBag.Lessons = context.Lessons.ToList();
                ViewBag.LessonName = lesson.LessonName;
                ViewBag.TotalWeight = totalWeight;
                ViewBag.RemainingWeight = 100 - totalWeight;
                return View(quizDto);
            }

            if (ModelState.IsValid)
            {
                Quiz quiz = new Quiz
                {
                    QuizName = quizDto.QuizName,
                    QuizWeight = quizDto.QuizWeight,
                    teacherId = teacher.Id,
                    LessonId = lesson.LessonId,
                    Lesson = lesson,
                    Teacher = teacher
                };

                context.Quizzes.Add(quiz);
                context.SaveChanges();
                return RedirectToAction("Grades");
            }

            // ModelState geçersizse formu tekrar göster
            ViewBag.Lessons = context.Lessons.ToList();
            ViewBag.LessonName = lesson.LessonName;
            ViewBag.TotalWeight = totalWeight;
            ViewBag.RemainingWeight = 100 - totalWeight;
            return View(quizDto);
        }


        public IActionResult DeleteQuiz(int id)
        {
            var quiz = context.Quizzes
                .Include(q => q.Lesson)
                .FirstOrDefault(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound("Quiz bulunamadı.");
            }

            // Quiz'e bağlı tüm Grade kayıtlarını sil
            var gradesToDelete = context.Grades.Where(g => g.QuizId == id).ToList();
            context.Grades.RemoveRange(gradesToDelete);

            context.Quizzes.Remove(quiz);
            context.SaveChanges();

            return RedirectToAction("Grades");
        }

        public IActionResult EditQuiz(int id)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);

            if (teacher == null)
            {
                return RedirectToAction("StuTeaLog", "Account");
            }

            var quiz = context.Quizzes
                .Include(q => q.Lesson)
                .FirstOrDefault(x => x.Id == id);

            if (quiz == null)
            {
                return NotFound("Quiz bulunamadı.");
            }

            // Aynı derse ait diğer quiz'lerin toplam ağırlığını hesapla (mevcut quiz hariç)
            var totalWeight = context.Quizzes
                .Where(q => q.LessonId == quiz.LessonId && q.Id != id)
                .Sum(q => q.QuizWeight);

            var quizDto = new QuizDto()
            {
                Id = quiz.Id,
                QuizName = quiz.QuizName,
                QuizWeight = quiz.QuizWeight,
                LessonId = quiz.LessonId
            };

            ViewBag.TotalWeight = totalWeight;
            ViewBag.RemainingWeight = 100 - totalWeight;
            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(quizDto);
        }

        [HttpPost]
        public IActionResult EditQuiz(QuizDto quizDto)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);

            if (teacher == null)
            {
                return RedirectToAction("StuTeaLog", "Account");
            }

            var quiz = context.Quizzes
                .Include(q => q.Lesson)
                .FirstOrDefault(x => x.Id == quizDto.Id);

            if (quiz == null)
            {
                return NotFound("Quiz bulunamadı.");
            }

            // Aynı derse ait diğer quiz'lerin toplam ağırlığını hesapla (mevcut quiz hariç)
            var totalWeight = context.Quizzes
                .Where(q => q.LessonId == quiz.LessonId && q.Id != quizDto.Id)
                .Sum(q => q.QuizWeight);

            var newTotalWeight = totalWeight + quizDto.QuizWeight;

            // Toplam ağırlık kontrolü
            if (newTotalWeight > 100)
            {
                ModelState.AddModelError("QuizWeight", $"Toplam ağırlık %100'ü geçemez. Kalan ağırlık: {100 - totalWeight}%");
                ViewBag.TotalWeight = totalWeight;
                ViewBag.RemainingWeight = 100 - totalWeight;
                ViewBag.ImageLayout = teacher.ImageFileName;
                ViewData["ImageFileName"] = teacher.ImageFileName;
                return View(quizDto);
            }

            if (ModelState.IsValid)
            {
                quiz.QuizName = quizDto.QuizName;
                quiz.QuizWeight = quizDto.QuizWeight;
                context.SaveChanges();
                return RedirectToAction("Grades");
            }

            ViewBag.TotalWeight = totalWeight;
            ViewBag.RemainingWeight = 100 - totalWeight;
            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(quizDto);
        }



        //Öğrenci notlandırma

        public IActionResult GradeList(int id) // id = lessonId
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            var teacher = context.Teachers
                                 .Include(t => t.Lessons)
                                 .FirstOrDefault(t => t.TeacherMail == teacherUsername);

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            ViewData["ImageFileName"] = teacher.ImageFileName;

            // 🔴 SADECE bu derse ait quiz'ler
            var quizzes = context.Quizzes
                                 .Where(q => q.LessonId == id)
                                 .ToList();

            // 🔴 SADECE bu derse kayıtlı öğrenciler
            var students = context.StudentLessons
                                  .Where(sl => sl.LessonId == id)
                                  .Select(sl => sl.Student)
                                  .ToList();

            // 🔴 SADECE bu derse ait notlar (grade)
            var grades = context.Grades
                                .Where(g => g.LessonId == id)
                                .ToList();

            // View'a gönderilecek veriler
            ViewBag.Students = students;
            ViewBag.Quizzes = quizzes;
            ViewBag.Grades = grades;

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> UpdateAllScores(List<Grade> Grades)
        {
            foreach (var input in Grades)
            {
                var quiz = await context.Quizzes
                    .Include(q => q.Lesson)
                    .FirstOrDefaultAsync(q => q.Id == input.QuizId);

                if (quiz == null)
                {
                    TempData["ErrorMessage"] = $"Quiz bulunamadı. QuizId: {input.QuizId}";
                    continue;
                }

                var existingGrade = await context.Grades
                    .FirstOrDefaultAsync(g => g.StudentId == input.StudentId && g.QuizId == input.QuizId);

                if (existingGrade == null)
                {
                    // Yeni not
                    var newGrade = new Grade
                    {
                        StudentId = input.StudentId,
                        QuizId = input.QuizId,
                        LessonId = quiz.Lesson.LessonId, // DİKKAT: Buradan lessonId alınmalı
                        Score = input.Score
                    };
                    context.Grades.Add(newGrade);
                }
                else
                {
                    // Güncelleme
                    existingGrade.Score = input.Score;
                    context.Grades.Update(existingGrade);
                }
            }

            try
            {
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tüm notlar başarıyla güncellendi.";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Veritabanı hatası: " + ex.InnerException?.Message;
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
                .Include(c => c.Department)
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
            if (string.IsNullOrEmpty(teacherUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);

            if (teacher == null)
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


        public IActionResult DeleteMessages(int id)
        {
            var message = context.StudentMessages.Find(id);
            if (message == null)
            {
                return NotFound("Duyuru bulunamadı.");
            }
            context.StudentMessages.Remove(message);
            context.SaveChanges();
            return RedirectToAction("ListMessages");

        }




        public IActionResult DeleteNotification(int id) // notification silme 
        {
            var notification = context.Notifications.Find(id);
            if (notification == null)
            {
                return NotFound("Duyuru bulunamadı.");
            }
            context.Notifications.Remove(notification);
            context.SaveChanges();
            return RedirectToAction("ListNotifications");
        }
        #endregion


        #region Devamsızlık Durumu

        // İşlev: Öğretmenin bir ders seçip, o derse kayıtlı öğrencilerin devamsızlık durumlarını girmesini sağlar.
        public IActionResult Attendance(int lessonId = 0, DateTime? attendanceDate = null)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(teacherUsername))
            {
                return RedirectToAction("StuTeaLog", "Account");
            }

            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);
            ViewData["ImageFileName"] = teacher.ImageFileName;
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Öğretmen bulunamadı.";
                return RedirectToAction("StuTeaLog", "Account");
            }

            // Lessons tablosundan öğretmenin derslerini al
            var lessons = context.Lessons
                .Where(l => l.TeacherId == teacher.Id)
                .Select(l => new SelectListItem { Value = l.LessonId.ToString(), Text = l.LessonName })
                .ToList();
            ViewBag.Lessons = lessons;

            if (!lessons.Any())
            {
                TempData["WarningMessage"] = "Bu öğretmene ait ders bulunamadı.";
                return View(new List<Attendance>());
            }

            if (lessonId == 0)
            {
                ViewBag.SelectedLessonId = 0;
                return View(new List<Attendance>());
            }

            // Haftaları hesapla (1’den 14’e kadar)
            var startDate = new DateTime(2024, 10, 1); // Derslerin başlangıç tarihi (örneğin 1 Ekim 2024)
            var weeks = new List<SelectListItem>();
            for (int week = 1; week <= 14; week++)
            {
                var weekDate = startDate.AddDays((week - 1) * 7);
                weeks.Add(new SelectListItem
                {
                    Value = weekDate.ToString("yyyy-MM-dd"),
                    Text = $"{week}. Hafta "
                });
            }
            ViewBag.Weeks = weeks;

            // Session’dan seçili tarihi al veya güncelle
            string sessionDate = HttpContext.Session.GetString("SelectedAttendanceDate");
            DateTime selectedDate;
            if (attendanceDate.HasValue)
            {
                selectedDate = attendanceDate.Value.Date;
                HttpContext.Session.SetString("SelectedAttendanceDate", selectedDate.ToString("yyyy-MM-dd"));
                TempData["DebugMessage"] = $"Yeni attendanceDate alındı: {selectedDate:yyyy-MM-dd}";
            }
            else if (!string.IsNullOrEmpty(sessionDate))
            {
                selectedDate = DateTime.Parse(sessionDate);
                TempData["DebugMessage"] = $"Session’dan alındı: {selectedDate:yyyy-MM-dd}";
            }
            else
            {
                selectedDate = weeks.Any() ? DateTime.Parse(weeks.First().Value) : DateTime.Today;
                HttpContext.Session.SetString("SelectedAttendanceDate", selectedDate.ToString("yyyy-MM-dd"));
                TempData["DebugMessage"] = $"Varsayılan ayarlandı: {selectedDate:yyyy-MM-dd}";
            }
            ViewBag.SelectedDate = selectedDate;

            // StudentLessons tablosundan dersin öğrencilerini al
            var studentLessons = context.StudentLessons
                .Where(sl => sl.LessonId == lessonId)
                .Include(sl => sl.Student)
                .ToList();

            TempData["DebugMessage"] += $", Seçilen ders (LessonId: {lessonId}) için {studentLessons.Count} öğrenci bulundu.";

            if (!studentLessons.Any())
            {
                TempData["WarningMessage"] = "Bu derse kayıtlı öğrenci bulunamadı.";
                ViewBag.SelectedLessonId = lessonId;
                return View(new List<Attendance>());
            }

            // Devamsızlık girişi oluştur
            var attendances = studentLessons
                .Select(sl => new Attendance
                {
                    StudentId = sl.StudentId,
                    Student = sl.Student,
                    LessonId = lessonId,
                    AttendanceDate = selectedDate,
                    IsComeHour1 = true,
                    IsComeHour2 = true,
                    IsComeHour3 = true
                })
                .ToList();

            // Mevcut devamsızlık bilgilerini al
            var existingAttendances = context.Attendance
                .Where(a => a.LessonId == lessonId && a.AttendanceDate.Date == selectedDate)
                .Include(a => a.Student)
                .ToList();

            if (existingAttendances.Any())
            {
                attendances = existingAttendances;
            }

            // Devamsızlık raporu
            var absenceReport = context.StudentLessons
                .Where(sl => sl.LessonId == lessonId)
                .Include(sl => sl.Student)
                .GroupJoin(
                    context.Attendance.Where(a => a.LessonId == lessonId),
                    sl => sl.StudentId,
                    a => a.StudentId,
                    (sl, attendances) => new
                    {
                        StudentName = $"{sl.Student.StudentName} {sl.Student.StudentSurname}",
                        AbsenceCount = attendances.Sum(a => (!a.IsComeHour1 ? 1 : 0) + (!a.IsComeHour2 ? 1 : 0) + (!a.IsComeHour3 ? 1 : 0))
                    }
                )
                .ToList();

            ViewBag.AbsenceReport = absenceReport;
            ViewBag.SelectedLessonId = lessonId;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(attendances);
        }

        // İşlev: Öğretmenin girdiği devamsızlık bilgilerini kaydeder veya günceller.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Attendance(List<Attendance> attendances)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Öğretmen bulunamadı.";
                return RedirectToAction("StuTeaLog", "Account");
            }

            if (!attendances.Any())
            {
                TempData["ErrorMessage"] = "Kaydedilecek veri bulunamadı.";
                return RedirectToAction("Attendance");
            }

            var lessonId = attendances.First().LessonId;    // lessonId'yi attnedances tablonun ilk LessonId'yi alıyoruz.
            var attendanceDate = attendances.First().AttendanceDate;    // attnedanceDate'e attnedances tablonun ilk AttendanceDate'yi alıyoruz.

           

            int addedCount = 0; // Yeni kayıt sayısını tutar.
            int updatedCount = 0;   // Güncellenen kayıt sayısını tutar.

            // Devamsızlık bilgilerini güncelleme veya ekleme işlemi
            foreach (var attendance in attendances)
            {
                // Eğer Attendance tablosunda belirtilen değerler varsa, güncelleme işlemi yapıyoruz.
                var existing = context.Attendance.FirstOrDefault(a =>
                    a.StudentId == attendance.StudentId &&
                    a.LessonId == lessonId &&
                    a.AttendanceDate.Date == attendanceDate.Date);

                // Eğer mevcut kayıt yoksa, yeni bir kayıt ekliyoruz.
                if (existing == null)
                {
                    context.Attendance.Add(new Attendance
                    {
                        StudentId = attendance.StudentId,
                        LessonId = attendance.LessonId,
                        AttendanceDate = attendanceDate,
                        IsComeHour1 = attendance.IsComeHour1,
                        IsComeHour2 = attendance.IsComeHour2,
                        IsComeHour3 = attendance.IsComeHour3,
                        CreatedDate = DateTime.Now
                    });
                    addedCount++;
                }
                else
                {
                    existing.IsComeHour1 = attendance.IsComeHour1;
                    existing.IsComeHour2 = attendance.IsComeHour2;
                    existing.IsComeHour3 = attendance.IsComeHour3;
                    existing.CreatedDate = DateTime.Now;
                    context.Attendance.Update(existing);
                    updatedCount++;
                }
            }

            try
            {
                context.SaveChanges();
                TempData["SuccessMessage"] = $"{addedCount} yeni kayıt eklendi, {updatedCount} kayıt güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kayıt sırasında bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Attendance", new { lessonId, attendanceDate });
        }


        public IActionResult AttendanceReport(int lessonId = 0)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(teacherUsername))
            {
                return RedirectToAction("StuTeaLog", "Account");
            }

            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Öğretmen bulunamadı.";
                return RedirectToAction("StuTeaLog", "Account");
            }

            var lessons = context.Lessons
                .Where(l => l.TeacherId == teacher.Id)
                .Select(l => new SelectListItem { Value = l.LessonId.ToString(), Text = l.LessonName })
                .ToList();
            ViewBag.Lessons = lessons;

            if (!lessons.Any())
            {
                TempData["WarningMessage"] = "Bu öğretmene ait ders bulunamadı.";
                return View(new AttendanceReportView());
            }

            var model = new AttendanceReportView();

            if (lessonId != 0)
            {
                var studentAttendances = context.StudentLessons
                    .Where(sl => sl.LessonId == lessonId)
                    .Include(sl => sl.Student)
                    .GroupJoin(
                        context.Attendance.Where(a => a.LessonId == lessonId),
                        sl => sl.StudentId,
                        a => a.StudentId,
                        (sl, attendances) => new
                        {
                            sl.Student,
                            AbsenceCount = attendances.Count(a => !a.IsComeHour1) +
                                           attendances.Count(a => !a.IsComeHour2) +
                                           attendances.Count(a => !a.IsComeHour3),
                            TotalLessons = attendances.Count() * 3
                        }
                    )
                    .ToList();

                TempData["DebugMessage"] = $"Seçilen ders (LessonId: {lessonId}) için {studentAttendances.Count} öğrenci bulundu.";

                if (!studentAttendances.Any())
                {
                    TempData["WarningMessage"] = "Bu derse kayıtlı öğrenci bulunamadı.";
                }
                else
                {
                    model.StudentReports = studentAttendances.Select(sa => new StudentAttendanceReport
                    {
                        StudentId = sa.Student.StudentId,
                        StudentName = $"{sa.Student.StudentName} {sa.Student.StudentSurname}",
                        AbsenceCount = sa.AbsenceCount,
                        TotalLessons = sa.TotalLessons,
                        AttendanceCount = sa.TotalLessons - sa.AbsenceCount // Devamsızlık sayısı
                    }).ToList();

                    model.LessonId = lessonId;
                    model.LessonName = context.Lessons
                        .FirstOrDefault(x => x.LessonId == lessonId)?.LessonName ?? "Ders Adı Bulunamadı";
                }
            }

            ViewData["ImageFileName"] = teacher.ImageFileName;
            return View(model);
        }

        #endregion

        #region Verdiğim Dersler

        public IActionResult myCourses()
        {
            var teacherUsername = HttpContext.User.Identity?.Name; // Kullanıcı adına göre id alıyoruz
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);
            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            var teacherId = context.Teachers
                .Where(x => x.TeacherMail == teacherUsername)
                .Select(x => x.Id)
                .FirstOrDefault();
            var lessons = context.Lessons
                .Include(x=>x.Teacher)
                .Include(x=>x.Department)
                .Where(x=>x.TeacherId == teacherId).ToList();
            return View(lessons);
        }

        public IActionResult detailMyCourses(int id)
        {
            var teacherUsername = HttpContext.User.Identity?.Name; // Kullanıcı adına göre id alıyoruz
            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);
            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            var courseDetails = context.StudentLessons
                .Where(x => x.LessonId == id) // Ders ID'sine göre filtreleme yapılmalı
                .Include(x => x.Student)
                .Include(x => x.Lesson)
                .ToList();

            if (courseDetails == null || !courseDetails.Any())
            {
                ViewBag.Message = "Bu derse kayıtlı öğrenci bulunamadı";
            }

            return View(courseDetails);
        }

        #endregion
    }
}
