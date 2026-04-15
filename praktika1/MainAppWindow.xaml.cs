using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using praktika1.Helpers;
using praktika1.Models;
using AppModel = praktika1.Models.Application;

namespace praktika1
{
    public partial class MainAppWindow : Window
    {
        private User _currentUser;
        private string _photoPath;
        private string _scanPath;

        public MainAppWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadDepartments();
            dtpStart.SelectedDate = DateTime.Today.AddDays(1);
            dtpEnd.SelectedDate = DateTime.Today.AddDays(2);
            cmbFilterStatus.SelectedIndex = 0;
            LoadApplications();
        }

        private void LoadDepartments()
        {
            using (var context = new AppDbContext())
            {
                cmbDepartment.ItemsSource = context.Departments.ToList();
            }
        }

        private void CmbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDepartment.SelectedValue is int deptId)
            {
                using (var context = new AppDbContext())
                {
                    cmbEmployee.ItemsSource = context.Employees
                        .Where(emp => emp.DepartmentId == deptId && emp.IsActive == true)
                        .ToList();
                }
            }
        }

        private void BtnLoadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "JPEG files|*.jpg|All files|*.*" };
            if (dlg.ShowDialog() == true)
            {
                _photoPath = dlg.FileName;
                // Проверка размера и пропорций
                imgPhoto.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_photoPath));
            }
        }

        private void BtnLoadScan_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "PDF files|*.pdf" };
            if (dlg.ShowDialog() == true)
            {
                _scanPath = dlg.FileName;
                txtScanPath.Text = System.IO.Path.GetFileName(_scanPath);
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Валидация полей
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) || cmbDepartment.SelectedValue == null ||
                cmbEmployee.SelectedValue == null || string.IsNullOrWhiteSpace(txtSeries.Text) ||
                string.IsNullOrWhiteSpace(txtNumber.Text) || dtpBirth.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return;
            }

            if ((DateTime.Today.Year - dtpBirth.SelectedDate.Value.Year) < 16)
            {
                MessageBox.Show("Возраст посетителя должен быть не менее 16 лет.");
                return;
            }

            if (_scanPath == null)
            {
                MessageBox.Show("Необходимо загрузить скан паспорта.");
                return;
            }

            using (var context = new AppDbContext())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var statusCheck = context.Statuses.First(s => s.Name == "проверка");
                    var app = new AppModel
                    {
                        UserId = _currentUser.Id,
                        Type = "личная",
                        StartDate = dtpStart.SelectedDate.Value,
                        EndDate = dtpEnd.SelectedDate.Value,
                        Purpose = cmbPurpose.Text,
                        DepartmentId = (int)cmbDepartment.SelectedValue,
                        EmployeeId = (int)cmbEmployee.SelectedValue,
                        StatusId = statusCheck.Id,
                        CreatedAt = DateTime.Now
                    };
                    context.Applications.Add(app);
                    context.SaveChanges();

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
                        BirthDate = dtpBirth.SelectedDate.Value,
                        PassportSeries = txtSeries.Text,
                        PassportNumber = txtNumber.Text,
                        PhotoPath = _photoPath
                    };
                    context.PersonalVisitors.Add(visitor);
                    context.SaveChanges();

                    var doc = new Document
                    {
                        PersonalVisitorId = visitor.Id,
                        DocType = "скан_паспорта",
                        FilePath = _scanPath,
                        UploadedAt = DateTime.Now
                    };
                    context.Documents.Add(doc);
                    context.SaveChanges();

                    transaction.Commit();
                    MessageBox.Show("Заявка успешно отправлена!");
                    ClearForm();
                    LoadApplications();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void ClearForm()
        {
            txtLastName.Clear(); txtFirstName.Clear(); txtMiddleName.Clear();
            txtPhone.Clear(); txtEmail.Clear(); txtOrg.Clear(); txtNote.Clear();
            txtSeries.Clear(); txtNumber.Clear();
            _photoPath = null; imgPhoto.Source = null;
            _scanPath = null; txtScanPath.Text = "";
        }

        private void LoadApplications()
        {
            using (var context = new AppDbContext())
            {
                var query = from app in context.Applications
                            join dept in context.Departments on app.DepartmentId equals dept.Id
                            join status in context.Statuses on app.StatusId equals status.Id
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
                    string statusName = ((ComboBoxItem)cmbFilterStatus.SelectedItem).Content.ToString();
                    query = query.Where(a => a.Status == statusName);
                }

                dgApplications.ItemsSource = query.ToList();
            }
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