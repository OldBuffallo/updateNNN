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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class AddCompanyViewModel : INotifyPropertyChanged
    {
        #region properties
        public static readonly string AddComapanyProperty = "AddComapany";
        //public static readonly string DistrictProperty = "District";
        //public static readonly string SelectDistrictProperty = "SelectDistrict";
        public static readonly string WardProperty = "Ward";
        public static readonly string SelectWardProperty = "SelectWard";
        public static readonly string InputInvestmentProperty = "InputInvestment";
        public static readonly string InputNationalityProperty = "InputNationality";
        public static readonly string ListInvestmentProperty = "ListInvestment";
        public static readonly string SelectInvestmentProperty = "SelectInvestment";
        public static readonly string ListFieldsProperty = "ListFields";
        public static readonly string SelectFieldProperty = "SelectField";
        public static readonly string ListTrackerProperty = "ListTracker";
        public static readonly string SelectTrackerProperty = "SelectTracker";
        public static readonly string ListnationalitiesProperty = "Listnationalities";
        public static readonly string ListEmailsProperty = "ListEmails";
        public static readonly string SelectEmailProperty = "SelectEmail";
        public static readonly string ListPhoneNumbersProperty = "ListPhoneNumbers";
        public static readonly string SelectPhoneNumberProperty = "SelectPhoneNumber";
        private Company _addComapany;
        //private ObservableCollection<string> _district;
        //private string _selectDistrict;
        private ObservableCollection<string> _ward;
        private string _selectWard;
        private Investment _inputInvestment;
        // set nationality object in input investment
        private Nationality _inputNationality;
        private ObservableCollection<Investment> _listInvestment;
        private Investment _selectInvestment;
        private ObservableCollection<Investment> _listUpdateInvestment;
        private ObservableCollection<Field> _listFields;
        private Field _selectField;
        private ObservableCollection<Account> _listTracker;
        private Account _selectTracker;
        private ObservableCollection<Nationality> _listnationalities;
        private ObservableCollection<Email> _listEmails;
        private Email _selectEmail;
        private ObservableCollection<Email> _listUpdateEmails;
        private ObservableCollection<PhoneNumber> _listPhoneNumbers;
        private PhoneNumber _selectPhoneNumber;
        private ObservableCollection<PhoneNumber> _listUpdatePhoneNumbers;
        // info check exist company when update in database
        private string oldCompanyName;
        private string oldAddress;
        private string oldLegalRepresentative;
        private ReportManagerWindow _window;
        private ICommand _addInvestmentCommand;
        private ICommand _btnAttachCommand;
        private ICommand _btnAddEmailCommand;
        private ICommand _btnAddPhoneNumberCommand;
        private ICommand _btnExitCommand;
        private ICommand _btnAddCompanyCommand;
        private ICommand _loadDataFieldsCommamnd;
        private ICommand _loadDataTrackerCommamnd;
        private ICommand _loadDataInvestmentCommand;
        private ICommand _loadDataPhoneNumberCommand;
        private ICommand _loadDataEmailCommand;
        private ICommand _btnUpdateCompanyCommand;
        private ICommand _btnEditEmailCommand;
        private ICommand _btnEditPhoneNumberCommand;
        private ICommand _btnEditInvestmentCommand;
        private ICommand _deletePhoneNumberCommand;
        private ICommand _deleteEmailCommand;
        private ICommand _deleteInvestmentCommand;
        private ICommand _btnDeleteCompanyCommand;
        //icommand for export from report manager window
        private ICommand _exportFileCompanyCommand;
        private bool _isEdit;
        private UserControl _userControl;
        #endregion

        #region set get method
        public AddCompanyViewModel(ReportManagerWindow window, UserControl userControl)
        {
            _isEdit = false;
            _window = window;
            _userControl = userControl;
            _addComapany = new Company();
            //_district = MethodHandler.getDistricts();
            //_selectDistrict = _district[0];
            _ward = MethodHandler.getWards();
            _selectWard= _ward[0];
            _inputInvestment = new Investment();
            _listInvestment = new ObservableCollection<Investment>();
            _listUpdateInvestment = new ObservableCollection<Investment>();
            _selectField = new Field();
            _selectTracker = new Account();
            _listnationalities = MethodHandler.getNationalities();
            _listnationalities.Insert(0, new Nationality(0, "", ""));
            if (_listnationalities != null && _listnationalities.Count > 0)
            {
                InputNationality = _listnationalities[0];
            }
            // load list field
            _listFields = new ObservableCollection<Field>();
            LoadDataFieldsCommand.Execute(true);
            _selectField = _listFields[0];
            // load list account
            _listTracker = new ObservableCollection<Account>();
            LoadDataTrackerCommand.Execute(true);
            _selectTracker = _listTracker[0];
            _listEmails = new ObservableCollection<Email>();
            _listPhoneNumbers = new ObservableCollection<PhoneNumber>();
            _listUpdateEmails = new ObservableCollection<Email>();
            _listUpdatePhoneNumbers = new ObservableCollection<PhoneNumber>();
            //set command for export file
            _window.exportFileCompany.Command = ExportFileCompanyCommand;
        }
        public AddCompanyViewModel(ReportManagerWindow window, Company selectCompany, UserControl userControl)
        {
            _isEdit = true;
            _window = window;
            _userControl= userControl;
            _addComapany = new Company(selectCompany);
            //_district = MethodHandler.getDistricts();
            //foreach(string dis in _district)
            //{
            //    if (selectCompany.Address.ToUpper().IndexOf(dis.ToUpper()) >= 0)
            //    {
            //        _selectDistrict = dis;
            //        string strCut = ", " + dis;
            //        AddComapany.Address = AddComapany.Address.Substring(0, AddComapany.Address.Length - strCut.Length).Trim();
            //        break;
            //    }
            //}
            _ward = MethodHandler.getWards();
            foreach (string war in _ward)
            {
                if (selectCompany.Address.ToUpper().EndsWith(war.ToUpper()))
                {
                    _selectWard = war;
                    string strCut = ", " + war;
                    AddComapany.Address = AddComapany.Address.Substring(0, AddComapany.Address.Length - strCut.Length).Trim();
                    break;
                }
            }
            _inputInvestment = new Investment();
            _listInvestment = new ObservableCollection<Investment>();
            LoadDataInvestmentCommand.Execute(true);
            _selectField = new Field();
            _selectTracker = new Account();
            _listnationalities = MethodHandler.getNationalities();
            _listnationalities.Insert(0, new Nationality(0, "", ""));
            if (_listnationalities != null && _listnationalities.Count > 0)
            {
                InputNationality = _listnationalities[0];
            }
            // load list field
            _listFields = new ObservableCollection<Field>();
            LoadDataFieldsCommand.Execute(true);
            foreach(Field field in _listFields)
            {
                if(field.IDField == selectCompany.Field.IDField)
                {
                    _selectField = field;
                    break;
                }
            }
            // load list tracker
            _listTracker = new ObservableCollection<Account>();
            LoadDataTrackerCommand.Execute(true);
            foreach(Account ac in _listTracker)
            {
                if(ac.IDUser == selectCompany.TrackerID)
                {
                    _selectTracker = ac;
                }
            }
            _listEmails = new ObservableCollection<Email>();
            LoadDataEmailCommand.Execute(true);
            _listPhoneNumbers = new ObservableCollection<PhoneNumber>();
            LoadDataPhoneNumberCommand.Execute(true);
            _listUpdateInvestment = new ObservableCollection<Investment>();
            _listUpdateEmails = new ObservableCollection<Email>();
            _listUpdatePhoneNumbers = new ObservableCollection<PhoneNumber>();
            // save info old company
            oldCompanyName = selectCompany.CompanyName;
            oldAddress = selectCompany.Address;
            oldLegalRepresentative = selectCompany.LegalRepresentative;
            //set command for export file
            _window.exportFileCompany.Command = ExportFileCompanyCommand;
        }
        public Company AddComapany
        {
            get
            {
                return _addComapany;
            }
            set
            {
                this.SetValue(ref _addComapany, value, AddComapanyProperty);
            }
        }
        //public ObservableCollection<string> District
        //{
        //    get
        //    {
        //        return _district;
        //    }
        //    set
        //    {
        //        this.SetValue(ref _district, value, DistrictProperty);
        //    }
        //}
        //public string SelectDistrict
        //{
        //    get
        //    {
        //        return _selectDistrict;
        //    }
        //    set
        //    {
        //        this.SetValue(ref _selectDistrict, value, SelectDistrictProperty);
        //    }
        //}
        public ObservableCollection<string> Ward
        {
            get
            {
                return _ward;
            }
            set
            {
                this.SetValue(ref _ward, value, WardProperty);
            }
        }
        public string SelectWard
        {
            get
            {
                return _selectWard;
            }
            set
            {
                this.SetValue(ref _selectWard, value, SelectWardProperty);
            }
        }

        public Investment InputInvestment
        {
            get
            {
                return _inputInvestment;
            }
            set
            {
                this.SetValue(ref _inputInvestment, value, InputInvestmentProperty);
            }
        }
        public Nationality InputNationality
        {
            get
            {
                return _inputNationality;
            }
            set
            {
                this.SetValue(ref _inputNationality, value, InputNationalityProperty);
                InputInvestment.Nationality = _inputNationality.NationalityName;
            }
        }
        public ObservableCollection<Investment> ListInvestment
        {
            get
            {
                return _listInvestment;
            }
            set
            {
                this.SetValue(ref _listInvestment, value, ListInvestmentProperty);
            }
        }
        public Investment SelectInvestment
        {
            get
            {
                return _selectInvestment;
            }
            set
            {
                this.SetValue(ref _selectInvestment, value, SelectInvestmentProperty);
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
        public ObservableCollection<Nationality> Listnationalities
        {
            get
            {
                return _listnationalities;
            }
            set
            {
                this.SetValue(ref _listnationalities, value, ListnationalitiesProperty);
                OnPropertyChanged("Listnationalities");
            }
        }
        public ObservableCollection<Email> ListEmails
        {
            get
            {
                return _listEmails;
            }
            set
            {
                this.SetValue(ref _listEmails, value, ListEmailsProperty);
            }
        }
        public Email SelectEmail
        {
            get
            {
                return _selectEmail;
            }
            set
            {
                this.SetValue(ref _selectEmail, value, SelectEmailProperty);
            }
        }
        public ObservableCollection<PhoneNumber> ListPhoneNumbers
        {
            get
            {
                return _listPhoneNumbers;
            }
            set
            {
                this.SetValue(ref _listPhoneNumbers, value, ListPhoneNumbersProperty);
            }
        }
        public PhoneNumber SelectPhoneNumber
        {
            get
            {
                return _selectPhoneNumber;
            }
            set
            {
                this.SetValue(ref _selectPhoneNumber, value, SelectPhoneNumberProperty);
            }
        }
        #endregion

        #region method
        private bool checkFormatUpTime(string str)
        {
            // format uptime is yyyy or yyyy-yyyy
            if (!(Regex.IsMatch(str, @"^(19|20)\d\d$") || Regex.IsMatch(str, @"^((19|20)\d\d)-((19|20)\d\d)$")))
            {
                return false;
            }
            return true;
        }

        private bool checkCompanyName(string company_name, int company_id = 0)
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select * from Companies where CompanyName like N'" + MethodHandler.convertStringOwned(company_name.ToUpper()) + "'";
                    if (company_id != 0)
                    {
                        sql += " and IDCompany != " + company_id;
                    }
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        return true;
                    }
                    return false;
                } catch {
                    return false;
                }
            }
        }
        #endregion

        #region method ICommand
        public ICommand AddInvestmentCommand
        {
            get
            {
                return _addInvestmentCommand ?? (_addInvestmentCommand = new CommandHandler(AddInvestment, true));
            }
        }
        private void AddInvestment()
        {
            if (string.IsNullOrWhiteSpace(InputInvestment.Name))
            {
                MessageBox.Show("không cho phép trống chủ sở hữu");
                return;
            }
            if (string.IsNullOrWhiteSpace(InputInvestment.Nationality))
            {
                MessageBox.Show("chọn một quốc tịch");
                return;
            }
            if (string.IsNullOrWhiteSpace(InputInvestment.Passport))
            {
                MessageBox.Show("không cho phép trống số hộ chiếu");
                return;
            }
            if (!(InputInvestment.AmountOfMoney > 0))
            {
                MessageBox.Show("vui lòng nhập vốn đầu tư");
                InputInvestment.AmountOfMoney = 0;
                return;
            }
            // check exist invest in list
            int i = 0;
            for(; i < ListInvestment.Count; i++)
            {
                if(ListInvestment[i].Name.ToUpper().Equals(InputInvestment.Name.ToUpper()) 
                    && ListInvestment[i].Nationality.ToUpper().Equals(InputInvestment.Nationality.ToUpper())
                    && ListInvestment[i].Passport.ToUpper().Equals(InputInvestment.Passport.ToUpper())
                    && ListInvestment[i].AmountOfMoney == InputInvestment.AmountOfMoney)
                {
                    MessageBox.Show("vốn đầu tư đã tồn tại \n vui lòng kiểm tra lại");
                    break;
                }
            }
            if(i == ListInvestment.Count)
            {
                // invest is not yet
                ListInvestment.Add(InputInvestment);
                // current screen is edit
                if (_isEdit)
                {
                    _listUpdateInvestment.Add(InputInvestment);
                }
                if (string.IsNullOrWhiteSpace(AddComapany.TypeOfBusiniess))
                {
                    AddComapany.TypeOfBusiniess += InputInvestment.Nationality;
                }
                else
                {
                    if (AddComapany.TypeOfBusiniess.ToUpper().IndexOf(InputInvestment.Nationality.ToUpper()) < 0)
                    {
                        AddComapany.TypeOfBusiniess += " - " + InputInvestment.Nationality;
                    }
                }
                InputInvestment = new Investment();
                InputNationality = _listnationalities[0];
            }
            
        }


        public ICommand BtnAttachCommand
        {
            get
            {
                return _btnAttachCommand ?? (_btnAttachCommand = new CommandHandler(ListAttach, true));
            }
        }

        private void ListAttach()
        {
            AttachWindow attachWindow = new AttachWindow(AddComapany.IDCompany, AddComapany.TrackerID);
            attachWindow.ShowDialog();
        }

        public ICommand BtnAddEmailCommand
        {
            get
            {
                return _btnAddEmailCommand ?? (_btnAddEmailCommand = new CommandHandler(AddEmail, true));
            }
        }
        private void AddEmail()
        {
            AddEmailWindow dialog = new AddEmailWindow(ListEmails, _listUpdateEmails);
            dialog.ShowDialog();
        }

        public ICommand BtnAddPhoneNumberCommand
        {
            get
            {
                return _btnAddPhoneNumberCommand ?? (_btnAddPhoneNumberCommand = new CommandHandler(AddPhoneNumber, true));
            }
        }
        private void AddPhoneNumber()
        {
            PhoneNumberWindow dialog = new PhoneNumberWindow(ListPhoneNumbers, _listUpdatePhoneNumbers);
            dialog.ShowDialog();
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
            if (_userControl!= null)
            {
                _window.ContentArea.Content = _userControl;
            } else
            {
                _window.ContentArea.Content = new ListCompaniesUC(_window);
            }
        }

        public ICommand BtnAddCompanyCommand
        {
            get
            {
                return _btnAddCompanyCommand ?? (_btnAddCompanyCommand = new CommandHandler(AddCompany, true));
            }
        }
        private void AddCompany()
        {
            if (string.IsNullOrWhiteSpace(AddComapany.CompanyName) || string.IsNullOrWhiteSpace(AddComapany.Uptime) || string.IsNullOrWhiteSpace(AddComapany.TypeOfBusiniess)
                || string.IsNullOrWhiteSpace(AddComapany.Address) || string.IsNullOrWhiteSpace(AddComapany.LegalRepresentative) || string.IsNullOrWhiteSpace(AddComapany.RegistrationProfile))
            {
                MessageBox.Show("Các trường * là bắt buộc");
                return;
            }
            //if (!checkFormatUpTime(AddComapany.Uptime))
            //{
            //    MessageBox.Show("Sai định dạng thời gian hoạt động \n Định dạng là yyyy hoặc yyyy-yyyy");
            //    return;
            //}
            //if (ListInvestment.Count < 1)
            //{
            //    MessageBox.Show("Ít nhất có 1 vốn đầu tư");
            //    return;
            //}
            // check name company
            if (checkCompanyName(AddComapany.CompanyName))
            {
                MessageBox.Show("Tên công ty này đã tồn tại. \nVui lòng kiểm tra lại.");
                return;
            }
            if (string.IsNullOrEmpty(SelectWard))
            {
                MessageBox.Show("Phường xã không đúng hoặc đang để trống");
                return;
            }
            // check exist
            string saddress = AddComapany.Address.Trim() + ", " + SelectWard;
            string sqlCheckExist = "select * from Companies where CompanyName like N'" + MethodHandler.convertStringOwned(AddComapany.CompanyName)
                + "' and LegalRepresentative like N'" + AddComapany.LegalRepresentative.Trim()
                + "' and Address like N'" + saddress.Trim() + "'";
            if (MethodHandler.checkExistInDatabase(sqlCheckExist))
            {
                MessageBox.Show("Công ty này đã có sẵn. \nVui lòng kiểm tra lại.");
                return;
            }
            int idCompany;
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    // insert data company
                    DateTime today = DateTime.Today;
                    con.Open();
                    SqlCommand comm;
                    string insertSQL = "insert into Companies output INSERTED.IDCompany values (N'" + MethodHandler.convertStringOwned(AddComapany.CompanyName) + "', N'" + MethodHandler.convertStringOwned(AddComapany.TypeOfBusiniess) + "',"
                        + " '" + AddComapany.Uptime.Trim() + "', N'" + AddComapany.Address.Trim() + ", " + SelectWard + "', " + SelectField.IDField + ", N'" + MethodHandler.convertStringOwned(AddComapany.LegalRepresentative) + "',"
                        + " 0, 0, 0, 0, 0, N'" + MethodHandler.convertStringOwned(AddComapany.RegistrationProfile) + "', N' " + MethodHandler.convertStringOwned(AddComapany.Note) + "', '"
                        + today.ToString("yyyy-MM-dd") + "', 0, N' "
                        + (String.IsNullOrWhiteSpace(AddComapany.DescriptionOfActivities) ? "" : MethodHandler.convertStringOwned(AddComapany.DescriptionOfActivities))
                        + "', " + Constants.IdUser.Trim() + ")";
                    comm = new SqlCommand(insertSQL, con);
                    idCompany = (int)comm.ExecuteScalar();

                    // insert investment
                    foreach (Investment invest in ListInvestment)
                    {
                        string insertInvestment = "insert into Investment values(N'" + MethodHandler.convertStringOwned(invest.Name) + "', N'" + invest.Nationality + "'"
                            + ", " + invest.AmountOfMoney + ", " + idCompany + ", N'" + invest.Passport + "')";
                        comm = new SqlCommand(insertInvestment, con);
                        comm.ExecuteNonQuery();
                    }
                    // insert emails
                    foreach(Email email in ListEmails)
                    {
                        string insertEmail = "insert into Emails values(N'" + MethodHandler.convertStringOwned(email.Name) + "', N'" + email.Mail + "', " + idCompany + ")";
                        comm = new SqlCommand(insertEmail, con);
                        comm.ExecuteNonQuery();
                    }
                    // insert phone number
                    foreach (PhoneNumber phone in ListPhoneNumbers)
                    {
                        string insertPhone = "insert into PhoneNumbers values(N'" + MethodHandler.convertStringOwned(phone.Name) + "', N'" + phone.Phone + "', " + idCompany + ")";
                        comm = new SqlCommand(insertPhone, con);
                        comm.ExecuteNonQuery();
                    }
                    _window.ContentArea.Content = new ListCompaniesUC(_window);
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

        public ICommand LoadDataFieldsCommand
        {
            get
            {
                return _loadDataFieldsCommamnd ?? (_loadDataFieldsCommamnd = new CommandHandler(LoadDataFields, true));
            }
        }
        private void LoadDataFields()
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
                    adapter.SelectCommand = new SqlCommand("select * from Accounts order by Username", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListTracker.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Account rowAccount = new Account();
                            rowAccount.IDUser = int.Parse(dr["IDUser"].ToString());
                            rowAccount.Username = dr["Username"].ToString();
                            rowAccount.Name = dr["Name"].ToString();
                            ListTracker.Add(rowAccount);
                        }
                    }
                    //else
                    //{
                    //    MessageBox.Show("Không có cán bộ theo dõi");
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

        public ICommand LoadDataInvestmentCommand
        {
            get
            {
                return _loadDataInvestmentCommand ?? (_loadDataInvestmentCommand = new CommandHandler(LoadDataInvestment, true));
            }
        }
        private void LoadDataInvestment()
        {
            if (!_isEdit)
            {
                return;
            }
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Investment where IDCompany = " + AddComapany.IDCompany, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListInvestment.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Investment rowInvestment = new Investment();
                            rowInvestment.IDInvestment = int.Parse(dr["IDInvestment"].ToString());
                            rowInvestment.Name = dr["Name"].ToString();
                            rowInvestment.Nationality = dr["Nationality"].ToString();
                            rowInvestment.AmountOfMoney = decimal.Parse(dr["AmountOfMoney"].ToString());
                            rowInvestment.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowInvestment.Passport = dr["Passport"].ToString();
                            ListInvestment.Add(rowInvestment);
                        }
                    }
                    //else
                    //{
                    //    MessageBox.Show("Không có vốn đầu tư");
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

        public ICommand LoadDataPhoneNumberCommand
        {
            get
            {
                return _loadDataPhoneNumberCommand ?? (_loadDataPhoneNumberCommand = new CommandHandler(LoadDataPhoneNumber, true));
            }
        }
        private void LoadDataPhoneNumber()
        {
            if (!_isEdit)
            {
                return;
            }
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from PhoneNumbers where IDCompany = "+ AddComapany.IDCompany, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListPhoneNumbers.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            PhoneNumber rowPhoneNumber = new PhoneNumber();
                            rowPhoneNumber.IDPhoneNumber = int.Parse(dr["IDPhoneNumber"].ToString());
                            rowPhoneNumber.Name = dr["Name"].ToString();
                            rowPhoneNumber.Phone = dr["Phone"].ToString();
                            rowPhoneNumber.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            ListPhoneNumbers.Add(rowPhoneNumber);
                        }
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

        public ICommand LoadDataEmailCommand
        {
            get
            {
                return _loadDataEmailCommand ?? (_loadDataEmailCommand = new CommandHandler(LoadDataEmail, true));
            }
        }
        private void LoadDataEmail()
        {
            if (!_isEdit)
            {
                return;
            }
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Emails where IDCompany = " + AddComapany.IDCompany, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListEmails.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Email rowEmail = new Email();
                            rowEmail.IDEmail = int.Parse(dr["IDEmail"].ToString());
                            rowEmail.Name = dr["Name"].ToString();
                            rowEmail.Mail = dr["Mail"].ToString();
                            rowEmail.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            ListEmails.Add(rowEmail);
                        }
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

        public ICommand DeletePhoneNumberCommand
        {
            get
            {
                return _deletePhoneNumberCommand ?? (_deletePhoneNumberCommand = new CommandHandler(DeletePhoneNumber, true));
            }
        }
        private void DeletePhoneNumber()
        {
            if(SelectPhoneNumber != null)
            {
                if (_listUpdatePhoneNumbers != null)
                {
                    int i = 0;
                    for (; i < _listUpdatePhoneNumbers.Count; i++)
                    {
                        if (_listUpdatePhoneNumbers[i].Name.ToUpper().Equals(SelectPhoneNumber.Name.ToUpper()) && _listUpdatePhoneNumbers[i].Phone.ToUpper().Equals(SelectPhoneNumber.Phone.ToUpper()) && _listUpdatePhoneNumbers[i].IDPhoneNumber == SelectPhoneNumber.IDPhoneNumber)
                        {
                            break;
                        }
                    }
                    if (i == _listUpdatePhoneNumbers.Count && SelectPhoneNumber.IDPhoneNumber != 0)
                    {
                        _listUpdatePhoneNumbers.Add(SelectPhoneNumber);
                    } else if(i < _listUpdatePhoneNumbers.Count && SelectPhoneNumber.IDPhoneNumber == 0)
                    {
                        _listUpdatePhoneNumbers.Remove(SelectPhoneNumber);
                    }
                }
                ListPhoneNumbers.Remove(SelectPhoneNumber);
            }
        }

        public ICommand DeleteEmailCommand
        {
            get
            {
                return _deleteEmailCommand ?? (_deleteEmailCommand = new CommandHandler(DeleteEmail, true));
            }
        }
        private void DeleteEmail()
        {  
            if(SelectEmail != null)
            {
                if (_listUpdateEmails != null)
                {
                    int i = 0;
                    for (; i < _listUpdateEmails.Count; i++)
                    {
                        if (_listUpdateEmails[i].Name.ToUpper().Equals(SelectEmail.Name.ToUpper()) && _listUpdateEmails[i].Mail.ToUpper().Equals(SelectEmail.Mail.ToUpper()) && _listUpdateEmails[i].IDEmail == SelectEmail.IDEmail)
                        {
                            break;
                        }
                    }
                    if (i == _listUpdateEmails.Count && SelectEmail.IDEmail != 0)
                    {
                        _listUpdateEmails.Add(SelectEmail);
                    } else if(i < _listUpdateEmails.Count && SelectEmail.IDEmail == 0)
                    {
                        _listUpdateEmails.Remove(SelectEmail);
                    }
                }
                ListEmails.Remove(SelectEmail);
            }
        }

        public ICommand DeleteInvestmentCommand
        {
            get
            {
                return _deleteInvestmentCommand ?? (_deleteInvestmentCommand = new CommandHandler(DeleteInvestment, true));
            }
        }
        private void DeleteInvestment()
        {
            if(SelectInvestment != null)
            {
                if (_listUpdateInvestment != null)
                {
                    int i = 0;
                    for (; i < _listUpdateInvestment.Count; i++)
                    {
                        if (_listUpdateInvestment[i].Name.ToUpper().Equals(SelectInvestment.Name.ToUpper()) && _listUpdateInvestment[i].Nationality.ToUpper().Equals(SelectInvestment.Nationality.ToUpper())
                            && _listUpdateInvestment[i].Passport.ToUpper().Equals(SelectInvestment.Passport.ToUpper())
                            && _listUpdateInvestment[i].AmountOfMoney == SelectInvestment.AmountOfMoney && _listUpdateInvestment[i].IDInvestment == SelectInvestment.IDInvestment)
                        {
                            break;
                        }
                    }
                    if (i == _listUpdateInvestment.Count && SelectInvestment.IDInvestment != 0)
                    {
                        _listUpdateInvestment.Add(SelectInvestment);
                    }
                    else if (i < _listUpdateInvestment.Count && SelectInvestment.IDInvestment == 0)
                    {
                        _listUpdateInvestment.Remove(SelectInvestment);
                    }
                }
                ListInvestment.Remove(SelectInvestment);
            }
        }

        public ICommand BtnUpdateCompanyCommand
        {
            get
            {
                return _btnUpdateCompanyCommand ?? (_btnUpdateCompanyCommand = new CommandHandler(UpdateCompany, true));
            }
        }
        private void UpdateCompany()
        {
            if(AddComapany.TrackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Chỉ có cán bộ theo dõi mới được phép cập nhật thông tin công ty này");
                return;
            }
            if (string.IsNullOrWhiteSpace(AddComapany.CompanyName) || string.IsNullOrWhiteSpace(AddComapany.Uptime) || string.IsNullOrWhiteSpace(AddComapany.TypeOfBusiniess)
                || string.IsNullOrWhiteSpace(AddComapany.Address) || string.IsNullOrWhiteSpace(AddComapany.LegalRepresentative) || string.IsNullOrWhiteSpace(AddComapany.RegistrationProfile))
            {
                MessageBox.Show("Các trường * là bắt buộc");
                return;
            }
            if (string.IsNullOrEmpty(SelectWard))
            {
                MessageBox.Show("Phường xã không đúng hoặc đang để trống");
                return;
            }
            //if (!checkFormatUpTime(AddComapany.Uptime))
            //{
            //    MessageBox.Show("Sai định dạng thời gian hoạt động \n Định dạng là yyyy hoặc yyyy-yyyy");
            //    return;
            //}
            //if (ListInvestment.Count < 1)
            //{
            //    MessageBox.Show("Ít nhất có 1 vốn đầu tư");
            //    return;
            //}
            // check name company
            if (checkCompanyName(AddComapany.CompanyName, AddComapany.IDCompany))
            {
                MessageBox.Show("Tên công ty này đã tồn tại. \nVui lòng kiểm tra lại.");
                return;
            }
            string saddress = AddComapany.Address.Trim() + ", " + SelectWard;
            if (AddComapany.CompanyName.Trim().Equals(oldCompanyName) 
                && saddress.Trim().Equals(oldAddress) && AddComapany.LegalRepresentative.Trim().Equals(oldLegalRepresentative))
            {
                // no check exist
            }
            else
            {
                string sqlCheckExist = "select * from Companies where CompanyName like N'" + MethodHandler.convertStringOwned(AddComapany.CompanyName)
                + "' and LegalRepresentative like N'" + AddComapany.LegalRepresentative.Trim()
                + "' and Address like N'" + saddress.Trim() + "' and IDCompany != " + AddComapany.IDCompany;
                if (MethodHandler.checkExistInDatabase(sqlCheckExist))
                {
                    MessageBox.Show("Công ty này đã có sẵn. \nVui lòng kiểm tra lại.");
                    return;
                }
            }
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    // update data company
                    DateTime today = DateTime.Today;
                    con.Open();
                    SqlCommand comm;
                    string UpdateSQL = "update Companies set CompanyName = N'" + MethodHandler.convertStringOwned(AddComapany.CompanyName) + "', TypeOfBusiniess = N'" + AddComapany.TypeOfBusiniess.Trim()
                        + "', Uptime = '" + AddComapany.Uptime.Trim() + "', Address = N'" + AddComapany.Address.Trim() + ", " + SelectWard + "', IDField = " + SelectField.IDField
                        + ", LegalRepresentative = N'" + MethodHandler.convertStringOwned(AddComapany.LegalRepresentative) + "', RegistrationProfile = N'" + AddComapany.RegistrationProfile.Trim()
                        + "', Note = N' " + AddComapany.Note.Trim() + "', DescriptionOfActivities = N' "
                        + (String.IsNullOrWhiteSpace(AddComapany.DescriptionOfActivities) ? "" : MethodHandler.convertStringOwned(AddComapany.DescriptionOfActivities))
                        + "', TrackerID = " + SelectTracker.IDUser + " where IDCompany = " + AddComapany.IDCompany;
                    comm = new SqlCommand(UpdateSQL, con);
                    comm.ExecuteNonQuery();

                    // insert and delete investment
                    foreach (Investment invest in _listUpdateInvestment)
                    {
                        string sqlInvestMent = "";
                        if (invest.IDInvestment != 0)
                        {
                            // delete investment
                            sqlInvestMent = "delete from Investment where IDInvestment = "+ invest.IDInvestment;
                        }
                        else
                        {
                            // insert new investment
                            sqlInvestMent = "insert into Investment values(N'" + MethodHandler.convertStringOwned(invest.Name) + "', N'" + invest.Nationality + "'"
                            + ", " + invest.AmountOfMoney + ", " + AddComapany.IDCompany + ", N'" + invest.Passport + "')";
                        }
                        comm = new SqlCommand(sqlInvestMent, con);
                        comm.ExecuteNonQuery();
                    }
                    // insert and delete emails
                    foreach (Email email in _listUpdateEmails)
                    {
                        string sqlEmail = "";
                        if(email.IDEmail != 0)
                        {
                            // delete email
                            sqlEmail = "delete from Emails where IDEmail = " + email.IDEmail;
                        } else
                        {
                            // insert new email
                            sqlEmail = "insert into Emails values(N'" + MethodHandler.convertStringOwned(email.Name) + "', N'" + email.Mail + "', " + AddComapany.IDCompany + ")";
                        }
                        comm = new SqlCommand(sqlEmail, con);
                        comm.ExecuteNonQuery();
                    }
                    // insert and delete phone number
                    foreach (PhoneNumber phone in _listUpdatePhoneNumbers)
                    {
                        string sqlPhone = "";
                        if (phone.IDPhoneNumber != 0)
                        {
                            // delete phone number
                            sqlPhone = "delete from PhoneNumbers where IDPhoneNumber = " + phone.IDPhoneNumber;
                        }
                        else
                        {
                            // insert new phone number
                            sqlPhone = "insert into PhoneNumbers values(N'" + MethodHandler.convertStringOwned(phone.Name) + "', N'" + phone.Phone + "', " + AddComapany.IDCompany + ")";
                        }
                        comm = new SqlCommand(sqlPhone, con);
                        comm.ExecuteNonQuery();
                    }
                    //BtnExitCommand.Execute(true);
                    _window.ContentArea.Content = new ListCompaniesUC(_window);
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

        public ICommand BtnEditEmailCommand
        {
            get
            {
                return _btnEditEmailCommand ?? (_btnEditEmailCommand = new CommandHandler(EditEmail, true));
            }
        }
        private void EditEmail()
        {
            if(SelectEmail != null)
            {
                AddEmailWindow dialog = new AddEmailWindow(SelectEmail, ListEmails, _listUpdateEmails);
                dialog.ShowDialog();
            }
        }

        public ICommand BtnEditPhoneNumberCommand
        {
            get
            {
                return _btnEditPhoneNumberCommand ?? (_btnEditPhoneNumberCommand = new CommandHandler(EditPhoneBumber, true));
            }
        }
        private void EditPhoneBumber()
        {
            if (SelectPhoneNumber != null)
            {
                PhoneNumberWindow dialog = new PhoneNumberWindow(SelectPhoneNumber, ListPhoneNumbers, _listUpdatePhoneNumbers);
                dialog.ShowDialog();
            }
        }

        public ICommand BtnEditInvestmentCommand
        {
            get
            {
                return _btnEditInvestmentCommand ?? (_btnEditInvestmentCommand = new CommandHandler(EditInvestment, true));
            }
        }
        private void EditInvestment()
        {
            if (SelectInvestment != null)
            {
                InvestmentWindow dialog = new InvestmentWindow(SelectInvestment, ListInvestment, _listUpdateInvestment);
                dialog.ShowDialog();
            }
        }

        public ICommand BtnDeleteCompanyCommand
        {
            get
            {
                return _btnDeleteCompanyCommand ?? (_btnDeleteCompanyCommand = new CommandHandler(DeleteCompany, true));
            }
        }
        private void DeleteCompany()
        {
            if (!_isEdit)
            {
                return;
            }
            if(AddComapany.TrackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Bạn không có quyền xóa công ty này.\nVui lòng kiểm tra lại hoặc đăng nhập tài khoản khác");
                return;
            }
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa công ty này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        SqlCommand comm = new SqlCommand("update Companies set Delete_flag = 1 where IDCompany = " + AddComapany.IDCompany, con);
                        comm.ExecuteNonQuery();
                        _window.ContentArea.Content = new ListCompaniesUC(_window);
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
        }

        public ICommand ExportFileCompanyCommand
        {
            get
            {
                return _exportFileCompanyCommand ?? (_exportFileCompanyCommand = new CommandHandler(ExportFileCompany, true));
            }
        }
        private void ExportFileCompany()
        {
            ExportFileCompanyWindow dialog = new ExportFileCompanyWindow(null);
            dialog.ShowDialog();
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
