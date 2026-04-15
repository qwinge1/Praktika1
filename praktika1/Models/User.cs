using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace praktika1.Models
{
    [Table("Пользователь")]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        [Column("хеш_пароля")]
        public string PasswordHash { get; set; }
        [Column("роль_id")]
        public int RoleId { get; set; }
        [Column("дата_регистрации")]
        public DateTime? RegistrationDate { get; set; }
        public virtual Role Role { get; set; }
    }

    [Table("Роль")]
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Table("Подразделение")]
    public class Department
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    [Table("Сотрудник")]
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public int? DepartmentId { get; set; }
        public string EmployeeCode { get; set; }
        public bool? IsActive { get; set; }
        public virtual Department Department { get; set; }
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }

    [Table("Статус")]
    public class Status
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Table("Заявка")]
    public class Application
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Purpose { get; set; }
        public int DepartmentId { get; set; }
        public int EmployeeId { get; set; }
        public int StatusId { get; set; }
        public string RejectionReason { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual User User { get; set; }
        public virtual Department Department { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Status Status { get; set; }
    }

    [Table("ПосетительЛичный")]
    public class PersonalVisitor
    {
        [Key]
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public string Note { get; set; }
        public DateTime BirthDate { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public string PhotoPath { get; set; }
        public virtual Application Application { get; set; }
    }

    [Table("Документ")]
    public class Document
    {
        [Key]
        public int Id { get; set; }
        public int? PersonalVisitorId { get; set; }
        public string DocType { get; set; }
        public string FilePath { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}