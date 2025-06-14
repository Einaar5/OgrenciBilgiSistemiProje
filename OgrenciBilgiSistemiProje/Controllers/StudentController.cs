﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Models;
using OgrenciBilgiSistemiProje.Services;

using System.Text.Json;

namespace OgrenciBilgiSistemiProje.Controllers
{

    [Authorize(Roles = "Student")] // Sadece öğrenci rolündeki kullanıcılar bu controller'a erişebilir
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext context; // Veritabanı bağlantısı
        private readonly IWebHostEnvironment environment;

        public StudentController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public void profileDetailViewer()
        {
            var studentUsername = HttpContext.User.Identity?.Name; // Kullanıcı adını alıyoruz.
            // Öğrenciyi bul ve Department bilgisini de dahil et
            var student = context.Students
                .Include(s => s.Department) // Department bilgisini dahil et
                .FirstOrDefault(x => x.StudentEmail == studentUsername);

            if (student != null)
            {
                ViewData["ImageFileName"] = student.ImageFileName;

                // DepartmentName'i ViewData'ya ekleyebilirsiniz
                ViewData["DepartmentName"] = student.Department.Name;
            }


            ViewBag.Name = student.StudentName;
            ViewBag.Surname = student.StudentSurname;
        }

        public IActionResult Index()
        {
            var studentUsername = HttpContext.User.Identity?.Name; // Kullanıcı adını alıyoruz.
            // Öğrenciyi bul ve Department bilgisini de dahil et
            var student = context.Students
                .Include(s => s.Department) // Department bilgisini dahil et
                .FirstOrDefault(x => x.StudentEmail == studentUsername);

            if (student != null)
            {
                ViewData["ImageFileName"] = student.ImageFileName;

                // DepartmentName'i ViewData'ya ekleyebilirsiniz
                ViewData["DepartmentName"] = student.Department.Name;
            }


            profileDetailViewer();

            return View(student);
        }


        #region Öğrenci Edit
        public IActionResult Edit()
        {
            profileDetailViewer();
            var studentUsername = HttpContext.User.Identity?.Name; // Kullanıcı adını alıyoruz.
            var student = context.Students.Include(s=>s.Department).FirstOrDefault(x => x.StudentEmail == studentUsername);

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
                                                               // DepartmentName'i ViewData'ya ekleyebilirsiniz
            ViewData["DepartmentName"] = student.Department.Name;

            return View(studentDto);
        }


        // Öğrenci Edit Sayfası için 

        [HttpPost]
        public IActionResult Edit(StudentDto studentDto)
        {
            var studentUsername = HttpContext.User.Identity.Name; // Kullanıcı adını alıyoruz.
            var student = context.Students.FirstOrDefault(x => x.StudentEmail == studentUsername);
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
        #endregion


        #region Öğrenci Not
        public IActionResult Grades()
        {

            profileDetailViewer();
            var studentUsername = HttpContext.User.Identity?.Name;
            var student = context.Students.FirstOrDefault(s => s.StudentEmail == studentUsername);
            ViewData["ImageFileName"] = student.ImageFileName;

            if (student == null)
                return RedirectToAction("StuTeaLog", "Account");

            var grades = context.Grades
                .Include(g => g.Quiz)
                    .ThenInclude(q => q.Lesson)
                .Where(g => g.StudentId == student.StudentId)
                .ToList();

            var lessons = grades
                .Select(g => g.Lesson)
                .Distinct()
                .ToList();

            var allQuizzes = context.Quizzes
                .Include(q => q.Lesson)
                .Where(q => lessons.Select(l => l.LessonId).Contains(q.Lesson.LessonId))
                .ToList();

            ViewBag.Lessons = lessons;
            ViewBag.Quizzes = allQuizzes;
            ViewBag.Grades = grades;

           
            return View();
        }
        #endregion


        #region Öğrenci Courses
        public IActionResult Courses()
        {
            profileDetailViewer();
            // Öğrenci bilgilerini al
            var studentUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(studentUsername))
            {
                return RedirectToAction("Login", "Account");
            }
           
            // Öğrenciyi ve bölümünü bul
            var student = context.Students
                .Include(s => s.Department) // Bölüm bilgisini de yüklüyoruz
                .FirstOrDefault(s => s.StudentEmail == studentUsername);

            if (student == null)
            {
                return NotFound("Öğrenci bulunamadı");
            }

            // ViewBag ve ViewData atamaları
            ViewBag.ImageLayout = student.ImageFileName;
            ViewData["ImageFileName"] = student.ImageFileName;

            // Öğrencinin bölümüne ait dersleri getir
            // Öğrencinin kayıtlı olduğu dersleri al
            var studentLessons = context.StudentLessons
                .Where(sl => sl.StudentId == student.StudentId)
                .Select(sl => sl.LessonId)
                .ToList();

            // Bu derslere ait programı getir
            var courses = context.CourseList
             .Include(c => c.Lesson)
             .ThenInclude(l => l.Teacher) // Öğretmen bilgisini de yüklüyoruz
             .Include(c => c.Department) // Bölüm bilgisini yüklüyoruz
             .Where(c => studentLessons.Contains(c.LessonId))
             .OrderBy(c => c.CourseDay)
             .ThenBy(c => c.CourseTime)
             .ToList();

            return View(courses);
        }
        #endregion


        #region Öğrenci Duyuru


        public IActionResult ListNotifications()
        {
            profileDetailViewer();
            var studentUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(studentUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = context.Students
                .FirstOrDefault(x => x.StudentEmail == studentUsername);

            if (student == null)
            {
                return NotFound("Öğrenci bulunamadı.");
            }

            // Öğrencinin departmanına ait duyuruları al
            var notifications = context.Notifications       // Notification tablosunu alıyoruz
                .Include(n => n.Department)     // İçerisndeki bildirimle ilişkili bölüm bilgisini
                .Where(n => n.DepartmentId == student.DepartmentId) // Eğer DeperatmentId si öğrencinin departmentId ile aynıysa
                .OrderByDescending(n => n.NotificationDate) // Gönderilen tarihe göre sıralıyoruz
                .ToList();  // Listele

            ViewData["ImageFileName"] = student.ImageFileName;
            return View(notifications);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsRead(int notificationId)
        {
            var studentUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(studentUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = context.Students
                .FirstOrDefault(x => x.StudentEmail == studentUsername);

            if (student == null)
            {
                return NotFound("Öğrenci bulunamadı.");
            }

            var notification = context.Notifications    // Notification tablosunu alıyoruz
                .FirstOrDefault(n => n.NotificationId == notificationId && n.DepartmentId == student.DepartmentId); // DuyuruId ile oluşan duyuruId aynı ve öğrencinin bölümüne ait aynısa

            if (notification == null)
            {
                return NotFound("Duyuru bulunamadı.");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                context.Notifications.Update(notification);
                context.SaveChanges();
            }

            return RedirectToAction("ListNotifications");
        }


        public IActionResult NotificationDetail(int id)
        {
            profileDetailViewer();
            var studentUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(studentUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = context.Students
                .FirstOrDefault(x => x.StudentEmail == studentUsername);

            if (student == null)
            {
                return NotFound("Öğrenci bulunamadı.");
            }

            var notification = context.Notifications
                .Include(n => n.Department)
                .Include(n => n.Teacher)
                .FirstOrDefault(n => n.NotificationId == id && n.DepartmentId == student.DepartmentId);

            if (notification == null)
            {
                return NotFound("Duyuru bulunamadı.");
            }

            // Otomatik okundu işaretle
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                context.Notifications.Update(notification);
                context.SaveChanges();
            }
            ViewData["ImageFileName"] = student.ImageFileName;

            return View(notification);
        }


        public IActionResult ListTeacher()
        {
            profileDetailViewer();
            var studentUsername = HttpContext.User.Identity?.Name;
            if(string.IsNullOrEmpty(studentUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = context.Students
                .FirstOrDefault(x => x.StudentEmail == studentUsername);

            if(student==null)
            {
                return NotFound("Öğrenci bulunamadı.");
            }

            var teachers = context.Teachers
                .OrderBy(t => t.TeacherName)
                .ToList();
            ViewData["ImageFileName"] = student.ImageFileName;

            return View(teachers);
        }


        public IActionResult SendMessage(int teacherId)
        {
            profileDetailViewer();
            var studentUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(studentUsername))
            {
                return RedirectToAction("Login", "Home");
            }

            var student = context.Students
                .FirstOrDefault(x => x.StudentEmail == studentUsername);

            if(student == null)
            {
                return NotFound("Öğrenci bulunamadı.");
            }
            var teachers = context.Teachers
               .OrderBy(t => t.TeacherName)
               .ToList();

            var teacherSendId = context.Teachers.Find(teacherId);

            ViewBag.TeacherName = teacherSendId?.TeacherName +" "+ teacherSendId?.TeacherSurname;
            

            var message = new StudentMessage
            {
                SenderStudentId = student.StudentId,
                ReceiverTeacherId = teacherId,
            };

            ViewBag.ReceiverTeacherId = teacherId; // Öğretmenin ID'sini ViewBag'e ekliyoruz
            ViewData["ImageFileName"] = student.ImageFileName;
            return View(message);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(StudentMessage message)
        {
            var studentUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(studentUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = context.Students
                .FirstOrDefault(x => x.StudentEmail == studentUsername);

            if (student == null)
            {
                return NotFound("Öğrenci bulunamadı.");
            }

            var teacher = context.Teachers
                .FirstOrDefault(t => t.Id == message.ReceiverTeacherId);

            if (teacher == null)
            {
                return NotFound("Öğretmen bulunamadı.");
            }

            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Sender");
            ModelState.Remove("Receiver");

            if (!ModelState.IsValid)
            {
                // Hataları ViewBag'e ekle
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                ViewBag.Errors = errors;

                ViewBag.ReceiverName = $"{teacher.TeacherName} {teacher.TeacherSurname}";
                return View(message);
            }

            // Mesajı tamamla
            message.SenderStudentId = student.StudentId;
            message.ReceiverTeacherId = teacher.Id;
            message.SentDate = DateTime.Now;
            message.IsRead = false;

            context.StudentMessages.Add(message);
            context.SaveChanges();

            return RedirectToAction("ListTeacher");
        }

        #endregion


        #region Devamsızlık Durumu

        
        public IActionResult Attendance()
        {

            profileDetailViewer();
            var studentUsername = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(studentUsername))
            {
                return RedirectToAction("StuTeaLog", "Account");
            }

            var student = context.Students.FirstOrDefault(x => x.StudentEmail == studentUsername);
            if (student == null)
            {
                return RedirectToAction("Index");
            }

            var studentLessons = context.StudentLessons
                .Where(sl => sl.StudentId == student.StudentId)
                .Include(sl => sl.Lesson)
                .GroupJoin(
                    context.Attendance.Where(a => a.StudentId == student.StudentId),
                    sl => sl.LessonId,
                    a => a.LessonId,
                    (sl, attendances) => new
                    {
                        sl.Lesson,
                        AbsenceCount = attendances.Count(a => !a.IsComeHour1) +
                                       attendances.Count(a => !a.IsComeHour2) +
                                       attendances.Count(a => !a.IsComeHour3),
                        TotalLessons = attendances.Count() * 3, // Her ders kaydı 3 saat
                        AbsenceDatesHour1 = attendances
                            .Where(a => !a.IsComeHour1)
                            .Select(a => a.AttendanceDate)
                            .ToList(),
                        AbsenceDatesHour2 = attendances
                            .Where(a => !a.IsComeHour2)
                            .Select(a => a.AttendanceDate)
                            .ToList(),
                        AbsenceDatesHour3 = attendances
                            .Where(a => !a.IsComeHour3)
                            .Select(a => a.AttendanceDate)
                            .ToList()
                    }
                )
                .ToList();

            var model = new StudentAttendanceReportView
            {
                StudentId = student.StudentId,
                StudentName = $"{student.StudentName} {student.StudentSurname}",
                LessonReports = studentLessons.Select(sl => new LessonAttendanceReport
                {
                    LessonId = sl.Lesson.LessonId,
                    LessonName = sl.Lesson.LessonName,
                    AbsenceCount = sl.AbsenceCount,
                    AbsenceDatesHour1 = sl.AbsenceDatesHour1,
                    AbsenceDatesHour2 = sl.AbsenceDatesHour2,
                    AbsenceDatesHour3 = sl.AbsenceDatesHour3
                }).ToList()
            };

            TempData["DebugMessage"] = $"Öğrenci (ID: {student.StudentId}) için {model.LessonReports.Sum(r => r.AbsenceCount)} devamsız saat bulundu.";

            ViewData["ImageFileName"] = student.ImageFileName;
            return View(model);
        }
        #endregion

        #region derskayit

        // GET: Dersleri listele ve seçilen dersleri göster
        public IActionResult CourseSelect()
        {


            profileDetailViewer();
            var studentEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = context.Students.FirstOrDefault(s => s.StudentEmail == studentEmail);
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tüm dersleri al (öğretmen ve bölüm bilgisi dahil)
            var lessons = context.Lessons
                .Include(l => l.Department)
                .Include(l => l.Teacher)
                .Where(l=>l.Department.Id == student.DepartmentId) 
                .ToList();

            // Session'dan geçici olarak seçilen dersleri al
            var selectedLessonIdsJson = HttpContext.Session.GetString("SelectedLessonIds");
            var selectedLessonIds = string.IsNullOrEmpty(selectedLessonIdsJson)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(selectedLessonIdsJson);

            // Geçici olarak seçilmiş derslerin detayları
            var selectedLessons = lessons
                .Where(l => selectedLessonIds.Contains(l.LessonId))
                .ToList();

            // Toplam kredi
            var totalCredits = selectedLessons.Sum(l => l.Credit ?? 0);

            // Öğrencinin daha önce kalıcı olarak seçtiği dersler (StudentLessons tablosundan)
            var studentLessonIds = context.StudentLessons
                .Where(sl => sl.StudentId == student.StudentId)
                .Select(sl => sl.LessonId)
                .ToList();

            // ViewBag'e gerekli verileri gönder
            ViewBag.SelectedLessons = selectedLessons;
            ViewBag.TotalCredits = totalCredits;
            ViewBag.DisabledLessonIds = studentLessonIds; // Kalıcı seçilen dersler disable olacak
            ViewData["ImageFileName"] = student.ImageFileName;

            return View(lessons);
        }


        // POST: Dersi geçici listeye ekle
        [HttpPost]
        [Route("Student/CourseSelect/Add")]
        public IActionResult AddToSelection(int lessonId)
        {
            var studentEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = context.Students.FirstOrDefault(s => s.StudentEmail == studentEmail);
            if (student == null)
            {
                TempData["Error"] = true;
                TempData["Message"] = "Öğrenci bulunamadı.";
                return RedirectToAction("CourseSelect");
            }

            var lesson = context.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson == null)
            {
                TempData["Error"] = true;
                TempData["Message"] = "Ders bulunamadı.";
                return RedirectToAction("CourseSelect");
            }

            var isSelectedCourse = context.Grades
                .FirstOrDefault(g => g.StudentId == student.StudentId && g.LessonId == lessonId);
            if (isSelectedCourse != null)
            {
                TempData["Error"] = true;
                TempData["Message"] = $"{lesson.LessonName} dersi zaten seçilmiş.";
                return RedirectToAction("CourseSelect");
            }

            var selectedLessonIdsJson = HttpContext.Session.GetString("SelectedLessonIds");
            var selectedLessonIds = string.IsNullOrEmpty(selectedLessonIdsJson)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(selectedLessonIdsJson);

            if (!selectedLessonIds.Contains(lessonId))
            {
                selectedLessonIds.Add(lessonId);
                HttpContext.Session.SetString("SelectedLessonIds", JsonSerializer.Serialize(selectedLessonIds));
                TempData["Message"] = $"{lesson.LessonName} dersi geçici listeye eklendi.";
            }
            else
            {
                TempData["Error"] = true;
                TempData["Message"] = $"{lesson.LessonName} dersi zaten seçildi.";
            }

            return RedirectToAction("CourseSelect");
        }

        // POST: Dersleri Session'dan sil
        [HttpPost]
        [Route("Student/CourseSelect/Remove")]
        public IActionResult RemoveFromSelection(int lessonId)
        {
            var selectedLessonIdsJson = HttpContext.Session.GetString("SelectedLessonIds");
            var selectedLessonIds = string.IsNullOrEmpty(selectedLessonIdsJson)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(selectedLessonIdsJson);

            if (selectedLessonIds.Contains(lessonId))
            {
                selectedLessonIds.Remove(lessonId);
                HttpContext.Session.SetString("SelectedLessonIds", JsonSerializer.Serialize(selectedLessonIds));
                TempData["Message"] = "Ders listeden kaldırıldı.";
            }
            else
            {
                TempData["Error"] = true;
                TempData["Message"] = "Ders listeden bulunamadı.";
            }

            return RedirectToAction("CourseSelect");
        }

        // POST: Geçici listedeki dersleri Grade tablosuna kaydet
        [HttpPost]
        [Route("Student/CourseSelect/SaveAll")]
        public async Task<IActionResult> SaveAllSelected()
        {
            var studentEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await context.Students
                .FirstOrDefaultAsync(s => s.StudentEmail == studentEmail);
            if (student == null)
            {
                TempData["Error"] = true;
                TempData["Message"] = "Öğrenci bulunamadı.";
                return RedirectToAction("CourseSelect");
            }

            var selectedLessonIdsJson = HttpContext.Session.GetString("SelectedLessonIds");
            var selectedLessonIds = string.IsNullOrEmpty(selectedLessonIdsJson)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(selectedLessonIdsJson);

            if (!selectedLessonIds.Any())
            {
                TempData["Error"] = true;
                TempData["Message"] = "Kaydedilecek ders yok.";
                return RedirectToAction("CourseSelect");
            }

            foreach (var lessonId in selectedLessonIds)
            {
                var lesson = context.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
                if (lesson == null) continue;

                var isSelectedCourse = context.Grades
                    .FirstOrDefault(g => g.StudentId == student.StudentId && g.LessonId == lessonId);
                if (isSelectedCourse == null)
                {
                    context.StudentLessons.Add(new StudentLesson
                    {
                        StudentId = student.StudentId,
                        LessonId = lessonId
                    });
                    //context.Grades.Add(new Grade
                    //{
                    //    StudentId = student.StudentId,
                    //    LessonId = lessonId,
                    //    QuizId = , // Varsayılan değer
                    //    Midterm = 0,
                    //    Final = 0,
                    //    Average = 0

                    //});
                }
            }

            try
            {
                await context.SaveChangesAsync();
                TempData["Message"] = $"{selectedLessonIds.Count} ders başarıyla kaydedildi.";
                HttpContext.Session.Remove("SelectedLessonIds");
            }
            catch (Exception ex)
            {
                TempData["Error"] = true;
                TempData["Message"] = $"Hata oluştu: {ex.Message}";
            }

            return RedirectToAction("CourseSelect");
        }

        #endregion
    }
}
