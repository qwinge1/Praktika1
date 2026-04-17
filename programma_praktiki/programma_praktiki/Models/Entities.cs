using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace programma_praktiki.Models
{
    [Table("Роль")]
    public class Role
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("название")]
        public string Name { get; set; } = null!;
    }

    [Table("Пользователь")]
    public class User
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("хеш_пароля")]
        public string PasswordHash { get; set; } = null!;

        [Column("роль_id")]
        public int RoleId { get; set; }

        [Column("дата_регистрации")]
        public DateTime? RegistrationDate { get; set; }

        public virtual Role? Role { get; set; }
    }

    [Table("Подразделение")]
    public class Department
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("название")]
        public string Name { get; set; } = null!;

        [Column("описание")]
        public string? Description { get; set; }
    }

    [Table("Сотрудник")]
    public class Employee
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("фамилия")]
        public string LastName { get; set; } = null!;

        [Column("имя")]
        public string FirstName { get; set; } = null!;

        [Column("отчество")]
        public string? MiddleName { get; set; }

        [Column("подразделение_id")]
        public int? DepartmentId { get; set; }

        [Column("табельный_номер")]
        public string? EmployeeCode { get; set; }

        [Column("активен")]
        public bool? IsActive { get; set; }

        public virtual Department? Department { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }

    [Table("Статус")]
    public class Status
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("название")]
        public string Name { get; set; } = null!;
    }

    [Table("Заявка")]
    public class Application
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("пользователь_id")]
        public int UserId { get; set; }

        [Column("тип")]
        public string Type { get; set; } = null!;

        [Column("дата_начала")]
        public DateTime StartDate { get; set; }

        [Column("дата_окончания")]
        public DateTime EndDate { get; set; }

        [Column("цель_посещения")]
        public string Purpose { get; set; } = null!;

        [Column("подразделение_id")]
        public int DepartmentId { get; set; }

        [Column("сотрудник_id")]
        public int EmployeeId { get; set; }

        [Column("статус_id")]
        public int StatusId { get; set; }

        [Column("причина_отказа")]
        public string? RejectionReason { get; set; }

        [Column("дата_создания")]
        public DateTime? CreatedAt { get; set; }

        [Column("дата_обновления")]
        public DateTime? UpdatedAt { get; set; }

        public virtual User? User { get; set; }
        public virtual Department? Department { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual Status? Status { get; set; }
        public virtual PersonalVisitor? PersonalVisitor { get; set; }
        public virtual ICollection<GroupVisitor> GroupVisitors { get; set; } = new List<GroupVisitor>();
    }

    [Table("ПосетительЛичный")]
    public class PersonalVisitor
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("заявка_id")]
        public int ApplicationId { get; set; }

        [Column("фамилия")]
        public string LastName { get; set; } = null!;

        [Column("имя")]
        public string FirstName { get; set; } = null!;

        [Column("отчество")]
        public string? MiddleName { get; set; }

        [Column("телефон")]
        [MaxLength(100)]
        public string? Phone { get; set; }

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("организация")]
        public string? Organization { get; set; }

        [Column("примечание")]
        public string Note { get; set; } = null!;

        [Column("дата_рождения")]
        public DateTime BirthDate { get; set; }

        [Column("серия_паспорта")]
        public string PassportSeries { get; set; } = null!;

        [Column("номер_паспорта")]
        public string PassportNumber { get; set; } = null!;

        [Column("путь_к_фото")]
        public string? PhotoPath { get; set; }

        public virtual Application? Application { get; set; }
    }

    [Table("ПосетительГрупповой")]
    public class GroupVisitor
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("заявка_id")]
        public int ApplicationId { get; set; }

        [Column("номер_по_порядку")]
        public int LineNumber { get; set; }

        [Column("фамилия")]
        public string LastName { get; set; } = null!;

        [Column("имя")]
        public string FirstName { get; set; } = null!;

        [Column("отчество")]
        public string? MiddleName { get; set; }

        [Column("телефон")]
        [MaxLength(100)]
        public string? Phone { get; set; }

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("организация")]
        public string? Organization { get; set; }

        [Column("примечание")]
        public string Note { get; set; } = null!;

        [Column("дата_рождения")]
        public DateTime BirthDate { get; set; }

        [Column("серия_паспорта")]
        public string PassportSeries { get; set; } = null!;

        [Column("номер_паспорта")]
        public string PassportNumber { get; set; } = null!;

        [Column("путь_к_фото")]
        public string? PhotoPath { get; set; }

        // Навигационное свойство
        public virtual Application? Application { get; set; }
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }

    [Table("Документ")]
    public class Document
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("посетитель_личный_id")]
        public int? PersonalVisitorId { get; set; }

        [Column("посетитель_групповой_id")]
        public int? GroupVisitorId { get; set; }   // <-- ВАЖНО: добавить это свойство

        [Column("тип_документа")]
        public string DocType { get; set; } = null!;

        [Column("путь_к_файлу")]
        public string FilePath { get; set; } = null!;

        [Column("дата_загрузки")]
        public DateTime? UploadedAt { get; set; }

        public virtual PersonalVisitor? PersonalVisitor { get; set; }
        public virtual GroupVisitor? GroupVisitor { get; set; }
    }
}