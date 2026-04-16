using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using programma_praktiki.Helpers;
using programma_praktiki.Models;   // ← оставляем, но ниже используем полное имя

namespace programma_praktiki
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly StoredProcHelper _spHelper;
        private readonly EfCoreHelper _efHelper;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(DatabaseHelper dbHelper, StoredProcHelper spHelper, EfCoreHelper efHelper, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _dbHelper = dbHelper;
            _spHelper = spHelper;
            _efHelper = efHelper;
            _serviceProvider = serviceProvider;

            if (!_dbHelper.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте строку подключения в appsettings.json и доступность сервера PostgreSQL.",
                                "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();   // ← ЯВНОЕ УКАЗАНИЕ
            }
        }

        // Остальные методы без изменений
        private void ChkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            txtPassword.Visibility = Visibility.Collapsed;
        }

        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите email и пароль!");
                return;
            }

            User? user = null;
            int method = cmbAuthMethod.SelectedIndex;
            try
            {
                switch (method)
                {
                    case 0:
                        user = _dbHelper.LoginSql(email, password);
                        break;
                    case 1:
                        user = _spHelper.LoginStoredProc(email, password);
                        break;
                    case 2:
                        user = _efHelper.LoginEF(email, password);
                        break;
                }

                if (user != null)
                {
                    var mainApp = _serviceProvider.GetRequiredService<MainAppWindow>();
                    mainApp.SetCurrentUser(user);
                    mainApp.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный email или пароль!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var regWin = _serviceProvider.GetRequiredService<RegisterWindow>();
            regWin.Owner = this;
            regWin.ShowDialog();
        }
    }
}