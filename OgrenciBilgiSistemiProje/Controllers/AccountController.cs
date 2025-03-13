using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemiProje.Services;

namespace OgrenciBilgiSistemiProje.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context; // Veritabanı işlemleri için context

        public AccountController(ApplicationDbContext context) // Constructor
        {
            _context = context; // Context'i enjekte eder
        }

        public IActionResult Login() => View(); // Login için view döndürür

        [HttpPost]
        public IActionResult Login(string username, string password) //login işlemi
        {
            var admin = _context.Admins.FirstOrDefault(x => x.Username == username && x.Password == password); // Kullanıcı adı ve şifre kontrolü
            if (admin != null) // Eğer admin varsa null değilse
            {
                HttpContext.Session.SetString("username", admin.Username); // Session'a kullanıcı adını ekler
                return RedirectToAction("Index", "Admin"); // Admin paneline yönlendirir
            }
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı";
            return View();

        }

        public IActionResult StuTeaLog() => View(); // Login için view döndürür

        [HttpPost]
        public IActionResult StuTeaLog(string usernameSTU, string passwordSTU, string usernameTeach, string passwordTeach) //login işlemi
        {
            //Sadece öğrenci Kısmı
            var student = _context.Students.FirstOrDefault(x => x.StudentEmail == usernameSTU && x.Password == passwordSTU); // Kullanıcı adı ve şifre kontrolü
            if (student != null) // Eğer admin varsa null değilse
            {
                HttpContext.Session.SetString("username", student.StudentEmail); // Session'a kullanıcı adını ekler
                return RedirectToAction("Index", "Student"); // Admin paneline yönlendirir
            }
            //Sadece öğretmen Kısmı
            var teacher = _context.Teachers.FirstOrDefault(x => x.TeacherMail == usernameTeach && x.TeacherPassword == passwordTeach); // Kullanıcı adı ve şifre kontrolü
            if (teacher != null) // Eğer admin varsa null değilse
            {
                HttpContext.Session.SetString("username", teacher.TeacherMail); // Session'a kullanıcı adını ekler
                return RedirectToAction("Index", "Teacher"); // Admin paneline yönlendirir
            }

            return View();

        }

        public IActionResult Logout() // Çıkış işlemi
        {
            HttpContext.Session.Clear(); // Session'ı temizler
            return RedirectToAction("Login"); // Login sayfasına yönlendirir
        }

        public IActionResult LogoutStuTeach() // Çıkış işlemi
        {
            HttpContext.Session.Clear(); // Session'ı temizler
            return RedirectToAction("StuTeaLog"); // Login sayfasına yönlendirir
        }
    }
}
