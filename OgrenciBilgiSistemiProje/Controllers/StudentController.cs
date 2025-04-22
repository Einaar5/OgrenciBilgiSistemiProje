using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Models;
using OgrenciBilgiSistemiProje.Services;

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

            return View(student);
        }


        #region Öğrenci Edit
        public IActionResult Edit()
        {
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
           
            var studentUsername = HttpContext.User.Identity.Name;
            var student = context.Students.FirstOrDefault(x => x.StudentEmail == studentUsername);
            if (student == null)
            {
                return RedirectToAction("Index", "Student");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }

            

            var studentID = student.StudentId; // öğrenci idsini alıyoruz
            ViewData["ImageFileName"] = student.ImageFileName;
            var grades = context.Grades.Where(s=>s.StudentId == studentID)
            .Include(l=> l.Lesson).ToList(); // Burada StudentId sine göre grades tablosunu çekip Lessonu dahil ediyoruz böylece Viewde Lesson.LessonName ile adını çağırabiliyoruz

            var gradesColor = context.Grades.Select(a => a.Average).ToList();
            
            if(gradesColor.Average() < 50)
            {
                ViewBag.GradesColor = "bg-danger";
            }
            

                return View(grades);
        }
        #endregion


        #region Öğrenci Courses
        public IActionResult Courses()
        {
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
            var courses = context.CourseList
                .Include(c => c.Lesson)
                .Include (c => c.Lesson.Teacher)
                .Include(c => c.Department)
                .Where(c => c.DepartmentId == student.DepartmentId) // Sadece öğrencinin bölümündeki dersler
                .OrderBy(c => c.CourseDay)
                .ThenBy(c => c.CourseTime)
                .ToList();

            return View(courses);
        }
        #endregion


        #region Öğrenci Duyuru


        public IActionResult ListNotifications()
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

        // İşlev: Öğrencinin devamsızlık durumunu gösterir.
        [Authorize(Roles = "Student")]
        public IActionResult Attendance()
        {
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
                    AbsenceRate = sl.TotalLessons > 0 ? (sl.AbsenceCount * 100.0 / sl.TotalLessons) : 0,
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
    }
}
