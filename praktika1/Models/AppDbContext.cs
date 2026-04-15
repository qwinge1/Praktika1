using System.Data.Entity;
using Npgsql;

namespace praktika1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=DefaultConnection") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<PersonalVisitor> PersonalVisitors { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Настройка имен таблиц и столбцов уже через атрибуты
            base.OnModelCreating(modelBuilder);
        }
    }
}