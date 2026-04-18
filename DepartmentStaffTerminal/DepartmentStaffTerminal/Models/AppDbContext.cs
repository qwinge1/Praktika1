using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DepartmentStaffTerminal.Models
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
        public DbSet<VisitLog> VisitLogs => Set<VisitLog>();
        public DbSet<BlacklistEntry> Blacklist => Set<BlacklistEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.PersonalVisitor)
                .WithOne(p => p.Application)
                .HasForeignKey<PersonalVisitor>(p => p.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasMany(a => a.GroupVisitors)
                .WithOne(g => g.Application)
                .HasForeignKey(g => g.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitLog>()
                .HasOne(v => v.Application)
                .WithMany(a => a.VisitLogs)
                .HasForeignKey(v => v.ApplicationId);

            modelBuilder.Entity<VisitLog>()
                .HasOne(v => v.PersonalVisitor)
                .WithMany()
                .HasForeignKey(v => v.PersonalVisitorId);

            modelBuilder.Entity<VisitLog>()
                .HasOne(v => v.GroupVisitor)
                .WithMany()
                .HasForeignKey(v => v.GroupVisitorId);

            modelBuilder.Entity<VisitLog>()
                .HasOne(v => v.SecurityEmployee)
                .WithMany()
                .HasForeignKey(v => v.SecurityEmployeeId);

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
        }
    }
}