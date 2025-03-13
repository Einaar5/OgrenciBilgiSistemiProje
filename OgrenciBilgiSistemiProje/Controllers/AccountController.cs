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
        public IActionResult StuTeaLog(string username, string password) //login işlemi
        {
            
            return View();

        }

        public IActionResult Logout() // Çıkış işlemi
        {
            HttpContext.Session.Clear(); // Session'ı temizler
            return RedirectToAction("Login"); // Login sayfasına yönlendirir
        }
    }
}
