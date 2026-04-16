using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using programma_praktiki.Models;
using AppModel = programma_praktiki.Models.Application;

namespace programma_praktiki
{
    public partial class MainAppWindow : Window
    {
        private readonly AppDbContext _context;
        private User? _currentUser;
        private string? _photoPath;
        private string? _scanPath;

        public MainAppWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            dtpStart.SelectedDate = DateTime.Today.AddDays(1);
            dtpEnd.SelectedDate = DateTime.Today.AddDays(2);
            LoadDepartments();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            LoadApplications();
        }

        private void LoadDepartments()
        {
            cmbDepartment.ItemsSource = _context.Departments.ToList();
        }

        private void CmbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDepartment.SelectedValue is int deptId)
            {
                cmbEmployee.ItemsSource = _context.Employees
                    .Where(emp => emp.DepartmentId == deptId && emp.IsActive == true)
                    .ToList();
            }
        }

        private void BtnLoadPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JPEG files|*.jpg" };
            if (dlg.ShowDialog() == true)
            {
                _photoPath = dlg.FileName;
                imgPhoto.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_photoPath));
            }
        }

        private void BtnLoadScan_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "PDF files|*.pdf" };
            if (dlg.ShowDialog() == true)
            {
                _scanPath = dlg.FileName;
                txtScanPath.Text = System.IO.Path.GetFileName(_scanPath);
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Ошибка: пользователь не авторизован.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) || cmbDepartment.SelectedValue == null ||
                cmbEmployee.SelectedValue == null || string.IsNullOrWhiteSpace(txtSeries.Text) ||
                string.IsNullOrWhiteSpace(txtNumber.Text) || dtpBirth.SelectedDate == null ||
                dtpStart.SelectedDate == null || dtpEnd.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return;
            }

            if ((DateTime.Today.Year - dtpBirth.SelectedDate.Value.Year) < 16)
            {
                MessageBox.Show("Возраст посетителя должен быть не менее 16 лет.");
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtSeries.Text, @"^\d{4}$"))
            {
                MessageBox.Show("Серия паспорта должна содержать ровно 4 цифры.");
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtNumber.Text, @"^\d{6}$"))
            {
                MessageBox.Show("Номер паспорта должен содержать ровно 6 цифр.");
                return;
            }

            if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            {
                MessageBox.Show("Введите корректный email.");
                return;
            }

            if (_scanPath == null)
            {
                MessageBox.Show("Необходимо загрузить скан паспорта.");
                return;
            }

            // === ИСПРАВЛЕНИЕ: используем Unspecified для timestamp without time zone ===
            var startDate = DateTime.SpecifyKind(dtpStart.SelectedDate.Value, DateTimeKind.Unspecified);
            var endDate = DateTime.SpecifyKind(dtpEnd.SelectedDate.Value, DateTimeKind.Unspecified);
            var birthDate = DateTime.SpecifyKind(dtpBirth.SelectedDate.Value, DateTimeKind.Unspecified);

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var statusCheck = _context.Statuses.First(s => s.Name == "проверка");

                var app = new AppModel
                {
                    UserId = _currentUser.Id,
                    Type = "личная",
                    StartDate = startDate,
                    EndDate = endDate,
                    Purpose = cmbPurpose.Text,
                    DepartmentId = (int)cmbDepartment.SelectedValue,
                    EmployeeId = (int)cmbEmployee.SelectedValue,
                    StatusId = statusCheck.Id
                };

                _context.Applications.Add(app);
                _context.SaveChanges();

                var visitor = new PersonalVisitor
                {
                    ApplicationId = app.Id,
                    LastName = txtLastName.Text,
                    FirstName = txtFirstName.Text,
                    MiddleName = txtMiddleName.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text,
                    Organization = txtOrg.Text,
                    Note = txtNote.Text,
                    BirthDate = birthDate,
                    PassportSeries = txtSeries.Text,
                    PassportNumber = txtNumber.Text,
                    PhotoPath = _photoPath
                };

                _context.PersonalVisitors.Add(visitor);
                _context.SaveChanges();

                var doc = new Document
                {
                    PersonalVisitorId = visitor.Id,
                    DocType = "скан_паспорта",
                    FilePath = _scanPath
                };

                _context.Documents.Add(doc);
                _context.SaveChanges();

                transaction.Commit();

                MessageBox.Show("Заявка успешно отправлена!");
                ClearForm();
                LoadApplications();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "\n" + ex.InnerException.Message;
                MessageBox.Show($"Ошибка при сохранении: {errorMessage}");
            }
        }

        private void ClearForm()
        {
            txtLastName.Clear(); txtFirstName.Clear(); txtMiddleName.Clear();
            txtPhone.Clear(); txtEmail.Clear(); txtOrg.Clear(); txtNote.Clear();
            txtSeries.Clear(); txtNumber.Clear();
            _photoPath = null; imgPhoto.Source = null;
            _scanPath = null; txtScanPath.Text = "";
            dtpStart.SelectedDate = DateTime.Today.AddDays(1);
            dtpEnd.SelectedDate = DateTime.Today.AddDays(2);
            dtpBirth.SelectedDate = null;
        }

        private void LoadApplications()
        {
            if (_currentUser == null) return;

            var query = from app in _context.Applications
                        join dept in _context.Departments on app.DepartmentId equals dept.Id
                        join status in _context.Statuses on app.StatusId equals status.Id
                        where app.UserId == _currentUser.Id
                        select new
                        {
                            app.Type,
                            Department = dept.Name,
                            app.StartDate,
                            app.EndDate,
                            Status = status.Name,
                            app.RejectionReason
                        };

            if (cmbFilterStatus.SelectedIndex > 0)
            {
                string statusName = ((ComboBoxItem)cmbFilterStatus.SelectedItem).Content.ToString()!;
                query = query.Where(a => a.Status == statusName);
            }

            dgApplications.ItemsSource = query.ToList();
        }

        private void CmbFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadApplications();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadApplications();
        }
    }
}