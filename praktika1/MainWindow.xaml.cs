using System.Windows;
using System.Windows.Controls;
using praktika1.Helpers;
using praktika1.Models;

namespace praktika1
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            // Для простоты не реализую замену PasswordBox на TextBox, оставлю как есть
        }

        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e) { }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите email и пароль!");
                return;
            }

            User user = null;
            int method = cmbAuthMethod.SelectedIndex;
            try
            {
                switch (method)
                {
                    case 0: user = DatabaseHelper.LoginSql(email, password); break;
                    case 1: user = StoredProcHelper.LoginStoredProc(email, password); break;
                    case 2: user = EntityFrameworkHelper.LoginEF(email, password); break;
                }

                if (user != null)
                {
                    _currentUser = user;
                    MainAppWindow mainApp = new MainAppWindow(user);
                    mainApp.Show();
                    this.Close();
                }
                else
                    MessageBox.Show("Неверный email или пароль!");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow regWin = new RegisterWindow();
            regWin.Owner = this;
            regWin.ShowDialog();
        }
    }
}