using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemiProje.Models;
using OgrenciBilgiSistemiProje.Services;
using Microsoft.AspNetCore.Authorization;

namespace OgrenciBilgiSistemiProje.Controllers
{
    // Sadece öğrenci rolündeki kullanıcılar bu controller'a erişebilir
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context; // Veritabanı işlemleri için context

        public AccountController(ApplicationDbContext context) // Constructor
        {
            _context = context; // Context'i enjekte eder
        }

        public IActionResult Login() => View(); // Login için view döndürür

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var admin = _context.Admins.FirstOrDefault(x => x.Username == username && x.Password == password);

            if (admin != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal);

                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı";
            return View();
        }

        public IActionResult StuTeaLog() => View(); // Login için view döndürür

        [HttpPost]
        public async Task<IActionResult> StuTeaLog(string usernameSTU, string passwordSTU, string usernameTeach, string passwordTeach)
        {
            // Öğrenci girişi kontrolü
            if (!string.IsNullOrEmpty(usernameSTU) && !string.IsNullOrEmpty(passwordSTU))
            {
                var student = _context.Students.FirstOrDefault(x => x.StudentEmail == usernameSTU && x.Password == passwordSTU);
                if (student != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usernameSTU),
                new Claim(ClaimTypes.Role, "Student")
            };
                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("CookieAuth", principal);
                    return RedirectToAction("Index", "Student");
                }
            }

            // Öğretmen girişi kontrolü
            if (!string.IsNullOrEmpty(usernameTeach) && !string.IsNullOrEmpty(passwordTeach))
            {
                var teacher = _context.Teachers.FirstOrDefault(x => x.TeacherMail == usernameTeach && x.TeacherPassword == passwordTeach);
                if (teacher != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usernameTeach),
                new Claim(ClaimTypes.Role, "Teacher")
            };
                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("CookieAuth", principal);
                    return RedirectToAction("Index", "Teacher");
                }
            }

            // Eğer hiçbir giriş başarılı değilse, hata mesajı göster
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı";
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth"); // Cookie'yi temizler ve kullanıcıyı çıkış yaptırır
            return RedirectToAction("Index", "Home"); // Anasayfaya yönlendirir
        }

        public async Task<IActionResult> LogoutStuTeach()
        {
            await HttpContext.SignOutAsync("CookieAuth"); // Cookie'yi temizler ve kullanıcıyı çıkış yaptırır
            return RedirectToAction("StuTeaLog", "Account"); // Giriş sayfasına yönlendirir
        }
    }
}
