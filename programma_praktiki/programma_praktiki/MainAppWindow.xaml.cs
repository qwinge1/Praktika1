using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Npgsql;
using programma_praktiki.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppModel = programma_praktiki.Models.Application;

namespace programma_praktiki
{
    public partial class MainAppWindow : Window
    {
        private readonly AppDbContext _context;
        private User? _currentUser;
        private string? _photoPath;
        private string? _scanPath;
        private ObservableCollection<GroupVisitorViewModel> _groupVisitors = new();
        private string? _organizerScanPath;

        public MainAppWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            dtpStart.SelectedDate = DateTime.Today.AddDays(1);
            dtpEnd.SelectedDate = DateTime.Today.AddDays(2);
            LoadDepartments();
            dtpGroupStart.SelectedDate = DateTime.Today.AddDays(1);
            dtpGroupEnd.SelectedDate = DateTime.Today.AddDays(2);
            LoadGroupDepartments();
            dgGroupVisitors.ItemsSource = _groupVisitors;
            dgGroupVisitors.SelectedItem = null;
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            LoadApplications();
        }

        private void LoadGroupDepartments()
        {
            cmbGroupDepartment.ItemsSource = _context.Departments.ToList();
        }

        private void CmbGroupDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbGroupDepartment.SelectedValue is int deptId)
            {
                cmbGroupEmployee.ItemsSource = _context.Employees
                    .Where(emp => emp.DepartmentId == deptId && emp.IsActive == true)
                    .ToList();
            }
        }

        private void LoadDepartments()
        {
            cmbDepartment.ItemsSource = _context.Departments.ToList();
        }

        private void BtnAddGroupVisitor_Click(object sender, RoutedEventArgs e)
        {
            var newVisitor = new GroupVisitorViewModel
            {
                LineNumber = _groupVisitors.Count + 1,
                BirthDate = DateTime.Today.AddYears(-20)
            };
            _groupVisitors.Add(newVisitor);
        }

        private void BtnLoadGroupVisitorPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GroupVisitorViewModel visitor)
            {
                var dlg = new OpenFileDialog { Filter = "JPEG files|*.jpg" };
                if (dlg.ShowDialog() == true)
                {
                    visitor.PhotoPath = dlg.FileName;
                }
            }
        }

        private void BtnLoadGroupVisitorScan_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GroupVisitorViewModel visitor)
            {
                var dlg = new OpenFileDialog { Filter = "PDF files|*.pdf" };
                if (dlg.ShowDialog() == true)
                {
                    visitor.ScanPath = dlg.FileName;
                }
            }
        }

        private void BtnSubmitGroup_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Пользователь не авторизован.");
                return;
            }

            // Валидация полей пропуска
            if (dtpGroupStart.SelectedDate == null || dtpGroupEnd.SelectedDate == null ||
                string.IsNullOrWhiteSpace(cmbGroupPurpose.Text) ||
                cmbGroupDepartment.SelectedValue == null || cmbGroupEmployee.SelectedValue == null)
            {
                MessageBox.Show("Заполните информацию о пропуске и принимающей стороне.");
                return;
            }

            if (_groupVisitors.Count < 5)
            {
                MessageBox.Show("В группе должно быть не менее 5 участников.");
                return;
            }

            // Валидация каждого участника
            foreach (var v in _groupVisitors)
            {
                if (string.IsNullOrWhiteSpace(v.LastName) || string.IsNullOrWhiteSpace(v.FirstName) ||
                    string.IsNullOrWhiteSpace(v.Email) || string.IsNullOrWhiteSpace(v.PassportSeries) ||
                    string.IsNullOrWhiteSpace(v.PassportNumber) || v.BirthDate == default)
                {
                    MessageBox.Show($"Участник #{v.LineNumber}: заполните все обязательные поля.");
                    return;
                }

                // ← НОВАЯ ПРОВЕРКА ДЛИНЫ ТЕЛЕФОНА
                if (!string.IsNullOrWhiteSpace(v.Phone) && v.Phone.Length > 100)
                {
                    MessageBox.Show($"Участник #{v.LineNumber}: телефон слишком длинный (максимум 100 символов).");
                    return;
                }

                if ((DateTime.Today.Year - v.BirthDate.Year) < 16)
                {
                    MessageBox.Show($"Участник #{v.LineNumber}: возраст менее 16 лет.");
                    return;
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(v.PassportSeries, @"^\d{4}$") ||
                    !System.Text.RegularExpressions.Regex.IsMatch(v.PassportNumber, @"^\d{6}$"))
                {
                    MessageBox.Show($"Участник #{v.LineNumber}: серия (4 цифры) или номер (6 цифр) паспорта неверны.");
                    return;
                }

                if (!v.Email.Contains("@") || !v.Email.Contains("."))
                {
                    MessageBox.Show($"Участник #{v.LineNumber}: некорректный email.");
                    return;
                }

                if (string.IsNullOrEmpty(v.ScanPath))
                {
                    MessageBox.Show($"Участник #{v.LineNumber}: не загружен скан паспорта.");
                    return;
                }
            }

            if (string.IsNullOrEmpty(_organizerScanPath))
            {
                MessageBox.Show("Загрузите скан паспорта организатора.");
                return;
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var statusCheck = _context.Statuses.First(s => s.Name == "проверка");
                var startDate = DateTime.SpecifyKind(dtpGroupStart.SelectedDate.Value, DateTimeKind.Unspecified);
                var endDate = DateTime.SpecifyKind(dtpGroupEnd.SelectedDate.Value, DateTimeKind.Unspecified);
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                var app = new AppModel
                {
                    UserId = _currentUser.Id,
                    Type = "групповая",
                    StartDate = startDate,
                    EndDate = endDate,
                    Purpose = cmbGroupPurpose.Text,
                    DepartmentId = (int)cmbGroupDepartment.SelectedValue,
                    EmployeeId = (int)cmbGroupEmployee.SelectedValue,
                    StatusId = statusCheck.Id,
                    CreatedAt = now
                };

                _context.Applications.Add(app);
                _context.SaveChanges();

                foreach (var v in _groupVisitors)
                {
                    var gv = new GroupVisitor
                    {
                        ApplicationId = app.Id,
                        LineNumber = v.LineNumber,
                        LastName = v.LastName,
                        FirstName = v.FirstName,
                        MiddleName = v.MiddleName,
                        Phone = v.Phone,
                        Email = v.Email,
                        Organization = v.Organization,
                        Note = v.Note,
                        BirthDate = DateTime.SpecifyKind(v.BirthDate, DateTimeKind.Unspecified),
                        PassportSeries = v.PassportSeries,
                        PassportNumber = v.PassportNumber,
                        PhotoPath = v.PhotoPath
                    };

                    _context.GroupVisitors.Add(gv);
                    _context.SaveChanges();

                    // Скан паспорта
                    _context.Documents.Add(new Document
                    {
                        GroupVisitorId = gv.Id,
                        DocType = "скан_паспорта",
                        FilePath = v.ScanPath,
                        UploadedAt = now
                    });

                    // Фото (если загружено)
                    if (!string.IsNullOrEmpty(v.PhotoPath))
                    {
                        _context.Documents.Add(new Document
                        {
                            GroupVisitorId = gv.Id,
                            DocType = "фото",
                            FilePath = v.PhotoPath,
                            UploadedAt = now
                        });
                    }
                }

                // Скан организатора
                _context.Documents.Add(new Document
                {
                    DocType = "скан_паспорта",
                    FilePath = _organizerScanPath,
                    UploadedAt = now
                });

                _context.SaveChanges();
                transaction.Commit();

                MessageBox.Show("Групповая заявка успешно отправлена!");
                ClearGroupForm();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "\n" + ex.InnerException.Message;
                if (ex is DbUpdateException dbEx && dbEx.InnerException is PostgresException pgEx)
                    errorMessage += $"\n[PostgreSQL] {pgEx.MessageText}";
                MessageBox.Show($"Ошибка при сохранении:\n{errorMessage}");
            }
        }

        private void ClearGroupForm()
        {
            _groupVisitors.Clear();
            dtpGroupStart.SelectedDate = DateTime.Today.AddDays(1);
            dtpGroupEnd.SelectedDate = DateTime.Today.AddDays(2);
            cmbGroupPurpose.SelectedIndex = -1;
            cmbGroupDepartment.SelectedIndex = -1;
            cmbGroupEmployee.ItemsSource = null;
            txtOrganizerScanPath.Text = "";
            _organizerScanPath = null;
        }

        private void BtnLoadOrganizerScan_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "PDF files|*.pdf" };
            if (dlg.ShowDialog() == true)
            {
                _organizerScanPath = dlg.FileName;
                txtOrganizerScanPath.Text = System.IO.Path.GetFileName(_organizerScanPath);
            }
        }

        private void BtnRemoveGroupVisitor_Click(object sender, RoutedEventArgs e)
        {
            if (dgGroupVisitors.SelectedItem is GroupVisitorViewModel selected)
            {
                _groupVisitors.Remove(selected);
                for (int i = 0; i < _groupVisitors.Count; i++)
                    _groupVisitors[i].LineNumber = i + 1;
            }
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

            var startDate = DateTime.SpecifyKind(dtpStart.SelectedDate.Value, DateTimeKind.Unspecified);
            var endDate = DateTime.SpecifyKind(dtpEnd.SelectedDate.Value, DateTimeKind.Unspecified);
            var birthDate = DateTime.SpecifyKind(dtpBirth.SelectedDate.Value, DateTimeKind.Unspecified);
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

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
                    StatusId = statusCheck.Id,
                    CreatedAt = now
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
                    FilePath = _scanPath,
                    UploadedAt = now
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
                if (ex is DbUpdateException dbEx && dbEx.InnerException is PostgresException pgEx)
                    errorMessage += $"\n[PostgreSQL] {pgEx.MessageText}";
                MessageBox.Show($"Ошибка при сохранении:\n{errorMessage}");
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
        private void TextBoxOnlyDigits_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^\d+$");
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