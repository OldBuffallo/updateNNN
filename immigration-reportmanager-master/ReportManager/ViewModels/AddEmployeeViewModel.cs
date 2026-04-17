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
    class AddEmployeeViewModel : INotifyPropertyChanged
    {
        #region properties
        public static readonly string CompanyProperty = "Company";
        public static readonly string NewEmployeeProperty = "NewEmployee";
        public static readonly string ListNationalitiesProperty = "ListNationalities";
        public static readonly string ListCareerGroupProperty = "ListCareerGroup";
        public static readonly string SelectCareerGroupProperty = "SelectCareerGroup";
        public static readonly string ListCareersProperty = "ListCareers";
        public static readonly string SelectCareerProperty = "SelectCareer";
        public static readonly string SelectGenderProperty = "SelectGender";
        public static readonly string SelectWardProperty = "SelectWard";
        public static readonly string ListWardsProperty = "ListWards";
        private Company _company;
        private Employee _newEmployee;
        private ObservableCollection<Nationality> _listNationalities;
        private ObservableCollection<CareerGroup> _listCareerGroup;
        private CareerGroup _selectCareerGroup;
        private ObservableCollection<Career> _listCareers;
        private Career _selectCareer;
        private int _selectGender;
        //private ObservableCollection<string> listDistricts;
        private string _selectWard;
        private ObservableCollection<string> listWards;
        private ReportManagerWindow _window;
        private ICommand _btnSetNationalityCommand;
        private ICommand _btnCancelCommand;
        private ICommand _btnAddEmployeeCommand;
        private ICommand _btnEditEmployeeCommand;
        private ICommand _btnDeleteEmployeeCommand;
        //icommand for export from report manager window
        private ICommand _exportFileCompanyCommand;
        private ICommand _exportFileEmployeeCommand;
        private bool _isEdit = false;
        private Employee _oldEmployee;
        private UserControl preUserControl;
        #endregion

        #region set get method
        public AddEmployeeViewModel(ReportManagerWindow window, UserControl userControl, Company company)
        {
            _window = window;
            preUserControl = userControl;
            Company = company;
            _newEmployee = new Employee();
            _newEmployee.Gender = 1;
            ListNationalities = MethodHandler.getNationalities();
            //listDistricts = MethodHandler.getDistricts();
            listWards = MethodHandler.getWards();
            _listCareerGroup = MethodHandler.getListCareerGroup();
            _listCareers = new ObservableCollection<Career>();
            if (ListCareerGroup.Count > 0)
            {
                SelectCareerGroup = ListCareerGroup[0];
            }
            _isEdit = false;
            //set command for export file
            _window.exportFileCompany.Command = ExportFileCompanyCommand;
            _window.exportFileEmployee.Command = ExportFileEmployeeCommand;
        }
        public AddEmployeeViewModel(ReportManagerWindow window, UserControl userControl, Company company, Employee editEmployee)
        {
            _window = window;
            preUserControl = userControl;
            Company = company;
            ListNationalities = MethodHandler.getNationalities();
            
            _listCareerGroup = MethodHandler.getListCareerGroup();
            _listCareers = new ObservableCollection<Career>();
            NewEmployee = new Employee(editEmployee);
            foreach (Nationality nationality in ListNationalities)
            {
                if (nationality.NationalityCode.ToUpper().Equals(editEmployee.Nationality.NationalityCode.ToUpper()))
                {
                    NewEmployee.Nationality = new Nationality(nationality);
                    break;
                }
            }
            //listDistricts = MethodHandler.getDistricts();
            listWards = MethodHandler.getWards();
            foreach (string war in listWards)
            {
                if (editEmployee.Address.ToUpper().EndsWith(war.ToUpper()))
                {
                    _selectWard = war;
                    string strCut = ", " + war;
                    NewEmployee.Address = NewEmployee.Address.Substring(0, NewEmployee.Address.Length - strCut.Length).Trim();
                    break;
                }
            }
            foreach (CareerGroup cg in ListCareerGroup)
            {
                if (cg.IDCG == editEmployee.Career.IDCG)
                {
                    SelectCareerGroup = cg;
                    break;
                }
            }
            foreach (Career car in ListCareers)
            {
                if (car.IDCareer == editEmployee.Career.IDCareer)
                {
                    SelectCareer = car;
                    break;
                }
            }
            _oldEmployee = new Employee(editEmployee);
            SelectGender = editEmployee.Gender - 1;
            _isEdit = true;
            //set command for export file
            _window.exportFileCompany.Command = ExportFileCompanyCommand;
            _window.exportFileEmployee.Command = ExportFileEmployeeCommand;
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
        public Employee NewEmployee
        {
            get
            {
                return _newEmployee;
            }

            set
            {
                this.SetValue(ref _newEmployee, value, NewEmployeeProperty);
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
        public ObservableCollection<CareerGroup> ListCareerGroup
        {
            get
            {
                return _listCareerGroup;
            }

            set
            {
                this.SetValue(ref _listCareerGroup, value, ListCareerGroupProperty);
            }
        }
        public CareerGroup SelectCareerGroup
        {
            get
            {
                return _selectCareerGroup;
            }

            set
            {
                this.SetValue(ref _selectCareerGroup, value, SelectCareerGroupProperty);
                LoadCareerAfterChangeGroup();
            }
        }
        public ObservableCollection<Career> ListCareers
        {
            get
            {
                return _listCareers;
            }

            set
            {
                this.SetValue(ref _listCareers, value, ListCareersProperty);
            }
        }
        public Career SelectCareer
        {
            get
            {
                return _selectCareer;
            }

            set
            {
                this.SetValue(ref _selectCareer, value, SelectCareerProperty);
                foreach (CareerGroup cg in ListCareerGroup)
                {
                    if (cg.IDCG == _selectCareer.IDCG)
                    {
                        _selectCareerGroup = cg;
                        break;
                    }
                }
                OnPropertyChanged(SelectCareerGroupProperty);
            }
        }
        public int SelectGender
        {
            get
            {
                return _selectGender;
            }

            set
            {
                NewEmployee.Gender = value + 1;
                this.SetValue(ref _selectGender, value, SelectGenderProperty);
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
        public ObservableCollection<string> ListWards
        {
            get
            {
                return listWards;
            }
            set
            {
                this.SetValue(ref listWards, value, ListWardsProperty);
            }
        }
        #endregion

        #region method
        private bool checkDifferent(Employee old, Employee current)
        {
            if (isSame(old.StaffName, current.StaffName)
                && old.Gender == current.Gender && old.Birthday.Equals(current.Birthday)
                && isSame(old.Address, current.Address)
                && isSame(old.Nationality.NationalityCode, current.Nationality.NationalityCode)
                && isSame(old.Passport, current.Passport) && old.Career.IDCareer == current.Career.IDCareer
                && old.WorkPermit == current.WorkPermit && old.VisaNumber.Equals(current.VisaNumber)
                && isSame(old.WorkPermitNumber, current.WorkPermitNumber)
                && old.SettlementResults == current.SettlementResults
                && isSame(old.SettlementResultsString, current.SettlementResultsString)
                && isSame(old.TemporaryStay, current.TemporaryStay)
                && isSame(old.Note, current.Note)
                && old.WorkingStatus == current.WorkingStatus
                && isSame(old.DateOfJoin, current.DateOfJoin)
                && isSame(old.DateOfLeave, current.DateOfLeave))
            {
                return false;
            }
            return true;
        }
        private bool isSame(string str1, string str2)
        {
            return (String.IsNullOrWhiteSpace(str1) && String.IsNullOrWhiteSpace(str2) || (!String.IsNullOrWhiteSpace(str1) && String.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0));
        }

        private void LoadCareerAfterChangeGroup()
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sqlQuery = "";
                    if(SelectCareerGroup.IDCG == 1)
                    {
                        sqlQuery = "select * from Careers where Delete_flag = 0";
                    }
                    else
                    {
                        sqlQuery = "select * from Careers where Delete_flag = 0 and IDCG = " + SelectCareerGroup.IDCG;
                    }
                    adapter.SelectCommand = new SqlCommand(sqlQuery, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListCareers.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Career rowCareer = new Career();
                            rowCareer.IDCareer = int.Parse(dr["IDCareer"].ToString());
                            rowCareer.CareerName = dr["CareerName"].ToString();
                            rowCareer.IDCG = int.Parse(dr["IDCG"].ToString());
                            ListCareers.Add(rowCareer);
                        }
                        SelectCareer = ListCareers[0];
                    }
                    else
                    {
                        MessageBox.Show("Không có nhóm ngành nghề");
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
            if (string.IsNullOrWhiteSpace(NewEmployee.Nationality.NationalityCode))
            {
                NewEmployee.Nationality.NationalityName = "";
                return;
            }
            foreach (Nationality na in ListNationalities)
            {
                if (na.NationalityCode.Equals(NewEmployee.Nationality.NationalityCode))
                {
                    NewEmployee.Nationality.NationalityName = na.NationalityName;
                    return;
                }
            }
            MessageBox.Show("Mã quốc tịch chưa có trong dữ liệu hoặc đã bị xóa.\nVui lòng kiểm tra lại.");
            NewEmployee.Nationality.NationalityName = "";
            return;
        }

        public ICommand BtnCancelCommand
        {
            get
            {
                return _btnCancelCommand ?? (_btnCancelCommand = new CommandHandler(Cancel, true));
            }
        }
        private void Cancel()
        {
            if (preUserControl != null)
            {
                _window.ContentArea.Content = preUserControl;
            } else
            {
                _window.ContentArea.Content = new ListEmployeesUC(_window, Company, null);
            }
        }

        public ICommand BtnDeleteEmployeeCommand
        {
            get
            {
                return _btnDeleteEmployeeCommand ?? (_btnDeleteEmployeeCommand = new CommandHandler(DeleteEmployee, true));
            }
        }
        private void DeleteEmployee()
        {
            // check tracker and username login is same
            if (_isEdit && Company.TrackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Bạn chỉ có quyền xem thông tin nhân viên.\nChỉ có người theo dõi công ty của nhân viên này mới được xóa nhân viên.\nVui lòng lòng đăng nhập đúng tài khoản.");
                return;
            }
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa nhân viên này?", "Xác nhận", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        SqlCommand comm = new SqlCommand("delete Employees where IDEmployee =  " + NewEmployee.IDEmployee, con);
                        comm.ExecuteNonQuery();
                        //if (preUserControl != null)
                        //{
                        //    _window.ContentArea.Content = preUserControl;
                        //} else
                        //{
                        //    _window.ContentArea.Content = new ListEmployeesUC(_window, Company, null);
                        //}
                        _window.ContentArea.Content = new ListEmployeesUC(_window, Company, null);
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
                MethodHandler.updateCompanyByDate(Company.IDCompany);
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
            // set career for NewEmployee before add data
            NewEmployee.Career = SelectCareer;
            // Only allow creators to edit
            // check tracker and username login is same
            if (_isEdit && Company.TrackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Bạn chỉ có quyền xem thông tin nhân viên.\nChỉ có người theo dõi công ty của nhân viên này mới được sửa thông tin.\nVui lòng lòng đăng nhập đúng tài khoản.");
                return;
            }
            if (_isEdit && !checkDifferent(_oldEmployee, NewEmployee))
            {
                // check data is change, if data is not change then return this here
                MessageBox.Show("Dữ liệu không được thay đổi");
                return;
            }
            if ((string.IsNullOrWhiteSpace(NewEmployee.StaffName)) || (string.IsNullOrWhiteSpace(NewEmployee.Birthday))
                || (string.IsNullOrWhiteSpace(NewEmployee.Passport)) || (string.IsNullOrWhiteSpace(NewEmployee.Address))
                || (string.IsNullOrWhiteSpace(NewEmployee.VisaNumber)) || (string.IsNullOrWhiteSpace(NewEmployee.TemporaryStay))
                || string.IsNullOrWhiteSpace(NewEmployee.Nationality.NationalityCode))
            {
                MessageBox.Show("Không được để trống các trường *");
                return;
            }
            if ((NewEmployee.WorkPermit == Constants.WorkPermitAvailable || NewEmployee.WorkPermit == Constants.WorkPermitAvailable_Invest) 
                && string.IsNullOrWhiteSpace(NewEmployee.WorkPermitNumber))
            {
                MessageBox.Show("Không được để trống các trường *");
                return;
            }
            if (NewEmployee.WorkingStatus == 1 && string.IsNullOrWhiteSpace(NewEmployee.DateOfLeave))
            {
                MessageBox.Show("Không được để trống các trường *");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectWard))
            {
                MessageBox.Show("Phường (Xã) để trống hoặc nhập sai.\nVui lòng kiểm tra lại");
                return;
            }
            // check format date
            if (!(MethodHandler.checkFormatDate(NewEmployee.Birthday) && MethodHandler.checkFormatDate(NewEmployee.TemporaryStay)))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            if(!string.IsNullOrWhiteSpace(NewEmployee.DateOfJoin) && !MethodHandler.checkFormatDate(NewEmployee.DateOfJoin))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            if (!string.IsNullOrWhiteSpace(NewEmployee.DateOfLeave) && !MethodHandler.checkFormatDate(NewEmployee.DateOfLeave))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            // check nationality code
            if (string.IsNullOrWhiteSpace(NewEmployee.Nationality.NationalityName))
            {
                MessageBox.Show("Vui lòng kiểm tra lại mã quốc tịch");
                return;
            }
            // check address
            int wardCount = 0;
            foreach (string str in listWards)
            {
                if (StringsConvert.RemoveDiacritics(SelectWard.ToUpper()).EndsWith(StringsConvert.RemoveDiacritics(str.ToUpper())))
                {
                    break;
                }
                wardCount++;
            }
            if (wardCount == listWards.Count)
            {
                MessageBox.Show("Địa chỉ không có phường/ xã trong lưu trữ.\nVui lòng kiểm tra lại");
                return;
            }
            // check passport number in this company
            // add new employee
            string sqlCompany = "select * from Employees where IDCompany = " + Company.IDCompany + " and Passport  like N'" + NewEmployee.Passport + "' and Hidden_flag = 0"
                + " and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
            if (_isEdit == false && (MethodHandler.checkExistInDatabase(sqlCompany)))
            {
                MessageBox.Show("Nhân viên có passport này đã có trong công ty.\n vui lòng kiểm tra lại");
                return;
            }
            // check work permit
            int emp_work_permit = NewEmployee.WorkPermit;
            if (SelectCareerGroup.IDCG == int.Parse(Constants.OtherCareerGroup))
            {
                if (emp_work_permit != Constants.WorkPermitOther)
                {
                    MessageBox.Show("Nhóm nghề là khác thì chỉ được chọn số GPLĐ là khác.\nVui lòng chọn lại Số GPLĐ cho phù hợp.");
                    return;
                }
            } else
            {
                // check employee is investment
                bool isInvest = MethodHandler.checkInvestment(Company.IDCompany, NewEmployee.Passport);
                if (isInvest)
                {
                    if (emp_work_permit != Constants.WorkPermitExemption_Invest && emp_work_permit != Constants.WorkPermitAvailable_Invest
                        && emp_work_permit != Constants.WorkPermitNotYet_Invest)
                    {
                        MessageBox.Show("Số hộ chiếu là nhà đầu tư.\nVui lòng chọn lại Số GPLĐ cho phù hợp.");
                        return;
                    }
                }
                else
                {
                    if (emp_work_permit != Constants.WorkPermitExemption && emp_work_permit != Constants.WorkPermitAvailable
                        && emp_work_permit != Constants.WorkPermitNotYet)
                    {
                        MessageBox.Show("Số hộ chiếu là người lao động.\nVui lòng chọn lại Số GPLĐ cho phù hợp.");
                        return;
                    }
                }
            }
            
            // edit employee
            string sqlPassportUsed = "select * from Employees where IDCompany = " + Company.IDCompany + " and Passport  like N'"
                + NewEmployee.Passport + "' and IDEmployee != " + (_oldEmployee == null ? -1 : _oldEmployee.IDEmployee) + " and Hidden_flag = 0";
            if (_isEdit && (MethodHandler.countDataInDatebase(sqlPassportUsed) == 1))
            {
                MessageBox.Show("Nhân viên có passport này đã có trong công ty.\n vui lòng kiểm tra lại");
                return;
            }
            // has employee working another company?
            bool checkOtherCompany = true;
            string sqlAnotherCompany = "select * from Employees where IDCompany != " + Company.IDCompany + " and Passport  like N'" + NewEmployee.Passport 
                + "' and Hidden_flag = 0 and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
            if (MethodHandler.checkExistInDatabase(sqlAnotherCompany))
            {
                MessageBoxResult confirm = MessageBox.Show("Nhân viên có số passport này đang làm việc tại một công ty khác.\nBạn muốn tiếp tục không?", "Xác nhận", MessageBoxButton.YesNo);
                if (confirm == MessageBoxResult.No)
                {
                    checkOtherCompany = false;
                }
            }
            if (!checkOtherCompany)
            {
                return;
            }
            // check settlmentResultString
            string visaNumber = "", cardCreationDate = "", temporaryStay = "";
            if (!string.IsNullOrWhiteSpace(NewEmployee.SettlementResultsString))
            {
                if (!MethodHandler.checkSettlementResultsString(NewEmployee.SettlementResultsString.Trim(), out visaNumber, out cardCreationDate, out temporaryStay))
                {
                    MessageBox.Show("Sai định dạng kết quả giải quyết\n (DNxxx) dd/MM/yyyy-dd/MM/yyyy");
                    return;
                }
                // check temporary stay and settlement results
                if (!MethodHandler.formatDatefromUsertoDatabase(NewEmployee.TemporaryStay).Trim().Equals(MethodHandler.formatDatefromUsertoDatabase(temporaryStay).Trim()))
                {
                    MessageBox.Show("Thời hạn tạm trú nhập vào và kết quả giải quyết là khác nhau");
                    return;
                }
                // check vissa number
                if ((!string.IsNullOrWhiteSpace(visaNumber))&&(!NewEmployee.VisaNumber.Trim().ToUpper().Equals(visaNumber.ToUpper().Trim())))
                {
                    MessageBox.Show("Thẻ tạm trú nhập vào và kết quả giải quyết là khác nhau");
                    return;
                }
            }
            else
            {
                // settlement result is 3
                NewEmployee.SettlementResults = 3;
                NewEmployee.SettlementResultsString = " ";
            }
            
            bool checkConfirm = true;
            if (_isEdit && !isSame(_oldEmployee.Passport, NewEmployee.Passport))
            {
                MessageBoxResult confirm = MessageBox.Show("Bạn vừa sửa số hộ chiếu\nHai số hộ chiếu là cùng một người phải không?", "Xác nhận", MessageBoxButton.YesNo);
                if (confirm == MessageBoxResult.No)
                {
                    checkConfirm = false;
                }
            }
            
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    DateTime today = DateTime.Now;
                    con.Open();
                    SqlCommand comm;
                    string addressFull = NewEmployee.Address + ", " + SelectWard;
                    string insertSQL = "insert into Employees values(N'" + MethodHandler.convertStringOwned(NewEmployee.StaffName) + "', " + NewEmployee.Gender + ", '" + MethodHandler.formatDatefromUsertoDatabase(NewEmployee.Birthday)
                        + "', N'" + NewEmployee.Nationality.NationalityCode + "', N'" + NewEmployee.Passport + "', N'" + addressFull
                        + "', " + NewEmployee.Career.IDCareer + ", " + NewEmployee.WorkPermit + ",N'"
                        + (string.IsNullOrEmpty(NewEmployee.WorkPermitNumber) ? " " : NewEmployee.WorkPermitNumber) + "', N'" + NewEmployee.VisaNumber
                        + "', '" + MethodHandler.formatDatefromUsertoDatabase(NewEmployee.TemporaryStay) + "'," + NewEmployee.SettlementResults + ", N'" + NewEmployee.SettlementResultsString
                        + "', " + Constants.IdUser + ", " + Company.IDCompany + ", 0, N'"
                        + (string.IsNullOrEmpty(NewEmployee.Note) ? " " : NewEmployee.Note)
                        + "', '" + today.ToString("yyyy-MM-dd HH:mm:ss")
                        + "', " + (string.IsNullOrWhiteSpace(cardCreationDate) ? "null" : "'" + MethodHandler.formatDatefromUsertoDatabase(cardCreationDate) + "'")
                        + ", " + NewEmployee.WorkingStatus + ", "+ (string.IsNullOrWhiteSpace(NewEmployee.DateOfJoin) ? "null" : "'" + MethodHandler.formatDatefromUsertoDatabase(NewEmployee.DateOfJoin) + "'") 
                        + ", "+ (string.IsNullOrWhiteSpace(NewEmployee.DateOfLeave) ? "null" : "'" + MethodHandler.formatDatefromUsertoDatabase(NewEmployee.DateOfLeave) + "'") + ")";
                    comm = new SqlCommand(insertSQL, con);
                    comm.ExecuteNonQuery();
                    if (_isEdit && checkConfirm)
                    {
                        comm = new SqlCommand("update Employees set Hidden_flag = 1 where IDEmployee = " + _oldEmployee.IDEmployee, con);
                        comm.ExecuteNonQuery();
                    }
                    //BtnCancelCommand.Execute(true);
                    _window.ContentArea.Content = new ListEmployeesUC(_window, Company, null);
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
            MethodHandler.updateCompanyByDate(Company.IDCompany);
        }

        public ICommand BtnEditEmployeeCommand
        {
            get
            {
                return _btnEditEmployeeCommand ?? (_btnEditEmployeeCommand = new CommandHandler(EditEmployee, true));
            }
        }
        private void EditEmployee()
        {
            // set career for NewEmployee before update data
            NewEmployee.Career = SelectCareer;
            // Only allow creators to edit
            // check tracker and username login is same
            if (_isEdit && Company.TrackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Bạn chỉ có quyền xem thông tin nhân viên.\nChỉ có người theo dõi công ty của nhân viên này mới được cập nhật thông tin.\nVui lòng lòng đăng nhập đúng tài khoản.");
                return;
            }
            if (_isEdit && !checkDifferent(_oldEmployee, NewEmployee))
            {
                // check data is change, if data is not change then return this here
                MessageBox.Show("Dữ liệu không được thay đổi");
                return;
            }
            if ((string.IsNullOrEmpty(NewEmployee.StaffName)) || (string.IsNullOrWhiteSpace(NewEmployee.Birthday))
                || (string.IsNullOrWhiteSpace(NewEmployee.Passport)) || (string.IsNullOrWhiteSpace(NewEmployee.Address))
                || (string.IsNullOrWhiteSpace(NewEmployee.VisaNumber)) || (string.IsNullOrWhiteSpace(NewEmployee.TemporaryStay))
                || (string.IsNullOrWhiteSpace(NewEmployee.Nationality.NationalityCode)))
            {
                MessageBox.Show("Không được để trống các trường *");
                return;
            }
            if((NewEmployee.WorkPermit == Constants.WorkPermitAvailable || NewEmployee.WorkPermit == Constants.WorkPermitAvailable_Invest) 
                && string.IsNullOrWhiteSpace(NewEmployee.WorkPermitNumber))
            {
                MessageBox.Show("Không được để trống các trường *");
                return;
            }
            if (NewEmployee.WorkingStatus == 1 && string.IsNullOrWhiteSpace(NewEmployee.DateOfLeave))
            {
                MessageBox.Show("Không được để trống các trường *");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectWard))
            {
                MessageBox.Show("Phường (Xã) để trống hoặc nhập sai.\nVui lòng kiểm tra lại");
                return;
            }
            if (!(MethodHandler.checkFormatDate(NewEmployee.Birthday) && MethodHandler.checkFormatDate(NewEmployee.TemporaryStay)))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            if (!string.IsNullOrWhiteSpace(NewEmployee.DateOfJoin) && !MethodHandler.checkFormatDate(NewEmployee.DateOfJoin))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            if (!string.IsNullOrWhiteSpace(NewEmployee.DateOfLeave) && !MethodHandler.checkFormatDate(NewEmployee.DateOfLeave))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            // check nationality code
            if (string.IsNullOrWhiteSpace(NewEmployee.Nationality.NationalityName))
            {
                MessageBox.Show("Vui lòng kiểm tra lại mã quốc tịch");
                return;
            }
            // check address
            int wardCount = 0;
            foreach (string str in listWards)
            {
                if (StringsConvert.RemoveDiacritics(SelectWard.ToUpper()).EndsWith(StringsConvert.RemoveDiacritics(str.ToUpper())))
                {
                    break;
                }
                wardCount++;
            }
            if (wardCount == listWards.Count)
            {
                MessageBox.Show("Địa chỉ không có phường/ xã trong lưu trữ.\nVui lòng kiểm tra lại");
                return;
            }
            // check passport number in this company
            string sqlPassportUsed = "select * from Employees where IDCompany = " + Company.IDCompany + " and Passport  like N'"
                + NewEmployee.Passport + "' and IDEmployee != " + (_oldEmployee == null ? -1 : _oldEmployee.IDEmployee) + " and Hidden_flag = 0"
                + " and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
            // edit employee
            if (_isEdit && (MethodHandler.countDataInDatebase(sqlPassportUsed) == 1))
            {
                MessageBox.Show("Nhân viên có passport này đã có trong công ty.\n vui lòng kiểm tra lại");
                return;
            }
            // has employee working another company?
            bool checkOtherCompany = true;
            string sqlAnotherCompany = "select * from Employees where IDCompany != " + Company.IDCompany + " and Passport  like N'" + NewEmployee.Passport 
                + "' and Hidden_flag = 0 and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
            if (MethodHandler.checkExistInDatabase(sqlAnotherCompany))
            {
                MessageBoxResult confirm = MessageBox.Show("Nhân viên có số passport này đang làm việc tại một công ty khác.\nBạn muốn tiếp tục không?", "Xác nhận", MessageBoxButton.YesNo);
                if (confirm == MessageBoxResult.No)
                {
                    checkOtherCompany = false;
                }
            }
            if (!checkOtherCompany)
            {
                return;
            }

            // check work permit
            int emp_work_permit = NewEmployee.WorkPermit;
            if (SelectCareerGroup.IDCG == int.Parse(Constants.OtherCareerGroup))
            {
                if (emp_work_permit != Constants.WorkPermitOther)
                {
                    MessageBox.Show("Nhóm nghề là khác thì chỉ được chọn số GPLĐ là khác.\nVui lòng chọn lại Số GPLĐ cho phù hợp.");
                    return;
                }
            }
            else
            {
                // check employee is investment
                bool isInvest = MethodHandler.checkInvestment(Company.IDCompany, NewEmployee.Passport);
                if (isInvest)
                {
                    if (emp_work_permit != Constants.WorkPermitExemption_Invest && emp_work_permit != Constants.WorkPermitAvailable_Invest
                        && emp_work_permit != Constants.WorkPermitNotYet_Invest)
                    {
                        MessageBox.Show("Số hộ chiếu là nhà đầu tư.\nVui lòng chọn lại Số GPLĐ cho phù hợp.");
                        return;
                    }
                }
                else
                {
                    if (emp_work_permit != Constants.WorkPermitExemption && emp_work_permit != Constants.WorkPermitAvailable
                        && emp_work_permit != Constants.WorkPermitNotYet)
                    {
                        MessageBox.Show("Số hộ chiếu là người lao động.\nVui lòng chọn lại Số GPLĐ cho phù hợp.");
                        return;
                    }
                }
            }

            // check settlmentResultString
            string visaNumber = "", cardCreationDate = "", temporaryStay = "";
            if (!string.IsNullOrWhiteSpace(NewEmployee.SettlementResultsString))
            {
                if (!MethodHandler.checkSettlementResultsString(NewEmployee.SettlementResultsString.Trim(),out visaNumber, out cardCreationDate, out temporaryStay))
                {
                    MessageBox.Show("Sai định dạng kết quả giải quyết\n (DNxxx) dd/MM/yyyy-dd/MM/yyyy");
                    return;
                }
                // check temporary stay and settlement results
                if (!MethodHandler.formatDatefromUsertoDatabase(NewEmployee.TemporaryStay).Trim().Equals(MethodHandler.formatDatefromUsertoDatabase(temporaryStay).Trim()))
                {
                    MessageBox.Show("Thời hạn tạm trú nhập vào và kết quả giải quyết là khác nhau");
                    return;
                }
                // check vissa number
                if ((!string.IsNullOrWhiteSpace(visaNumber)) && (!NewEmployee.VisaNumber.Trim().ToUpper().Equals(visaNumber.ToUpper().Trim())))
                {
                    MessageBox.Show("Thẻ tạm trú nhập vào và kết quả giải quyết là khác nhau");
                    return;
                }
            }
            else
            {
                // settlement result is 3
                NewEmployee.SettlementResults = 3;
                NewEmployee.SettlementResultsString = " ";
            }
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    con.Open();
                    string addressFull = NewEmployee.Address + ", " + SelectWard;
                    string sql = "update Employees set StaffName = N'" + MethodHandler.convertStringOwned(NewEmployee.StaffName) + "', Gender = " + NewEmployee.Gender
                        + ", Birthday = '" + MethodHandler.formatDatefromUsertoDatabase(NewEmployee.Birthday) + "', Passport = N'" + NewEmployee.Passport
                        + "', Nationality = N'" + NewEmployee.Nationality.NationalityCode + "', Address = N'" + addressFull + "', IDCareer = " + NewEmployee.Career.IDCareer
                        + ", WorkPermit = " + NewEmployee.WorkPermit + ", WorkPermitNumber = N'" + (string.IsNullOrEmpty(NewEmployee.WorkPermitNumber) ? " " : NewEmployee.WorkPermitNumber)
                        + "', VisaNumber = N'" + NewEmployee.VisaNumber + "', TemporaryStay = '" + MethodHandler.formatDatefromUsertoDatabase(NewEmployee.TemporaryStay)
                        + "', SettlementResults = " + NewEmployee.SettlementResults + ", SettlementResultsString = N'" + NewEmployee.SettlementResultsString
                        + "', Note = N'" + (string.IsNullOrEmpty(NewEmployee.Note) ? " " : NewEmployee.Note)
                        + "', CardCreationDate = " + (string.IsNullOrWhiteSpace(cardCreationDate) ? "null" : "'" + MethodHandler.formatDatefromUsertoDatabase(cardCreationDate) + "'")
                        + ", WorkingStatus = " + NewEmployee.WorkingStatus + ", DateOfJoin = " + (string.IsNullOrWhiteSpace(NewEmployee.DateOfJoin) ? "null" : "'" + MethodHandler.formatDatefromUsertoDatabase(NewEmployee.DateOfJoin) + "'")
                        + ", DateOfLeave = " + (string.IsNullOrWhiteSpace(NewEmployee.DateOfLeave) ? "null" : "'" + MethodHandler.formatDatefromUsertoDatabase(NewEmployee.DateOfLeave) + "'")
                        + " where IDEmployee = " + NewEmployee.IDEmployee;
                    SqlCommand comm;
                    comm = new SqlCommand(sql, con);
                    comm.ExecuteNonQuery();
                    //BtnCancelCommand.Execute(true);
                    _window.ContentArea.Content = new ListEmployeesUC(_window, Company, null);
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
            MethodHandler.updateCompanyByDate(Company.IDCompany);
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
