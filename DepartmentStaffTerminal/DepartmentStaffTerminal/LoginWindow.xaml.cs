using DepartmentStaffTerminal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Windows;

namespace DepartmentStaffTerminal.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public LoginWindow(AppDbContext context, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _context = context;
            _serviceProvider = serviceProvider;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Введите код сотрудника.");
                return;
            }

            var employee = _context.Employees
                .Include(e => e.Department)
                .FirstOrDefault(e => e.EmployeeCode == code && e.IsActive == true);

            if (employee == null)
            {
                MessageBox.Show("Неверный код или сотрудник не активен.");
                return;
            }

            // Проверяем, что сотрудник относится к подразделению (не охрана, не общий отдел)
            if (employee.Department != null &&
                (employee.Department.Name == "Охрана" || employee.Department.Name == "Общий отдел"))
            {
                MessageBox.Show("Этот терминал предназначен для сотрудников подразделений.");
                return;
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.SetCurrentEmployee(employee);
            mainWindow.Show();
            Close();
        }
    }
}