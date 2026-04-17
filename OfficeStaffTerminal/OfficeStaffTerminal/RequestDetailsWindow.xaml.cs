using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using OfficeStaffTerminal.Models;
using AppModel = OfficeStaffTerminal.Models.Application;

namespace OfficeStaffTerminal
{
    public partial class RequestDetailsWindow : Window
    {
        private readonly AppDbContext _context;
        private int _requestId;
        private AppModel? _application;
        private bool _isBlacklisted = false;

        public RequestDetailsWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            cmbNewStatus.ItemsSource = _context.Statuses.ToList();
        }

        public void SetRequestId(int id)
        {
            _requestId = id;
            LoadData();
        }

        private void LoadData()
        {
            _application = _context.Applications
                .Include(a => a.Department)
                .Include(a => a.Status)
                .Include(a => a.PersonalVisitor)
                .Include(a => a.GroupVisitors)
                .FirstOrDefault(a => a.Id == _requestId);

            if (_application == null)
            {
                MessageBox.Show("Заявка не найдена.");
                Close();
                return;
            }

            txtType.Text = _application.Type == "личная" ? "Личная" : "Групповая";
            txtStatus.Text = _application.Status?.Name;
            txtDepartment.Text = _application.Department?.Name;
            txtStartDate.Text = _application.StartDate.ToShortDateString();
            txtEndDate.Text = _application.EndDate.ToShortDateString();
            txtPurpose.Text = _application.Purpose;

            if (_application.VisitTime.HasValue)
            {
                dtpVisitDate.SelectedDate = _application.VisitTime.Value.Date;
                txtVisitTime.Text = _application.VisitTime.Value.ToString("HH:mm");
            }

            var visitors = new ObservableCollection<VisitorInfo>();
            if (_application.Type == "личная" && _application.PersonalVisitor != null)
            {
                var pv = _application.PersonalVisitor;
                bool inBlacklist = _context.Blacklist.Any(b => b.PersonalVisitorId == pv.Id);
                if (inBlacklist) _isBlacklisted = true;

                visitors.Add(new VisitorInfo
                {
                    LastName = pv.LastName,
                    FirstName = pv.FirstName,
                    MiddleName = pv.MiddleName,
                    Passport = $"{pv.PassportSeries} {pv.PassportNumber}",
                    BirthDate = pv.BirthDate,
                    InBlacklist = inBlacklist
                });

                var docs = _context.Documents.Where(d => d.PersonalVisitorId == pv.Id).ToList();
                lbDocuments.ItemsSource = docs.Select(d => d.FilePath).ToList();
            }
            else if (_application.Type == "групповая")
            {
                foreach (var gv in _application.GroupVisitors)
                {
                    bool inBlacklist = _context.Blacklist.Any(b => b.GroupVisitorId == gv.Id);
                    if (inBlacklist) _isBlacklisted = true;

                    visitors.Add(new VisitorInfo
                    {
                        LastName = gv.LastName,
                        FirstName = gv.FirstName,
                        MiddleName = gv.MiddleName,
                        Passport = $"{gv.PassportSeries} {gv.PassportNumber}",
                        BirthDate = gv.BirthDate,
                        InBlacklist = inBlacklist
                    });
                }
            }

            dgVisitors.ItemsSource = visitors;

            if (_isBlacklisted)
            {
                var rejectedStatus = _context.Statuses.First(s => s.Name == "не одобрена");
                if (_application.StatusId != rejectedStatus.Id)
                {
                    _application.StatusId = rejectedStatus.Id;
                    _application.RejectionReason = "Заявка на посещение объекта КИИ отклонена в связи с нарушением Федерального закона от 26.07.2017 № 187-ФЗ";
                    _context.SaveChanges();
                    txtStatus.Text = rejectedStatus.Name;
                }

                dtpVisitDate.IsEnabled = false;
                txtVisitTime.IsEnabled = false;
                cmbNewStatus.IsEnabled = false;
                btnSave.IsEnabled = false;

                MessageBox.Show("Внимание! Один или несколько посетителей находятся в чёрном списке. Заявка автоматически отклонена. Редактирование невозможно.",
                                "Чёрный список", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                dtpVisitDate.IsEnabled = true;
                txtVisitTime.IsEnabled = true;
                cmbNewStatus.IsEnabled = true;
                btnSave.IsEnabled = true;
                cmbNewStatus.SelectedValue = _application.StatusId;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_application == null) return;

            if (dtpVisitDate.SelectedDate.HasValue && !string.IsNullOrWhiteSpace(txtVisitTime.Text))
            {
                if (TimeSpan.TryParse(txtVisitTime.Text, out TimeSpan time))
                    _application.VisitTime = dtpVisitDate.SelectedDate.Value.Add(time);
                else
                {
                    MessageBox.Show("Неверный формат времени (ЧЧ:ММ).");
                    return;
                }
            }
            else
                _application.VisitTime = null;

            int newStatusId = (int)cmbNewStatus.SelectedValue;
            var newStatus = _context.Statuses.Find(newStatusId);
            if (newStatus == null) return;

            bool nowRejected = newStatus.Name == "не одобрена";
            bool nowApproved = newStatus.Name == "одобрена";

            _application.StatusId = newStatusId;
            _application.UpdatedAt = DateTime.UtcNow;

            if (nowRejected)
                _application.RejectionReason = "Недостоверные данные / проблемы с документами";
            else if (nowApproved)
                _application.RejectionReason = null;

            _context.SaveChanges();

            if (nowRejected && _application.RejectionReason?.Contains("Недостоверные") == true)
                CheckAndAddToBlacklist();

            string message = nowApproved
                ? $"Заявка одобрена. Дата посещения: {_application.VisitTime:dd.MM.yyyy HH:mm}"
                : $"Заявка отклонена. Причина: {_application.RejectionReason}";
            MessageBox.Show(message, "Уведомление");

            DialogResult = true;
            Close();
        }

        private void CheckAndAddToBlacklist()
        {
            if (_application == null) return;

            if (_application.Type == "личная" && _application.PersonalVisitor != null)
            {
                int rejectCount = _context.Applications
                    .Where(a => a.PersonalVisitor.Id == _application.PersonalVisitor.Id &&
                                a.Status.Name == "не одобрена" &&
                                a.RejectionReason.Contains("Недостоверные"))
                    .Count();

                if (rejectCount >= 2 && !_context.Blacklist.Any(b => b.PersonalVisitorId == _application.PersonalVisitor.Id))
                {
                    _context.Blacklist.Add(new BlacklistEntry
                    {
                        VisitorType = "личный",
                        PersonalVisitorId = _application.PersonalVisitor.Id,
                        Reason = "Двукратное указание недостоверных данных",
                        AddedDate = DateTime.UtcNow
                    });
                    _context.SaveChanges();
                    MessageBox.Show("Посетитель добавлен в чёрный список.");
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class VisitorInfo
    {
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string? MiddleName { get; set; }
        public string Passport { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public bool InBlacklist { get; set; }
        public string InBlacklistText => InBlacklist ? "Да" : "Нет";
    }
}