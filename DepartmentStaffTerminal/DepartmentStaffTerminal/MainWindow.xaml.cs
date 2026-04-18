using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DepartmentStaffTerminal.Models;
using AppModel = DepartmentStaffTerminal.Models.Application;

namespace DepartmentStaffTerminal.Views
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private Employee _currentEmployee = null!;

        public MainWindow(AppDbContext context, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _context = context;
            _serviceProvider = serviceProvider;
            dtpFrom.SelectedDate = DateTime.Today.AddDays(-7);
            dtpTo.SelectedDate = DateTime.Today.AddDays(7);
            // LoadRequests не вызывается – данные загрузятся после SetCurrentEmployee
        }

        public void SetCurrentEmployee(Employee employee)
        {
            _currentEmployee = employee;
            Title = $"Терминал подразделения – {employee.FullName} ({employee.Department?.Name})";
            LoadRequests();
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e) => LoadRequests();
        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadRequests();

        private void LoadRequests()
        {
            try
            {
                // Если сотрудник не установлен или у него нет подразделения – очищаем таблицу
                if (_currentEmployee?.DepartmentId == null)
                {
                    dgRequests.ItemsSource = null;
                    return;
                }

                DateTime? from = dtpFrom.SelectedDate;
                DateTime? to = dtpTo.SelectedDate;

                var approvedStatus = _context.Statuses.FirstOrDefault(s => s.Name == "одобрена");
                if (approvedStatus == null)
                {
                    dgRequests.ItemsSource = null;
                    return;
                }

                var query = _context.Applications
                    .Include(a => a.Department)
                    .Include(a => a.Status)
                    .Include(a => a.PersonalVisitor)
                    .Include(a => a.GroupVisitors)
                    .Include(a => a.VisitLogs)
                    .Where(a => a.StatusId == approvedStatus.Id &&
                                a.DepartmentId == _currentEmployee.DepartmentId);

                if (from.HasValue)
                    query = query.Where(a => a.StartDate >= from.Value);
                if (to.HasValue)
                    query = query.Where(a => a.StartDate <= to.Value);

                var list = query.AsEnumerable().OrderByDescending(a => a.StartDate).ToList();

                var viewModels = list.Select(a => new RequestListItem
                {
                    Id = a.Id,
                    Type = a.Type ?? "Неизвестно",
                    DepartmentName = a.Department?.Name ?? "—",
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Purpose = a.Purpose ?? "",
                    Applicant = a.Type == "личная"
                        ? $"{a.PersonalVisitor?.LastName} {a.PersonalVisitor?.FirstName}"
                        : $"Группа ({a.GroupVisitors.Count} чел.)",
                    VisitStatus = GetVisitStatus(a)
                }).ToList();

                dgRequests.ItemsSource = viewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заявок:\n{ex.Message}");
            }
        }

        private string GetVisitStatus(AppModel app)
        {
            bool hasEntry = app.VisitLogs.Any();
            bool hasExit = app.VisitLogs.Any(v => v.ExitTime != null);
            if (!hasEntry) return "Не начато";
            if (hasExit) return "Завершено";
            return "На территории";
        }

        private void DgRequests_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgRequests.SelectedItem is RequestListItem selected)
            {
                var detailsWindow = _serviceProvider.GetRequiredService<RequestDetailsWindow>();
                detailsWindow.SetRequestId(selected.Id, _currentEmployee);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
                LoadRequests();
            }
        }
    }

    public class RequestListItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Purpose { get; set; } = "";
        public string Applicant { get; set; } = "";
        public string VisitStatus { get; set; } = "";
    }
}