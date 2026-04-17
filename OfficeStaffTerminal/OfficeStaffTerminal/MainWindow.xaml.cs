using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeStaffTerminal.Models;

namespace OfficeStaffTerminal
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private Employee _currentEmployee = null!;
        private List<Department> _departments = new();
        private List<Status> _statuses = new();

        public MainWindow(AppDbContext context, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Загружаем ВСЁ только после полной загрузки окна
            Loaded += MainWindow_Loaded;
        }

        public void SetCurrentEmployee(Employee employee)
        {
            _currentEmployee = employee;
            Title = $"Терминал общего отдела – {employee.FullName}";
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilters();
            LoadRequests();
        }

        private void LoadFilters()
        {
            _departments = _context.Departments.ToList();
            _departments.Insert(0, new Department { Id = 0, Name = "Все" });
            cmbDepartment.ItemsSource = _departments;
            cmbDepartment.SelectedIndex = 0;

            _statuses = _context.Statuses.ToList();
            _statuses.Insert(0, new Status { Id = 0, Name = "Все" });
            cmbStatus.ItemsSource = _statuses;
            cmbStatus.SelectedIndex = 0;
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) LoadRequests();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadRequests();

        private void LoadRequests()
        {
            // Полная защита от NullReference
            if (cmbType == null || cmbDepartment == null || cmbStatus == null) return;

            try
            {
                string? type = (cmbType.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (type == "Все") type = null;

                int? deptId = cmbDepartment.SelectedValue as int?;
                if (deptId.GetValueOrDefault() == 0) deptId = null;

                int? statusId = cmbStatus.SelectedValue as int?;
                if (statusId.GetValueOrDefault() == 0) statusId = null;

                var query = _context.Applications
                    .Include(a => a.Department)
                    .Include(a => a.Status)
                    .Include(a => a.PersonalVisitor)
                    .Include(a => a.GroupVisitors)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(a => a.Type == type);

                if (deptId.HasValue)
                    query = query.Where(a => a.DepartmentId == deptId.Value);

                if (statusId.HasValue)
                    query = query.Where(a => a.StatusId == statusId.Value);

                var list = query.AsEnumerable().ToList();

                var sorted = list.OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue).ToList();

                var viewModels = sorted.Select(a => new RequestListItem
                {
                    Id = a.Id,
                    Type = a.Type ?? "Неизвестно",
                    DepartmentName = a.Department?.Name ?? "—",
                    StatusName = a.Status?.Name ?? "—",
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    VisitTime = a.VisitTime,
                    Purpose = a.Purpose ?? "",
                    Applicant = a.Type == "личная"
                        ? $"{a.PersonalVisitor?.LastName ?? ""} {a.PersonalVisitor?.FirstName ?? ""}".Trim()
                        : $"Группа ({(a.GroupVisitors?.Count ?? 0)} чел.)"
                }).ToList();

                dgRequests.ItemsSource = viewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заявок:\n{ex.Message}\n\n{ex.StackTrace}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgRequests_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgRequests.SelectedItem is RequestListItem selected)
            {
                var detailsWindow = _serviceProvider.GetRequiredService<RequestDetailsWindow>();
                detailsWindow.SetRequestId(selected.Id);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
                LoadRequests(); // обновляем после закрытия модального окна
            }
        }
    }

    // Вспомогательный класс для отображения в DataGrid
    public class RequestListItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string StatusName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? VisitTime { get; set; }
        public string Purpose { get; set; } = "";
        public string Applicant { get; set; } = "";
    }
}