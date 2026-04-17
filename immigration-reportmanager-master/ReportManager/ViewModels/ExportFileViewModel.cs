using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ReportManager.Bases;
using ReportManager.Models;
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
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class ExportFileViewModel : INotifyPropertyChanged
    {
        #region properties
        public static readonly string ListEmployeesProperty = "ListEmployees";
        public static readonly string LinkFileProperty = "LinkFile";
        public static readonly string SelectFormatFileProperty = "SelectFormatFile";
        public static readonly string IsGHTTProperty = "IsGHTT";
        public static readonly string IsVISAProperty = "IsVISA";
        public static readonly string IsTTTProperty = "IsTTT";
        public static readonly string IsOtherProperty = "IsOther";
        public static readonly string StartDateProperty = "StartDate";
        public static readonly string EndDateProperty = "EndDate";
        private ObservableCollection<Employee> _listEmployees;
        private string _linkFile;
        private int _selectFormatFile;
        private bool _isGHTT;
        private bool _isVISA;
        private bool _isTTT;
        private bool _isOther;
        private string _startDate;
        private string _endDate;
        private Company _company;
        private ExportFileWindow _window;
        private ICommand _btnSaveLinkCommand;
        private ICommand _btnExportFileCommand;
        private ICommand _btnCancelCommand;
        #endregion

        #region set get method
        public ExportFileViewModel(ExportFileWindow window, ObservableCollection<Employee> listEmployees, Company company)
        {
            _window = window;
            _company = company;
            _isGHTT = false;
            _isVISA = false;
            _isTTT = false;
            _isOther = false;
            ListEmployees = new ObservableCollection<Employee>();
            if(listEmployees == null || listEmployees.Count == 0)
            {
                window.cbbScreenEmployee.Visibility = Visibility.Collapsed;
                SelectFormatFile = 1;
            }
            else
            {
                foreach (Employee emloy in listEmployees)
                {
                    Employee item = new Employee(emloy);
                    ListEmployees.Add(item);
                }
                SelectFormatFile = 0;
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
                OnPropertyChanged("ListEmployees");
            }
        }
        public string LinkFile
        {
            get
            {
                return _linkFile;
            }
            set
            {
                this.SetValue(ref _linkFile, value, LinkFileProperty);
                OnPropertyChanged("LinkFile");
            }
        }
        public int SelectFormatFile
        {
            get
            {
                return _selectFormatFile;
            }
            set
            {
                this.SetValue(ref _selectFormatFile, value, SelectFormatFileProperty);
                OnPropertyChanged("SelectFormatFile");
                switch (_selectFormatFile)
                {
                    case 0:
                        _window.Time.Visibility = System.Windows.Visibility.Collapsed;
                        _window.SettlementResultsType.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    case 1:
                    case 2:
                        _window.Time.Visibility = System.Windows.Visibility.Visible;
                        _window.SettlementResultsType.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    case 3:
                        _window.Time.Visibility = System.Windows.Visibility.Visible;
                        _window.SettlementResultsType.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    case 4:
                        _window.Time.Visibility = System.Windows.Visibility.Visible;
                        _window.SettlementResultsType.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }
        }
        public bool IsGHTT
        {
            get
            {
                return _isGHTT;
            }
            set
            {
                this.SetValue(ref _isGHTT, value, IsGHTTProperty);
                OnPropertyChanged("IsGHTT");
            }
        }
        public bool IsVISA
        {
            get
            {
                return _isVISA;
            }
            set
            {
                this.SetValue(ref _isVISA, value, IsVISAProperty);
                OnPropertyChanged("IsVISA");
            }
        }
        public bool IsTTT
        {
            get
            {
                return _isTTT;
            }
            set
            {
                this.SetValue(ref _isTTT, value, IsTTTProperty);
                OnPropertyChanged("IsTTT");
            }
        }
        public bool IsOther
        {
            get
            {
                return _isOther;
            }
            set
            {
                this.SetValue(ref _isOther, value, IsOtherProperty);
                OnPropertyChanged("IsOther");
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
        public ICommand BtnSaveLinkCommand
        {
            get
            {
                return _btnSaveLinkCommand ?? (_btnSaveLinkCommand = new CommandHandler(SaveLink, true));
            }
        }
        private void SaveLink()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";
            var result = sfd.ShowDialog();
            if (result == false)
            {
                return;
            }
            LinkFile = sfd.FileName;
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
            if (string.IsNullOrWhiteSpace(LinkFile))
            {
                MessageBox.Show("Vui lòng đặt tên và chọn vị trí lưu file");
                return;
            }
            //if(SelectFormatFile !=0 && (string.IsNullOrWhiteSpace(StartDate) && string.IsNullOrWhiteSpace(EndDate)))
            //{
            //    MessageBox.Show("Vui lòng giới hạn thời gian");
            //    return;
            //}
            switch (SelectFormatFile)
            {
                case 0:
                    ExportFileFromScreen();
                    break;
                case 1:
                    ExportFileByCareerGroup();
                    break;
                case 2:
                    ExportFileByAddress();
                    break;
                case 3:
                    ExportFileByAddressWard();
                    break;
                case 4:
                    ExportFileBySettlementResultsType();
                    break;
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
        private void ExportFileFromScreen()
        {
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
                    string[] arrColumnHeader = {"Stt", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu", "Địa chỉ tạm trú"
                            , "Nghề nghiệp (Chức danh)", "Số GPLĐ/Thời hạn", "Số visa/Thẻ tạm trú", "Thời hạn tạm trú", "Kết quả giải quyết", "Ghi chú"};
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
                    // info company
                    ws.Cells[1, 2].Value = _company.CompanyName;
                    ws.Cells[1, 2, 1, countColHeader - 1].Merge = true;
                    // in đậm
                    ws.Cells[1, 2, 1, countColHeader - 1].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 2, 1, countColHeader - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[2, 2].Value = "Địa chỉ: " + _company.Address;
                    ws.Cells[2, 2, 2, countColHeader - 1].Merge = true;
                    ws.Cells[2, 2, 2, countColHeader - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[3, 2].Value = "Số điện thoại: " + _company.PhoneNumber;
                    ws.Cells[3, 2, 3, countColHeader - 1].Merge = true;
                    ws.Cells[3, 2, 3, countColHeader - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[6, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI LÀM VIỆC TẠI CÔNG TY";
                    ws.Cells[6, 1, 6, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[6, 1, 6, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[6, 1, 6, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int colIndex = 1;
                    int rowIndex = 9;
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
                    int order = 0;
                    foreach (Employee item in ListEmployees)
                    {
                        colIndex = 1;
                        rowIndex++;
                        ws.Cells[rowIndex, colIndex++].Value = ++order;
                        ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                        ws.Cells[rowIndex, colIndex++].Value = item.GenderString;
                        ws.Cells[rowIndex, colIndex++].Value = item.Birthday;
                        ws.Cells[rowIndex, colIndex++].Value = item.Nationality.NationalityName;
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
                    string[] resultsColumnHeader_1 = {"Có GPLĐ", "Miễn GPLĐ", "Chưa có GPLĐ", "Thân nhân sống cùng"};
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

                    // show statistics work permit
                    string[] resultsColumnHeader_3 = { "NLĐ có GPLĐ", "NLĐ miễn", "NLĐ chưa GPLĐ", "NĐT có GPLĐ", "NĐT miễn", "NĐT chưa GPLĐ", "Thân nhân" };
                    rowIndex++;
                    colIndex = 3;
                    foreach (var item in resultsColumnHeader_3)
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
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[8];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[9];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[10];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[11];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[12];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[13];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[14];
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
                    File.WriteAllBytes(LinkFile, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception e)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + e.Message);
            }
            finally
            {
                _window.Close();
            }
        }
        private void ExportFileByCareerGroup()
        {
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
            // format file
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
                    string[] arrColumnHeader = {"Stt", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu", "Địa chỉ tạm trú", "GPLĐ"
                            , "Cơ quan làm việc", "Thời hạn tạm trú", "Nghề nghiệp" };
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
                    // list fields from database
                    ObservableCollection<CareerGroup> listCareerGroup = getListCareerGroup();
                    int startLineField = 1;
                    foreach(CareerGroup cg in listCareerGroup)
                    {
                        ObservableCollection<Employee> listEmployeesByField = getListEmployeesByCareerGroup(cg.IDCG);
                        if(listEmployeesByField.Count == 0)
                        {
                            continue;
                        }
                        // set info field
                        ws.Cells[startLineField, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH THUỘC " + cg.CareerGroupName.ToUpper();
                        ws.Cells[startLineField, 1, startLineField, countColHeader].Merge = true;
                        // in đậm
                        ws.Cells[startLineField, 1, startLineField, countColHeader].Style.Font.Bold = true;
                        // căn giữa
                        ws.Cells[startLineField, 1, startLineField, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        int colIndex = 1;
                        int rowIndex = startLineField + 2;
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
                        // show content field
                        int order = 0;
                        foreach (Employee item in listEmployeesByField)
                        {
                            colIndex = 1;
                            rowIndex++;
                            ws.Cells[rowIndex, colIndex++].Value = ++order;
                            ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                            ws.Cells[rowIndex, colIndex++].Value = item.GenderString;
                            ws.Cells[rowIndex, colIndex++].Value = item.Birthday;
                            ws.Cells[rowIndex, colIndex++].Value = item.Nationality.NationalityName;
                            ws.Cells[rowIndex, colIndex++].Value = item.Passport;
                            ws.Cells[rowIndex, colIndex++].Value = item.Address;
                            ws.Cells[rowIndex, colIndex++].Value = item.WorkPermitDisplay;
                            // use Note save company name from database
                            ws.Cells[rowIndex, colIndex++].Value = item.Note;
                            ws.Cells[rowIndex, colIndex++].Value = item.TemporaryStay;
                            ws.Cells[rowIndex, colIndex++].Value = item.Career.CareerName;
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
                        ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeesByField);
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
                        // show statistics work permit
                        string[] resultsColumnHeader_3 = { "NLĐ có GPLĐ", "NLĐ miễn", "NLĐ chưa GPLĐ", "NĐT có GPLĐ", "NĐT miễn", "NĐT chưa GPLĐ", "Thân nhân" };
                        rowIndex++;
                        colIndex = 3;
                        foreach (var item in resultsColumnHeader_3)
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
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[8];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[9];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[10];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[11];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[12];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[13];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[14];
                        for (int i = 3; i < colIndex; i++)
                        {
                            var border = ws.Cells[rowIndex, i].Style.Border;
                            border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }


                        // set start line for new field
                        startLineField = rowIndex + 2;
                    }
      
                    // save file
                    Byte[] bin = package.GetAsByteArray();
                    File.WriteAllBytes(LinkFile, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception e)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + e.Message);
            }
            finally
            {
                _window.Close();
            }
        }

        private void ExportFileByAddress()
        {
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
            // format file
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
                    string[] arrColumnHeader = {"Stt", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu", "Địa chỉ tạm trú"
                            , "Cơ quan làm việc", "Thời hạn tạm trú" };
                    // lấy ra số lượng cột cần dùng dựa vào số lượng header
                    var countColHeader = arrColumnHeader.Count();
                    // merge các column lại từ column 1 đến số column header
                    // gán giá trị cho cell vừa merge là Thống kê thông tni User Kteam
                    for (int i = 1; i <= countColHeader; i++)
                    {
                        ws.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Column(i).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Column(i).Style.WrapText = true;
                        ws.Column(i).AutoFit();
                    }
                    // list fields from database
                    ObservableCollection<string> listAddress = MethodHandler.getDistricts();
                    // set title
                    // set info field
                    DateTime today = DateTime.Today;
                    if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO ĐỊA CHỈ \n TỪ NGÀY " + StartDate + " ĐẾN " + EndDate;
                    } else if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO ĐỊA CHỈ \n TỪ NGÀY "+StartDate+" ĐẾN "+ today.ToString("dd-MM-yyyy");
                    } else if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO ĐỊA CHỈ \n TỪ TRƯỚC ĐẾN "+ EndDate;
                    }
                    else
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO ĐỊA CHỈ \n TỪ TRƯỚC ĐẾN " + today.ToString("dd-MM-yyyy");
                    }
                    ws.Cells[1, 1, 3, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 3, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 3, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[1, 1, 3, countColHeader].Style.WrapText = true;

                    int startLineAddress = 5;
                    foreach (string ad in listAddress)
                    {
                        ObservableCollection<Employee> listEmployeesByAddress = getListEmployeesByAddress(ad);
                        if (listEmployeesByAddress.Count == 0)
                        {
                            continue;
                        }
                        // set info field
                        ws.Cells[startLineAddress, 1].Value = "QUẬN " + ad.ToUpper();
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Merge = true;
                        // in đậm
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Style.Font.Bold = true;
                        // căn giữa
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        int colIndex = 1;
                        int rowIndex = startLineAddress + 2;
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
                        // show content field
                        int order = 0;
                        foreach (Employee item in listEmployeesByAddress)
                        {
                            colIndex = 1;
                            rowIndex++;
                            ws.Cells[rowIndex, colIndex++].Value = ++order;
                            ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                            ws.Cells[rowIndex, colIndex++].Value = item.GenderString;
                            ws.Cells[rowIndex, colIndex++].Value = item.Birthday;
                            ws.Cells[rowIndex, colIndex++].Value = item.Nationality.NationalityName;
                            ws.Cells[rowIndex, colIndex++].Value = item.Passport;
                            ws.Cells[rowIndex, colIndex++].Value = item.Address;
                            // use Note save company name from database
                            ws.Cells[rowIndex, colIndex++].Value = item.Note;
                            ws.Cells[rowIndex, colIndex++].Value = item.TemporaryStay;
                            for(int i = 1; i < colIndex; i++)
                            {
                                var border = ws.Cells[rowIndex, i].Style.Border;
                                border.Bottom.Style =
                                border.Top.Style =
                                border.Left.Style =
                                border.Right.Style = ExcelBorderStyle.Thin;
                            }
                        }
                        // show workpermit statistics
                        ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeesByAddress);
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
                        // show statistics work permit
                        string[] resultsColumnHeader_3 = { "NLĐ có GPLĐ", "NLĐ miễn", "NLĐ chưa GPLĐ", "NĐT có GPLĐ", "NĐT miễn", "NĐT chưa GPLĐ", "Thân nhân" };
                        rowIndex++;
                        colIndex = 3;
                        foreach (var item in resultsColumnHeader_3)
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
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[8];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[9];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[10];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[11];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[12];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[13];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[14];
                        for (int i = 3; i < colIndex; i++)
                        {
                            var border = ws.Cells[rowIndex, i].Style.Border;
                            border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }


                        // set start line for new field
                        startLineAddress = rowIndex + 2;
                    }

                    // save file
                    Byte[] bin = package.GetAsByteArray();
                    File.WriteAllBytes(LinkFile, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception e)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + e.Message);
            }
            finally
            {
                _window.Close();
            }
        }
        private void ExportFileByAddressWard()
        {
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
            // format file
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
                    string[] arrColumnHeader = {"Stt", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu", "Địa chỉ tạm trú"
                            , "Cơ quan làm việc", "Thời hạn tạm trú" };
                    // lấy ra số lượng cột cần dùng dựa vào số lượng header
                    var countColHeader = arrColumnHeader.Count();
                    // merge các column lại từ column 1 đến số column header
                    // gán giá trị cho cell vừa merge là Thống kê thông tni User Kteam
                    for (int i = 1; i <= countColHeader; i++)
                    {
                        ws.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Column(i).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Column(i).Style.WrapText = true;
                        ws.Column(i).AutoFit();
                    }
                    // list fields from database
                    ObservableCollection<string> listAddressWard = MethodHandler.getWards();
                    // set title
                    // set info field
                    DateTime today = DateTime.Today;
                    if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO ĐỊA CHỈ \n TỪ NGÀY " + StartDate + " ĐẾN " + EndDate;
                    }
                    else if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO ĐỊA CHỈ \n TỪ NGÀY " + StartDate + " ĐẾN " + today.ToString("dd-MM-yyyy");
                    }
                    else if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO ĐỊA CHỈ \n TỪ TRƯỚC ĐẾN " + EndDate;
                    }
                    else
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO ĐỊA CHỈ \n TỪ TRƯỚC ĐẾN " + today.ToString("dd-MM-yyyy");
                    }
                    ws.Cells[1, 1, 3, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 3, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 3, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[1, 1, 3, countColHeader].Style.WrapText = true;

                    int startLineAddress = 5;
                    foreach (string ad in listAddressWard)
                    {
                        ObservableCollection<Employee> listEmployeesByAddress = getListEmployeesByAddressWard(ad);
                        if (listEmployeesByAddress.Count == 0)
                        {
                            continue;
                        }
                        // set info field
                        ws.Cells[startLineAddress, 1].Value = "Phường (Xã) " + ad.ToUpper();
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Merge = true;
                        // in đậm
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Style.Font.Bold = true;
                        // căn giữa
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        int colIndex = 1;
                        int rowIndex = startLineAddress + 2;
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
                        // show content field
                        int order = 0;
                        foreach (Employee item in listEmployeesByAddress)
                        {
                            colIndex = 1;
                            rowIndex++;
                            ws.Cells[rowIndex, colIndex++].Value = ++order;
                            ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                            ws.Cells[rowIndex, colIndex++].Value = item.GenderString;
                            ws.Cells[rowIndex, colIndex++].Value = item.Birthday;
                            ws.Cells[rowIndex, colIndex++].Value = item.Nationality.NationalityName;
                            ws.Cells[rowIndex, colIndex++].Value = item.Passport;
                            ws.Cells[rowIndex, colIndex++].Value = item.Address;
                            // use Note save company name from database
                            ws.Cells[rowIndex, colIndex++].Value = item.Note;
                            ws.Cells[rowIndex, colIndex++].Value = item.TemporaryStay;
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
                        ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeesByAddress);
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
                        // show statistics work permit
                        string[] resultsColumnHeader_3 = { "NLĐ có GPLĐ", "NLĐ miễn", "NLĐ chưa GPLĐ", "NĐT có GPLĐ", "NĐT miễn", "NĐT chưa GPLĐ", "Thân nhân" };
                        rowIndex++;
                        colIndex = 3;
                        foreach (var item in resultsColumnHeader_3)
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
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[8];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[9];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[10];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[11];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[12];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[13];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[14];
                        for (int i = 3; i < colIndex; i++)
                        {
                            var border = ws.Cells[rowIndex, i].Style.Border;
                            border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }


                        // set start line for new field
                        startLineAddress = rowIndex + 2;
                    }

                    // save file
                    Byte[] bin = package.GetAsByteArray();
                    File.WriteAllBytes(LinkFile, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception e)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + e.Message);
            }
            finally
            {
                _window.Close();
            }
        }

        private void ExportFileBySettlementResultsType()
        {
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
            // format file
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
                    string[] arrColumnHeader = {"Stt", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu", "Địa chỉ tạm trú"
                            , "Cơ quan làm việc", "Thời hạn tạm trú" };
                    // lấy ra số lượng cột cần dùng dựa vào số lượng header
                    var countColHeader = arrColumnHeader.Count();
                    // merge các column lại từ column 1 đến số column header
                    // gán giá trị cho cell vừa merge là Thống kê thông tni User Kteam
                    for (int i = 1; i <= countColHeader; i++)
                    {
                        ws.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Column(i).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Column(i).Style.WrapText = true;
                        ws.Column(i).AutoFit();
                    }
                    // list SettlementResults Type selected
                    ObservableCollection<int> listSettlementResultsType = new ObservableCollection<int>();
                    if (IsGHTT)
                    {
                        listSettlementResultsType.Add(0);
                    }
                    if (IsVISA)
                    {
                        listSettlementResultsType.Add(1);
                    }
                    if (IsTTT)
                    {
                        listSettlementResultsType.Add(2);
                    }
                    if (IsOther)
                    {
                        listSettlementResultsType.Add(3);
                    }
                    // set title
                    // set info field
                    DateTime today = DateTime.Today;
                    if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO LOẠI KQGQ \n TỪ NGÀY " + StartDate + " ĐẾN " + EndDate;
                    }
                    else if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO LOẠI KQGQ \n TỪ NGÀY " + StartDate + " ĐẾN " + today.ToString("dd-MM-yyyy");
                    }
                    else if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO LOẠI KQGQ \n TỪ TRƯỚC ĐẾN " + EndDate;
                    }
                    else
                    {
                        ws.Cells[1, 1].Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI ĐƯỢC GIẢI QUYẾT THỦ TỤC XUẤT NHẬP CẢNH PHÂN THEO LOẠI KQGQ \n TỪ TRƯỚC ĐẾN " + today.ToString("dd-MM-yyyy");
                    }
                    ws.Cells[1, 1, 3, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 3, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 3, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[1, 1, 3, countColHeader].Style.WrapText = true;

                    int startLineAddress = 5;
                    foreach (int settlementResultsType in listSettlementResultsType)
                    {
                        ObservableCollection<Employee> listEmployeesByAddress = getListEmployeesBySettlementResultsType(settlementResultsType);
                        if (listEmployeesByAddress.Count == 0)
                        {
                            continue;
                        }
                        // set info field
                        ws.Cells[startLineAddress, 1].Value = "KQGQ: " + MethodHandler.getSettlementResults(settlementResultsType).ToUpper();
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Merge = true;
                        // in đậm
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Style.Font.Bold = true;
                        // căn giữa
                        ws.Cells[startLineAddress, 1, startLineAddress, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        int colIndex = 1;
                        int rowIndex = startLineAddress + 2;
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
                        // show content field
                        int order = 0;
                        foreach (Employee item in listEmployeesByAddress)
                        {
                            colIndex = 1;
                            rowIndex++;
                            ws.Cells[rowIndex, colIndex++].Value = ++order;
                            ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                            ws.Cells[rowIndex, colIndex++].Value = item.GenderString;
                            ws.Cells[rowIndex, colIndex++].Value = item.Birthday;
                            ws.Cells[rowIndex, colIndex++].Value = item.Nationality.NationalityName;
                            ws.Cells[rowIndex, colIndex++].Value = item.Passport;
                            ws.Cells[rowIndex, colIndex++].Value = item.Address;
                            // use Note save company name from database
                            ws.Cells[rowIndex, colIndex++].Value = item.Note;
                            ws.Cells[rowIndex, colIndex++].Value = item.TemporaryStay;
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
                        ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeesByAddress);
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
                        // show statistics work permit
                        string[] resultsColumnHeader_3 = { "NLĐ có GPLĐ", "NLĐ miễn", "NLĐ chưa GPLĐ", "NĐT có GPLĐ", "NĐT miễn", "NĐT chưa GPLĐ", "Thân nhân" };
                        rowIndex++;
                        colIndex = 3;
                        foreach (var item in resultsColumnHeader_3)
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
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[8];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[9];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[10];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[11];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[12];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[13];
                        ws.Cells[rowIndex, colIndex++].Value = listStatistics[14];
                        for (int i = 3; i < colIndex; i++)
                        {
                            var border = ws.Cells[rowIndex, i].Style.Border;
                            border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }


                        // set start line for new field
                        startLineAddress = rowIndex + 2;
                    }

                    // save file
                    Byte[] bin = package.GetAsByteArray();
                    File.WriteAllBytes(LinkFile, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception e)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + e.Message);
            }
            finally
            {
                _window.Close();
            }
        }

        private ObservableCollection<CareerGroup> getListCareerGroup()
        {
            ObservableCollection<CareerGroup> list = new ObservableCollection<CareerGroup>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from CareerGroups", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            CareerGroup rowField = new CareerGroup();
                            rowField.IDCG = int.Parse(dr["IDCG"].ToString());
                            rowField.CareerGroupName = dr["CareerGroupName"].ToString();
                            list.Add(rowField);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có nhóm nghề");
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
            return list;
        }

        private ObservableCollection<Employee> getListEmployeesByCareerGroup(int idCareerGroup)
        {
            ObservableCollection<Employee> list = new ObservableCollection<Employee>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select IDEmployee, StaffName, Gender, CONVERT(varchar, Birthday, 105) birth, Nationality, Passport, Employees.Address diachi, NationalityName, NationalityCode,"
                        + " CompanyName, CONVERT(varchar, TemporaryStay, 105) temporary, CareerName, WorkPermit, WorkPermitNumber, SettlementResults from Employees, Companies, Careers, Nationality where Employees.IDCompany = Companies.IDCompany"
                        + " and Employees.IDCareer = Careers.IDCareer and Nationality.NationalityCode like Employees.Nationality";
                    // get all when id = 1
                    if(idCareerGroup != 1)
                    {
                        sql += " and Careers.IDCG = " + idCareerGroup;
                    } else
                    {
                        sql += " and Careers.IDCG != " + Constants.OtherCareerGroup;
                    }
                    if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        sql += " and CardCreationDate >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'";
                        //if (!string.IsNullOrWhiteSpace(EndDate))
                        //{
                        //    sql += " and CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
                        //}
                        //else
                        //{
                        //    DateTime today = DateTime.Today;
                        //    sql += " and CardCreationDate <= '" + today.ToString("yyyy-MM-dd") + "'";
                        //}
                    }
                    if(!string.IsNullOrWhiteSpace(EndDate))
                    {
                        sql += " and CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
                    }
                    // order by
                    sql += " order by CompanyName, StaffName, NationalityName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Employee rowEmployees = new Employee();
                            rowEmployees.IDEmployee = int.Parse(dr["IDEmployee"].ToString());
                            rowEmployees.StaffName = dr["StaffName"].ToString();
                            rowEmployees.Gender = int.Parse(dr["Gender"].ToString());
                            rowEmployees.Birthday = dr["birth"].ToString();
                            rowEmployees.Nationality.NationalityCode = dr["NationalityCode"].ToString();
                            rowEmployees.Nationality.NationalityName = dr["NationalityName"].ToString();
                            rowEmployees.Passport = dr["Passport"].ToString();
                            rowEmployees.Address = dr["diachi"].ToString();
                            rowEmployees.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployees.WorkPermitNumber = dr["WorkPermitNumber"].ToString();
                            rowEmployees.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                            // use Note to save company name
                            rowEmployees.Note = dr["CompanyName"].ToString();
                            rowEmployees.TemporaryStay = dr["temporary"].ToString();
                            rowEmployees.Career.CareerName = dr["CareerName"].ToString();
                            list.Add(rowEmployees);
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
            return list;
        }

        private ObservableCollection<Employee> getListEmployeesByAddress(string address)
        {
            ObservableCollection<Employee> list = new ObservableCollection<Employee>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select IDEmployee, StaffName, Gender, CONVERT(varchar, Birthday, 105) birth,"
                        + " Nationality, Passport, Employees.Address diachi, CompanyName, CONVERT(varchar, TemporaryStay, 105) temporary, WorkPermit, SettlementResults"
                        + ", NationalityCode, NationalityName from Employees, Companies, Nationality where Employees.IDCompany = Companies.IDCompany and Nationality.NationalityCode like Employees.Nationality "
                        + "and Employees.Address like N'%" + address + "%'";
                    if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        sql += " and CardCreationDate >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'";
                        //if (!string.IsNullOrWhiteSpace(EndDate))
                        //{
                        //    sql += " and CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
                        //}
                        //else
                        //{
                        //    DateTime today = DateTime.Today;
                        //    sql += " and CardCreationDate <= '" + today.ToString("yyyy-MM-dd") + "'";
                        //}
                    }
                    if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        sql += " and CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
                    }
                    // order by
                    sql += "order by CompanyName, StaffName, Nationality";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Employee rowEmployees = new Employee();
                            rowEmployees.IDEmployee = int.Parse(dr["IDEmployee"].ToString());
                            rowEmployees.StaffName = dr["StaffName"].ToString();
                            rowEmployees.Gender = int.Parse(dr["Gender"].ToString());
                            rowEmployees.Birthday = dr["birth"].ToString();
                            rowEmployees.Nationality.NationalityCode = dr["NationalityCode"].ToString();
                            rowEmployees.Nationality.NationalityName = dr["NationalityName"].ToString();
                            rowEmployees.Passport = dr["Passport"].ToString();
                            rowEmployees.Address = dr["diachi"].ToString();
                            rowEmployees.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployees.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                            // use Note to save company name
                            rowEmployees.Note = dr["CompanyName"].ToString();
                            rowEmployees.TemporaryStay = dr["temporary"].ToString();
                            list.Add(rowEmployees);
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
            return list;
        }
        private ObservableCollection<Employee> getListEmployeesByAddressWard(string address)
        {
            ObservableCollection<Employee> list = new ObservableCollection<Employee>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select IDEmployee, StaffName, Gender, CONVERT(varchar, Birthday, 105) birth,"
                        + " Nationality, Passport, Employees.Address diachi, CompanyName, CONVERT(varchar, TemporaryStay, 105) temporary, WorkPermit, SettlementResults"
                        + ", NationalityCode, NationalityName from Employees, Companies, Nationality where Employees.IDCompany = Companies.IDCompany and Nationality.NationalityCode like Employees.Nationality "
                        + "and Employees.Address like N'%" + address + "'";
                    if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        sql += " and CardCreationDate >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'";
                        //if (!string.IsNullOrWhiteSpace(EndDate))
                        //{
                        //    sql += " and CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
                        //}
                        //else
                        //{
                        //    DateTime today = DateTime.Today;
                        //    sql += " and CardCreationDate <= '" + today.ToString("yyyy-MM-dd") + "'";
                        //}
                    }
                    if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        sql += " and CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
                    }
                    // order by
                    sql += "order by CompanyName, StaffName, Nationality";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Employee rowEmployees = new Employee();
                            rowEmployees.IDEmployee = int.Parse(dr["IDEmployee"].ToString());
                            rowEmployees.StaffName = dr["StaffName"].ToString();
                            rowEmployees.Gender = int.Parse(dr["Gender"].ToString());
                            rowEmployees.Birthday = dr["birth"].ToString();
                            rowEmployees.Nationality.NationalityCode = dr["NationalityCode"].ToString();
                            rowEmployees.Nationality.NationalityName = dr["NationalityName"].ToString();
                            rowEmployees.Passport = dr["Passport"].ToString();
                            rowEmployees.Address = dr["diachi"].ToString();
                            rowEmployees.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployees.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                            // use Note to save company name
                            rowEmployees.Note = dr["CompanyName"].ToString();
                            rowEmployees.TemporaryStay = dr["temporary"].ToString();
                            list.Add(rowEmployees);
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
            return list;
        }
        private ObservableCollection<Employee> getListEmployeesBySettlementResultsType(int settlementResultsType)
        {
            ObservableCollection<Employee> list = new ObservableCollection<Employee>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select IDEmployee, StaffName, Gender, CONVERT(varchar, Birthday, 105) birth,"
                        + " Nationality, Passport, Employees.Address diachi, CompanyName, CONVERT(varchar, TemporaryStay, 105) temporary, WorkPermit, SettlementResults"
                        + ", NationalityCode, NationalityName from Employees, Companies, Nationality where Employees.IDCompany = Companies.IDCompany and Nationality.NationalityCode like Employees.Nationality "
                        + "and Employees.SettlementResults = " + settlementResultsType;
                    if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        sql += " and CardCreationDate >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'";
                        //if (!string.IsNullOrWhiteSpace(EndDate))
                        //{
                        //    sql += " and CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
                        //}
                        //else
                        //{
                        //    DateTime today = DateTime.Today;
                        //    sql += " and CardCreationDate <= '" + today.ToString("yyyy-MM-dd") + "'";
                        //}
                    }
                    if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        sql += " and CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'";
                    }
                    // order by
                    sql += "order by CompanyName, StaffName, Nationality";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Employee rowEmployees = new Employee();
                            rowEmployees.IDEmployee = int.Parse(dr["IDEmployee"].ToString());
                            rowEmployees.StaffName = dr["StaffName"].ToString();
                            rowEmployees.Gender = int.Parse(dr["Gender"].ToString());
                            rowEmployees.Birthday = dr["birth"].ToString();
                            rowEmployees.Nationality.NationalityCode = dr["NationalityCode"].ToString();
                            rowEmployees.Nationality.NationalityName = dr["NationalityName"].ToString();
                            rowEmployees.Passport = dr["Passport"].ToString();
                            rowEmployees.Address = dr["diachi"].ToString();
                            rowEmployees.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployees.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                            // use Note to save company name
                            rowEmployees.Note = dr["CompanyName"].ToString();
                            rowEmployees.TemporaryStay = dr["temporary"].ToString();
                            list.Add(rowEmployees);
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
            return list;
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
