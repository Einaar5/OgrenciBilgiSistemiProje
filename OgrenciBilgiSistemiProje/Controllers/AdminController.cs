using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemiProje.Services;
using OgrenciBilgiSistemiProje.Models;
using Microsoft.AspNetCore.Authorization;

namespace OgrenciBilgiSistemiProje.Controllers
{

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

        // Öğrenci listesi
        public IActionResult StudentList(string searchParam)
        {
            var students = context.Students.AsQueryable(); // Tüm öğrencileri çekiyoruz ve sorgu yapabilmek için IQueryable türünde bir değişkene atıyoruz.
            if (!string.IsNullOrWhiteSpace(searchParam)) // Arama terimi boş değilse
            {
                //Bu kısım idsine göre arama yapmak için kullanılır.
                var student = from s in context.Students
                              where s.StudentName.Contains(searchParam) || s.StudentEmail.Contains(searchParam) || s.StudentSurname.Contains(searchParam) // Öğrenci adı, emaili veya soyadı arama terimini içeriyorsa
                              select s; // Arama terimine göre ürünleri çekiyoruz.

                students = student;
            }

            var StudentList = students.OrderByDescending(s => s.StudentId).ToList(); // Öğrencileri öğrenci numarasına göre sıralıyoruz ve listeye çeviriyoruz.
            ViewBag.Search = searchParam; // Arama terimini view'a gönderiyoruz.
            return View(StudentList);
        }

        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddStudent(StudentDto studentDto)
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
    }
}
