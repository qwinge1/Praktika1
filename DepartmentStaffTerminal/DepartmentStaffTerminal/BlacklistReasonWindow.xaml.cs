using System.Windows;

namespace DepartmentStaffTerminal.Views
{
    public partial class BlacklistReasonWindow : Window
    {
        public string Reason { get; private set; } = "";

        public BlacklistReasonWindow()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Reason = txtReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("Введите причину.");
                return;
            }
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}