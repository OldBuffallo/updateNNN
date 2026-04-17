using ReportManager.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager.Models
{
    public class Employee : INotifyPropertyChanged
    {
        private static readonly string IDEmployeeProperty = "IDEmployee";
        private static readonly string StaffNameProperty = "StaffName";
        private static readonly string GenderProperty = "Gender";
        private static readonly string BirthdayProperty = "Birthday";
        private static readonly string NationalityProperty = "Nationality";
        private static readonly string PassportProperty = "Passport";
        private static readonly string AddressProperty = "Address";
        private static readonly string CareerProperty = "Career";
        private static readonly string WorkPermitProperty = "WorkPermit";
        private static readonly string WorkPermitNumberProperty = "WorkPermitNumber";
        private static readonly string WorkPermitStringProperty = "WorkPermitString";
        private static readonly string VisaNumberProperty = "VisaNumber";
        private static readonly string TemporaryStayProperty = "TemporaryStay";
        private static readonly string NoteProperty = "Note";
        private static readonly string SettlementResultsProperty = "SettlementResults";
        private static readonly string SettlementResultsStringProperty = "SettlementResultsString";
        private static readonly string IDUserProperty = "IDUser";
        private static readonly string IDCompanyProperty = "IDCompany";
        private static readonly string LineNumberProperty = "LineNumber";
        private static readonly string DateCreatedProperty = "DateCreated";
        private static readonly string CardCreationDateProperty = "CardCreationDate";
        private static readonly string WorkingStatusProperty = "WorkingStatus";
        private static readonly string DateOfJoinProperty = "DateOfJoin";
        private static readonly string DateOfLeaveProperty = "DateOfLeave";
        private static readonly string HiddenProperty = "Hidden";
        private int _idEmployee;
        private string _staffName;
        private int _gender;
        private string _birthday;
        private Nationality _nationality;
        private string _passport;
        private string _address;
        private Career _career;
        private int _workPermit;
        private string _workPermitNumber;
        private string _workPermitString;
        private string _visaNumber;
        private string _temporaryStay;
        private string _note;
        private int _settlementResults;
        private string _settlementResultsString;
        private int _idUser;
        private int _idCompany;
        private int _lineNumber;
        private string _dateCreated;
        private string _cardCreationDate;
        private int _workingStatus;
        private string _dateOfJoin;
        private string _dateOfLeave;
        private int _hidden;

        #region set get method
        public Employee()
        {
            _career = new Career();
            _nationality = new Nationality();
        }
        public Employee(Employee employee)
        {
            _idEmployee = employee.IDEmployee;
            _staffName = employee.StaffName;
            _gender = employee.Gender;
            _birthday = employee.Birthday;
            _nationality = new Nationality(employee.Nationality);
            _passport = employee.Passport;
            _address = employee.Address;
            _career = new Career(employee.Career);
            _workPermit = employee.WorkPermit;
            _workPermitNumber = employee.WorkPermitNumber;
            _workPermitString = employee.WorkPermitString;
            _visaNumber = employee.VisaNumber;
            _temporaryStay = employee.TemporaryStay;
            _note = employee.Note;
            _settlementResults = employee.SettlementResults;
            _settlementResultsString = employee.SettlementResultsString;
            _idCompany = employee.IDCompany;
            _idUser = employee.IDUser;
            _lineNumber = employee.LineNumber;
            _cardCreationDate = employee.CardCreationDate;
            _workingStatus = employee.WorkingStatus;
            _dateOfJoin = employee.DateOfJoin;
            _dateOfLeave = employee.DateOfLeave;
        }
        public int IDEmployee
        {
            get
            {
                return _idEmployee;
            }
            set
            {
                this.SetValue(ref _idEmployee, value, IDEmployeeProperty);
                OnPropertyChanged("IDEmployee");
            }
        }
        public string StaffName
        {
            get
            {
                return MethodHandler.ToTitleCase(_staffName);
            }
            set
            {
                this.SetValue(ref _staffName, value, StaffNameProperty);
                OnPropertyChanged("StaffName");
            }
        }
        public int Gender
        {
            get
            {
                return _gender;
            }
            set
            {
                this.SetValue(ref _gender, value, GenderProperty);
                OnPropertyChanged("Gender");
            }
        }
        public string GenderString
        {
            get
            {
                switch (Gender)
                {
                    case 1:
                        return "Nam";
                    case 2:
                    default:
                        return "Nữ";
                }
            }
            set
            {
                if ("Nam".Equals(value))
                {
                    Gender = 1;
                }
                else
                {
                    Gender = 2;
                }
            }
        }
        public string Birthday
        {
            get
            {
                return _birthday;
            }
            set
            {
                this.SetValue(ref _birthday, value, BirthdayProperty);
                OnPropertyChanged("Birthday");
            }
        }
        // include nationality code
        public Nationality Nationality
        {
            get
            {
                return _nationality;
            }
            set
            {
                this.SetValue(ref _nationality, value, NationalityProperty);
                OnPropertyChanged("Nationality");
            }
        }
        public string Passport
        {
            get
            {
                return String.IsNullOrEmpty(_passport) ? "" : _passport.ToUpper();
            }
            set
            {
                this.SetValue(ref _passport, value, PassportProperty);
                OnPropertyChanged("Passport");
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
        public Career Career
        {
            get
            {
                return _career;
            }
            set
            {
                this.SetValue(ref _career, value, CareerProperty);
                OnPropertyChanged("Career");
            }
        }
        public int WorkPermit
        {
            get
            {
                return _workPermit;
            }
            set
            {
                this.SetValue(ref _workPermit, value, WorkPermitProperty);
                OnPropertyChanged("WorkPermit");
            }
        }
        public string WorkPermitNumber
        {
            get
            {
                return _workPermitNumber;
            }
            set
            {
                this.SetValue(ref _workPermitNumber, value, WorkPermitNumberProperty);
                OnPropertyChanged("WorkPermitNumber");
            }
        }
        public string WorkPermitString
        {
            get
            {
                switch (_workPermit)
                {
                    case 0:
                        return "NLĐ miễn GPLĐ";
                    case 1:
                        return "NLĐ đã có GPLĐ";
                    case 2:
                        return "NLĐ chưa có GPLĐ";
                    case 3:
                        return "Nhà đầu tư miễn GPLĐ";
                    case 4:
                        return "Nhà đầu tư đã có GPLĐ";
                    case 5:
                        return "Nhà đầu tư chưa có GPLĐ";
                    case 6:
                    default:
                        return "Khác";
                    
                }
            }
            set
            {
                this.SetValue(ref _workPermitString, value, WorkPermitStringProperty);
                OnPropertyChanged("WorkPermitString");
            }
        }
        public string WorkPermitDisplay
        {
            get
            {
                switch (_workPermit)
                {
                    case 0:
                        return "NLĐ miễn GPLĐ";
                    case 1:
                        return WorkPermitNumber;
                    case 2:
                        return "NLĐ chưa có GPLĐ";
                    case 3:
                        return "Nhà đầu tư miễn GPLĐ";
                    case 4:
                        return WorkPermitNumber;
                    case 5:
                        return "Nhà đầu tư chưa có GPLĐ";
                    case 6:
                    default:
                        return "Khác";
                }
            }
        }
        public string VisaNumber
        {
            get
            {
                return String.IsNullOrEmpty(_visaNumber) ? "" : _visaNumber.ToUpper();
            }
            set
            {
                this.SetValue(ref _visaNumber, value, VisaNumberProperty);
                OnPropertyChanged("VisaNumber");
            }
        }
        public string TemporaryStay
        {
            get
            {
                return _temporaryStay;
            }
            set
            {
                this.SetValue(ref _temporaryStay, value, TemporaryStayProperty);
                OnPropertyChanged("TemporaryStay");
            }
        }
        public string Note
        {
            get
            {
                return String.IsNullOrEmpty(_note) ? "" : _note;
            }
            set
            {
                this.SetValue(ref _note, value, NoteProperty);
                OnPropertyChanged("Note");
            }
        }
        public int SettlementResults
        {
            get
            {
                return _settlementResults;
            }
            set
            {
                this.SetValue(ref _settlementResults, value, SettlementResultsProperty);
                OnPropertyChanged("SettlementResults");
            }
        }
        public string SettlementResultsString
        {
            get
            {
                return _settlementResultsString;
            }
            set
            {
                this.SetValue(ref _settlementResultsString, value, SettlementResultsStringProperty);
                OnPropertyChanged("SettlementResultsString");
            }
        }
        public string SettlementResultsDisplay
        {
            get
            {
                string str = "";
                switch (SettlementResults)
                {
                    case 0:
                        str = "GHTT:";
                        break;
                    case 1:
                        str = "Visa:";
                        break;
                    case 2:
                        str = "TTT:";
                        break;
                    case 3:
                        str = "";
                        break;
                }
                str += " " + SettlementResultsString;
                return str.Trim();
            }
        }
        public int IDUser
        {
            get
            {
                return _idUser;
            }
            set
            {
                this.SetValue(ref _idUser, value, IDUserProperty);
                OnPropertyChanged("IDUser");
            }
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
        public string DateCreated
        {
            get
            {
                return _dateCreated;
            }
            set
            {
                this.SetValue(ref _dateCreated, value, DateCreatedProperty);
                OnPropertyChanged("DateCreated");
            }
        }
        public string CardCreationDate
        {
            get
            {
                return _cardCreationDate;
            }
            set
            {
                this.SetValue(ref _cardCreationDate, value, CardCreationDateProperty);
                OnPropertyChanged("CardCreationDate");
            }
        }
        public int WorkingStatus
        {
            get
            {
                return _workingStatus;
            }
            set
            {
                this.SetValue(ref _workingStatus, value, WorkingStatusProperty);
            }
        }
        public string DateOfJoin
        {
            get
            {
                return _dateOfJoin;
            }
            set
            {
                this.SetValue(ref _dateOfJoin, value, DateOfJoinProperty);
            }
        }
        public string DateOfLeave
        {
            get
            {
                return _dateOfLeave;
            }
            set
            {
                this.SetValue(ref _dateOfLeave, value, DateOfLeaveProperty);
            }
        }
        public int Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                this.SetValue(ref _hidden, value, HiddenProperty);
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
