using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Packaging.Ionic.Zip;
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
    
    class AlmostExpiredViewModel : INotifyPropertyChanged
    {
        public static readonly string ListAlmostExpiredProperty = "ListAlmostExpired";
        public static readonly string SelectEmployeeProperty = "SelectEmployee";
        private ObservableCollection<Employee> _listAlmostExpired;
        private Employee _selectEmployee;
        private ICommand _btnEmployeeInfoCommand;
        private ICommand _btnCloseCommand;
        private AlmostExpiredWindow _window;
        private ReportManagerWindow _reportManagerWindow;

        #region set get method
        public AlmostExpiredViewModel(AlmostExpiredWindow window, ReportManagerWindow reportManagerWindow, ObservableCollection<Employee> listAlmostExpired)
        {
            _window = window;
            _reportManagerWindow = reportManagerWindow;
            ListAlmostExpired = listAlmostExpired;
        }

        public ObservableCollection<Employee> ListAlmostExpired
        {
            get { return _listAlmostExpired; }
            set 
            {
                this.SetValue(ref _listAlmostExpired, value, ListAlmostExpiredProperty);
                OnPropertyChanged("ListAlmostExpired");
            }
        }

        public Employee SelectEmployee
        {
            get { return _selectEmployee; }
            set
            {
                this.SetValue(ref _selectEmployee, value, SelectEmployeeProperty);
                OnPropertyChanged("SelectEmployee");
            }
        }
        #endregion

        #region method ICommand
        public ICommand BtnCloseCommand
        {
            get
            {
                return _btnCloseCommand ?? (_btnCloseCommand = new CommandHandler(Close, true));
            }
        }

        private void Close()
        {
            _window.Close();
        }

        public ICommand BtnEmployeeInfoCommand
        {
            get
            {
                return _btnEmployeeInfoCommand ?? (_btnEmployeeInfoCommand = new CommandHandler(EmployeeInfo, true));
            }
        }

        private void EmployeeInfo()
        {
            if (SelectEmployee!= null)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        // get info employee
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.SelectCommand = new SqlCommand("select CONVERT(varchar, Birthday, 105) birth, CONVERT(varchar, TemporaryStay, 105) temporary,"
                        + " CONVERT(varchar, DateOfJoin, 105) datejoin, CONVERT(varchar, DateOfLeave, 105) dateleave, Employees.Hidden_flag hidden, *"
                        + " from Employees, Nationality where Employees.Nationality like Nationality.NationalityCode and IDCompany = "
                        + SelectEmployee.IDCompany
                        + " and Employees.IDEmployee = " + SelectEmployee.IDEmployee, con);
                        adapter.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                SelectEmployee.IDEmployee = int.Parse(dr["IDEmployee"].ToString());
                                SelectEmployee.StaffName = dr["StaffName"].ToString().Trim();
                                SelectEmployee.Gender = int.Parse(dr["Gender"].ToString());
                                SelectEmployee.Birthday = dr["birth"].ToString().Trim();
                                SelectEmployee.Nationality.NationalityCode = dr["NationalityCode"].ToString().Trim();
                                SelectEmployee.Nationality.NationalityName = dr["NationalityName"].ToString().Trim();
                                SelectEmployee.Passport = dr["Passport"].ToString().Trim();
                                SelectEmployee.Address = dr["Address"].ToString().Trim();
                                SelectEmployee.Career.IDCareer = int.Parse(dr["IDCareer"].ToString());
                                SelectEmployee.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                                SelectEmployee.WorkPermitNumber = dr["WorkPermitNumber"].ToString().Trim();
                                SelectEmployee.VisaNumber = dr["VisaNumber"].ToString().Trim();
                                SelectEmployee.TemporaryStay = dr["temporary"].ToString().Trim();
                                SelectEmployee.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                                SelectEmployee.SettlementResultsString = dr["SettlementResultsString"].ToString().Trim();
                                SelectEmployee.IDUser = int.Parse(dr["IDUser"].ToString());
                                SelectEmployee.IDCompany = int.Parse(dr["IDCompany"].ToString());
                                SelectEmployee.Note = dr["Note"].ToString().Trim();
                                SelectEmployee.WorkingStatus = int.Parse(dr["WorkingStatus"].ToString());
                                SelectEmployee.DateOfJoin = dr["datejoin"].ToString().Trim();
                                SelectEmployee.DateOfLeave = dr["dateleave"].ToString().Trim();
                                SelectEmployee.Hidden = int.Parse(dr["hidden"].ToString());
                                // get career
                                DataTable dtGet = new DataTable();
                                adapter.SelectCommand = new SqlCommand("select CareerName, IDCG from Careers where IDCareer = " + SelectEmployee.Career.IDCareer, con);
                                adapter.Fill(dtGet);
                                if (dtGet.Rows.Count > 0)
                                {
                                    foreach (DataRow row in dtGet.Rows)
                                    {
                                        SelectEmployee.Career.CareerName = row["CareerName"].ToString().Trim();
                                        SelectEmployee.Career.IDCG = int.Parse(row["IDCG"].ToString());
                                    }
                                }
                            }
                        }

                        // get info company
                        dt.Clear();
                        string sqlCompany = "SELECT * FROM Companies, Fields, Accounts where Companies.Delete_flag = 0 "
                            + "and Companies.IDCompany = " + SelectEmployee.IDCompany
                            + "and Companies.IDField = Fields.IDField and Companies.TrackerID = Accounts.IDUser order by FieldName, CompanyName";
                        adapter.SelectCommand = new SqlCommand(sqlCompany, con);
                        adapter.Fill(dt);
                        Company rowCompany = new Company();
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
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
                                if (dtGet.Rows.Count > 0)
                                {
                                    string phoneString = "";
                                    foreach (DataRow row in dtGet.Rows)
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
                            }
                        }

                        // set content window
                        if (SelectEmployee != null && rowCompany.IDCompany != 0)
                        {
                            //_window.exportFileCompany.Command = null;
                            _reportManagerWindow.ContentArea.Content = new AddEmployeeUC(_reportManagerWindow, null, rowCompany, SelectEmployee);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    } finally
                    {
                        con.Close();
                    }
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
