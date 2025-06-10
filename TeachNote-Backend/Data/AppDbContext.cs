using Microsoft.EntityFrameworkCore;
using TeachNote_Backend.Models;

namespace TeachNote_Backend.Models {
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Subjects> Subjects { get; set; }
        public DbSet<Notes> Notes { get; set; }
        public DbSet<Marks> Marks { get; set;}
        
    }
}