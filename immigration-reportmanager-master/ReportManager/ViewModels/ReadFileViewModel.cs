using Microsoft.Win32;
using OfficeOpenXml;
using ReportManager.Bases;
using ReportManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
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
    class ReadFileViewModel : INotifyPropertyChanged
    {
        #region properties
        public static readonly string PathStringProperty = "PathString";
        public static readonly string SheetProperty = "Sheet";
        public static readonly string StartRowProperty = "StartRow";
        public static readonly string EndRowProperty = "EndRow";
        public static readonly string ListCareerGroupProperty = "ListCareerGroup";
        public static readonly string SelectCareerGroupProperty = "SelectCareerGroup";
        public static readonly string StaffNameProperty = "StaffName";
        public static readonly string GenderProperty = "Gender";
        public static readonly string BirthdayProperty = "Birthday";
        public static readonly string PassportProperty = "Passport";
        public static readonly string NationalityProperty = "Nationality";
        public static readonly string AddressProperty = "Address";
        public static readonly string CareerProperty = "Career";
        public static readonly string WorkPermitProperty = "WorkPermit";
        public static readonly string VisaNumberProperty = "VisaNumber";
        public static readonly string TemporaryStayProperty = "TemporaryStay";
        public static readonly string SettlementResultsProperty = "SettlementResults";
        public static readonly string NoteProperty = "Note";
        private int _idCompany;
        private string _pathString;
        private int _sheet;
        private int _startRow;
        private int _endRow;
        private ObservableCollection<CareerGroup> _listCareerGroup;
        private CareerGroup _selectCareerGroup;
        private int _staffName;
        private int _gender;
        private int _birthday;
        private int _passport;
        private int _nationality;
        private int _address;
        private int _career;
        private int _workPermit;
        private int _visaNumber;
        private int _temporaryStay;
        private int _settlementResults;
        private int _note;
        private ReadFileWindow _window;
        private ICommand _btnOpenFileCommand;
        private ICommand _btnReadFileCommand;
        private ICommand _btnCancelCommand;
        private UserControl preUserControl;
        #endregion

        #region set get method
        public ReadFileViewModel(ReadFileWindow window, UserControl userControl, int idCompany)
        {
            _window = window;
            preUserControl = userControl;
            _idCompany = idCompany;
            Sheet = StartRow = EndRow = 0;
            ListCareerGroup = MethodHandler.getListCareerGroup();
            if (ListCareerGroup.Count > 0)
            {
                SelectCareerGroup = ListCareerGroup[0];
            }
            StaffName = 2;
            Gender = 3;
            Birthday = 4;
            Passport = 6;
            Nationality = 5;
            Address = 7;
            Career = 8;
            WorkPermit = 9;
            VisaNumber = 10;
            TemporaryStay = 11;
            SettlementResults = 20;
            Note = 12;
        }
        public string PathString
        {
            get
            {
                return _pathString;
            }
            set
            {
                this.SetValue(ref _pathString, value, PathStringProperty);
            }
        }
        public int Sheet
        {
            get
            {
                return _sheet;
            }
            set
            {
                this.SetValue(ref _sheet, value, SheetProperty);
            }
        }
        public int StartRow
        {
            get
            {
                return _startRow;
            }
            set
            {
                this.SetValue(ref _startRow, value, StartRowProperty);
            }
        }
        public int EndRow
        {
            get
            {
                return _endRow;
            }
            set
            {
                this.SetValue(ref _endRow, value, EndRowProperty);
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
            }
        }
        public int StaffName
        {
            get
            {
                return _staffName;
            }
            set
            {
                this.SetValue(ref _staffName, value, StaffNameProperty);
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
            }
        }
        public int Birthday
        {
            get
            {
                return _birthday;
            }
            set
            {
                this.SetValue(ref _birthday, value, BirthdayProperty);
            }
        }
        public int Passport
        {
            get
            {
                return _passport;
            }
            set
            {
                this.SetValue(ref _passport, value, PassportProperty);
            }
        }
        public int Nationality
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
        public int Address
        {
            get
            {
                return _address;
            }
            set
            {
                this.SetValue(ref _address, value, AddressProperty);
            }
        }
        public int Career
        {
            get
            {
                return _career;
            }
            set
            {
                this.SetValue(ref _career, value, CareerProperty);
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
            }
        }
        public int VisaNumber
        {
            get
            {
                return _visaNumber;
            }
            set
            {
                this.SetValue(ref _visaNumber, value, VisaNumberProperty);
            }
        }
        public int TemporaryStay
        {
            get
            {
                return _temporaryStay;
            }
            set
            {
                this.SetValue(ref _temporaryStay, value, TemporaryStayProperty);
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
            }
        }
        public int Note
        {
            get
            {
                return _note;
            }
            set
            {
                this.SetValue(ref _note, value, NoteProperty);
            }
        }
        #endregion

        #region method ICommand
        public ICommand BtnOpenFileCommand
        {
            get
            {
                return _btnOpenFileCommand ?? (_btnOpenFileCommand = new CommandHandler(OpenFile, true));
            }
        }
        private void OpenFile()
        {
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.Filter = "Excel Files (*.xls, *.xlsx)|*.xls;*.xlsx|CSV Files (*.csv)|*.csv";
            var result = sfd.ShowDialog();
            if (result == false)
            {
                return;
            }
            PathString = sfd.FileName;
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
            if (string.IsNullOrWhiteSpace(PathString))
            {
                MessageBox.Show("Vui lòng chọn một file excel để mở");
                return;
            }
            if(Sheet == 0)
            {
                MessageBox.Show("Vui lòng chọn một sheet để đọc");
                return;
            }
            if(StartRow == 0 || EndRow == 0)
            {
                MessageBox.Show("Vui lòng nhập dòng để đọc dữ liệu");
                return;
            } else if( StartRow > EndRow)
            {
                MessageBox.Show("Dòng bắt đầu không thể lớn hơn dòng kết thúc");
                return;
            }
            if(StaffName == 0 || Gender == 0 || Birthday== 0|| Passport==0|| Nationality==0
                || Address==0|| Career==0|| WorkPermit==0 || VisaNumber==0|| TemporaryStay==0
                || SettlementResults==0|| Note == 0)
            {
                MessageBox.Show("Vui lòng kiểm tra lại các cột");
                return;
            }
            // save data from file
            ObservableCollection<Employee> listLoadData = new ObservableCollection<Employee>();
            // save info employees not added
            ObservableCollection<Employee> listErrorEmployees = new ObservableCollection<Employee>();
            try
            {
                var package = new ExcelPackage(new FileInfo(PathString));
                ExcelWorksheet workSheet = package.Workbook.Worksheets[Sheet];
                for (int i = StartRow; i <= EndRow; i++)
                {
                    try
                    {
                        Employee employ = new Employee();
                        var vstaffName = workSheet.Cells[i, StaffName].Value;
                        if (vstaffName != null && !string.IsNullOrWhiteSpace(vstaffName.ToString()))
                        {
                            employ.StaffName = vstaffName.ToString().Trim();
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc tên nhân viên tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }
                        var vpassport = workSheet.Cells[i, Passport].Value;
                        if (vpassport != null && !string.IsNullOrWhiteSpace(vpassport.ToString()))
                        {
                            employ.Passport = vpassport.ToString().Trim();
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc số hộ chiếu tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }
                        var vgender = workSheet.Cells[i, Gender].Value;
                        if (vgender != null && !string.IsNullOrWhiteSpace(vgender.ToString()))
                        {
                            if ("NAM".Equals(vgender.ToString().Trim().ToUpper()))
                            {
                                employ.Gender = 1;
                            }
                            else
                            {
                                employ.Gender = 2;
                            }
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc giới tính tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }
                        var vbirthday = workSheet.Cells[i, Birthday].Value;
                        long dateNum;
                        if (long.TryParse(vbirthday.ToString(), out dateNum))
                        {
                            DateTime result = DateTime.FromOADate(dateNum);
                            vbirthday = result.ToString("dd/MM/yyyy");
                        }
                        string[] checkBirthday = vbirthday.ToString().Split(' ');
                        int bi = 0;
                        foreach(string str in checkBirthday)
                        {
                            if (MethodHandler.checkFormatDate(str))
                            {
                                vbirthday = str.Trim();
                                break;
                            }
                            bi++;
                        }
                        if(bi == checkBirthday.Length)
                        {
                            vbirthday = null;
                        }
                        if (vbirthday != null && !string.IsNullOrWhiteSpace(vbirthday.ToString()))
                        {
                            employ.Birthday = vbirthday.ToString();
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc ngày sinh nhật tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }
                        var vnationality = workSheet.Cells[i, Nationality].Value;
                        if (vnationality != null && !string.IsNullOrWhiteSpace(vnationality.ToString()))
                        {
                            employ.Nationality.NationalityName = vnationality.ToString().Trim();
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc quốc tịch tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }
                        var vaddress = workSheet.Cells[i, Address].Value;
                        if (vaddress != null && !string.IsNullOrWhiteSpace(vaddress.ToString()))
                        {
                            employ.Address = vaddress.ToString().Trim();
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc địa chỉ tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }
                        var vcareer = workSheet.Cells[i, Career].Value;
                        if (vcareer != null && !string.IsNullOrWhiteSpace(vcareer.ToString()))
                        {
                            employ.Career.CareerName = vcareer.ToString().Trim();
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc tên nghề nghiệp tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }

                        var vworkPermit = workSheet.Cells[i, WorkPermit].Value;
                        //check passport is invest?
                        bool isInvestment = MethodHandler.checkInvestment(_idCompany, employ.Passport);
                        if (vworkPermit != null && !string.IsNullOrWhiteSpace(vworkPermit.ToString()))
                        {
                            ////employ.WorkPermit = int.Parse(workSheet.Cells[i, WorkPermit].Value.ToString());
                            if (StringsConvert.RemoveDiacritics(vworkPermit.ToString().ToUpper().Trim()).IndexOf(StringsConvert.RemoveDiacritics("Miễn".ToUpper())) >= 0)
                            {
                                employ.WorkPermit = isInvestment ? Constants.WorkPermitExemption_Invest : Constants.WorkPermitExemption;
                                employ.WorkPermitNumber = " ";
                            } else if (StringsConvert.RemoveDiacritics(vworkPermit.ToString().ToUpper().Trim()).IndexOf(StringsConvert.RemoveDiacritics("Khác".ToUpper())) >= 0)
                            {
                                employ.WorkPermit = Constants.WorkPermitOther;
                                employ.WorkPermitNumber = " ";
                            } else
                            {
                                employ.WorkPermit = isInvestment ? Constants.WorkPermitAvailable_Invest : Constants.WorkPermitAvailable;
                                employ.WorkPermitNumber = vworkPermit.ToString().Trim();
                            }
                        }
                        else
                        {
                            // not yet
                            employ.WorkPermit = isInvestment ? Constants.WorkPermitNotYet_Invest: Constants.WorkPermitNotYet;
                            employ.WorkPermitNumber = " ";
                        }

                        var vvisaNumber = workSheet.Cells[i, VisaNumber].Value;
                        if (vvisaNumber != null && !string.IsNullOrWhiteSpace(vvisaNumber.ToString()))
                        {
                            employ.VisaNumber = vvisaNumber.ToString().Trim();
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc số visa tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }
                        var vtemporaryStay = workSheet.Cells[i, TemporaryStay].Value;
                        long temporaryNum;
                        if (long.TryParse(vtemporaryStay.ToString(), out temporaryNum))
                        {
                            DateTime result = DateTime.FromOADate(temporaryNum);
                            vtemporaryStay = result.ToString("dd/MM/yyyy");
                        }
                        string[] checkTemporary = vtemporaryStay.ToString().Split(' ');
                        int ti = 0;
                        foreach (string str in checkTemporary)
                        {
                            if (MethodHandler.checkFormatDate(str))
                            {
                                vtemporaryStay = str.Trim();
                                break;
                            }
                            ti++;
                        }
                        if (ti == checkTemporary.Length)
                        {
                            vtemporaryStay = null;
                        }
                        if (vtemporaryStay != null && !string.IsNullOrWhiteSpace(vtemporaryStay.ToString()))
                        {
                            employ.TemporaryStay = vtemporaryStay.ToString();
                        }
                        else
                        {
                            employ.DateCreated = "Lỗi đọc thời hạn tạm trú tại dòng " + i;
                            listErrorEmployees.Add(employ);
                            continue;
                        }
                        var vsettlementResults = workSheet.Cells[i, SettlementResults].Value;
                        if (vsettlementResults !=null && !string.IsNullOrWhiteSpace(vsettlementResults.ToString()))
                        {                     
                            if (StringsConvert.RemoveDiacritics(vsettlementResults.ToString().ToUpper().Trim()).IndexOf(StringsConvert.RemoveDiacritics("GHTT:".ToUpper())) == 0)
                            {
                                employ.SettlementResults = 0;
                                employ.SettlementResultsString = vsettlementResults.ToString().Trim().Substring("GHTT:".Length).Trim();
                            } else if (StringsConvert.RemoveDiacritics(vsettlementResults.ToString().ToUpper().Trim()).IndexOf(StringsConvert.RemoveDiacritics("Visa:".ToUpper())) == 0)
                            {
                                employ.SettlementResults = 1;
                                employ.SettlementResultsString = vsettlementResults.ToString().Trim().Substring("Visa:".Length - 1).Trim();
                            }
                            else if (StringsConvert.RemoveDiacritics(vsettlementResults.ToString().ToUpper().Trim()).IndexOf(StringsConvert.RemoveDiacritics("TTT:".ToUpper())) == 0)
                            {
                                employ.SettlementResults = 2;
                                employ.SettlementResultsString = vsettlementResults.ToString().Trim().Substring("TTT:".Length - 1).Trim();
                            }
                            else
                            {
                                employ.SettlementResults = 3;
                                employ.SettlementResultsString = vsettlementResults.ToString().Trim();
                            }
                            string visaNumber = "", cardCreation = "", temporaryStay = "";
                            if(MethodHandler.checkSettlementResultsString(employ.SettlementResultsString,out visaNumber, out cardCreation, out temporaryStay))
                            {
                                employ.CardCreationDate = cardCreation;
                                // check temporary stay and settlement results
                                if ((!string.IsNullOrWhiteSpace(temporaryStay)) && (!MethodHandler.formatDatefromUsertoDatabase(employ.TemporaryStay).Trim().Equals(MethodHandler.formatDatefromUsertoDatabase(temporaryStay).Trim())))
                                {
                                    employ.DateCreated = "Thời hạn tạm trú nhập vào và kết quả giải quyết là khác nhau tại dòng " + i;
                                    listErrorEmployees.Add(employ);
                                    continue;
                                }
                                // check visa number
                                if ((!string.IsNullOrWhiteSpace(visaNumber)) && (!employ.VisaNumber.Trim().ToUpper().Equals(visaNumber.ToUpper().Trim())))
                                {
                                    employ.DateCreated = "Thẻ tạm trú nhập vào và kết quả giải quyết là khác nhau tại dòng " + i;
                                    listErrorEmployees.Add(employ);
                                    continue;
                                }
                            }
                            else
                            {
                                employ.DateCreated = "Sai định dạng kết quả giải quyết tại dòng " + i;
                                listErrorEmployees.Add(employ);
                                continue;
                            }
                        }
                        else
                        {
                            // when read cell is null or space then default is other
                            employ.SettlementResults = 3;
                            employ.SettlementResultsString = " ";
                        }
                        var vnote = workSheet.Cells[i, Note].Value;
                        if (vnote != null && !string.IsNullOrWhiteSpace(vnote.ToString()))
                        {
                            employ.Note = vnote.ToString();
                        }
                        else
                        {
                            employ.Note = "";
                        }
                        listLoadData.Add(employ);

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            // list passport of company
            ObservableCollection<string> listPassport = MethodHandler.getPassportByCompany(_idCompany);
            foreach (Employee employee in listLoadData)
            {
                listPassport.Remove(employee.Passport);
                if (checkExist(employee))
                {
                    if (checkDifferent(employee))
                    {
                        //if (!addEmployee(employee, true))
                        //{
                        //    listErrorEmployees.Add(employee);
                        //}
                        employee.DateCreated = "Nhân viên có thay đổi";
                    }
                    listErrorEmployees.Add(employee);
                    continue;
                }
                else
                {
                    if (!addEmployee(employee))
                    {
                        listErrorEmployees.Add(employee);
                    }
                }
            }
            //check listPassport
            if (listPassport.Count > 0)
            {
                foreach (string passport in listPassport)
                {
                    Employee employeeByPassport = MethodHandler.getEmployeeByPassport(_idCompany, passport);
                    if (employeeByPassport == null || string.IsNullOrWhiteSpace(employeeByPassport.StaffName))
                    {
                        continue;
                    }
                    employeeByPassport.DateCreated = "Nhân viên còn hạn tạm trú nhưng không được cập nhật trong file";
                    listErrorEmployees.Add(employeeByPassport);
                }
            }
            //check error
            if (listErrorEmployees.Count > 0)
            {
                // show window error employees
                ErrorListWindow errWindow = new ErrorListWindow(listErrorEmployees);
                _window.Close();
                errWindow.Show();
            }
            else
            {
                MessageBox.Show("Thêm nhân viên thành công");
                _window.Close();
            }
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
            _window.Close();
        }
        #endregion

        #region method
        /// <summary>
        /// check by passport
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        private bool checkExist(Employee employee)
        {
            if (!string.IsNullOrWhiteSpace(employee.Passport))
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.SelectCommand = new SqlCommand("select * from Employees where IDCompany = " + _idCompany + " and Passport like N'" + MethodHandler.convertStringOwned(employee.Passport) + "'", con);
                        adapter.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            con.Close();
                            return true;
                        }
                        else
                        {
                            con.Close();
                            return false;
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
            else
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// if different then return true
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        private bool checkDifferent(Employee employee)
        {
            ObservableCollection<Employee> listData = new ObservableCollection<Employee>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sqlSelect = "select *, CONVERT(varchar, Birthday, 105) birth, CONVERT(varchar, TemporaryStay, 105) temporary "
                        + "from Nationality, Employees where Nationality.NationalityCode like Employees.Nationality and Passport like N'" + MethodHandler.convertStringOwned(employee.Passport) + "'";
                    adapter.SelectCommand = new SqlCommand(sqlSelect, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
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
                            // get career
                            DataTable dtGet = new DataTable();
                            adapter.SelectCommand = new SqlCommand("select CareerName from Careers where IDCareer = " + rowEmployee.Career.IDCareer, con);
                            adapter.Fill(dtGet);
                            if (dtGet.Rows.Count > 0)
                            {
                                foreach (DataRow row in dtGet.Rows)
                                {
                                    rowEmployee.Career.CareerName = row["CareerName"].ToString();
                                }
                            }
                            listData.Add(rowEmployee);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có nhân viên");
                    }
                    foreach(Employee item in listData)
                    {
                        int check = 0;
                        if (StringsConvert.RemoveDiacritics(item.StaffName.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.StaffName.Trim().ToUpper())))
                        {
                            check++;
                        }
                        if (item.Gender == employee.Gender)
                        {
                            check++;
                        }
                        if (item.Birthday.Trim().Equals(fullDate(employee.Birthday.Trim())))
                        {
                            check++;
                        }
                        if (StringsConvert.RemoveDiacritics(item.Nationality.NationalityName.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.Nationality.NationalityName.Trim().ToUpper())))
                        {
                            check++;
                        }
                        if (StringsConvert.RemoveDiacritics(item.Passport.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.Passport.Trim().ToUpper())))
                        {
                            check++;
                        }
                        if (StringsConvert.RemoveDiacritics(item.Address.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.Address.Trim().ToUpper())))
                        {
                            check++;
                        }
                        if (StringsConvert.RemoveDiacritics(item.Career.CareerName.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.Career.CareerName.Trim().ToUpper())))
                        {
                            check++;
                        }
                        if (item.WorkPermit == employee.WorkPermit)
                        {
                            check++;
                        }
                        if ((string.IsNullOrWhiteSpace(item.WorkPermitNumber) && string.IsNullOrWhiteSpace(employee.WorkPermitNumber)) || (StringsConvert.RemoveDiacritics(item.WorkPermitNumber.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.WorkPermitNumber.Trim().ToUpper()))))
                        {
                            check++;
                        }
                        if (StringsConvert.RemoveDiacritics(item.VisaNumber.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.VisaNumber.Trim().ToUpper())))
                        {
                            check++;
                        }
                        if (item.TemporaryStay.Trim().Equals(fullDate(employee.TemporaryStay.Trim())))
                        {
                            check++;
                        }
                        if (StringsConvert.RemoveDiacritics(item.SettlementResultsString.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.SettlementResultsString.Trim().ToUpper())))
                        {
                            check++;
                        }
                        if ((string.IsNullOrWhiteSpace(item.Note) && string.IsNullOrWhiteSpace(employee.Note)) ||(StringsConvert.RemoveDiacritics(item.Note.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.Note.Trim().ToUpper()))))
                        {
                            check++;
                        }
                        if(check == 13)
                        {
                            con.Close();
                            employee.DateCreated = "Thông tin đã có sẵn";
                            break;
                        }
                    }
                    con.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            if (!string.IsNullOrWhiteSpace(employee.DateCreated))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private string fullDate(string sDate)
        {
            // full string date
            string str = "";
            string[] dmy1 = sDate.Split('-');
            string[] dmy2 = sDate.Split('/');
            if (dmy1.Length == 3)
            {
                for (int i = 0; i < dmy1.Length; i++)
                {
                    int number;
                    if (int.TryParse(dmy1[i], out number))
                    {
                        if (number < 10)
                        {
                            dmy1[i] = "0" + number;
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                str = dmy1[0] + "-" + dmy1[1] + "-" + dmy1[2];
            }
            else if (dmy2.Length == 3)
            {
                for (int i = 0; i < dmy2.Length; i++)
                {
                    int number;
                    if (int.TryParse(dmy2[i], out number))
                    {
                        if (number < 10)
                        {
                            dmy2[i] = "0" + number;
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                str = dmy2[0] + "-" + dmy2[1] + "-" + dmy2[2];
            }
            else
            {
                return "";
            }
            return str;
        }

        /// <summary>
        /// add employee success then return true
        /// save info error in datecreated
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        private bool addEmployee(Employee employee, bool isExist = false)
        {
            //check nationality
            ObservableCollection<Nationality> listNationalities = MethodHandler.getNationalities();
            int nationality = 0;
            foreach(Nationality na in listNationalities)
            {
                if (StringsConvert.RemoveDiacritics(na.NationalityName.Trim().ToUpper()).Equals(StringsConvert.RemoveDiacritics(employee.Nationality.NationalityName.Trim().ToUpper()
                    )))
                {
                    employee.Nationality.NationalityCode = na.NationalityCode;
                    employee.Nationality.NationalityName = na.NationalityName;
                    break;
                }
                nationality++;
            }
            if(nationality == listNationalities.Count)
            {
                employee.DateCreated = "Quốc tịch " + employee.Nationality.NationalityName + " chưa có trong dữ liệu \n Vui lòng thêm quốc tịch";
                return false;
            }
            //check address
            ObservableCollection<string> listWards = MethodHandler.getWards();
            int wardCount = 0;
            foreach(string str in listWards)
            {
                if (StringsConvert.RemoveDiacritics(employee.Address.ToUpper()).EndsWith(StringsConvert.RemoveDiacritics(str.ToUpper())))
                {
                    break;
                }
                wardCount++;
            }
            if (wardCount == listWards.Count)
            {
                employee.DateCreated = "Địa chỉ không có phường/ xã trong lưu trữ \n Vui lòng kiểm tra lại";
                return false;
            }
            //check workpermit
            if (employee.WorkPermit == Constants.WorkPermitOther)
            {
                int careerRelative = MethodHandler.getCareerID(Constants.OtherCareerGroup, "thân nhân");
                if (careerRelative != -1)
                {
                    employee.Career.IDCareer = careerRelative;
                    employee.Career.CareerName = "thân nhân";
                    employee.Career.IDCG = int.Parse(Constants.OtherCareerGroup);
                }
            }
            //check career
            int checkCareer = checkExistCareer(employee.Career.CareerName);
            if (checkCareer != 0)
            {
                employee.Career.IDCareer = checkCareer;
                // career is relative then workpermit is other
                int careerRelative = MethodHandler.getCareerID(Constants.OtherCareerGroup, "thân nhân");
                if (careerRelative != -1 && careerRelative == checkCareer)
                {
                    employee.WorkPermit = Constants.WorkPermitOther;
                    employee.WorkPermitNumber = "";
                }
            }
            else
            {
                employee.DateCreated = "Nghề nghiệp chưa có trong dữ liệu \n Vui lòng thêm nghề nghiệp";
                return false;
            }
            //add employee
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    DateTime today = DateTime.Now;
                    con.Open();
                    SqlCommand comm;
                    string insertSQL = "insert into Employees output INSERTED.IDEmployee values(N'" + MethodHandler.convertStringOwned(employee.StaffName) + "', " + employee.Gender + ", '" + MethodHandler.formatDatefromUsertoDatabase(employee.Birthday)
                        + "', N'" + MethodHandler.convertStringOwned(employee.Nationality.NationalityCode) + "', N'" + MethodHandler.convertStringOwned(employee.Passport) + "', N'" + MethodHandler.convertStringOwned(employee.Address)
                        + "', " + employee.Career.IDCareer + ", " + employee.WorkPermit + ",N'"
                        + (string.IsNullOrEmpty(employee.WorkPermitNumber) ? " " : MethodHandler.convertStringOwned(employee.WorkPermitNumber)) + "', N'" + employee.VisaNumber
                        + "', '" + MethodHandler.formatDatefromUsertoDatabase(employee.TemporaryStay) + "'," + employee.SettlementResults + ", N'"
                        + (string.IsNullOrEmpty(employee.SettlementResultsString) ? " " : MethodHandler.convertStringOwned(employee.SettlementResultsString))
                        + "', " + Constants.IdUser + ", " + _idCompany + ", 0, N'"
                        + (string.IsNullOrEmpty(employee.Note) ? " " : MethodHandler.convertStringOwned(employee.Note)) + "', '"
                        + today.ToString("yyyy-MM-dd HH:mm:ss") + "', "
                        + (string.IsNullOrWhiteSpace(employee.CardCreationDate) ? "null" : "'" + MethodHandler.formatDatefromUsertoDatabase(employee.CardCreationDate) + "'") + ", 0, null, null)";
                    comm = new SqlCommand(insertSQL, con);
                    int idNewEmployeeCreated = (int)comm.ExecuteScalar();
                    if (isExist)
                    {
                        comm = new SqlCommand("update Employees set Hidden_flag = 1 where IDEmployee <> " + idNewEmployeeCreated + "and Passport like N'"+ MethodHandler.convertStringOwned(employee.Passport) + "'", con);
                        comm.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    employee.DateCreated = "Lỗi khi thực hiên thêm nhân viên";
                    con.Close();
                    return false;
                }
                con.Close();
            }
            MethodHandler.updateCompanyByDate(_idCompany);
            // has employee working another company?
            string sqlAnotherCompany = "select * from Employees where IDCompany != " + _idCompany + " and Passport  like N'" + employee.Passport + "' and Hidden_flag = 0 and WorkingStatus = 0";
            if (MethodHandler.checkExistInDatabase(sqlAnotherCompany))
            {
                employee.DateCreated = "Nhân viên đang làm ở một công ty khác";
                return false;
            }
            return true;
        }

        //private void addNationalityToLocal(ObservableCollection<string> listNationalities)
        //{
        //    var path = @Directory.GetCurrentDirectory() + "\\Resources\\nationality.txt";
        //    File.WriteAllText(path, string.Empty);
        //    using (StreamWriter stream = new FileInfo(path).AppendText())
        //    {
        //        foreach(string str in listNationalities)
        //        {
        //            stream.WriteLine(str);
        //        }
        //    }
        //}

        private int checkExistCareer(string careerName)
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select IDCareer from Careers where Delete_flag = 0 and IDCG = " + SelectCareerGroup.IDCG + " and CareerName like N'" + MethodHandler.convertStringOwned(careerName) + "'";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            con.Close();
                            return int.Parse(dr["IDCareer"].ToString());
                        }
                    }
                    else
                    {
                        con.Close();
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }

            }
            return 0;
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
