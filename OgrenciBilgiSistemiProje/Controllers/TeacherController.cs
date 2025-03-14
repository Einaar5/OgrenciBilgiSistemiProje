using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemiProje.Services;

namespace OgrenciBilgiSistemiProje.Controllers
{
    
    public class TeacherController : BaseController
    {
        private readonly ApplicationDbContext _context; // Veritabanı bağlantısı

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var teacher = _context.Teachers.FirstOrDefault(x => x.TeacherMail == HttpContext.Session.GetString("username"));
            return View(teacher);
        }
    }
}
