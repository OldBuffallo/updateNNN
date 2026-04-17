using ReportManager.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager.Models
{
    public class Company : INotifyPropertyChanged
    {
        private static readonly string IDCompanyProperty = "IDCompany";
        private static readonly string CompanyNameProperty = "CompanyName";
        private static readonly string TypeOfBusiniessProperty = "TypeOfBusiniess";
        private static readonly string InvestmentProperty = "Investment";
        private static readonly string UptimeProperty = "Uptime";
        private static readonly string AddressProperty = "Address";
        private static readonly string FieldProperty = "Field";
        private static readonly string LegalRepresentativeProperty = "LegalRepresentative";
        private static readonly string ListPhoneNumberProperty = "ListPhoneNumber";
        private static readonly string PhoneNumberProperty = "PhoneNumber";
        private static readonly string ListEmailProperty = "ListEmail";
        private static readonly string EmailProperty = "Email";
        private static readonly string TotalAmountProperty = "TotalAmount";
        private static readonly string AmountOfExemptionProperty = "AmountOfExemption";
        private static readonly string QuantityAvailableProperty = "QuantityAvailable";
        private static readonly string QuantityNotYetProperty = "QuantityNotYet";
        private static readonly string NumberOfPersonalitiesProperty = "NumberOfPersonalities";
        private static readonly string RegistrationProfileProperty = "RegistrationProfile";
        private static readonly string RegistrationProfileIndexProperty = "RegistrationProfileIndex";
        private static readonly string DescriptionOfActivitiesProperty = "DescriptionOfActivities";
        private static readonly string TrackerProperty = "Tracker";
        private static readonly string TrackerIDProperty = "TrackerID";
        private static readonly string NoteProperty = "Note";
        private static readonly string UpdateDayProperty = "UpdateDay";
        private static readonly string LineNumberProperty = "LineNumber";
        private int _idCompany;
        private string _companyName;
        private string _typeOfBusiniess;
        private string _investment;
        private string _uptime;
        private string _address;
        private Field _field;
        private string _legalRepresentative;
        private ObservableCollection<PhoneNumber> _listPhoneNumber;
        private string _phoneNumber;
        private ObservableCollection<Email> _listEmail;
        private string _email;
        private int _totalAmount;
        private int _amountOfExemption;
        private int _quantityAvailable;
        private int _quantityNotYet;
        private int _numberOfPersonalities;
        private string _registrationProfile;
        private int _registrationProfileIndex;
        private string _descriptionOfActivities;
        private string _tracker;
        private int _trackerId; // userId of creator
        private string _note;
        private string _updateDay;
        private int _lineNumber;

        #region set get method
        public Company()
        {
            _field = new Field();
            ListPhoneNumber = new ObservableCollection<PhoneNumber>();
            ListEmail = new ObservableCollection<Email>();
        }
        public Company(Company company)
        {
            _idCompany = company.IDCompany;
            _companyName = company.CompanyName;
            _typeOfBusiniess = company.TypeOfBusiniess;
            _investment = company.Investment;
            _uptime = company.Uptime;
            _address = company.Address;
            _field = company.Field;
            _legalRepresentative = company.LegalRepresentative;
            ListPhoneNumber = new ObservableCollection<PhoneNumber>();
            foreach (PhoneNumber item in company.ListPhoneNumber)
            {
                ListPhoneNumber.Add(new PhoneNumber(item));
            }
            _phoneNumber = company.PhoneNumber;
            ListEmail = new ObservableCollection<Email>();
            foreach(Email item in company.ListEmail)
            {
                ListEmail.Add(new Email(item));
            }
            _email = company.Email;
            _totalAmount = company.TotalAmount;
            _amountOfExemption = company.AmountOfExemption;
            _quantityAvailable = company.QuantityAvailable;
            _quantityNotYet = company.QuantityNotYet;
            _numberOfPersonalities = company.NumberOfPersonalities;
            _registrationProfile = company.RegistrationProfile;
            _registrationProfileIndex = company.RegistrationProfileIndex;
            _descriptionOfActivities = company.DescriptionOfActivities;
            _tracker = company.Tracker;
            _trackerId = company.TrackerID;
            _note = company.Note;
            _updateDay = company.UpdateDay;
            _lineNumber = company.LineNumber;
        }
        public int IDCompany
        {
            get
            {
                return _idCompany;
            }
            set
            {
                this.SetValue(ref _idCompany, value, IDCompanyProperty);
                OnPropertyChanged("IDCompany");
            }
        }
        public string CompanyName
        {
            get
            {
                return string.IsNullOrWhiteSpace(_companyName) ? "" : _companyName.ToUpper();
            }
            set
            {
                this.SetValue(ref _companyName, value, CompanyNameProperty);
                OnPropertyChanged("CompanyName");
            }
        }
        public string TypeOfBusiniess
        {
            get
            {
                return _typeOfBusiniess;
            }
            set
            {
                this.SetValue(ref _typeOfBusiniess, value, TypeOfBusiniessProperty);
                OnPropertyChanged("TypeOfBusiniess");
            }
        }
        public string Investment
        {
            get
            {
                return _investment;
            }
            set
            {
                this.SetValue(ref _investment, value, InvestmentProperty);
                OnPropertyChanged("Investment");
            }
        }
        public string Uptime
        {
            get
            {
                return _uptime;
            }
            set
            {
                this.SetValue(ref _uptime, value, UptimeProperty);
                OnPropertyChanged("Uptime");
            }
        }
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                this.SetValue(ref _address, value, AddressProperty);
                OnPropertyChanged("Address");
            }
        }
        public Field Field
        {
            get
            {
                return _field;
            }
            set
            {
                this.SetValue(ref _field, value, FieldProperty);
                OnPropertyChanged("Field");
            }
        }
        public string LegalRepresentative
        {
            get
            {
                return MethodHandler.ToTitleCase(_legalRepresentative);
            }
            set
            {
                this.SetValue(ref _legalRepresentative, value, LegalRepresentativeProperty);
                OnPropertyChanged("LegalRepresentative");
            }
        }
        public ObservableCollection<PhoneNumber> ListPhoneNumber
        {
            get
            {
                return _listPhoneNumber;
            }
            set
            {
                this.SetValue(ref _listPhoneNumber, value, ListPhoneNumberProperty);
            }
        }
        public string PhoneNumber
        {
            get
            {
                return _phoneNumber;
            }
            set
            {
                this.SetValue(ref _phoneNumber, value, PhoneNumberProperty);
            }
        }
        public ObservableCollection<Email> ListEmail
        {
            get
            {
                return _listEmail;
            }
            set
            {
                this.SetValue(ref _listEmail, value, ListEmailProperty);
            }
        }
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                this.SetValue(ref _email, value, EmailProperty);
            }
        }
        public int TotalAmount
        {
            get
            {
                return _totalAmount;
            }
            set
            {
                this.SetValue(ref _totalAmount, value, TotalAmountProperty);
                OnPropertyChanged("TotalAmount");
            }
        }
        public int AmountOfExemption
        {
            get
            {
                return _amountOfExemption;
            }
            set
            {
                this.SetValue(ref _amountOfExemption, value, AmountOfExemptionProperty);
                OnPropertyChanged("AmountOfExemption");
            }
        }
        public int QuantityAvailable
        {
            get
            {
                return _quantityAvailable;
            }
            set
            {
                this.SetValue(ref _quantityAvailable, value, QuantityAvailableProperty);
                OnPropertyChanged("QuantityAvailable");
            }
        }
        public int QuantityNotYet
        {
            get
            {
                return _quantityNotYet;
            }
            set
            {
                this.SetValue(ref _quantityNotYet, value, QuantityNotYetProperty);
                OnPropertyChanged("QuantityNotYet");
            }
        }
        public int NumberOfPersonalities
        {
            get
            {
                return _numberOfPersonalities;
            }
            set
            {
                this.SetValue(ref _numberOfPersonalities, value, NumberOfPersonalitiesProperty);
                OnPropertyChanged("NumberOfPersonalities");
            }
        }
        public string RegistrationProfile
        {
            get
            {
                if (RegistrationProfileIndex == 0)
                {
                    return "không";
                }
                return _registrationProfile;
            }
            set
            {
                this.SetValue(ref _registrationProfile, value, RegistrationProfileProperty);
                OnPropertyChanged("RegistrationProfile");

                if (_registrationProfile.Contains("chưa") || _registrationProfile.Contains("không"))
                {
                    RegistrationProfileIndex = 0;
                } else
                {
                    RegistrationProfileIndex = 1;
                }
            }
        }

        public int RegistrationProfileIndex
        {
            get
            {
                return _registrationProfileIndex;
            }
            set
            {
                this.SetValue(ref _registrationProfileIndex, value, RegistrationProfileProperty);
                OnPropertyChanged("RegistrationProfileIndex");
            }
        }
        public string DescriptionOfActivities
        {
            get
            {
                return _descriptionOfActivities;
            }
            set
            {
                this.SetValue(ref _descriptionOfActivities, value, DescriptionOfActivitiesProperty);
                OnPropertyChanged("DescriptionOfActivities");
            }
        }
        public string Tracker
        {
            get
            {
                return _tracker;
            }
            set
            {
                this.SetValue(ref _tracker, value, TrackerProperty);
                OnPropertyChanged("Tracker");
            }
        }
        public int TrackerID
        {
            get
            {
                return _trackerId;
            }
            set
            {
                this.SetValue(ref _trackerId, value, TrackerIDProperty);
                OnPropertyChanged("TrackerID");
            }
        }
        public string Note
        {
            get
            {
                return _note;
            }
            set
            {
                this.SetValue(ref _note, value, NoteProperty);
                OnPropertyChanged("Note");
            }
        }
        public string UpdateDay
        {
            get
            {
                return _updateDay;
            }
            set
            {
                this.SetValue(ref _updateDay, value, UpdateDayProperty);
                OnPropertyChanged("UpdateDay");
            }
        }
        public int LineNumber
        {
            get
            {
                return _lineNumber;
            }
            set
            {
                this.SetValue(ref _lineNumber, value, LineNumberProperty);
                OnPropertyChanged("LineNumber");
            }
        }
        #endregion

        #region Protected Methods
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetValue<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
