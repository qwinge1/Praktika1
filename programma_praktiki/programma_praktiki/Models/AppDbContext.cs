using Microsoft.EntityFrameworkCore;
using programma_praktiki.Models;

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
        public DbSet<GroupVisitor> GroupVisitors => Set<GroupVisitor>();
        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ====================== НАВИГАЦИОННЫЕ СВЯЗИ ======================
            modelBuilder.Entity<Document>()
                .HasOne(d => d.GroupVisitor)
                .WithMany(g => g.Documents)
                .HasForeignKey(d => d.GroupVisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.PersonalVisitor)
                .WithMany()
                .HasForeignKey(d => d.PersonalVisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasMany(a => a.GroupVisitors)
                .WithOne(g => g.Application)
                .HasForeignKey(g => g.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.PersonalVisitor)
                .WithOne(p => p.Application)
                .HasForeignKey<PersonalVisitor>(p => p.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ====================== ТИПЫ КОЛОНОК ДЛЯ ДАТ ======================
            // Поля типа DATE в базе
            modelBuilder.Entity<Application>()
                .Property(a => a.StartDate)
                .HasColumnType("date");

            modelBuilder.Entity<Application>()
                .Property(a => a.EndDate)
                .HasColumnType("date");

            modelBuilder.Entity<PersonalVisitor>()
                .Property(v => v.BirthDate)
                .HasColumnType("date");

            modelBuilder.Entity<GroupVisitor>()
                .Property(v => v.BirthDate)
                .HasColumnType("date");

            // Поля типа TIMESTAMP WITHOUT TIME ZONE
            modelBuilder.Entity<Application>()
                .Property(a => a.CreatedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Application>()
                .Property(a => a.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Document>()
                .Property(d => d.UploadedAt)
                .HasColumnType("timestamp without time zone");
        }
    }
}