using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemiProje.Services;
using OgrenciBilgiSistemiProje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace OgrenciBilgiSistemiProje.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece admin rolündeki kullanıcılar bu controller'a erişebilir
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment) // 
        {
            this.context = context; // Veritabanı işlemleri yapabilmek için ApplicationDbContext sınıfını kullanıyoruz.
            this.environment = environment; // Sunucu tarafında dosya işlemleri yapabilmek için IWebHostEnvironment sınıfını kullanıyoruz.
        }
        public IActionResult Index()
        {
            return View();
        }

        #region Öğrenci Başlangıç
        //-------------------------------------------------------------------------------Öğrenci Başlangıç
        // Öğrenci listesi
        public IActionResult StudentList(string searchParam)
        {
            var students = context.Students.Include(s => s.Department).AsQueryable(); // Burada öğrencileri çekiyoruz ve sorgu yapabilmek için IQueryable türünde bir değişkene atıyoruz ve Department ilişkisini yüklüyoruz.
            if (!string.IsNullOrWhiteSpace(searchParam)) // Arama terimi boş değilse
            {
                //Bu kısım idsine göre arama yapmak için kullanılır.
                var student = from s in context.Students.Include(s => s.Department) // Department ilişkisini yükle
                              where s.StudentName.Contains(searchParam) || Convert.ToString(s.DepartmentId).Contains(searchParam) || s.StudentSurname.Contains(searchParam) || Convert.ToString(s.StudentId).Contains(searchParam) // Öğrenci adı, emaili, soyadı veya id ile arama terimini içeriyorsa
                              select s; // Arama terimine göre ürünleri çekiyoruz.

                students = student;
            }

            var StudentList = students.OrderByDescending(s => s.StudentId).ToList(); // Öğrencileri öğrenci numarasına göre sıralıyoruz ve listeye çeviriyoruz.
            ViewBag.Search = searchParam; // Arama terimini view'a gönderiyoruz.

            return View(StudentList);
        }

        public IActionResult AddStudent()
        {
            ViewBag.Departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.           
            return View(new StudentDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(StudentDto studentDto)
        {
            if (studentDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Resim seçiniz.");
            }

            // Model doğrulamasını kontrol et
            if (!ModelState.IsValid)
            {
                // Doğrulama hatalarını log’la (isteğe bağlı, hata ayıklama için)
                var errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage); // Hataları konsola yazdır (debug için)
                }

                // Departmanları tekrar yükle ve formu göster
                ViewBag.Departments = context.Departments.OrderByDescending(d => d.Id).ToList();
                return View(studentDto); // Hata varsa formu tekrar göster
            }

            // Resim dosyasını yüklemek için bir dosya adı oluştur
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(studentDto.ImageFile!.FileName);

            // Resmi yükle
            string imageFullPath = Path.Combine(environment.WebRootPath, "img", newFileName);
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                await studentDto.ImageFile.CopyToAsync(stream); // Async olarak kopyala
            }

            // Öğrenci objesi oluştur ve veritabanına ekle
            Student student = new Student
            {
                StudentName = studentDto.StudentName,
                StudentSurname = studentDto.StudentSurname,
                StudentEmail = studentDto.StudentEmail,
                StudentPhone = studentDto.StudentPhone,
                StudentAddress = studentDto.StudentAddress,
                StudentGender = studentDto.StudentGender,
                StudentRegisterDate = DateTime.Now,
                ImageFileName = newFileName,
                DepartmentId = studentDto.DepartmentId, // DepartmentId'yi doğru şekilde ata
                Password = studentDto.Password
            };

            // Kontenjan kontrolü ve işlem
            var department = context.Departments.FirstOrDefault(d => d.Id == studentDto.DepartmentId);
            if (department == null)
            {
                ModelState.AddModelError("DepartmentId", "Geçerli bir bölüm seçiniz.");
                ViewBag.Departments = context.Departments.OrderByDescending(d => d.Id).ToList();
                return View(studentDto);
            }

            if (department.Quota > 0)
            {
                if (department.Quota <= 10)
                {
                    ViewBag.QuotaWarning = $"Bu bölümün kontenjanı azalmak üzere! (Kalan kontenjan: {department.Quota})";
                }
                else
                {
                    ViewBag.QuotaWarning = $"Kalan kontenjan: {department.Quota}";
                }

                using (var transaction = context.Database.BeginTransaction()) // burada transection demek birden fazla işlemi bir arada yapmak demektir. Eğer işlemlerden biri başarısız olursa diğer işlemleri geri alır.
                {
                    try
                    {
                        // Öğrenciyi veritabanına ekle
                        context.Students.Add(student);
                        await context.SaveChangesAsync(); // Async olarak kaydet

                        // Öğrencinin bölümündeki dersleri bul
                        var lessons = context.Lessons.Where(l => l.DepartmentId == student.DepartmentId).ToList();

                        // Her ders için Grade tablosuna kayıt ekler ve öğrenciye atar yani ne kadar derse sahipse department 
                        foreach (var lesson in lessons)
                        {
                            Grade grade = new Grade
                            {
                                StudentId = student.StudentId, // Yeni eklenen öğrencinin ID'si
                                LessonId = lesson.LessonId, // Dersin ID'si
                                Midterm = 0, // Vize notu (varsayılan değer)
                                Final = 0, // Final notu (varsayılan değer)
                                Average = 0 // Ortalama (varsayılan değer)
                            };
                            context.Grades.Add(grade);
                        }

                        // Kontenjanı güncelle
                        department.Quota -= 1;
                        context.Departments.Update(department);

                        // Tüm değişiklikleri kaydet
                        await context.SaveChangesAsync();

                        // İşlemleri onayla
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Hata olursa geri al
                        transaction.Rollback();
                        ModelState.AddModelError("", "Bir hata oluştu: " + ex.Message);
                        ViewBag.Departments = context.Departments.OrderByDescending(d => d.Id).ToList();
                        return View(studentDto);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("DepartmentId", "Bu bölümün kontenjanı dolmuştur.");
                ViewBag.Departments = context.Departments.OrderByDescending(d => d.Id).ToList();
                return View(studentDto);
            }

            return RedirectToAction("StudentList");
        }

        public IActionResult EditStudent(int id)
        {
            var student = context.Students.Find(id);
            if (student == null)
            {
                return RedirectToAction("StudentList");
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

            ViewData["StudentId"] = id; // Öğrenci numarasını view'a gönderiyoruz.
            ViewData["ImageFileName"] = student.ImageFileName; // Resim dosya adını view'a gönderiyoruz.


            return View(studentDto);
        }

        [HttpPost]
        public IActionResult EditStudent(int id, StudentDto studentDto)
        {
            var student = context.Students.Find(id);
            if (student == null)
            {
                return RedirectToAction("StudentList");
            }

            if (!ModelState.IsValid) // Eğer model doğrulama başarısız olursa
            {
                ViewData["StudentId"] = id; // Öğrenci numarasını view'a gönderiyoruz.
                ViewData["ImageFileName"] = student.ImageFileName; // Resim dosya adını view'a gönderiyoruz.
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

            var departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.           
            ViewData["Departments"] = departments; // Bölümleri view'a gönderiyoruz.

            // Student objesine student dtoya atadığımız değerleri atıyoruz.

            student.StudentName = studentDto.StudentName;
            student.StudentSurname = studentDto.StudentSurname;
            student.StudentEmail = studentDto.StudentEmail;
            student.StudentPhone = studentDto.StudentPhone;
            student.StudentAddress = studentDto.StudentAddress;
            student.StudentGender = studentDto.StudentGender;
            student.DepartmentId = studentDto.DepartmentId;
            student.ImageFileName = newFileName;
            student.Password = studentDto.Password;
            //Burada kontenjan kontrolü yapılıyor ve kontenjan azaltılıyor böylece kontenjanı sıfırlanan bölüme öğrenci eklenemeyecek.
            var department = context.Departments.FirstOrDefault(d => d.Id == studentDto.DepartmentId); // Bölümü veritabanından çek
                                                                                                       //Kontenyaj kontrolü
            if (department != null)
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Öğrenci ekleme ve kontenjan azaltma işlemleri
                        context.Students.Update(student);
                        context.SaveChanges();

                        department.Quota -= 1;
                        context.Departments.Update(department);
                        context.SaveChanges();

                        transaction.Commit(); // İşlemleri onayla
                    }
                    catch (Exception)
                    {
                        transaction.Rollback(); // Hata olursa geri al
                        throw; // Hatayı yeniden fırlat
                    }
                }
            }

            return RedirectToAction("StudentList");
        }

        public IActionResult DeleteStudent(int id)
        {
            var student = context.Students.Find(id);
            if (student == null)
            {
                return RedirectToAction("StudentList");
            }

            string imagePath = environment.WebRootPath + "/img/" + student.ImageFileName; // Resmin dosya yolunu belirt
            System.IO.File.Delete(imagePath); // Resmi sil

            //Burada kontenjan kontrolü yapılıyor ve kontenjan azaltılıyor böylece kontenjanı sıfırlanan bölüme öğrenci eklenemeyecek.
            var department = context.Departments.FirstOrDefault(d => d.Id == student.DepartmentId); // Bölümü veritabanından çek
                                                                                                    //Kontenyaj kontrolü
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // Öğrenciyi sil
                    context.Students.Remove(student);
                    context.SaveChanges();

                    // Kontenjanı 1 artır
                    department.Quota += 1;
                    context.Departments.Update(department);
                    context.SaveChanges();

                    // İşlemleri onayla
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Hata olursa geri al
                    transaction.Rollback();
                    throw; // Hatayı yeniden fırlat
                }
            }
            return RedirectToAction("StudentList");


        }

        //-------------------------------------------------------------------------------Öğrenci Bitiş
        #endregion


        #region Bölüm Başlangıç
        //-------------------------------------------------------------------------------Bölüm Başlangıç

        public IActionResult DepartmentList()
        {
            var departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.
            return View(departments);
        }
        public IActionResult DepartmentAdd()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DepartmentAdd(DepartmentDto departmentDto)
        {
            if (!ModelState.IsValid) // Eğer model doğrulama başarısız olursa
            {
                return View(departmentDto);
            }

            Department department = new Department
            {
                Name = departmentDto.Name,
                Quota = departmentDto.Quota-- // Kontenjanı bir azaltıyoruz.
            };

            context.Departments.Add(department);
            context.SaveChanges();
            return RedirectToAction("DepartmentList");
        }



        public IActionResult DepartmentEdit(int id)
        {
            var department = context.Departments.Find(id);
            if (department == null)
            {
                return RedirectToAction("DepartmentList");
            }
            var departmentDto = new DepartmentDto
            {
                Name = department.Name,
                Quota = department.Quota
            };
            var departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.
            ViewData["Departments"] = departments; // Bölümleri view'a gönderiyoruz.
            ViewData["Id"] = id; // Bölüm numarasını view'a gönderiyoruz.
            return View(departmentDto);
        }


        [HttpPost]
        public IActionResult EditDepartment(int Id, DepartmentDto departmentDto)
        {
            var department = context.Departments.Find(Id);
            if (department == null)
            {
                return RedirectToAction("DepartmentList");
            }

            var departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.
            ViewData["Departments"] = departments; // Bölümleri view'a gönderiyoruz.

            // Student objesine student dtoya atadığımız değerleri atıyoruz.

            department.Name = departmentDto.Name;
            department.Quota = departmentDto.Quota;

            context.SaveChanges();

            return RedirectToAction("DepartmentList");
        }


        public IActionResult DeleteDepartment(int Id)
        {
            var department = context.Departments.Find(Id);

            if (department == null)
            {
                return RedirectToAction("DepartmenList");
            }

            context.Departments.Remove(department);

            context.SaveChanges();

            return RedirectToAction("DepartmentList");
        }
        //-------------------------------------------------------------------------------Bölüm Bitiş
        #endregion


        #region Öğretmen Başlangıç
        //-------------------------------------------------------------------------------Teacher Başlangıç

        public IActionResult TeacherList(string searchParam)
        {
            var teachers = context.Teachers.AsQueryable(); // Tüm öğrencileri çekiyoruz ve sorgu yapabilmek için IQueryable türünde bir değişkene atıyoruz.
            if (!string.IsNullOrWhiteSpace(searchParam)) // Arama terimi boş değilse
            {
                //Bu kısım idsine göre arama yapmak için kullanılır.
                var teacher = from s in context.Teachers
                              where s.TeacherName.Contains(searchParam) || s.TeacherSurname.Contains(searchParam) || Convert.ToString(s.Id).Contains(searchParam) // Öğrenci adı, emaili, soyadı veya id ile arama terimini içeriyorsa
                              select s; // Arama terimine göre ürünleri çekiyoruz.

                teachers = teacher;
            }
            var TeacherList = teachers.OrderByDescending(s => s.Id).ToList(); // Öğrencileri öğrenci numarasına göre sıralıyoruz ve listeye çeviriyoruz.
            ViewBag.Search = searchParam; // Arama terimini view'a gönderiyoruz.
            return View(TeacherList);

        }

        public IActionResult AddTeacher()
        {

            return View();
        }

        [HttpPost]
        public IActionResult AddTeacher(TeacherDto teacherDto)
        {
            if (teacherDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Resim Seçiniz.");
            }

            if (!ModelState.IsValid)
            {
                return View(teacherDto);
            }


            // Resim dosyasını yüklemek için bir dosya adı oluştur
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // burada dosya adı oluşturuluyor Örnek : 20211231235959999
            newFileName += Path.GetExtension(teacherDto.ImageFile!.FileName); // dosya adının sonuna uzantı ekleniyor Örnek : 20211231235959999.jpg

            // Resmi yükle
            string imageFullPath = environment.WebRootPath + "/img/" + newFileName; // Resmin yükleneceği yolu belirt ve dosya adını ekle
            using (var stream = System.IO.File.Create(imageFullPath)) // Dosyayı oluşturuyoruz. Çünkü burada dosyayı sunucuya kaydedeceğiz. Stream sınıfı, dosya işlemleri yapmamızı sağlar.
            {
                teacherDto.ImageFile.CopyTo(stream); // Dosyayı yeni dosyaya kopyalıyoruz.
            }


            Teacher teacher = new Teacher
            {
                TeacherName = teacherDto.TeacherName,
                TeacherSurname = teacherDto.TeacherSurname,
                TeacherMail = teacherDto.TeacherMail,
                TeacherPhone = teacherDto.TeacherPhone,
                TeacherPassword = teacherDto.TeacherPassword,
                TeacherAddress = teacherDto.TeacherAddress,
                TeacherGender = teacherDto.TeacherGender,
                TeacherRegisterDate = DateTime.Now,
                ImageFileName = newFileName
            };

            context.Teachers.Add(teacher);
            context.SaveChanges();
            return RedirectToAction("TeacherList");
        }

        public IActionResult EditTeacher(int Id)
        {
            var teacher = context.Teachers.Find(Id);
            if (teacher == null)
            {
                return RedirectToAction("TeacherList");
            }

            var teacherDto = new TeacherDto
            {
                TeacherName = teacher.TeacherName,
                TeacherSurname = teacher.TeacherSurname,
                TeacherMail = teacher.TeacherMail,
                TeacherPhone = teacher.TeacherPhone,
                TeacherAddress = teacher.TeacherAddress,
                TeacherGender = teacher.TeacherGender,
                TeacherPassword = teacher.TeacherPassword

            };

            ViewData["Id"] = Id; // Öğrenci numarasını view'a gönderiyoruz.
            ViewData["ImgFileName"] = teacher.ImageFileName; // Resim dosya adını view'a gönderiyoruz.


            return View(teacherDto);
        }



        [HttpPost]
        public IActionResult EditTeacher(int id, TeacherDto teacherDto)
        {
            var teacher = context.Teachers.Find(id);
            if (teacher == null)
            {
                return RedirectToAction("TeacherList");
            }

            if (!ModelState.IsValid) // Eğer model doğrulama başarısız olursa
            {
                ViewData["Id"] = id; // Öğrenci numarasını view'a gönderiyoruz.
                ViewData["ImgFileName"] = teacher.ImageFileName; // Resim dosya adını view'a gönderiyoruz.
                return View(teacherDto); // Hata varsa formu tekrar göster
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

            var departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.           
            ViewData["Departments"] = departments; // Bölümleri view'a gönderiyoruz.

            // Student objesine student dtoya atadığımız değerleri atıyoruz.

            teacher.TeacherName = teacherDto.TeacherName;
            teacher.TeacherSurname = teacherDto.TeacherSurname;
            teacher.TeacherMail = teacherDto.TeacherMail;
            teacher.TeacherPhone = teacherDto.TeacherPhone;
            teacher.TeacherAddress = teacherDto.TeacherAddress;
            teacher.TeacherGender = teacherDto.TeacherGender;
            teacher.ImageFileName = newFileName;
            teacher.TeacherPassword = teacherDto.TeacherPassword;

            context.SaveChanges();

            return RedirectToAction("TeacherList");
        }

        public IActionResult DeleteTeacher(int id)
        {
            var teacher = context.Teachers.Find(id);
            if (teacher == null)
            {
                return RedirectToAction("TeacherList");
            }

            string imagePath = environment.WebRootPath + "/img/" + teacher.ImageFileName; // Resmin dosya yolunu belirt
            System.IO.File.Delete(imagePath); // Resmi sil

            try
            {
                context.Teachers.Remove(teacher);
                context.SaveChanges();
            }
            catch
            {
                @ViewBag.Error = "Bu öğretmene ait dersler bulunmaktadır. Öğretmeni silemezsiniz.";

            }
            return RedirectToAction("TeacherList");


        }

        //-------------------------------------------------------------------------------Teacher Bitiş

        #endregion


        #region Ders Başlangıç
        //-------------------------------------------------------------------------------Lesson Başla

        public IActionResult LessonList(string searchParam)
        {
            /*
            var lessons = context.Lessons.AsQueryable(); // Tüm öğrencileri çekiyoruz ve sorgu yapabilmek için IQueryable türünde bir değişkene atıyoruz.
            if (!string.IsNullOrWhiteSpace(searchParam)) // Arama terimi boş değilse
            {
                //Bu kısım idsine göre arama yapmak için kullanılır.
                var lesson = from s in context.Lessons
                              where s.LessonName.Contains(searchParam) || Convert.ToString(s.LessonId).Contains(searchParam) // Öğrenci adı, emaili, soyadı veya id ile arama terimini içeriyorsa
                              select s; // Arama terimine göre ürünleri çekiyoruz.

                lessons = lesson;
            }
            var LessonList = lessons.OrderByDescending(s => s.LessonId).ToList(); // Öğrencileri öğrenci numarasına göre sıralıyoruz ve listeye çeviriyoruz.
            ViewBag.Search = searchParam; // Arama terimini view'a gönderiyoruz.
            return View(LessonList);
            */


            var lessons = context.Lessons
                .Include(l => l.Department) // Department ilişkisini yükle
                .Include(l => l.Teacher)   // Teacher ilişkisini yükle
                .OrderByDescending(l => l.LessonId)
                .ToList();

            return View(lessons);

        }

        public IActionResult AddLesson()
        {
            var departments = context.Departments?.OrderByDescending(d => d.Id).ToList() ?? new List<Department>();
            var teachers = context.Teachers?.OrderByDescending(t => t.Id).ToList() ?? new List<Teacher>();

            if (departments == null || teachers == null)
            {
                // Log veya hata mesajı (isteğe bağlı)
                Console.WriteLine("Departments veya Teachers yüklenemedi. Varsayılan boş liste kullanıldı.");
            }

            ViewBag.Departments = departments;
            ViewBag.Teachers = teachers;
            return View(new LessonDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddLesson(LessonDto lessonDto)
        {
            // Model doğrulamasını kontrol et
            if (!ModelState.IsValid)
            {
                // Doğrulama hatalarını log’la (isteğe bağlı, hata ayıklama için)
                var errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage); // Hataları konsola yazdır (debug için)
                }

                // Departmanları ve öğretmenleri tekrar yükle ve formu göster
                ViewBag.Departments = context.Departments.OrderByDescending(d => d.Id).ToList();
                ViewBag.Teachers = context.Teachers.OrderByDescending(t => t.Id).ToList();
                return View(lessonDto); // Hata varsa formu tekrar göster
            }

            // Ders objesi oluştur ve veritabanına ekle
            Lesson lesson = new Lesson
            {
                LessonName = lessonDto.LessonName,
                Credit = lessonDto.Credit,
                DepartmentId = lessonDto.DepartmentId,
                TeacherId = lessonDto.TeacherId
            };

            context.Lessons.Add(lesson);
            await context.SaveChangesAsync();

            return RedirectToAction("LessonList"); // Başarılıysa ana sayfaya yönlendir (veya LessonList gibi bir action)
        }

        public IActionResult EditLesson(int id)
        {
            var departments = context.Departments?.OrderByDescending(d => d.Id).ToList() ?? new List<Department>();
            var teachers = context.Teachers?.OrderByDescending(t => t.Id).ToList() ?? new List<Teacher>();
            var lesson = context.Lessons.Find(id);
            if (lesson == null)
            {
                return RedirectToAction("LessonList");
            }

            ViewBag.Departments = departments;
            ViewBag.Teachers = teachers;
            ViewData["LessonId"] = id; // LessonId'yi ViewData'ya ekleyin
            var lessonDto = new LessonDto
            {
                LessonName = lesson.LessonName,
                Credit = lesson.Credit,
                DepartmentId = lesson.DepartmentId,
                TeacherId = lesson.TeacherId
            };
            return View(lessonDto);
        }


        [HttpPost]
        public IActionResult EditLesson(int id, LessonDto lessonDto)
        {
           

            var lesson = context.Lessons.Find(id);
            if (lesson == null)
            {
                return RedirectToAction("LessonList");
            }

            if (!ModelState.IsValid) // Eğer model doğrulama başarısız olursa
            {
                ViewData["LessonId"] = id; // Öğrenci numarasını view'a gönderiyoruz.
                return View(lessonDto); // Hata varsa formu tekrar göster
            }

            // Student objesine student dtoya atadığımız değerleri atıyoruz.

            lesson.LessonName = lessonDto.LessonName;
            lesson.Credit = lessonDto.Credit;
            lesson.DepartmentId = lessonDto.DepartmentId;
            lesson.TeacherId = lessonDto.TeacherId;
            context.SaveChanges();
            return RedirectToAction("LessonList");


        }

        public IActionResult DeleteLesson(int id)
        {
            var lesson = context.Lessons.Find(id);
            if (lesson == null)
            {
                return RedirectToAction("LessonList");
            }

            try
            {
                context.Remove(lesson);
                context.SaveChanges();
            }
            catch (Exception)
            {
                @ViewBag.Error = "Bu dersi silemezsiniz. Bu dersi alan öğrenciler var.";

            }

            return RedirectToAction("LessonList");
        }

        //-------------------------------------------------------------------------------Lesson Bitir
        #endregion
    }
}
