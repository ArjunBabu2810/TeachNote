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
        public DbSet<Marks> Marks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    id = 1,
                    name = "adminDepartment"
                }
            );
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    id = 1,
                    name = "admin",
                    email = "admin@gmail.com",
                    role = "admin",
                    password = "$2a$11$4fDlR2V5kICabpxYaQVGx.f7G663bKMXdAKYm0b8GZlP1.uqNc1wG",
                    departmentId=1});
        }
    }
}