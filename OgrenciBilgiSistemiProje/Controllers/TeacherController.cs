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





        [Authorize(Roles = "Teacher")]
        public IActionResult Grades(int? lessonId)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            var teacher = context.Teachers
                .Include(t => t.Lessons)
                .FirstOrDefault(x => x.TeacherMail == teacherUsername);

            if (teacher == null)
            {
                return RedirectToAction("StuTeaLog", "Account");
            }

            // Öğretmenin dersleri
            var teacherLessons = teacher.Lessons.ToList();
            var selectedLessonId = lessonId ?? teacherLessons.FirstOrDefault()?.LessonId;

            // Seçilen derse kayıtlı öğrencileri getir
            List<Student> students = new();
            if (selectedLessonId.HasValue)
            {
                students = context.Grades
                    .Include(cl => cl.Student)
                    .Where(cl => cl.LessonId == selectedLessonId.Value)
                    .Select(cl => cl.Student)
                    .Distinct()
                    .ToList();
            }

            // Seçili dersin notlarını al
            List<Grade> grades = new();
            try
            {
                if (selectedLessonId.HasValue)
                {
                    grades = context.Grades
                        .Include(g => g.Student)
                        .Include(g => g.Lesson)
                        .Where(g => g.LessonId == selectedLessonId.Value && g.Lesson.TeacherId == teacher.Id)
                        .ToList();
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Notlar yüklenirken bir hata oluştu.";
            }

            ViewBag.ImageLayout = teacher.ImageFileName;
            ViewData["ImageFileName"] = teacher.ImageFileName;
            ViewBag.TeacherLessons = teacherLessons;
            ViewBag.Students = students;
            ViewBag.SelectedLessonId = selectedLessonId;

            return View(grades);
        }
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public IActionResult Grades(GradeDto gradeDto)
        {
            if (gradeDto == null)
            {
                TempData["ErrorMessage"] = "Geçersiz veri gönderildi.";
                return RedirectToAction("Grades");
            }

            // Öğretmen doğrulama
            var teacherUsername = HttpContext.User.Identity?.Name;
            var teacher = context.Teachers
                .Include(t => t.Lessons)
                .FirstOrDefault(t => t.TeacherMail == teacherUsername);

            if (teacher == null)
            {
                return Unauthorized();
            }

            // Ders kontrolü
            var selectedLesson = teacher.Lessons.FirstOrDefault(l => l.LessonId == gradeDto.LessonId);
            if (selectedLesson == null)
            {
                TempData["ErrorMessage"] = "Bu dersi vermiyorsunuz.";
                return RedirectToAction("Grades");
            }

            // Öğrenci kontrolü
            var student = context.Students.FirstOrDefault(s => s.StudentId == gradeDto.StudentId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Öğrenci bulunamadı.";
                return RedirectToAction("Grades");
            }

            // Öğrencinin derse kayıtlı olup olmadığını kontrol et
            var isStudentEnrolled = context.Grades
                .Any(cl => cl.StudentId == gradeDto.StudentId && cl.LessonId == gradeDto.LessonId);
            if (!isStudentEnrolled)
            {
                TempData["ErrorMessage"] = "Bu öğrenci bu derse kayıtlı değil.";
                return RedirectToAction("Grades");
            }

            // Not aralığı kontrolü
            if (gradeDto.Midterm < 0 || gradeDto.Midterm > 100 || gradeDto.Final < 0 || gradeDto.Final > 100)
            {
                TempData["ErrorMessage"] = "Vize ve final notları 0-100 arasında olmalıdır.";
                return RedirectToAction("Grades");
            }

            // Not işlemleri
            var average = (gradeDto.Midterm * 0.4) + (gradeDto.Final * 0.6);
            var grade = context.Grades
                .FirstOrDefault(g => g.StudentId == gradeDto.StudentId && g.LessonId == gradeDto.LessonId);

            try
            {
                if (grade == null)
                {
                    context.Grades.Add(new Grade
                    {
                        StudentId = gradeDto.StudentId,
                        LessonId = gradeDto.LessonId,
                        Midterm = gradeDto.Midterm,
                        Final = gradeDto.Final,
                        Average = (float)average
                    });
                }
                else
                {
                    grade.Midterm = gradeDto.Midterm;
                    grade.Final = gradeDto.Final;
                    grade.Average = (float)average;
                }

                context.SaveChanges();
                TempData["SuccessMessage"] = "Notlar başarıyla kaydedildi.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Not kaydedilirken bir hata oluştu.";
            }

            return RedirectToAction("Grades", new { lessonId = gradeDto.LessonId });
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

            var selectedDate = attendanceDate?.Date ?? DateTime.Today;


            var studentsAttendance = context.Attendance.ToList();
            ViewBag.AttendanceStudentList = studentsAttendance;

            var studentLessons = context.StudentLessons
                .Where(sl => sl.LessonId == lessonId)
                .Include(sl => sl.Student)
                .ToList();

            TempData["DebugMessage"] = $"Seçilen ders (LessonId: {lessonId}) için {studentLessons.Count} öğrenci bulundu.";

            if (!studentLessons.Any())
            {
                TempData["WarningMessage"] = "Bu derse kayıtlı öğrenci bulunamadı.";
                ViewBag.SelectedLessonId = lessonId;
                return View(new List<Attendance>());
            }

            var attendances = studentLessons
                .Select(sl => new Attendance
                {
                    StudentId = sl.StudentId,
                    Student = sl.Student,
                    LessonId = lessonId,
                    AttendanceDate = selectedDate,
                    IsComeHour1 = true, // Varsayılan olarak geldi
                    IsComeHour2 = true,
                    IsComeHour3 = true
                })
                .ToList();

            var existingAttendances = context.Attendance
                .Where(a => a.LessonId == lessonId && a.AttendanceDate.Date == selectedDate)
                .Include(a => a.Student)
                .ToList();

            if (existingAttendances.Any())
            {
                attendances = existingAttendances;
            }

            ViewBag.SelectedLessonId = lessonId;
            ViewBag.SelectedDate = selectedDate;
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

            var lessonId = attendances.First().LessonId;
            var attendanceDate = attendances.First().AttendanceDate;

            if (attendanceDate > DateTime.Today)
            {
                TempData["ErrorMessage"] = "Gelecek tarih için devamsızlık girilemez.";
                return RedirectToAction("Attendance", new { lessonId });
            }

            int addedCount = 0;
            int updatedCount = 0;

            foreach (var attendance in attendances)
            {
                var existing = context.Attendance.FirstOrDefault(a =>
                    a.StudentId == attendance.StudentId &&
                    a.LessonId == lessonId &&
                    a.AttendanceDate.Date == attendanceDate.Date);

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

        // Derse bağlı olan öğrencileri Öğretmen Buton ile kaydetme işlemi yapar.

        public IActionResult RegisterStudentsToLesson(int lessonId)
        {
            var teacherUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(teacherUsername))
            {
                TempData["ErrorMessage"] = "Öğretmen oturumu bulunamadı.";
                return RedirectToAction("StuTeaLog", "Account");
            }

            var teacher = context.Teachers.FirstOrDefault(x => x.TeacherMail == teacherUsername);
            if (teacher == null)
            {
                return RedirectToAction("StuTeaLog", "Account");
            }

            // Geçersiz lessonId kontrolü
            if (lessonId <= 0)
            {
                // lessonId sıfır veya negatifse hata göster
                TempData["ErrorMessage"] = "Geçersiz ders ID'si.";
                return RedirectToAction("Attendance");
            }

            // Dersi kontrol etmek için
            var lessons = context.Lessons
                .Include(l => l.Department)
                .FirstOrDefault(l => l.LessonId == lessonId && l.TeacherId == teacher.Id);

            if (lessons == null)
            {
                // Ders bulunamazsa veya öğretmene ait değilse hata mesajı göster
                TempData["ErrorMessage"] = $"Ders bulunamadı veya size ait değil. LessonId: {lessonId}, TeacherId: {teacher.Id}";
                return RedirectToAction("Attendance");
            }

            // Bölüm kontrolü
            if (lessons.Department == null)
            {
                // Dersin bağlı olduğu bölüm yoksa hata göster
                TempData["ErrorMessage"] = "Dersin bağlı olduğu bölüm bulunamadı.";
                return RedirectToAction("Attendance");
            }

            // Bölümdeki öğrencileri alırız.
            var students = context.Students
                .Where(s => s.DepartmentId == lessons.DepartmentId)
                .ToList();

            if (!students.Any())
            {
                // Bölümde öğrenci yoksa uyarı göster
                TempData["WarningMessage"] = "Bu bölümde kayıtlı öğrenci bulunamadı.";
                return RedirectToAction("Attendance", new { lessonId });
            }

            int addedCount = 0;
            foreach (var student in students)
            {
                // Öğrencinin derse kayıtlı olup olmadığını kontrol eder.
                if (!context.StudentLessons.Any(sl => sl.StudentId == student.StudentId && sl.LessonId == lessonId))
                {
                    // StudentLesson içersine öğrenci kayıtlı değilse ekler
                    context.StudentLessons.Add(new StudentLesson
                    {
                        StudentId = student.StudentId,
                        LessonId = lessonId
                    });
                    addedCount++;
                }
            }


            try
            {
                context.SaveChanges();
                TempData["SuccessMessages"] = "Öğrenciler derse başarıyla kaydedildi.";
            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kayıt sırasında bir hata oluştu: " + ex.Message;
            }

            // Devamsızlık sayfasına geri dön
            return RedirectToAction("Attendance", new { lessonId = lessonId });
        }

        // İşlev: Öğretmenin seçtiği ders için öğrencilerin devamsızlık raporlarını gösterir.

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
                        AttendanceCount = sa.TotalLessons - sa.AbsenceCount,
                        AbsenceRate = sa.TotalLessons > 0 ? (sa.AbsenceCount * 100.0 / sa.TotalLessons) : 0
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
    }
}
