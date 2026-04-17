using Microsoft.Win32;
using OfficeOpenXml;
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
    class SearchResultsViewModel : INotifyPropertyChanged
    {
        public static readonly string ListEmployeesProperty = "ListEmployees";
        public static readonly string SelectEmployeeProperty = "SelectEmployee";
        public static readonly string SumProperty = "Sum";
        public static readonly string SumGHTTProperty = "SumGHTT";
        public static readonly string SumVisaProperty = "SumVisa";
        public static readonly string SumTemporaryResidenceCardProperty = "SumTemporaryResidenceCard";
        public static readonly string SumOtherProperty = "SumOther";
        private ObservableCollection<Employee> _listEmployees;
        private Employee _selectEmployee;
        private int _sum;
        private int _sumGHTT;
        private int _sumVisa;
        private int _sumTemporaryResidenceCard;
        private int _sumOther;
        private ReportManagerWindow _window;
        private ICommand _btnBackCommand;
        private ICommand _btnOpenEditEmployeeCommand;
        private ICommand _btnExportFileCommand;
        private UserControl currentUserControl;

        #region set get method
        public SearchResultsViewModel(ReportManagerWindow window, ObservableCollection<Employee> listEmployees, UserControl curUserControl)
        {
            _window = window;
            currentUserControl = curUserControl;
            ListEmployees = listEmployees;
            _window.exportFileCompany.Visibility = Visibility.Collapsed;
            _window.exportFileEmployee.Command = BtnExportFileCommand;
            _window.exportFileCompanyAndEmployee.Visibility = Visibility.Collapsed;
            ObservableCollection<int> listStatistical = MethodHandler.getStatistics(listEmployees);
            Sum = listEmployees.Count;
            SumGHTT = listStatistical[4];
            SumVisa = listStatistical[5];
            SumTemporaryResidenceCard = listStatistical[6];
            SumOther = listStatistical[7];
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
            //_window.exportFileEmployee.Command = null;
            _window.exportFileCompany.Visibility = Visibility.Visible;
            _window.ContentArea.Content = new ListCompaniesUC(_window);
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
                //_window.exportFileEmployee.Command = null;
                _window.exportFileCompany.Visibility = Visibility.Visible;
                _window.ContentArea.Content = new AddEmployeeUC(_window, currentUserControl, getCompany(SelectEmployee.IDCompany), SelectEmployee);
            }
        }

        public ICommand BtnExportFileCommand
        {
            get
            {
                return _btnExportFileCommand ?? (_btnExportFileCommand = new CommandHandler(ExportFile, true));
            }
        }
        private void ExportFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";
            var result = sfd.ShowDialog();
            if (result == false)
            {
                return;
            }
            string filePath = sfd.FileName;
            try
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    //Tạo một sheet để làm việc trên đó
                    package.Workbook.Worksheets.Add("Báo cáo");
                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = package.Workbook.Worksheets[1];
                    // đặt tên cho sheet
                    ws.Name = "Báo cáo";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    // Tạo danh sách các column header
                    string[] arrColumnHeader = { "STT", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Cơ quan làm việc",
                        "Số hộ chiếu", "Địa chỉ tạm trú", "Nghề nghiệp", "Số GPLĐ/ Thời hạn", "Số Visa", "Thời hạn tạm trú", "Kết quả giải quyết", "Ghi chú"};
                    // lấy ra số lượng cột cần dùng dựa vào số lượng header
                    var countColHeader = arrColumnHeader.Count();
                    // merge các column lại từ column 1 đến số column header
                    // gán giá trị cho cell vừa merge là Thống kê thông tni User Kteam
                    for (int i = 1; i <= countColHeader; i++)
                    {
                        ws.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Column(i).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Column(i).Style.WrapText = true;
                        ws.Column(i).BestFit = true;
                    }
                    ws.Cells[1, 1].Value = "Kết quả tìm kiếm";
                    ws.Cells[1, 1, 1, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 1, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 1, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int colIndex = 1;
                    int rowIndex = 4;
                    //tạo các header từ column header đã tạo từ bên trên
                    foreach (var item in arrColumnHeader)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];

                        //set màu thành gray
                        var fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                        //căn chỉnh các border
                        var border = cell.Style.Border;
                        border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;

                        //gán giá trị
                        cell.Value = item;
                        colIndex++;
                    }
                    // show content
                    foreach (Employee item in ListEmployees)
                    {
                        colIndex = 1;
                        rowIndex++;
                        ws.Cells[rowIndex, colIndex++].Value = item.LineNumber;
                        ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                        ws.Cells[rowIndex, colIndex++].Value = item.GenderString;
                        ws.Cells[rowIndex, colIndex++].Value = item.Birthday;
                        ws.Cells[rowIndex, colIndex++].Value = item.Nationality.NationalityName;
                        ws.Cells[rowIndex, colIndex++].Value = item.DateCreated;// name company
                        ws.Cells[rowIndex, colIndex++].Value = item.Passport;
                        ws.Cells[rowIndex, colIndex++].Value = item.Address;
                        ws.Cells[rowIndex, colIndex++].Value = item.Career.CareerName;
                        ws.Cells[rowIndex, colIndex++].Value = item.WorkPermitDisplay;
                        ws.Cells[rowIndex, colIndex++].Value = item.VisaNumber;
                        ws.Cells[rowIndex, colIndex++].Value = item.TemporaryStay;
                        ws.Cells[rowIndex, colIndex++].Value = item.SettlementResultsDisplay;
                        ws.Cells[rowIndex, colIndex++].Value = item.Note;
                        for (int i = 1; i < colIndex; i++)
                        {
                            var border = ws.Cells[rowIndex, i].Style.Border;
                            border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }
                    }
                    // show workpermit statistics
                    ObservableCollection<int> listStatistics = MethodHandler.getStatistics(ListEmployees);
                    string[] resultsColumnHeader_1 = { "Có GPLĐ", "Miễn GPLĐ", "Chưa có GPLĐ", "Thân nhân sống cùng" };
                    rowIndex++;
                    ws.Cells[++rowIndex, 2].Value = "Trong đó:";
                    ws.Cells[rowIndex, 2, rowIndex, 5].Merge = true;
                    ws.Cells[rowIndex, 2, rowIndex, 5].Style.Font.Bold = true;
                    ws.Cells[rowIndex, 2, rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    colIndex = 3;
                    rowIndex++;
                    //tạo các header từ column header đã tạo từ bên trên
                    foreach (var item in resultsColumnHeader_1)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];

                        var border = cell.Style.Border;
                        border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;

                        //gán giá trị
                        cell.Value = item;
                        colIndex++;
                    }
                    rowIndex++;
                    colIndex = 3;
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[0];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[1];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[2];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[3];
                    for (int i = 3; i < colIndex; i++)
                    {
                        var border = ws.Cells[rowIndex, i].Style.Border;
                        border.Bottom.Style =
                        border.Top.Style =
                        border.Left.Style =
                        border.Right.Style = ExcelBorderStyle.Thin;
                    }
                    // show statistics settlement results
                    string[] resultsColumnHeader_2 = { "GHTT", "Visa", "TTT", "Khác" };
                    rowIndex++;
                    colIndex = 3;
                    foreach (var item in resultsColumnHeader_2)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];

                        var border = cell.Style.Border;
                        border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;

                        //gán giá trị
                        cell.Value = item;
                        colIndex++;
                    }
                    rowIndex++;
                    colIndex = 3;
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[4];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[5];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[6];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[7];
                    for (int i = 3; i < colIndex; i++)
                    {
                        var border = ws.Cells[rowIndex, i].Style.Border;
                        border.Bottom.Style =
                        border.Top.Style =
                        border.Left.Style =
                        border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // save file
                    Byte[] bin = package.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception e)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + e.Message);
            }
        }
        #endregion

        #region method
        private Company getCompany(int idCompany)
        {
            Company resultsCompany = new Company();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM Companies, Fields where Companies.Delete_flag = 0 and Companies.IDField = Fields.IDField and Companies.IDCompany = " + idCompany, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
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
                            rowCompany.Note = dr["Note"].ToString();
                            rowCompany.TrackerID = int.Parse(dr["TrackerID"].ToString());
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

                            resultsCompany = rowCompany;
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
            return resultsCompany;
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
