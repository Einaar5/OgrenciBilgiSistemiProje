using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Models;

namespace OgrenciBilgiSistemiProje.Services
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AdminAcc> Admins { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Teacher> Teachers { get; set; }


    }
    
}
