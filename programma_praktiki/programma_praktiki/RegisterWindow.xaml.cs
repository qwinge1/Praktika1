using System;
using System.Text.RegularExpressions;
using System.Windows;
using programma_praktiki.Helpers;

namespace programma_praktiki
{
    public partial class RegisterWindow : Window
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly StoredProcHelper _spHelper;
        private readonly EfCoreHelper _efHelper;

        public RegisterWindow(DatabaseHelper dbHelper, StoredProcHelper spHelper, EfCoreHelper efHelper)
        {
            InitializeComponent();
            _dbHelper = dbHelper;
            _spHelper = spHelper;
            _efHelper = efHelper;
        }

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

        private void ChkShowConfirm_Checked(object sender, RoutedEventArgs e)
        {
            txtConfirmVisible.Text = txtConfirm.Password;
            txtConfirmVisible.Visibility = Visibility.Visible;
            txtConfirm.Visibility = Visibility.Collapsed;
        }

        private void ChkShowConfirm_Unchecked(object sender, RoutedEventArgs e)
        {
            txtConfirm.Password = txtConfirmVisible.Text;
            txtConfirmVisible.Visibility = Visibility.Collapsed;
            txtConfirm.Visibility = Visibility.Visible;
        }

        private bool ValidatePassword(string password)
        {
            if (password.Length < 8) return false;
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
            return regex.IsMatch(password);
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string pass = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;
            string confirm = chkShowConfirm.IsChecked == true ? txtConfirmVisible.Text : txtConfirm.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }
            if (pass != confirm)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }
            if (!ValidatePassword(pass))
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов, включая заглавные, строчные, цифры и спецсимволы.");
                return;
            }

            bool success = false;
            int method = cmbMethod.SelectedIndex;
            try
            {
                switch (method)
                {
                    case 0:
                        success = _dbHelper.RegisterSql(email, pass);
                        break;
                    case 1:
                        success = _spHelper.RegisterStoredProc(email, pass);
                        break;
                    case 2:
                        success = _efHelper.RegisterEF(email, pass);
                        break;
                }

                if (success)
                {
                    MessageBox.Show("Регистрация успешна! Теперь войдите.");
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка регистрации (возможно, email уже занят).");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}