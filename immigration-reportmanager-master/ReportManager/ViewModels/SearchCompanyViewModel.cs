using ReportManager.Bases;
using ReportManager.Models;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class SearchCompanyViewModel : INotifyPropertyChanged
    {
        #region properties
        // search company
        public static readonly string ListCompaniesProperty = "ListCompanies";
        public static readonly string CompanyNameProperty = "CompanyName";
        public static readonly string TypeOfBusiniessProperty = "TypeOfBusiniess";
        public static readonly string MinInvestmentProperty = "MinInvestment";
        public static readonly string MaxInvestmentProperty = "MaxInvestment";
        public static readonly string IsCheckInvestmentProperty = "IsCheckInvestment";
        public static readonly string AddressProperty = "Address";
        public static readonly string ListFieldsProperty = "ListFields";
        public static readonly string SelectFieldProperty = "SelectField";
        public static readonly string DescriptionStringProperty = "DescriptionString";
        public static readonly string RegistrationNumberProperty = "RegistrationNumber";
        private ObservableCollection<Company> _listCompanies;
        private string _companyName;
        private string _typeOfBusiniess;
        private decimal _minInvestment;
        private decimal _maxInvestment;
        private bool _isCheckInvestment;
        private string _address;
        private ObservableCollection<Field> _listFields;
        private Field _selectField;
        private string _descriptionString;
        private int _registrationNumber;
        private SearchCompanyWindow _window;
        private ICommand _btnSearchCommand;
        private ICommand _btnExitCommand;
        private ICommand _loadCommand;

        // for search employee
        public static readonly string ListEmployeesProperty = "ListEmployees";
        public static readonly string StaffNameProperty = "StaffName";
        public static readonly string ListNationalitiesProperty = "ListNationalities";
        public static readonly string NationalityProperty = "Nationality";
        public static readonly string PassportProperty = "Passport";
        public static readonly string TemporaryResidenceAddressProperty = "TemporaryResidenceAddress";
        public static readonly string CareerStringProperty = "CareerString";
        public static readonly string WorkPermitProperty = "WorkPermit";
        public static readonly string StartDateProperty = "StartDate";
        public static readonly string EndDateProperty = "EndDate";
        public static readonly string ListTrackerProperty = "ListTracker";
        public static readonly string SelectTrackerProperty = "SelectTracker";
        private ObservableCollection<Employee> _listEmployees;
        private string _staffName;
        private ObservableCollection<Nationality> _listNationalities;
        private Nationality _nationality;
        private string _passport;
        private string _temporaryResidenceAddress;
        private string _careerString;
        private int _workPermit;
        private string _startDate;
        private string _endDate;
        private ObservableCollection<Account> _listTracker;
        private Account _selectTracker;
        private ReportManagerWindow _reportManagerWindow;
        private ICommand _loadDataTrackerCommamnd;
        private ICommand _btnSetNationalityCommand;
        private ICommand _btnEmployeeSearchCommand;
        #endregion

        #region set get method
        public SearchCompanyViewModel(SearchCompanyWindow window, ReportManagerWindow reportManagerWindow, ObservableCollection<Company> listCompanies)
        {
            _window = window;
            _reportManagerWindow = reportManagerWindow;
            ListCompanies = listCompanies;
            _listFields = new ObservableCollection<Field>();
            LoadCommand.Execute(true);
            _listTracker = new ObservableCollection<Account>();
            _listTracker.Add(new Account(-1, "", "Không chọn"));
            _selectTracker = new Account();
            LoadDataTrackerCommand.Execute(true);
            _selectTracker = _listTracker[0];
            // for search employee
            ListEmployees = new ObservableCollection<Employee>();
            Nationality = new Nationality();
            // get list nationalities from database
            ListNationalities = MethodHandler.getNationalities();
            WorkPermit = 7;

        }
        public ObservableCollection<Company> ListCompanies
        {
            get
            {
                return _listCompanies;
            }
            set
            {
                this.SetValue(ref _listCompanies, value, ListCompaniesProperty);
                OnPropertyChanged("ListCompanies");
            }
        }
        public string CompanyName
        {
            get
            {
                return _companyName;
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
        public decimal MinInvestment
        {
            get
            {
                return _minInvestment;
            }
            set
            {
                this.SetValue(ref _minInvestment, value, MinInvestmentProperty);
                OnPropertyChanged("MinInvestment");
            }
        }
        public decimal MaxInvestment
        {
            get
            {
                return _maxInvestment;
            }
            set
            {
                this.SetValue(ref _maxInvestment, value, MaxInvestmentProperty);
                OnPropertyChanged("MaxInvestment");
            }
        }
        public bool IsCheckInvestment
        {
            get
            {
                return _isCheckInvestment;
            }
            set
            {
                this.SetValue(ref _isCheckInvestment, value, IsCheckInvestmentProperty);
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
        public Field SelectField
        {
            get
            {
                return _selectField;
            }
            set
            {
                this.SetValue(ref _selectField, value, SelectFieldProperty);
            }
        }
        public ObservableCollection<Field> ListFields
        {
            get
            {
                return _listFields;
            }
            set
            {
                this.SetValue(ref _listFields, value, ListFieldsProperty);
            }
        }
        public string DescriptionString
        {
            get
            {
                return _descriptionString;
            }
            set
            {
                this.SetValue(ref _descriptionString, value, DescriptionStringProperty);
            }
        }
        public int RegistrationNumber
        {
            get
            {
                return _registrationNumber;
            }
            set
            {
                this.SetValue(ref _registrationNumber, value, RegistrationNumberProperty);
            }
        }
        public ObservableCollection<Account> ListTracker
        {
            get
            {
                return _listTracker;
            }
            set
            {
                this.SetValue(ref _listTracker, value, ListTrackerProperty);
            }
        }
        public Account SelectTracker
        {
            get
            {
                return _selectTracker;
            }
            set
            {
                this.SetValue(ref _selectTracker, value, SelectTrackerProperty);
            }
        }

        // for employee search
        public ObservableCollection<Employee> ListEmployees
        {
            get
            {
                return _listEmployees;
            }
            set
            {
                this.SetValue(ref _listEmployees, value, ListEmployeesProperty);
                OnPropertyChanged("ListEmployees");
            }
        }
        public string StaffName
        {
            get
            {
                return _staffName;
            }
            set
            {
                this.SetValue(ref _staffName, value, StaffNameProperty);
                OnPropertyChanged("StaffName");
            }
        }
        public ObservableCollection<Nationality> ListNationalities
        {
            get
            {
                return _listNationalities;
            }
            set
            {
                this.SetValue(ref _listNationalities, value, ListNationalitiesProperty);
            }
        }
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
                return _passport;
            }
            set
            {
                this.SetValue(ref _passport, value, PassportProperty);
                OnPropertyChanged("Passport");
            }
        }
        public string TemporaryResidenceAddress
        {
            get
            {
                return _temporaryResidenceAddress;
            }
            set
            {
                this.SetValue(ref _temporaryResidenceAddress, value, AddressProperty);
                OnPropertyChanged("TemporaryResidenceAddress");
            }
        }
        public string CareerString
        {
            get
            {
                return _careerString;
            }
            set
            {
                this.SetValue(ref _careerString, value, CareerStringProperty);
                OnPropertyChanged("CareerString");
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
                this.SetValue(ref _workPermit, value, CareerStringProperty);
                OnPropertyChanged("WorkPermit");
            }
        }
        public string StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                this.SetValue(ref _startDate, value, StartDateProperty);
                OnPropertyChanged("StartDate");
            }
        }
        public string EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                this.SetValue(ref _endDate, value, EndDateProperty);
                OnPropertyChanged("EndDate");
            }
        }
        #endregion

        #region method ICommand
        public ICommand LoadDataTrackerCommand
        {
            get
            {
                return _loadDataTrackerCommamnd ?? (_loadDataTrackerCommamnd = new CommandHandler(LoadDataTracker, true));
            }
        }
        private void LoadDataTracker()
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Accounts where Delete_flag = 0 order by Username", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Account rowAccount = new Account();
                            rowAccount.IDUser = int.Parse(dr["IDUser"].ToString());
                            rowAccount.Username = dr["Username"].ToString();
                            rowAccount.Name = dr["Name"].ToString();
                            ListTracker.Add(rowAccount);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có cán bộ theo dõi");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }

            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return _loadCommand ?? (_loadCommand = new CommandHandler(Load, true));
            }
        }
        private void Load()
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Fields", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListFields.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Field rowField = new Field();
                            rowField.IDField = int.Parse(dr["IDField"].ToString());
                            rowField.FieldName = dr["FieldName"].ToString();
                            rowField.Description = dr["Description"].ToString();
                            ListFields.Add(rowField);
                        }
                    }
                    //else
                    //{
                    //    MessageBox.Show("Không có lĩnh vực");
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }

            }
        }

        public ICommand BtnSetNationalityCommand
        {
            get
            {
                return _btnSetNationalityCommand ?? (_btnSetNationalityCommand = new CommandHandler(BtnSetNationality, true));
            }
        }
        private void BtnSetNationality()
        {
            if (string.IsNullOrWhiteSpace(Nationality.NationalityCode))
            {
                Nationality.NationalityName = "";
                return;
            }
            foreach(Nationality na in ListNationalities)
            {
                if (na.NationalityCode.Equals(Nationality.NationalityCode))
                {
                    Nationality.NationalityName = na.NationalityName;
                    return;
                }
            }
            MessageBox.Show("Mã quốc tịch chưa có trong dữ liệu hoặc đã bị xóa.\nVui lòng kiểm tra lại.");
            Nationality.NationalityName = "";
            return;
        }

        public ICommand BtnSearchCommand
        {
            get
            {
                return _btnSearchCommand ?? (_btnSearchCommand = new CommandHandler(CompanySearch, true));
            }
        }
        private void CompanySearch()
        {
            if(string.IsNullOrWhiteSpace(CompanyName) && string.IsNullOrWhiteSpace(TypeOfBusiniess)
                && string.IsNullOrWhiteSpace(Address) && SelectField == null
                && string.IsNullOrWhiteSpace(DescriptionString) && RegistrationNumber == 0
                && MinInvestment == 0 && MaxInvestment == 0 && SelectTracker.IDUser == -1 && IsCheckInvestment == false)
            {
                MessageBox.Show("Vui lòng nhập ít nhất một thông tin");
                return;
            }
            if (MinInvestment !=0 && MaxInvestment !=0 && MinInvestment > MaxInvestment)
            {
                MessageBox.Show("giá trị lớn nhất không được thấp hơn giá trị nhỏ nhất");
                return;
            }
            // set sql query
            string sqlSearch = "select * from Companies, Fields, Accounts where Companies.Delete_flag = 0 and Companies.IDField = Fields.IDField and Companies.TrackerID = Accounts.IDUser ";
            if (!string.IsNullOrWhiteSpace(CompanyName))
            {
                sqlSearch += " and CompanyName like N'%" + MethodHandler.convertStringOwned(CompanyName) + "%' ";
            }
            if (!string.IsNullOrWhiteSpace(TypeOfBusiniess))
            {
                sqlSearch += " and TypeOfBusiniess like N'%" + MethodHandler.convertStringOwned(TypeOfBusiniess) + "%' ";
            }
            if (!string.IsNullOrWhiteSpace(Address))
            {
                sqlSearch += " and Address like N'%" + MethodHandler.convertStringOwned(Address) + "%' ";
            }
            if (SelectField != null)
            {
                sqlSearch += " and FieldName like N'%"+SelectField.FieldName+"%' ";
            }
            if (!string.IsNullOrWhiteSpace(DescriptionString))
            {
                sqlSearch += " and DescriptionOfActivities like N'%" + MethodHandler.convertStringOwned(DescriptionString) + "%' ";
            }
            if (RegistrationNumber != 0)
            {
                switch (RegistrationNumber)
                {
                    case 1:
                        sqlSearch += " and RegistrationProfile like N'%chưa%' ";
                        break;
                    case 2:
                        sqlSearch += " and RegistrationProfile not like N'%chưa%' ";
                        break;
                }
            }
            if (MinInvestment !=0 || MaxInvestment != 0)
            {
                if(MinInvestment != 0)
                {
                    sqlSearch += "and IDCompany in (select Investment.IDCompany from Investment where Investment.AmountOfMoney >= "+ MinInvestment;
                    if(MaxInvestment != 0)
                    {
                        sqlSearch += " and Investment.AmountOfMoney<= "+MaxInvestment;
                    }
                    // check investment
                    if (IsCheckInvestment)
                    {
                        sqlSearch += " union select IDCompany from Companies where IDCompany not in (select IDCompany from Investment)";
                    }
                    sqlSearch += ")";
                } else
                {
                    sqlSearch += " and IDCompany in (select Investment.IDCompany from Investment where Investment.AmountOfMoney <= " + MaxInvestment;
                    // check investment
                    if (IsCheckInvestment)
                    {
                        sqlSearch += " union select IDCompany from Companies where IDCompany not in (select IDCompany from Investment)";
                    }
                    sqlSearch += ")";
                }
            } else
            {
                // MinInvestment ==0 && MaxInvestment == 0
                // check investment
                if (IsCheckInvestment)
                {
                    sqlSearch += " and IDCompany not in (select IDCompany from Investment)";
                }
            }
            if(SelectTracker.IDUser != -1)
            {
                sqlSearch += " and Companies.TrackerID = " + SelectTracker.IDUser;
            }
            // order by
            sqlSearch += " order by FieldName, CompanyName";
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(sqlSearch, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListCompanies.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Company rowCompany = new Company();
                            rowCompany.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowCompany.CompanyName = dr["CompanyName"].ToString();
                            rowCompany.TypeOfBusiniess = dr["TypeOfBusiniess"].ToString();
                            rowCompany.Uptime = dr["Uptime"].ToString();
                            rowCompany.Address = dr["Address"].ToString();
                            rowCompany.Field.IDField = int.Parse(dr["IDField"].ToString());
                            rowCompany.Field.FieldName = dr["FieldName"].ToString();
                            rowCompany.Field.Description = dr["Description"].ToString();
                            rowCompany.LegalRepresentative = dr["LegalRepresentative"].ToString();
                            //rowCompany.PhoneNumber = dr["PhoneNumber"].ToString();
                            //rowCompany.Email = dr["Email"].ToString();
                            rowCompany.TotalAmount = int.Parse(dr["TotalAmount"].ToString());
                            rowCompany.AmountOfExemption = int.Parse(dr["AmountOfExemption"].ToString());
                            rowCompany.QuantityAvailable = int.Parse(dr["QuantityAvailable"].ToString());
                            rowCompany.QuantityNotYet = int.Parse(dr["QuantityNotYet"].ToString());
                            rowCompany.NumberOfPersonalities = int.Parse(dr["NumberOfPersonalities"].ToString());
                            rowCompany.RegistrationProfile = dr["RegistrationProfile"].ToString();
                            rowCompany.DescriptionOfActivities = dr["DescriptionOfActivities"].ToString().Trim();
                            rowCompany.TrackerID = int.Parse(dr["TrackerID"].ToString());
                            rowCompany.Tracker = dr["Name"].ToString().Trim();
                            rowCompany.Note = dr["Note"].ToString();
                            rowCompany.LineNumber = ++lineNumber;
                            // get investment
                            DataTable dtGet = new DataTable();
                            adapter.SelectCommand = new SqlCommand("select * from Investment where IDCompany = " + rowCompany.IDCompany, con);
                            adapter.Fill(dtGet);
                            if (dtGet.Rows.Count > 0)
                            {
                                string investmentString = "";
                                foreach (DataRow row in dtGet.Rows)
                                {
                                    Investment rowInvestment = new Investment();
                                    rowInvestment.Name = row["Name"].ToString();
                                    rowInvestment.Nationality = row["Nationality"].ToString();
                                    rowInvestment.AmountOfMoney = decimal.Parse(row["AmountOfMoney"].ToString());
                                    investmentString += rowInvestment.ToString() + "\n";
                                }
                                rowCompany.Investment = investmentString;
                            }
                            //get phone number
                            adapter.SelectCommand = new SqlCommand("select * from PhoneNumbers where IDCompany = " + rowCompany.IDCompany, con);
                            dtGet = new DataTable();
                            adapter.Fill(dtGet);
                            if (dtGet.Rows.Count > 0)
                            {
                                string phoneString = "";
                                foreach (DataRow row in dtGet.Rows)
                                {
                                    PhoneNumber rowPhoneNumber = new PhoneNumber();
                                    rowPhoneNumber.Name = row["Name"].ToString();
                                    rowPhoneNumber.Phone = row["Phone"].ToString();
                                    phoneString += rowPhoneNumber.ToString() + "\n";
                                }
                                rowCompany.PhoneNumber = phoneString;
                            }
                            //get email
                            adapter.SelectCommand = new SqlCommand("select * from Emails where IDCompany = " + rowCompany.IDCompany, con);
                            dtGet = new DataTable();
                            adapter.Fill(dtGet);
                            if (dtGet.Rows.Count > 0)
                            {
                                string emailString = "";
                                foreach (DataRow row in dtGet.Rows)
                                {
                                    Email rowEmail = new Email();
                                    rowEmail.Name = row["Name"].ToString();
                                    rowEmail.Mail = row["Mail"].ToString();
                                    emailString += rowEmail.ToString() + "\n";
                                }
                                rowCompany.Email = emailString;
                            }

                            ListCompanies.Add(rowCompany);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có công ty phù hợp với thông tin tìm kiếm");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }

            }
            _window.Close();
        }

        public ICommand BtnEmployeeSearchCommand
        {
            get
            {
                return _btnEmployeeSearchCommand ?? (_btnEmployeeSearchCommand = new CommandHandler(EmployeeSearch, true));
            }
        }
        private void EmployeeSearch()
        {
            if (string.IsNullOrWhiteSpace(StaffName) && string.IsNullOrWhiteSpace(Nationality.NationalityCode)
                && string.IsNullOrWhiteSpace(Passport) && string.IsNullOrWhiteSpace(TemporaryResidenceAddress)
                && string.IsNullOrWhiteSpace(CareerString) && string.IsNullOrWhiteSpace(StartDate)
                && string.IsNullOrWhiteSpace(EndDate) && WorkPermit == 7)
            {
                MessageBox.Show("Vui lòng nhập ít nhất một thông tin");
                return;
            }
            if (!string.IsNullOrWhiteSpace(StartDate) && !MethodHandler.checkFormatDate(StartDate))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            if (!string.IsNullOrWhiteSpace(EndDate) && !MethodHandler.checkFormatDate(EndDate))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            // check nationality code
            if ((!string.IsNullOrWhiteSpace(Nationality.NationalityCode)) && (string.IsNullOrWhiteSpace(Nationality.NationalityName)))
            {
                MessageBox.Show("Vui lòng kiểm tra lại mã quốc tịch");
                return;
            }
            // set sql query
            string sqlString = "select CONVERT(varchar, Birthday, 105) birth, CONVERT(varchar, TemporaryStay, 105) temporary, IDEmployee, StaffName"
                + ", Gender, Nationality, Passport, Employees.Address temporaryResidenceAddress, IDCareer, WorkPermit, WorkPermitNumber, VisaNumber, SettlementResults"
                + ", SettlementResultsString, IDUser, Employees.IDCompany idCompany, Employees.Note note, CompanyName, NationalityCode, NationalityName, Employees.Hidden_flag hidden"
                + " from Employees, Companies, Nationality where Employees.Nationality like Nationality.NationalityCode"
                + " and Employees.IDCompany = Companies.IDCompany and Employees.Hidden_flag = 0 and Companies.Delete_flag = 0";
            if (!string.IsNullOrWhiteSpace(StaffName))
            {
                sqlString += " and Employees.StaffName like N'%" + MethodHandler.convertStringOwned(StaffName) + "%'";
            }
            // condition nationality code in employees table
            if (!string.IsNullOrWhiteSpace(Nationality.NationalityName))
            {
                sqlString += " and Employees.Nationality like N'%" + MethodHandler.convertStringOwned(Nationality.NationalityCode) + "%'";
            }
            if (!string.IsNullOrWhiteSpace(Passport))
            {
                sqlString += " and Employees.Passport like N'%" + Passport + "%'";
            }
            if (!string.IsNullOrWhiteSpace(Address))
            {
                sqlString += " and Employees.Address like N'%" + MethodHandler.convertStringOwned(Address) + "%'";
            }
            if (!string.IsNullOrWhiteSpace(CareerString))
            {
                sqlString += " and Employees.IDCareer in (select IDCareer from Careers where CareerName like N'%" + MethodHandler.convertStringOwned(CareerString) + "%')";
            }
            if(WorkPermit < 7 && WorkPermit >= 0)
            {
                sqlString += " and Employees.WorkPermit = " + WorkPermit;
            }
            if (!string.IsNullOrWhiteSpace(StartDate))
            {
                sqlString += " and Employees.TemporaryStay >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'";
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                sqlString += " and Employees.TemporaryStay <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
            }
            // order by
            sqlString += " order by CompanyName, IDCareer, temporaryResidenceAddress, StaffName";
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(sqlString, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListEmployees.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Employee rowEmployee = new Employee();
                            rowEmployee.IDEmployee = int.Parse(dr["IDEmployee"].ToString());
                            rowEmployee.StaffName = dr["StaffName"].ToString();
                            rowEmployee.Gender = int.Parse(dr["Gender"].ToString());
                            rowEmployee.Birthday = dr["birth"].ToString();
                            rowEmployee.Nationality.NationalityCode = dr["NationalityCode"].ToString();
                            rowEmployee.Nationality.NationalityName = dr["NationalityName"].ToString();
                            rowEmployee.Passport = dr["Passport"].ToString();
                            rowEmployee.Address = dr["temporaryResidenceAddress"].ToString();
                            rowEmployee.Career.IDCareer = int.Parse(dr["IDCareer"].ToString());
                            rowEmployee.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployee.WorkPermitNumber = dr["WorkPermitNumber"].ToString();
                            rowEmployee.VisaNumber = dr["VisaNumber"].ToString();
                            rowEmployee.TemporaryStay = dr["temporary"].ToString();
                            rowEmployee.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                            rowEmployee.SettlementResultsString = dr["SettlementResultsString"].ToString();
                            rowEmployee.IDUser = int.Parse(dr["IDUser"].ToString());
                            rowEmployee.IDCompany = int.Parse(dr["idCompany"].ToString());
                            rowEmployee.Note = dr["note"].ToString();
                            rowEmployee.Hidden = int.Parse(dr["hidden"].ToString());
                            // save company name
                            rowEmployee.DateCreated = dr["CompanyName"].ToString();
                            // get career
                            DataTable dtGet = new DataTable();
                            adapter.SelectCommand = new SqlCommand("select CareerName, IDCG from Careers where IDCareer = " + rowEmployee.Career.IDCareer, con);
                            adapter.Fill(dtGet);
                            if (dtGet.Rows.Count > 0)
                            {
                                foreach (DataRow row in dtGet.Rows)
                                {
                                    rowEmployee.Career.CareerName = row["CareerName"].ToString();
                                    rowEmployee.Career.IDCG = int.Parse(row["IDCG"].ToString());
                                }
                            }
                            //CountSum(rowEmployee.SettlementResults);
                            rowEmployee.LineNumber = ++lineNumber;
                            ListEmployees.Add(rowEmployee);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có nhân viên");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }

            }
            // show result
            if(ListEmployees.Count > 0)
            {
                _reportManagerWindow.ContentArea.Content = new SearchResultsUC(_reportManagerWindow, ListEmployees);
            }
            _window.Close();
        }

        public ICommand BtnExitCommand
        {
            get
            {
                return _btnExitCommand ?? (_btnExitCommand = new CommandHandler(Exit, true));
            }
        }
        private void Exit()
        {
            _window.Close();
        }
        #endregion

        #region Protected Methods
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
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
