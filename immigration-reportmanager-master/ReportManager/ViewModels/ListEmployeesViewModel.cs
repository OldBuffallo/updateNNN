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
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class ListEmployeesViewModel : INotifyPropertyChanged
    {
        public static readonly string CompanyProperty = "Company";
        public static readonly string SumProperty = "Sum";
        public static readonly string SumGHTTProperty = "SumGHTT";
        public static readonly string SumVisaProperty = "SumVisa";
        public static readonly string SumTemporaryResidenceCardProperty = "SumTemporaryResidenceCard";
        public static readonly string SumOtherProperty = "SumOther";
        public static readonly string ListEmployeesProperty = "ListEmployees";
        public static readonly string SelectEmployeeProperty = "SelectEmployee";
        private Company _company;
        private int _sum;
        private int _sumGHTT;
        private int _sumVisa;
        private int _sumTemporaryResidenceCard;
        private int _sumOther;
        private ObservableCollection<Employee> _listEmployees = new ObservableCollection<Employee>();
        private Employee _selectEmployee;
        private ReportManagerWindow _window;
        private ICommand _btnBackCommand;
        private ICommand _btnAddEmployeeCommand;
        private ICommand _btnReadFileCommand;
        private ICommand _loadDataEmployeesCommand;
        private ICommand _btnOpenEditEmployeeCommand;
        private ICommand _btnSearchCommand;
        private ICommand _exportFileCompanyCommand;
        private ICommand _exportFileEmployeeCommand;
        private UserControl preUserControl;
        private UserControl currentUserControl;

        #region set get method
        public ListEmployeesViewModel(ReportManagerWindow window, UserControl userControl, Company company, UserControl curUserControl)
        {
            _window = window;
            preUserControl = userControl;
            currentUserControl= curUserControl;
            Company = company;
            LoadDataEmployeesCommand.Execute(true);
            _window.exportFileCompany.Command = BtnExportFileCompanyCommand;
            _window.exportFileEmployee.Command = BtnExportFileEmployeeCommand;
        }
        public Company Company
        {
            get
            {
                return _company;
            }

            set
            {
                this.SetValue(ref _company, value, CompanyProperty);
            }
        }
        public int Sum
        {
            get
            {
                return _sum;
            }

            set
            {
                this.SetValue(ref _sum, value, SumProperty);
            }
        }
        public int SumGHTT
        {
            get
            {
                return _sumGHTT;
            }

            set
            {
                this.SetValue(ref _sumGHTT, value, SumGHTTProperty);
            }
        }
        public int SumVisa
        {
            get
            {
                return _sumVisa;
            }

            set
            {
                this.SetValue(ref _sumVisa, value, SumVisaProperty);
            }
        }
        public int SumTemporaryResidenceCard
        {
            get
            {
                return _sumTemporaryResidenceCard;
            }

            set
            {
                this.SetValue(ref _sumTemporaryResidenceCard, value, SumTemporaryResidenceCardProperty);
            }
        }
        public int SumOther
        {
            get
            {
                return _sumOther;
            }

            set
            {
                this.SetValue(ref _sumOther, value, SumOtherProperty);
            }
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
            }
        }
        public Employee SelectEmployee
        {
            get
            {
                return _selectEmployee;
            }

            set
            {
                this.SetValue(ref _selectEmployee, value, SelectEmployeeProperty);
            }
        }
        #endregion

        #region method ICommand
        public ICommand BtnBackCommand
        {
            get
            {
                return _btnBackCommand ?? (_btnBackCommand = new CommandHandler(Back, true));
            }
        }
        private void Back()
        {
            //_window.exportFileCompany.Command = null;
            if (preUserControl != null)
            {
                _window.ContentArea.Content = preUserControl;
            } else
            {
                _window.ContentArea.Content = new ListCompaniesUC(_window);
            }
        }

        public ICommand BtnAddEmployeeCommand
        {
            get
            {
                return _btnAddEmployeeCommand ?? (_btnAddEmployeeCommand = new CommandHandler(AddEmployee, true));
            }
        }
        private void AddEmployee()
        {
            // check tracker and username login is same
            if(Company.TrackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Bạn không phải cán bộ theo dõi công ty này.\nVui lòng kiểm tra lại công ty hoặc đăng nhập đúng tài khoản.");
                return;
            }
            //_window.exportFileCompany.Command = null;
            _window.ContentArea.Content = new AddEmployeeUC(_window, currentUserControl, Company);
        }

        public ICommand BtnReadFileCommand
        {
            get
            {
                return _btnReadFileCommand ?? (_btnReadFileCommand = new CommandHandler(ReadFile, true));
            }
        }
        private void ReadFile()
        {
            // check tracker and username login is same
            if (Company.TrackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Bạn không phải cán bộ theo dõi công ty này.\nVui lòng kiểm tra lại công ty hoặc đăng nhập đúng tài khoản.");
                return;
            }
            ReadFileWindow dialog = new ReadFileWindow(currentUserControl, Company.IDCompany);
            dialog.ShowDialog();
            LoadDataEmployeesCommand.Execute(true);
        }

        public ICommand BtnOpenEditEmployeeCommand
        {
            get
            {
                return _btnOpenEditEmployeeCommand ?? (_btnOpenEditEmployeeCommand = new CommandHandler(OpenEditEmployee, true));
            }
        }
        private void OpenEditEmployee()
        {
            if (SelectEmployee != null)
            {
                //_window.exportFileCompany.Command = null;
                _window.ContentArea.Content = new AddEmployeeUC(_window, currentUserControl, Company, SelectEmployee);
            }
        }

        public ICommand BtnExportFileCompanyCommand
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

        public ICommand BtnExportFileEmployeeCommand
        {
            get
            {
                return _exportFileEmployeeCommand ?? (_exportFileEmployeeCommand = new CommandHandler(ExportFileEmployee, true));
            }
        }
        private void ExportFileEmployee()
        {
            ExportFileWindow dialog = new ExportFileWindow(ListEmployees, Company);
            dialog.ShowDialog();
        }

        public ICommand LoadDataEmployeesCommand
        {
            get
            {
                return _loadDataEmployeesCommand ?? (_loadDataEmployeesCommand = new CommandHandler(LoadDataEmployees, true));
            }
        }
        private void LoadDataEmployees()
        {
            SumGHTT = 0;
            SumVisa = 0;
            SumTemporaryResidenceCard = 0;
            SumOther = 0;
            DateTime today = DateTime.Today;
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select CONVERT(varchar, Birthday, 105) birth, CONVERT(varchar, TemporaryStay, 105) temporary,"
                        + " CONVERT(varchar, DateOfJoin, 105) datejoin, CONVERT(varchar, DateOfLeave, 105) dateleave, Employees.Hidden_flag hidden, *"
                        + " from Employees, Nationality where Employees.Nationality like Nationality.NationalityCode and IDCompany = "
                        + Company.IDCompany + " and Hidden_flag = 0"
                        +" and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + today.ToString("yyyy-MM-dd") + "')"
                        +" and TemporaryStay >= '" + today.ToString("yyyy-MM-dd") + "' order by IDCareer, Address, StaffName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListEmployees.Clear();
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
                            rowEmployee.Note = dr["Note"].ToString().Trim();
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
                            CountSum(rowEmployee.SettlementResults);
                            rowEmployee.LineNumber = ++lineNumber;
                            ListEmployees.Add(rowEmployee);
                        }
                    }
                    //else
                    //{
                    //    MessageBox.Show("Chưa có nhân viên");
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                    Sum = ListEmployees.Count;
                }

            }
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
            SearchEmployeeWindow window = new SearchEmployeeWindow(ListEmployees, Company.IDCompany);
            window.ShowDialog();
            Sum = ListEmployees.Count;
            SumGHTT = 0;
            SumVisa = 0;
            SumTemporaryResidenceCard = 0;
            SumOther = 0;
            foreach (Employee employ in ListEmployees)
            {
                CountSum(employ.SettlementResults);
            }
        }
        #endregion

        private void CountSum(int intRS)
        {
            switch (intRS)
            {
                case 0:
                    SumGHTT++;
                    break;
                case 1:
                    SumVisa++;
                    break;
                case 2:
                    SumTemporaryResidenceCard++;
                    break;
                case 3:
                    SumOther++;
                    break;
            }
        }

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
