using System.Text.RegularExpressions;
using System.Windows;
using praktika1.Helpers;

namespace praktika1
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
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
            string pass = txtPassword.Password;
            string confirm = txtConfirm.Password;

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
                    case 0: success = DatabaseHelper.RegisterSql(email, pass); break;
                    case 1: success = StoredProcHelper.RegisterStoredProc(email, pass); break;
                    case 2: success = EntityFrameworkHelper.RegisterEF(email, pass); break;
                }

                if (success)
                {
                    MessageBox.Show("Регистрация успешна! Теперь войдите.");
                    this.Close();
                }
                else
                    MessageBox.Show("Ошибка регистрации (возможно, email уже занят).");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}