using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using DepartmentStaffTerminal.Models;
using AppModel = DepartmentStaffTerminal.Models.Application;   // псевдоним

namespace DepartmentStaffTerminal.Views
{
    public partial class RequestDetailsWindow : Window
    {
        private readonly AppDbContext _context;
        private int _requestId;
        private Employee _currentEmployee;
        private AppModel? _application;

        private const int TravelTimeMinutes = 10;

        public RequestDetailsWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        public void SetRequestId(int id, Employee employee)
        {
            _requestId = id;
            _currentEmployee = employee;
            LoadData();
        }

        private void LoadData()
        {
            _application = _context.Applications
                .Include(a => a.Department)
                .Include(a => a.PersonalVisitor)
                .Include(a => a.GroupVisitors)
                .Include(a => a.VisitLogs)
                .FirstOrDefault(a => a.Id == _requestId);

            if (_application == null)
            {
                MessageBox.Show("Заявка не найдена.");
                Close();
                return;
            }

            txtType.Text = _application.Type == "личная" ? "Личная" : "Групповая";
            txtDepartment.Text = _application.Department?.Name;
            txtStartDate.Text = _application.StartDate.ToShortDateString();
            txtEndDate.Text = _application.EndDate.ToShortDateString();
            txtPurpose.Text = _application.Purpose;

            var visitors = new ObservableCollection<VisitorInfo>();

            if (_application.Type == "личная" && _application.PersonalVisitor != null)
            {
                var pv = _application.PersonalVisitor;
                var log = _application.VisitLogs.FirstOrDefault();
                visitors.Add(new VisitorInfo
                {
                    Id = pv.Id,
                    VisitorType = "личный",
                    LastName = pv.LastName,
                    FirstName = pv.FirstName,
                    MiddleName = pv.MiddleName,
                    Passport = $"{pv.PassportSeries} {pv.PassportNumber}",
                    BirthDate = pv.BirthDate,
                    EntryTime = log?.EntryTime,
                    ExitTime = log?.ExitTime
                });

                var docs = _context.Documents.Where(d => d.PersonalVisitorId == pv.Id).ToList();
                lbDocuments.ItemsSource = docs.Select(d => d.FilePath).ToList();
            }
            else if (_application.Type == "групповая")
            {
                foreach (var gv in _application.GroupVisitors)
                {
                    var log = _application.VisitLogs.FirstOrDefault(l => l.GroupVisitorId == gv.Id);
                    visitors.Add(new VisitorInfo
                    {
                        Id = gv.Id,
                        VisitorType = "групповой",
                        LastName = gv.LastName,
                        FirstName = gv.FirstName,
                        MiddleName = gv.MiddleName,
                        Passport = $"{gv.PassportSeries} {gv.PassportNumber}",
                        BirthDate = gv.BirthDate,
                        EntryTime = log?.EntryTime,
                        ExitTime = log?.ExitTime
                    });
                }
            }

            dgVisitors.ItemsSource = visitors;

            bool hasEntry = _application.VisitLogs.Any(v => v.EntryTime != null);
            bool hasExit = _application.VisitLogs.Any(v => v.ExitTime != null);
            bool isArrivalAllowed = hasEntry && !hasExit;

            btnConfirmArrival.IsEnabled = isArrivalAllowed;
            btnConfirmDeparture.IsEnabled = hasEntry && !hasExit;
        }

        private void BtnConfirmArrival_Click(object sender, RoutedEventArgs e)
        {
            if (_application == null) return;

            var firstEntry = _application.VisitLogs.Min(v => (DateTime?)v.EntryTime);
            if (firstEntry.HasValue)
            {
                var travelTime = DateTime.Now - firstEntry.Value;
                if (travelTime.TotalMinutes > TravelTimeMinutes)
                {
                    MessageBox.Show($"Внимание! Превышено нормативное время перемещения ({TravelTimeMinutes} мин). Оповещение отправлено сотруднику охраны и подразделения.",
                                    "Нарушение времени", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            MessageBox.Show("Прибытие посетителя подтверждено.");
        }

        private void BtnConfirmDeparture_Click(object sender, RoutedEventArgs e)
        {
            if (_application == null) return;

            var now = DateTime.Now;
            var logs = _context.VisitLogs.Where(v => v.ApplicationId == _application.Id && v.ExitTime == null).ToList();
            foreach (var log in logs)
            {
                log.ExitTime = now;
            }

            _context.SaveChanges();
            LoadData();
            MessageBox.Show("Время убытия зафиксировано.");
        }

        private void DgVisitors_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) { }

        private void AddToBlacklist_Click(object sender, RoutedEventArgs e)
        {
            if (dgVisitors.SelectedItem is VisitorInfo selected)
            {
                var reasonWindow = new BlacklistReasonWindow();
                reasonWindow.Owner = this;
                if (reasonWindow.ShowDialog() == true)
                {
                    var entry = new BlacklistEntry
                    {
                        VisitorType = selected.VisitorType,
                        PersonalVisitorId = selected.VisitorType == "личный" ? selected.Id : null,
                        GroupVisitorId = selected.VisitorType == "групповой" ? selected.Id : null,
                        Reason = reasonWindow.Reason,
                        AddedDate = DateTime.UtcNow
                    };
                    _context.Blacklist.Add(entry);
                    _context.SaveChanges();
                    MessageBox.Show("Посетитель добавлен в чёрный список.");
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }

    public class VisitorInfo
    {
        public int Id { get; set; }
        public string VisitorType { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string? MiddleName { get; set; }
        public string Passport { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
    }
}