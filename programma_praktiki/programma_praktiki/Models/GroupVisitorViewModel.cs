using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace programma_praktiki.Models
{
    public class GroupVisitorViewModel : INotifyPropertyChanged
    {
        private int _lineNumber;
        private string _lastName = "";
        private string _firstName = "";
        private string? _middleName;
        private string? _phone;
        private string _email = "";
        private string? _organization;
        private string _note = "";
        private DateTime _birthDate;
        private string _passportSeries = "";
        private string _passportNumber = "";
        private string? _photoPath;
        private string? _scanPath;

        public int LineNumber { get => _lineNumber; set { _lineNumber = value; OnPropertyChanged(); } }
        public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(); } }
        public string FirstName { get => _firstName; set { _firstName = value; OnPropertyChanged(); } }
        public string? MiddleName { get => _middleName; set { _middleName = value; OnPropertyChanged(); } }
        public string? Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string? Organization { get => _organization; set { _organization = value; OnPropertyChanged(); } }
        public string Note { get => _note; set { _note = value; OnPropertyChanged(); } }
        public DateTime BirthDate { get => _birthDate; set { _birthDate = value; OnPropertyChanged(); } }
        public string PassportSeries { get => _passportSeries; set { _passportSeries = value; OnPropertyChanged(); } }
        public string PassportNumber { get => _passportNumber; set { _passportNumber = value; OnPropertyChanged(); } }
        public string? PhotoPath { get => _photoPath; set { _photoPath = value; OnPropertyChanged(); } }
        public string? ScanPath { get => _scanPath; set { _scanPath = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
