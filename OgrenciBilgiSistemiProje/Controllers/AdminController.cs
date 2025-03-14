using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemiProje.Services;
using OgrenciBilgiSistemiProje.Models;
using Microsoft.AspNetCore.Authorization;

namespace OgrenciBilgiSistemiProje.Controllers
{
    
    public class AdminController : BaseController
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

        //-------------------------------------------------------------------------------Öğrenci Başlangıç
        // Öğrenci listesi
        public IActionResult StudentList(string searchParam)
        {
            var students = context.Students.AsQueryable(); // Tüm öğrencileri çekiyoruz ve sorgu yapabilmek için IQueryable türünde bir değişkene atıyoruz.
            if (!string.IsNullOrWhiteSpace(searchParam)) // Arama terimi boş değilse
            {
                //Bu kısım idsine göre arama yapmak için kullanılır.
                var student = from s in context.Students
                              where s.StudentName.Contains(searchParam) || s.DepartmentName.Contains(searchParam) || s.StudentSurname.Contains(searchParam) || Convert.ToString(s.StudentId).Contains(searchParam) // Öğrenci adı, emaili, soyadı veya id ile arama terimini içeriyorsa
                              select s; // Arama terimine göre ürünleri çekiyoruz.

                students = student;
            }

            var StudentList = students.OrderByDescending(s => s.StudentId).ToList(); // Öğrencileri öğrenci numarasına göre sıralıyoruz ve listeye çeviriyoruz.
            ViewBag.Search = searchParam; // Arama terimini view'a gönderiyoruz.
            return View(StudentList);
        }

        public IActionResult AddStudent()
        {
            var departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.           
            ViewData["Departments"] = departments; // Bölümleri view'a gönderiyoruz.
            return View();
        }

        [HttpPost]
        public IActionResult AddStudent(StudentDto studentDto, DepartmentDto departmentDto)
        {
            if (studentDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Resim seçiniz.");
            }
            if (!ModelState.IsValid) // Eğer model doğrulama başarısız olursa
            {
                return View(studentDto); // Hata varsa formu tekrar göster
            }

            // Resim dosyasını yüklemek için bir dosya adı oluştur
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // burada dosya adı oluşturuluyor Örnek : 20211231235959999
            newFileName += Path.GetExtension(studentDto.ImageFile!.FileName); // dosya adının sonuna uzantı ekleniyor Örnek : 20211231235959999.jpg

            // Resmi yükle
            string imageFullPath = environment.WebRootPath + "/img/" + newFileName; // Resmin yükleneceği yolu belirt ve dosya adını ekle
            using (var stream = System.IO.File.Create(imageFullPath)) // Dosyayı oluşturuyoruz. Çünkü burada dosyayı sunucuya kaydedeceğiz. Stream sınıfı, dosya işlemleri yapmamızı sağlar.
            {
                studentDto.ImageFile.CopyTo(stream); // Dosyayı yeni dosyaya kopyalıyoruz.
            }



            // Öğrenci objesi oluştur ve veritabanına ekle 
            // 
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
                DepartmentName = studentDto.DepartmentName

            };

           

            context.Students.Add(student);
            context.SaveChanges();

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
                DepartmentName = student.DepartmentName
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
            student.DepartmentName = studentDto.DepartmentName;
            student.ImageFileName = newFileName;

            context.SaveChanges();

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

            context.Students.Remove(student);
            context.SaveChanges();
            return RedirectToAction("StudentList");


        }

        //-------------------------------------------------------------------------------Öğrenci Bitiş

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



        //-------------------------------------------------------------------------------Teacher Başlangıç



        public IActionResult TeacherList()
        {
            var teacherList = context.Teachers.ToList();
            return View(teacherList);
        }

        public IActionResult AddTeacher()
        {

            return View();
        }

        [HttpPost]
        public IActionResult AddTeacher(TeacherDto teacherDto)
        {
            if(teacherDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile","Resim Seçiniz.");
            }

            if(!ModelState.IsValid)
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
                TeacherAdrress = teacherDto.TeacherAdrress,
                TeacherGender = teacherDto.TeacherGender,
                TeacherRegisterDate = DateTime.Now,
                ImgFileName = newFileName,
                TeacherBrans = teacherDto.TeacherBrans
            };

            context.Teachers.Add(teacher);
            context.SaveChanges();
            return RedirectToAction("TeacherList");
        }

        public IActionResult EditTeacher(int Id)
        {
            var teacher = context.Teachers.Find(Id);
            if(teacher == null)
            {
                return RedirectToAction("TeacherList");
            }

            var teacherDto = new TeacherDto
            {
                TeacherName = teacher.TeacherName,
                TeacherSurname = teacher.TeacherSurname,
                TeacherMail = teacher.TeacherMail,
                TeacherPhone = teacher.TeacherPhone,
                TeacherAdrress = teacher.TeacherAdrress,
                TeacherGender = teacher.TeacherGender,
                TeacherBrans = teacher.TeacherBrans
            };

            ViewData["Id"] = Id; // Öğrenci numarasını view'a gönderiyoruz.
            ViewData["ImgFileName"] = teacher.ImgFileName; // Resim dosya adını view'a gönderiyoruz.
            

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
                ViewData["ImgFileName"] = teacher.ImgFileName; // Resim dosya adını view'a gönderiyoruz.
                return View(teacherDto); // Hata varsa formu tekrar göster
            }

            string newFileName = teacher.ImgFileName; // Eğer yeni bir resim yüklenmediyse eski resim dosya adını kullan
            if (teacherDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // burada dosya adı oluşturuluyor Örnek : 20211231235959999
                newFileName += Path.GetExtension(teacherDto.ImageFile!.FileName); // dosya adının sonuna uzantı ekleniyor Örnek : 20211231235959999.jpg

                string imageFullPath = environment.WebRootPath + "/img/" + newFileName; // Resmin yükleneceği yolu belirt ve dosya adını ekle
                using (var stream = System.IO.File.Create(imageFullPath)) // Dosyayı oluşturuyoruz. Çünkü burada dosyayı sunucuya kaydedeceğiz. Stream sınıfı, dosya işlemleri yapmamızı sağlar.
                {
                    teacherDto.ImageFile.CopyTo(stream); // Dosyayı yeni dosyaya kopyalıyoruz.
                }

                string oldImagePath = environment.WebRootPath + "/img/" + teacher.ImgFileName; // Eski resmin dosya yolunu belirt
                System.IO.File.Delete(oldImagePath); // Eski resmi siliyoruz çünkü artık kullanılmayacak

            }

            var departments = context.Departments.OrderByDescending(d => d.Id).ToList(); // Bölümleri bölüm numarasına göre sıralıyoruz ve listeye çeviriyoruz.           
            ViewData["Departments"] = departments; // Bölümleri view'a gönderiyoruz.

            // Student objesine student dtoya atadığımız değerleri atıyoruz.

            teacher.TeacherName = teacherDto.TeacherName;
            teacher.TeacherSurname = teacherDto.TeacherSurname;
            teacher.TeacherMail = teacherDto.TeacherMail;
            teacher.TeacherPhone = teacherDto.TeacherPhone;
            teacher.TeacherAdrress = teacherDto.TeacherAdrress;
            teacher.TeacherGender = teacherDto.TeacherGender;
            teacher.ImgFileName = newFileName;
            teacher.TeacherBrans = teacherDto.TeacherBrans;

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

            string imagePath = environment.WebRootPath + "/img/" + teacher.ImgFileName; // Resmin dosya yolunu belirt
            System.IO.File.Delete(imagePath); // Resmi sil

            context.Teachers.Remove(teacher);
            context.SaveChanges();
            return RedirectToAction("TeacherList");


        }





        //-------------------------------------------------------------------------------Teacher Bitiş
    }
}
