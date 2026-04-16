using Microsoft.EntityFrameworkCore;

namespace programma_praktiki.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Status> Statuses => Set<Status>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<PersonalVisitor> PersonalVisitors => Set<PersonalVisitor>();
        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);
            // Настройка связей при необходимости
            modelBuilder.Entity<Application>()
        .Property(a => a.StartDate)
        .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Application>()
                .Property(a => a.EndDate)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<PersonalVisitor>()
                .Property(v => v.BirthDate)
                .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Department)
                .WithMany()
                .HasForeignKey(a => a.DepartmentId);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Status)
                .WithMany()
                .HasForeignKey(a => a.StatusId);

            modelBuilder.Entity<PersonalVisitor>()
                .HasOne(p => p.Application)
                .WithOne()
                .HasForeignKey<PersonalVisitor>(p => p.ApplicationId);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.PersonalVisitor)
                .WithMany()
                .HasForeignKey(d => d.PersonalVisitorId);
        }
    }
}