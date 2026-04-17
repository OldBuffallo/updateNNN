using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;
using ReportManager.Bases;
using ReportManager.Models;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class ListCompaniesViewModel : INotifyPropertyChanged
    {
        public static readonly string NameProperty = "Name";
        public static readonly string ListCompaniesProperty = "ListCompanies";
        public static readonly string SelectCompanyProperty = "SelectCompany";

        public static readonly string SumCompaniesProperty = "SumCompanies";
        public static readonly string SumRegistrationCompaniesProperty = "SumRegistrationCompanies";
        public static readonly string SumUnregistrationCompaniesProperty = "SumUnregistrationCompanies";
        public static readonly string SumTotalAmountProperty = "SumTotalAmount";
        public static readonly string SumAmountOfExemptionProperty = "SumAmountOfExemption";
        public static readonly string SumQuantityAvailableProperty = "SumQuantityAvailable";
        public static readonly string SumQuantityNotYetProperty = "SumQuantityNotYet";
        public static readonly string SumNumberOfPersonalitiesProperty = "SumNumberOfPersonalities";
        
        private string _name;
        private ObservableCollection<Company> _listCompanies = new ObservableCollection<Company>();
        private Company _selectCompany;
        // statistical company
        private int _sumCompanies;
        private int _sumRegistrationCompanies;
        private int _sumUnregistrationCompanies;
        // statistical employees
        private int _sumTotalAmount;
        private int _sumAmountOfExemption;
        private int _sumQuantityAvailable;
        private int _sumQuantityNotYet;
        private int _sumNumberOfPersonalities;
        private ICommand _btnRefreshCommand;
        private ICommand _btnSearchCommand;
        private ICommand _btnAddCompanyCommand;
        private ICommand _btnEditCompanyCommand;
        private ICommand _btnListEmployeesCommand;
        private ICommand _btnShowEmailCommand;
        private ICommand _btnAlmostExpiredCommand;
        private ICommand _checkEverydayCommand;
        private ICommand _exportFileCompanyCommand;
        private ICommand _exportFileEmployeeCommand;
        private ICommand _exportFileCompanyAndEmployeeCommand;
        private ICommand _statisticalCommand;
        private ReportManagerWindow _window;
        private UserControl _userControl;

        #region set get method
        public ListCompaniesViewModel(ReportManagerWindow window, UserControl userControl)
        {
            _window = window;
            _window.exportFileCompany.Command = ExportFileCompanyCommand;
            _window.exportFileEmployee.Command = ExportFileEmployeeCommand;
            _window.exportFileCompanyAndEmployee.Command = ExportFileCompanyAndEmployeeCommand;
            CheckEverydayCommand.Execute(true);
            BtnRefreshCommand.Execute(true);
            _userControl = userControl;
            //BtnAlmostExpiredCommand.Execute(true);
        }
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                this.SetValue(ref _name, value, NameProperty);
            }
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
        public Company SelectCompany
        {
            get
            {
                return _selectCompany;
            }
            set
            {
                this.SetValue(ref _selectCompany, value, SelectCompanyProperty);
                OnPropertyChanged("SelectCompany");
            }
        }
        // Statisticcal
        public int SumCompanies
        {
            get
            {
                return _sumCompanies;
            }
            set
            {
                this.SetValue(ref _sumCompanies, value, SumCompaniesProperty);
            }
        }
        public int SumRegistrationCompanies
        {
            get
            {
                return _sumRegistrationCompanies;
            }
            set
            {
                this.SetValue(ref _sumRegistrationCompanies, value, SumRegistrationCompaniesProperty);
            }
        }
        public int SumUnregistrationCompanies
        {
            get
            {
                return _sumUnregistrationCompanies;
            }
            set
            {
                this.SetValue(ref _sumUnregistrationCompanies, value, SumUnregistrationCompaniesProperty);
            }
        }
        public int SumTotalAmount
        {
            get
            {
                return _sumTotalAmount;
            }
            set
            {
                this.SetValue(ref _sumTotalAmount, value, SumTotalAmountProperty);
            }
        }
        public int SumAmountOfExemption
        {
            get
            {
                return _sumAmountOfExemption;
            }
            set
            {
                this.SetValue(ref _sumAmountOfExemption, value, SumAmountOfExemptionProperty);
            }
        }
        public int SumQuantityAvailable
        {
            get
            {
                return _sumQuantityAvailable;
            }
            set
            {
                this.SetValue(ref _sumQuantityAvailable, value, SumQuantityAvailableProperty);
            }
        }
        public int SumQuantityNotYet
        {
            get
            {
                return _sumQuantityNotYet;
            }
            set
            {
                this.SetValue(ref _sumQuantityNotYet, value, SumQuantityNotYetProperty);
            }
        }
        public int SumNumberOfPersonalities
        {
            get
            {
                return _sumNumberOfPersonalities;
            }
            set
            {
                this.SetValue(ref _sumNumberOfPersonalities, value, SumNumberOfPersonalitiesProperty);
            }
        }
        #endregion

        #region method Icommand
        public ICommand BtnRefreshCommand
        {
            get
            {
                return _btnRefreshCommand ?? (_btnRefreshCommand = new CommandHandler(Refresh, true));
            }
        }
        private void Refresh()
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    // get name of account login
                    string sqlName = "select * from Accounts where IDUser = " + Constants.IdUser;
                    adapter.SelectCommand = new SqlCommand(sqlName, con);
                    adapter.Fill(dt);
                    if(dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Name = dr["Name"].ToString().Trim();
                        }
                    }
                    dt.Clear();
                    string sql = "SELECT * FROM Companies, Fields, Accounts where Companies.Delete_flag = 0 "
                        + "and Companies.IDField = Fields.IDField and Companies.TrackerID = Accounts.IDUser order by FieldName, CompanyName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListCompanies.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Company rowCompany = new Company();
                            rowCompany.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowCompany.CompanyName = dr["CompanyName"].ToString().Trim();
                            rowCompany.TypeOfBusiniess = dr["TypeOfBusiniess"].ToString().Trim();
                            rowCompany.Uptime = dr["Uptime"].ToString().Trim();
                            rowCompany.Address = dr["Address"].ToString().Trim();
                            rowCompany.Field.IDField = int.Parse(dr["IDField"].ToString());
                            rowCompany.Field.FieldName = dr["FieldName"].ToString().Trim();
                            rowCompany.Field.Description = dr["Description"].ToString().Trim();
                            rowCompany.LegalRepresentative = dr["LegalRepresentative"].ToString().Trim();
                            //rowCompany.PhoneNumber = dr["PhoneNumber"].ToString();
                            //rowCompany.Email = dr["Email"].ToString();
                            rowCompany.TotalAmount = int.Parse(dr["TotalAmount"].ToString());
                            rowCompany.AmountOfExemption = int.Parse(dr["AmountOfExemption"].ToString());
                            rowCompany.QuantityAvailable = int.Parse(dr["QuantityAvailable"].ToString());
                            rowCompany.QuantityNotYet = int.Parse(dr["QuantityNotYet"].ToString());
                            rowCompany.NumberOfPersonalities = int.Parse(dr["NumberOfPersonalities"].ToString());
                            rowCompany.RegistrationProfile = dr["RegistrationProfile"].ToString().Trim();
                            rowCompany.DescriptionOfActivities = dr["DescriptionOfActivities"].ToString().Trim();
                            rowCompany.TrackerID = int.Parse(dr["TrackerID"].ToString());
                            rowCompany.Tracker = dr["Name"].ToString().Trim();
                            rowCompany.Note = dr["Note"].ToString().Trim();
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
                                    rowInvestment.Name = row["Name"].ToString().Trim();
                                    rowInvestment.Nationality = row["Nationality"].ToString().Trim();
                                    rowInvestment.AmountOfMoney = decimal.Parse(row["AmountOfMoney"].ToString());
                                    investmentString += rowInvestment.ToString() + "\n";
                                }
                                rowCompany.Investment = investmentString;
                            }
                            //get phone number
                            adapter.SelectCommand = new SqlCommand("select * from PhoneNumbers where IDCompany = " + rowCompany.IDCompany, con);
                            dtGet = new DataTable();
                            adapter.Fill(dtGet);
                            if(dtGet.Rows.Count > 0)
                            {
                                string phoneString = "";
                                foreach(DataRow row in dtGet.Rows)
                                {
                                    PhoneNumber rowPhoneNumber = new PhoneNumber();
                                    rowPhoneNumber.Name = row["Name"].ToString().Trim();
                                    rowPhoneNumber.Phone = row["Phone"].ToString().Trim();
                                    phoneString += rowPhoneNumber.ToString() + "\n";
                                    rowCompany.ListPhoneNumber.Add(rowPhoneNumber);
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
                                    rowEmail.Name = row["Name"].ToString().Trim();
                                    rowEmail.Mail = row["Mail"].ToString().Trim();
                                    emailString += rowEmail.ToString() + "\n";
                                    rowCompany.ListEmail.Add(rowEmail);
                                }
                                rowCompany.Email = emailString;
                            }

                            ListCompanies.Add(rowCompany);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có công ty");
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
            StatisticalCommand.Execute(true);
        }

        public ICommand BtnSearchCommand
        {
            get
            {
                return _btnSearchCommand ?? (_btnSearchCommand = new CommandHandler(Search, true));
            }
        }
        private void Search()
        {
            // show dialog search window
            SearchCompanyWindow window = new SearchCompanyWindow(_window, ListCompanies);
            window.ShowDialog();
            StatisticalCommand.Execute(true);
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
            ExportFileCompanyWindow dialog = new ExportFileCompanyWindow(ListCompanies);
            dialog.ShowDialog();
        }

        public ICommand ExportFileEmployeeCommand
        {
            get
            {
                return _exportFileEmployeeCommand ?? (_exportFileEmployeeCommand = new CommandHandler(ExportFileEmployee, true));
            }
        }
        private void ExportFileEmployee()
        {
            ExportFileWindow dialog = new ExportFileWindow(null, null);
            dialog.ShowDialog();
        }

        public ICommand ExportFileCompanyAndEmployeeCommand
        {
            get
            {
                return _exportFileCompanyAndEmployeeCommand ?? (_exportFileCompanyAndEmployeeCommand = new CommandHandler(ExportFileCompanyAndEmployee, true));
            }
        }
        private void ExportFileCompanyAndEmployee()
        {
            ExportFileCompanyAndEmployeeWindow dialog = new ExportFileCompanyAndEmployeeWindow();
            dialog.ShowDialog();
        }

        public ICommand StatisticalCommand
        {
            get
            {
                return _statisticalCommand ?? (_statisticalCommand = new CommandHandler(Statistical, true));
            }
        }
        private void Statistical()
        {
            ObservableCollection<int> listStatistical = MethodHandler.getStatistics(ListCompanies);
            SumCompanies                = listStatistical[0];
            SumRegistrationCompanies    = listStatistical[6];
            SumUnregistrationCompanies  = listStatistical[7];
            SumTotalAmount              = listStatistical[1];
            SumAmountOfExemption        = listStatistical[3];
            SumQuantityAvailable        = listStatistical[2];
            SumQuantityNotYet           = listStatistical[4];
            SumNumberOfPersonalities    = listStatistical[5];
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
            //_window.exportFile.Command = null;
            _window.ContentArea.Content = new AddCompanyUC(_window, _userControl);
        }

        public ICommand BtnEditCompanyCommand
        {
            get
            {
                return _btnEditCompanyCommand ?? (_btnEditCompanyCommand = new CommandHandler(EditCompany, true));
            }
        }
        private void EditCompany()
        {
            if (SelectCompany == null)
            {
                MessageBox.Show("Làm ơn chọn một công ty");
                return;
            }
            //_window.exportFile.Command = null;
            _window.ContentArea.Content = new AddCompanyUC(_window, SelectCompany, _userControl);
        }

        public ICommand BtnListEmployeesCommand
        {
            get
            {
                return _btnListEmployeesCommand ?? (_btnListEmployeesCommand = new CommandHandler(ListEmployees, true));
            }
        }
        private void ListEmployees()
        {
            if( SelectCompany != null)
            {
                //_window.exportFile.Command = null;
                _window.ContentArea.Content = new ListEmployeesUC(_window, SelectCompany, _userControl);
            }
        }

        public ICommand BtnShowEmailCommand
        {
            get
            {
                return _btnShowEmailCommand ?? (_btnShowEmailCommand = new CommandHandler(ShowEmail, true));
            }
        }
        private void ShowEmail()
        {
            DisplayEmailWindow dialog = new DisplayEmailWindow(ListCompanies);
            dialog.ShowDialog();
        }

        public ICommand BtnAlmostExpiredCommand
        {
            get
            {
                return _btnAlmostExpiredCommand ?? (_btnAlmostExpiredCommand = new CommandHandler(AlmostExpired, true));
            }
        }

        private void AlmostExpired()
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    DateTime today = DateTime.Today;
                    DateTime date_1 = today.AddDays(1);
                    DateTime date_2 = today.AddDays(5);
                    string sql = "select CONVERT(varchar, Birthday, 105) birth, CONVERT(varchar, TemporaryStay, 105) temporary,"
                        + " CONVERT(varchar, DateOfJoin, 105) datejoin, CONVERT(varchar, DateOfLeave, 105) dateleave, Employees.Hidden_flag hidden, *"
                        + " from Employees , Nationality, Companies"
                        + " where Employees.IDCompany = Companies.IDCompany and Companies.Delete_flag = 0 and Employees.Nationality like Nationality.NationalityCode and Hidden_flag = 0 and WorkingStatus = 0 and TemporaryStay >= '" + date_1.ToString("yyyy-MM-dd") + "' and TemporaryStay <= '" + date_2.ToString("yyyy-MM-dd") + "'";
                    if (int.Parse(Constants.Permission) != 0)
                    {
                        int id_user_login = int.Parse(Constants.IdUser);
                        sql = sql + " and Employees.IDCompany in (select IDCompany from Companies where TrackerID = " + id_user_login + ")";
                    }
                    sql += " order by Employees.IDCompany, Employees.IDCareer";

                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ObservableCollection<Employee> listEmployeeAlmostExpired = new ObservableCollection<Employee>();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Employee rowEmployee = new Employee();
                            rowEmployee.IDEmployee = int.Parse(dr["IDEmployee"].ToString());
                            rowEmployee.StaffName = dr["StaffName"].ToString().Trim();
                            rowEmployee.Gender = int.Parse(dr["Gender"].ToString());
                            rowEmployee.Birthday = dr["birth"].ToString().Trim();
                            rowEmployee.Nationality.NationalityCode = dr["NationalityCode"].ToString().Trim();
                            rowEmployee.Nationality.NationalityName = dr["NationalityName"].ToString().Trim();
                            rowEmployee.Passport = dr["Passport"].ToString().Trim();
                            rowEmployee.Address = dr["Address"].ToString().Trim();
                            rowEmployee.Career.IDCareer = int.Parse(dr["IDCareer"].ToString());
                            rowEmployee.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployee.WorkPermitNumber = dr["WorkPermitNumber"].ToString().Trim();
                            rowEmployee.VisaNumber = dr["VisaNumber"].ToString().Trim();
                            rowEmployee.TemporaryStay = dr["temporary"].ToString().Trim();
                            rowEmployee.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                            rowEmployee.SettlementResultsString = dr["SettlementResultsString"].ToString().Trim();
                            rowEmployee.IDUser = int.Parse(dr["IDUser"].ToString());
                            rowEmployee.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowEmployee.Note = dr["CompanyName"].ToString().Trim();
                            rowEmployee.WorkingStatus = int.Parse(dr["WorkingStatus"].ToString());
                            rowEmployee.DateOfJoin = dr["datejoin"].ToString().Trim();
                            rowEmployee.DateOfLeave = dr["dateleave"].ToString().Trim();
                            rowEmployee.Hidden = int.Parse(dr["hidden"].ToString());
                            // get career
                            DataTable dtGet = new DataTable();
                            adapter.SelectCommand = new SqlCommand("select CareerName, IDCG from Careers where IDCareer = " + rowEmployee.Career.IDCareer, con);
                            adapter.Fill(dtGet);
                            if (dtGet.Rows.Count > 0)
                            {
                                foreach (DataRow row in dtGet.Rows)
                                {
                                    rowEmployee.Career.CareerName = row["CareerName"].ToString().Trim();
                                    rowEmployee.Career.IDCG = int.Parse(row["IDCG"].ToString());
                                }
                            }
                            rowEmployee.LineNumber = ++lineNumber;
                            listEmployeeAlmostExpired.Add(rowEmployee);
                        }
                        // show window
                        AlmostExpiredWindow dialog = new AlmostExpiredWindow(_window, listEmployeeAlmostExpired);
                        dialog.Show();
                    } else
                    {
                        MessageBox.Show("Không có nhân viên sắp hết hạn");
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

        public ICommand CheckEverydayCommand
        {
            get
            {
                return _checkEverydayCommand ?? (_checkEverydayCommand = new CommandHandler(CheckEveryday, true));
            }
        }
        private void CheckEveryday()
        {
            ObservableCollection<int> amountOfCompanyUpdate = new ObservableCollection<int>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    DateTime today = DateTime.Today;
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select IDCompany from Companies where Delete_flag = 0 and UpdateDay < '" + today.ToString("yyyy-MM-dd") + "'", con);
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        amountOfCompanyUpdate.Add(int.Parse(dr["IDCompany"].ToString()));
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
            if(amountOfCompanyUpdate.Count > 0)
            {
                foreach(int id in amountOfCompanyUpdate)
                {
                    MethodHandler.updateCompanyByDate(id, true);
                }
            }
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
