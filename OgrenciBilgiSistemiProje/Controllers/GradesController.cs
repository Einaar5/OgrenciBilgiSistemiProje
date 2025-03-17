using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemiProje.Services;
using OgrenciBilgiSistemiProje.Models;
using System;
using Microsoft.EntityFrameworkCore;

namespace OgrenciBilgiSistemiProje.Controllers
{
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Not ekleme formu
        public IActionResult Create()
        {
            ViewBag.Students = _context.Students.ToList();
            ViewBag.Lessons = _context.Lessons.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(GradeDto gradeDto)
        {
            if (ModelState.IsValid)
            {
                var grade = new Grade
                {
                    StudentId = gradeDto.StudentId,
                    LessonId = gradeDto.LessonId,
                    Midterm = gradeDto.Midterm,
                    Final = gradeDto.Final,
                    Average = (gradeDto.Midterm * 0.4) + (gradeDto.Final * 0.6) // Örnek hesaplama
                };

                _context.Grades.Add(grade);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Students = _context.Students.ToList();
            ViewBag.Lessons = _context.Lessons.ToList();
            return View(gradeDto);
        }

        // Notları listeleme
        public IActionResult Index()
        {
            var grades = _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Lesson)
                .ToList();
            return View(grades);
        }
    }
}