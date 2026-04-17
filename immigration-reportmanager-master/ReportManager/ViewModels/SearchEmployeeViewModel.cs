using ReportManager.Bases;
using ReportManager.Models;
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
    class SearchEmployeeViewModel : INotifyPropertyChanged
    {
        public static readonly string ListEmployeesProperty = "ListEmployees";
        public static readonly string StaffNameProperty = "StaffName";
        public static readonly string NationalityProperty = "Nationality";
        public static readonly string PassportProperty = "Passport";
        public static readonly string AddressProperty = "Address";
        public static readonly string CareerStringProperty = "CareerString";
        public static readonly string StartDateProperty = "StartDate";
        public static readonly string EndDateProperty = "EndDate";
        private ObservableCollection<Employee> _listEmployees;
        private string _staffName;
        private ObservableCollection<Nationality> _listNationalities;
        private Nationality _nationality;
        private string _passport;
        private string _address;
        private string _careerString;
        private string _startDate;
        private string _endDate;
        private SearchEmployeeWindow _window;
        private int _idCompany;
        private ICommand _btnSetNationalityCommand;
        private ICommand _btnSearchCommand;
        private ICommand _btnExitCommand;

        #region set get method
        public SearchEmployeeViewModel(SearchEmployeeWindow window, ObservableCollection<Employee> listEmployees, int IDCompany)
        {
            _window = window;
            ListEmployees = listEmployees;
            _listNationalities = MethodHandler.getNationalities();
            Nationality = new Nationality();
            _idCompany = IDCompany;
        }
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
        public Nationality Nationality
        {
            get
            {
                return _nationality;
            }
            set
            {
                this.SetValue(ref _nationality, value, NationalityProperty);
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
            foreach (Nationality na in _listNationalities)
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
                return _btnSearchCommand ?? (_btnSearchCommand = new CommandHandler(Search, true));
            }
        }
        private void Search()
        {
            if( string.IsNullOrWhiteSpace(StaffName) && string.IsNullOrWhiteSpace(Nationality.NationalityCode)
                && string.IsNullOrWhiteSpace(Passport) && string.IsNullOrWhiteSpace(Address)
                && string.IsNullOrWhiteSpace(CareerString) && string.IsNullOrWhiteSpace(StartDate)
                && string.IsNullOrWhiteSpace(EndDate))
            {
                MessageBox.Show("Vui lòng nhập ít nhất một thông tin");
                return;
            }
            if(!string.IsNullOrWhiteSpace(StartDate) && !MethodHandler.checkFormatDate(StartDate))
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
            string sqlString = "select CONVERT(varchar, Birthday, 105) birth, CONVERT(varchar, TemporaryStay, 105) temporary, Employees.Hidden_flag hidden, * from Employees, Nationality where Employees.Nationality like Nationality.NationalityCode and IDCompany = " + _idCompany;
            if (!string.IsNullOrWhiteSpace(StaffName))
            {
                sqlString += " and Employees.StaffName like N'%" + MethodHandler.convertStringOwned(StaffName) + "%'";
            }
            if (!string.IsNullOrWhiteSpace(Nationality.NationalityName))
            {
                sqlString += " and Employees.Nationality like N'%" + MethodHandler.convertStringOwned(Nationality.NationalityCode) + "%'";
            }
            if (!string.IsNullOrWhiteSpace(Passport))
            {
                sqlString += " and Employees.Passport like N'%" + MethodHandler.convertStringOwned(Passport) + "%'";
            }
            if (!string.IsNullOrWhiteSpace(Address))
            {
                sqlString += " and Employees.Address like N'%" + MethodHandler.convertStringOwned(Address) + "%'";
            }
            if (!string.IsNullOrWhiteSpace(CareerString))
            {
                sqlString += " and Employees.IDCareer in (select IDCareer from Careers where CareerName like N'%" + MethodHandler.convertStringOwned(CareerString) + "%')";
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
            sqlString += " order by IDCareer, Address, StaffName";

            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand( sqlString, con);
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
                            rowEmployee.Address = dr["Address"].ToString();
                            rowEmployee.Career.IDCareer = int.Parse(dr["IDCareer"].ToString());
                            rowEmployee.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployee.WorkPermitNumber = dr["WorkPermitNumber"].ToString();
                            rowEmployee.VisaNumber = dr["VisaNumber"].ToString();
                            rowEmployee.TemporaryStay = dr["temporary"].ToString();
                            rowEmployee.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                            rowEmployee.SettlementResultsString = dr["SettlementResultsString"].ToString();
                            rowEmployee.IDUser = int.Parse(dr["IDUser"].ToString());
                            rowEmployee.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowEmployee.Note = dr["Note"].ToString();
                            rowEmployee.Hidden = int.Parse(dr["hidden"].ToString());
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
                        MessageBox.Show("Chưa có nhân viên");
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
