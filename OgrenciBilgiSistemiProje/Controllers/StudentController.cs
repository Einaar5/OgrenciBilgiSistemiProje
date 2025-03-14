using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemiProje.Services;

namespace OgrenciBilgiSistemiProje.Controllers
{
   
    public class StudentController : BaseController
    {
        private readonly ApplicationDbContext _context; // Veritabanı bağlantısı

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var student = _context.Students.FirstOrDefault(x => x.StudentEmail == HttpContext.Session.GetString("username"));
            return View(student);
        }
    }
}
