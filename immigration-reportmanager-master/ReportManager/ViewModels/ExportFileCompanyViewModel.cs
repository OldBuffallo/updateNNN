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
    class ExportFileCompanyViewModel : INotifyPropertyChanged
    {
        #region properties
        public static readonly string ListCompaniesProperty = "ListCompanies";
        public static readonly string LinkFileProperty = "LinkFile";
        public static readonly string SelectFormatFileProperty = "SelectFormatFile";
        public static readonly string DateExportFileProperty = "DateExportFile";
        public static readonly string StartDateProperty = "StartDate";
        public static readonly string EndDateProperty = "EndDate";
        public static readonly string TemporaryStayProperty = "TemporaryStay";
        public static readonly string MinInvestmentProperty = "MinInvestment";
        public static readonly string MaxInvestmentProperty = "MaxInvestment";
        public static readonly string ListNationalitiesProperty = "ListNationalities";
        public static readonly string SelectNationalityProperty = "SelectNationality";
        public static readonly string IsColumnAddressProperty = "IsColumnAddress";
        public static readonly string IsColumnDateCreatedProperty = "IsColumnDateCreated";
        public static readonly string IsCheckInvestmentProperty = "IsCheckInvestment";
        public static readonly string IsColumnDiscriptionOfActivitiesProperty = "IsColumnDiscriptionOfActivities";
        private ObservableCollection<Company> _listCompanies;
        private string _linkFile;
        private int _selectFormatFile;
        private string _dateExportFile;
        private string _startDate;
        private string _endDate;
        private string _temporaryStay;
        private decimal _minInvestment;
        private decimal _maxInvestment;
        private ObservableCollection<Nationality> _listNationalities;
        private Nationality _selectNationality;
        private bool _isColumnAddress;
        private bool _isColumnDiscriptionOfActivities;
        private bool _isColumnDateCreated;
        private bool _isCheckInvestment;
        private ExportFileCompanyWindow _window;
        private ICommand _btnSaveLinkCommand;
        private ICommand _btnSetNationalityCommand;
        private ICommand _btnExportFileCommand;
        private ICommand _btnCancelCommand;
        #endregion

        #region set get method
        public ExportFileCompanyViewModel(ExportFileCompanyWindow window, ObservableCollection<Company> listCompanies)
        {
            _window = window;
            if(listCompanies == null || listCompanies.Count == 0)
            {
                window.cbbScreenCompany.Visibility = Visibility.Collapsed;
                SelectFormatFile = 1;
            }
            else
            {
                ListCompanies = listCompanies;
                SelectFormatFile = 0;
            }
            ListNationalities = MethodHandler.getNationalities();
            SelectNationality = new Nationality();
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
                _window.Time.Visibility = Visibility.Visible;
                switch (_selectFormatFile)
                {
                    case 0:
                        _window.gridcondition.Visibility = Visibility.Collapsed;
                        _window.Time.Visibility = Visibility.Collapsed;
                        _window.TemporaryStay.Visibility = Visibility.Collapsed;
                        _window.DateExportFile.Visibility = Visibility.Collapsed;
                        _window.gridCheckAddress.Visibility = Visibility.Collapsed;
                        _window.gridCheckDescriptionOfActivities.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        _window.gridcondition.Visibility = Visibility.Collapsed;
                        _window.TemporaryStay.Visibility = Visibility.Visible;
                        _window.DateExportFile.Visibility = Visibility.Visible;
                        _window.gridCheckAddress.Visibility = Visibility.Visible;
                        _window.gridCheckDescriptionOfActivities.Visibility = Visibility.Visible;
                        break;
                    case 2:
                    case 4:
                        _window.gridcondition.Visibility = Visibility.Visible;
                        _window.TemporaryStay.Visibility = Visibility.Visible;
                        _window.gridinvestment.Visibility = Visibility.Collapsed;
                        _window.gridnationality.Visibility = Visibility.Visible;
                        _window.DateExportFile.Visibility = Visibility.Visible;
                        _window.gridCheckAddress.Visibility = Visibility.Visible;
                        _window.gridCheckDescriptionOfActivities.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        _window.gridcondition.Visibility = Visibility.Visible;
                        _window.TemporaryStay.Visibility = Visibility.Visible;
                        _window.gridinvestment.Visibility = Visibility.Visible;
                        _window.gridnationality.Visibility = Visibility.Visible;
                        _window.DateExportFile.Visibility = Visibility.Visible;
                        _window.gridCheckAddress.Visibility = Visibility.Visible;
                        _window.gridCheckDescriptionOfActivities.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        public string DateExportFile
        {
            get
            {
                return _dateExportFile;
            }
            set
            {
                this.SetValue(ref _dateExportFile, value, DateExportFileProperty);
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
            }
        }
        public string TemporaryStay
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
        public decimal MinInvestment
        {
            get
            {
                return _minInvestment;
            }
            set
            {
                this.SetValue(ref _minInvestment, value, MinInvestmentProperty);
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
        public Nationality SelectNationality
        {
            get
            {
                return _selectNationality;
            }
            set
            {
                this.SetValue(ref _selectNationality, value, SelectNationalityProperty);
            }
        }
        public bool IsColumnAddress
        {
            get
            {
                return _isColumnAddress;
            }
            set
            {
                this.SetValue(ref _isColumnAddress, value, IsColumnAddressProperty);
            }
        }
        public bool IsColumnDiscriptionOfActivities
        {
            get
            {
                return _isColumnDiscriptionOfActivities;
            }
            set
            {
                this.SetValue(ref _isColumnDiscriptionOfActivities, value, IsColumnDiscriptionOfActivitiesProperty);
            }
        }
        public bool IsColumnDateCreated
        {
            get
            {
                return _isColumnDateCreated;
            }
            set
            {
                this.SetValue(ref _isColumnDateCreated, value, IsColumnDateCreatedProperty);
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

        public ICommand BtnSetNationalityCommand
        {
            get
            {
                return _btnSetNationalityCommand ?? (_btnSetNationalityCommand = new CommandHandler(BtnSetNationality, true));
            }
        }
        private void BtnSetNationality()
        {
            if (string.IsNullOrWhiteSpace(SelectNationality.NationalityCode))
            {
                SelectNationality.NationalityName = "";
                return;
            }
            foreach (Nationality na in ListNationalities)
            {
                if (na.NationalityCode.Equals(SelectNationality.NationalityCode))
                {
                    SelectNationality.NationalityName = na.NationalityName;
                    return;
                }
            }
            MessageBox.Show("Mã quốc tịch chưa có trong dữ liệu hoặc đã bị xóa.\nVui lòng kiểm tra lại.");
            SelectNationality.NationalityName = "";
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
            _window.Close();
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
            ////if (SelectFormatFile != 0 && (string.IsNullOrWhiteSpace(StartDate) && string.IsNullOrWhiteSpace(EndDate)))
            ////{
            ////    MessageBox.Show("Vui lòng giới hạn thời gian");
            ////    return;
            ////}
            switch (SelectFormatFile)
            {
                case 0:
                    ExportFileFromScreen();
                    break;
                case 1:
                    ExportSynthesisFile();
                    break;
                case 2:
                    ExportFileByNational();
                    break;
                case 3:
                    if( MinInvestment == 0 && MaxInvestment == 0 && IsCheckInvestment == false)
                    {
                        MessageBox.Show("Vui lòng giới hạn vốn đầu tư hoặc chọn không có vốn đầu tư");
                        break;
                    }
                    ExportSynthesisFile(true);
                    break;
                case 4:
                    ExportFileByNationalityOfInvestors();
                    break;
            }
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
                    string[] arrColumnHeader = { "Tên pháp nhân", "Quốc tịch pháp nhân", "Vốn đầu tư", "Thời gian hoạt động", "Địa chỉ",
                        "Lĩnh vực", "Đại diện pháp luật", "Điện thoại liên hệ", "Địa chỉ mail công ty", "Mô tả hoạt động", "Tổng số NNN", "Miễn GPLĐ", "Có GPLĐ",
                     "Chưa có GPLĐ", "Thân nhân sống cùng", "Hồ sơ đăng ký", "Ghi chú"};
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
                    ws.Column(2).Style.WrapText = true;
                    ws.Column(3).Style.WrapText = true;
                    ws.Column(5).Style.WrapText = true;
                    ws.Column(8).Style.WrapText = true;
                    ws.Column(9).Style.WrapText = true;
                    ws.Column(15).Style.WrapText = true;
                    ws.Column(16).Style.WrapText = true;
                    ws.Cells[1, 1].Value = "Kết quả từ màn hình";
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
                    foreach (Company item in ListCompanies)
                    {
                        colIndex = 1;
                        rowIndex++;
                        ws.Cells[rowIndex, colIndex++].Value = item.CompanyName;
                        ws.Cells[rowIndex, colIndex++].Value = item.TypeOfBusiniess;
                        ws.Cells[rowIndex, colIndex++].Value = item.Investment;
                        ws.Cells[rowIndex, colIndex++].Value = item.Uptime;
                        ws.Cells[rowIndex, colIndex++].Value = item.Address;
                        ws.Cells[rowIndex, colIndex++].Value = item.Field.FieldName;
                        ws.Cells[rowIndex, colIndex++].Value = item.LegalRepresentative;
                        ws.Cells[rowIndex, colIndex++].Value = item.PhoneNumber;
                        ws.Cells[rowIndex, colIndex++].Value = item.Email;
                        ws.Cells[rowIndex, colIndex++].Value = item.DescriptionOfActivities;
                        ws.Cells[rowIndex, colIndex++].Value = item.TotalAmount;
                        ws.Cells[rowIndex, colIndex++].Value = item.AmountOfExemption;
                        ws.Cells[rowIndex, colIndex++].Value = item.QuantityAvailable;
                        ws.Cells[rowIndex, colIndex++].Value = item.QuantityNotYet;
                        ws.Cells[rowIndex, colIndex++].Value = item.NumberOfPersonalities;
                        ws.Cells[rowIndex, colIndex++].Value = item.RegistrationProfile;
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
                    ObservableCollection<int> listStatistics = MethodHandler.getStatistics(ListCompanies);
                    string[] resultsColumnHeader = { "Số doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miễn GPLĐ", "Chưa có GPLĐ", "Thân nhân sống cùng" };
                    rowIndex++;
                    ws.Cells[++rowIndex, 2].Value = "Trong đó:";
                    ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                    ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Bold = true;
                    ws.Cells[rowIndex, 2, rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    colIndex = 3;
                    rowIndex++;
                    //tạo các header từ column header đã tạo từ bên trên
                    foreach (var item in resultsColumnHeader)
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
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[4];
                    ws.Cells[rowIndex, colIndex++].Value = listStatistics[5];
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

        /// <summary>
        /// include case 1 param is false and case 3 param is true
        /// </summary>
        /// <param name="isInvestment"></param>
        private void ExportSynthesisFile(bool isInvestment = false)
        {
            if (!string.IsNullOrWhiteSpace(DateExportFile) && !MethodHandler.checkFormatDate(DateExportFile))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
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
            if (!string.IsNullOrWhiteSpace(TemporaryStay) && !MethodHandler.checkFormatDate(TemporaryStay))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            if (isInvestment)
            {
                if (MinInvestment != 0 && MaxInvestment != 0 && MinInvestment > MaxInvestment)
                {
                    MessageBox.Show("giá trị lớn nhất không được thấp hơn giá trị nhỏ nhất");
                    return;
                }
            }
            try
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    //Tạo một sheet để làm việc trên đó
                    package.Workbook.Worksheets.Add("Báo cáo");
                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = package.Workbook.Worksheets[1];
                    // đặt tên cho sheet
                    if (!isInvestment)
                    {
                        ws.Name = "Báo cáo tổng hợp số lượng NNN";
                    }
                    else
                    {
                        ws.Name = "BC theo số vốn đầu tư";
                    }
                    
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    // Tạo danh sách các column header
                    string[] arrColumnHeader;
                    if (!isInvestment)
                    {
                        if (IsColumnAddress)
                        {
                            if (IsColumnDiscriptionOfActivities)
                            {
                                string[] arrColumnHeader_1 = {"STT", "Tên doanh nghiệp", "Địa chỉ doanh nghiệp", "Mô tả hoạt động", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                            , "Thân nhân sống cùng", "Ghi chú" };
                                arrColumnHeader = arrColumnHeader_1;
                            }
                            else
                            {
                                string[] arrColumnHeader_1 = {"STT", "Tên doanh nghiệp", "Địa chỉ doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                            , "Thân nhân sống cùng", "Ghi chú" };
                                arrColumnHeader = arrColumnHeader_1;
                            }
                        }
                        else
                        {
                            if (IsColumnDiscriptionOfActivities)
                            {
                                string[] arrColumnHeader_2 = {"STT", "Tên doanh nghiệp", "Mô tả hoạt động", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                            , "Thân nhân sống cùng", "Ghi chú" };
                                arrColumnHeader = arrColumnHeader_2;
                            }
                            else
                            {
                                string[] arrColumnHeader_2 = {"STT", "Tên doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                            , "Thân nhân sống cùng", "Ghi chú" };
                                arrColumnHeader = arrColumnHeader_2;
                            }
                        }
                    }
                    else
                    {
                        if (IsColumnAddress)
                        {
                            if (IsColumnDiscriptionOfActivities)
                            {
                                string[] arrColumnHeader_1 = {"STT", "Tên doanh nghiệp", "Địa chỉ doanh nghiệp","Số vốn góp", "Lĩnh vực", "Mô tả hoạt động", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                            , "Thân nhân sống cùng", "Ghi chú" };
                                arrColumnHeader = arrColumnHeader_1;
                            }
                            else
                            {
                                string[] arrColumnHeader_1 = {"STT", "Tên doanh nghiệp", "Địa chỉ doanh nghiệp","Số vốn góp", "Lĩnh vực", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                            , "Thân nhân sống cùng", "Ghi chú" };
                                arrColumnHeader = arrColumnHeader_1;
                            }
                        }
                        else
                        {
                            if (IsColumnDiscriptionOfActivities)
                            {
                                string[] arrColumnHeader_2 = {"STT", "Tên doanh nghiệp","Số vốn góp", "Lĩnh vực", "Mô tả hoạt động", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                            , "Thân nhân sống cùng", "Ghi chú" };
                                arrColumnHeader = arrColumnHeader_2;
                            }
                            else
                            {
                                string[] arrColumnHeader_2 = {"STT", "Tên doanh nghiệp","Số vốn góp", "Lĩnh vực", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                            , "Thân nhân sống cùng", "Ghi chú" };
                                arrColumnHeader = arrColumnHeader_2;
                            }
                        }
                    }
                    
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
                    // set title
                    DateTime today = DateTime.Today;
                    if (!isInvestment)
                    {
                        if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
                        {
                            ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI NƯỚC NGOÀI TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG \n TỪ NGÀY " + StartDate + " ĐẾN " + EndDate;
                        }
                        else if (!string.IsNullOrWhiteSpace(StartDate))
                        {
                            ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI NƯỚC NGOÀI TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG \n TỪ NGÀY " + StartDate + " ĐẾN " + today.ToString("dd-MM-yyyy");
                        }
                        else if (!string.IsNullOrWhiteSpace(EndDate))
                        {
                            ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI NƯỚC NGOÀI TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG \n TỪ TRƯỚC ĐẾN " + EndDate;
                        }
                        else
                        {
                            ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI NƯỚC NGOÀI TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG";
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
                        {
                            ws.Cells[1, 1].Value = "THỐNG KÊ CÁC DOANH NGHIỆP CÓ NGƯỜI NƯỚC NGOÀI ĐẦU TƯ GÓP VỐN TRÊN ĐỊA BÀN THÀNH PHỐ PHÂN THEO VỐN GÓP/LĨNH VỰC HOẠT ĐỘNG \n TỪ NGÀY " + StartDate + " ĐẾN " + EndDate;
                        }
                        else if (!string.IsNullOrWhiteSpace(StartDate))
                        {
                            ws.Cells[1, 1].Value = "THỐNG KÊ CÁC DOANH NGHIỆP CÓ NGƯỜI NƯỚC NGOÀI ĐẦU TƯ GÓP VỐN TRÊN ĐỊA BÀN THÀNH PHỐ PHÂN THEO VỐN GÓP/LĨNH VỰC HOẠT ĐỘNG \n TỪ NGÀY " + StartDate + " ĐẾN " + today.ToString("dd-MM-yyyy");
                        }
                        else if (!string.IsNullOrWhiteSpace(EndDate))
                        {
                            ws.Cells[1, 1].Value = "THỐNG KÊ CÁC DOANH NGHIỆP CÓ NGƯỜI NƯỚC NGOÀI ĐẦU TƯ GÓP VỐN TRÊN ĐỊA BÀN THÀNH PHỐ PHÂN THEO VỐN GÓP/LĨNH VỰC HOẠT ĐỘNG \n TỪ TRƯỚC ĐẾN " + EndDate;
                        }
                        else
                        {
                            ws.Cells[1, 1].Value = "THỐNG KÊ CÁC DOANH NGHIỆP CÓ NGƯỜI NƯỚC NGOÀI ĐẦU TƯ GÓP VỐN TRÊN ĐỊA BÀN THÀNH PHỐ PHÂN THEO VỐN GÓP/LĨNH VỰC HOẠT ĐỘNG";
                        }
                    }
                    ws.Cells[1, 1, 3, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 3, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 3, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[1, 1, 3, countColHeader].Style.WrapText = true;
                    //tạo các header từ column header đã tạo từ bên trên
                    int colIndex = 1, rowIndex = 5;
                    foreach (var item in arrColumnHeader)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];
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

                    // get list fields from database
                    ObservableCollection<Field> listFields = getFields();
                    int sumCompany = 0, sumTotal = 0, sumAvailable = 0, sumExemption = 0, sumNotYet = 0, sumRelative = 0;
                    if (listFields != null && listFields.Count > 0)
                    {
                        // format export file
                        foreach (Field field in listFields)
                        {
                            // get company from database by field id
                            ObservableCollection<Company> listCompanyByField = getCompanyByField(field.IDField, isInvestment);
                            if(listCompanyByField != null && listCompanyByField.Count > 0)
                            {
                                // content company
                                int order = 0;
                                colIndex = 1;
                                // set title field name
                                ws.Cells[++rowIndex, colIndex].Value = "LĨNH VỰC " + field.FieldName.ToUpper();
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Merge = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Font.Bold = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.WrapText = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                var borderField = ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Border;
                                borderField.Bottom.Style =
                                borderField.Top.Style =
                                borderField.Left.Style =
                                borderField.Right.Style = ExcelBorderStyle.Thin;

                                foreach (Company item in listCompanyByField)
                                {
                                    colIndex = 1;
                                    rowIndex++;
                                    ws.Cells[rowIndex, colIndex++].Value = ++order;
                                    ws.Cells[rowIndex, colIndex++].Value = item.CompanyName;
                                    if (IsColumnAddress)
                                    {
                                        ws.Cells[rowIndex, colIndex++].Value = item.Address;
                                    }
                                    if (isInvestment)
                                    {
                                        ws.Cells[rowIndex, colIndex++].Value = item.Investment;
                                        ws.Cells[rowIndex, colIndex++].Value = item.Field.FieldName;
                                    }
                                    if (IsColumnDiscriptionOfActivities)
                                    {
                                        ws.Cells[rowIndex, colIndex++].Value = item.DescriptionOfActivities;
                                    }
                                    ws.Cells[rowIndex, colIndex++].Value = item.TotalAmount;
                                    ws.Cells[rowIndex, colIndex++].Value = item.QuantityAvailable;
                                    ws.Cells[rowIndex, colIndex++].Value = item.AmountOfExemption;
                                    ws.Cells[rowIndex, colIndex++].Value = item.QuantityNotYet;
                                    ws.Cells[rowIndex, colIndex++].Value = item.NumberOfPersonalities;
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
                                // statistics after a field
                                ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listCompanyByField);
                                colIndex = 1;
                                rowIndex++;
                                ws.Cells[rowIndex, colIndex++].Value = "Tổng";
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[0] + " doanh nghiệp/đơn vị trên lĩnh vực " + field.FieldName.ToLower();
                                if (IsColumnAddress)
                                {
                                    colIndex++;
                                }
                                if (isInvestment)
                                {
                                    colIndex++;
                                    colIndex++;
                                }
                                if (IsColumnDiscriptionOfActivities)
                                {
                                    colIndex++;
                                }
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[1];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[2];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[3];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[4];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[5];
                                colIndex++;
                                for (int i = 1; i < colIndex; i++)
                                {
                                    var border = ws.Cells[rowIndex, i].Style.Border;
                                    border.Bottom.Style =
                                    border.Top.Style =
                                    border.Left.Style =
                                    border.Right.Style = ExcelBorderStyle.Thin;
                                }
                                sumCompany += listStatistics[0];
                                sumTotal += listStatistics[1];
                                sumAvailable += listStatistics[2];
                                sumExemption += listStatistics[3];
                                sumNotYet += listStatistics[4];
                                sumRelative += listStatistics[5];
                            }
                        }
                        // show statistics
                        string[] resultsColumnHeader_1 = { "Doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miễn GPLĐ", "Chưa có GPLĐ", "Thân nhân sống cùng" };
                        rowIndex++;
                        ws.Cells[++rowIndex, 2].Value = "Trong đó:";
                        ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                        ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Bold = true;
                        ws.Cells[rowIndex, 2, rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
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
                        ws.Cells[rowIndex, colIndex++].Value = sumCompany;
                        ws.Cells[rowIndex, colIndex++].Value = sumTotal;
                        ws.Cells[rowIndex, colIndex++].Value = sumAvailable;
                        ws.Cells[rowIndex, colIndex++].Value = sumExemption;
                        ws.Cells[rowIndex, colIndex++].Value = sumNotYet;
                        ws.Cells[rowIndex, colIndex++].Value = sumRelative;
                        for (int i = 3; i < colIndex; i++)
                        {
                            var border = ws.Cells[rowIndex, i].Style.Border;
                            border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }
                        // are employees doing many companies?
                        ObservableCollection<Employee> listEmployeeManyCompanies;
                        if (!isInvestment)
                        {
                            // Synthesis is 1
                            listEmployeeManyCompanies = getEmployeesManyCompanies(1);
                        }
                        else
                        {
                            // Investment is 3
                            listEmployeeManyCompanies = getEmployeesManyCompanies(3);
                        }
                        if(listEmployeeManyCompanies != null && listEmployeeManyCompanies.Count > 0)
                        {
                            rowIndex++;
                            string[] resultsColumnHeader_EmployeeManyCompanies = { "Tên nhân viên", "Số hộ chiếu", "Số lượng công công ty đang làm" };
                            ws.Cells[++rowIndex, 2].Value = "Danh sách nhân viên làm nhiều công ty";
                            ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                            ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Bold = true;
                            ws.Cells[rowIndex, 2, rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            colIndex = 3;
                            rowIndex++;
                            foreach (var item in resultsColumnHeader_EmployeeManyCompanies)
                            {
                                var cell = ws.Cells[rowIndex, colIndex];
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
                            foreach(Employee  item in listEmployeeManyCompanies)
                            {
                                rowIndex++;
                                colIndex = 3;
                                ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                                ws.Cells[rowIndex, colIndex++].Value = item.Passport;
                                // use IDCompany save number of company working
                                ws.Cells[rowIndex, colIndex++].Value = item.IDCompany;
                                for (int i = 3; i < colIndex; i++)
                                {
                                    var border = ws.Cells[rowIndex, i].Style.Border;
                                    border.Bottom.Style =
                                    border.Top.Style =
                                    border.Left.Style =
                                    border.Right.Style = ExcelBorderStyle.Thin;
                                }
                            }
                        }
                        // save file
                        Byte[] bin = package.GetAsByteArray();
                        File.WriteAllBytes(LinkFile, bin);
                        MessageBox.Show("Xuất excel thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Không có lĩnh vực\nVui lòng kiểm tra lại");
                        return;
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + ex.Message);
            }
            finally
            {
                _window.Close();
            }
        }
        private void ExportFileByNational()
        {
            if (!string.IsNullOrWhiteSpace(DateExportFile) && !MethodHandler.checkFormatDate(DateExportFile))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
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
            if (!string.IsNullOrWhiteSpace(TemporaryStay) && !MethodHandler.checkFormatDate(TemporaryStay))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            if( string.IsNullOrWhiteSpace(SelectNationality.NationalityName))
            {
                MessageBox.Show("Vui lòng nhập một mã quốc tịch");
                return;
            }
            try
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    //Tạo một sheet để làm việc trên đó
                    package.Workbook.Worksheets.Add("Báo cáo");
                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = package.Workbook.Worksheets[1];
                    // đặt tên cho sheet
                    ws.Name = "BC theo quốc tịch NNN";

                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    // Tạo danh sách các column header
                    string[] arrColumnHeader;
                    if (IsColumnAddress)
                    {
                        if (IsColumnDiscriptionOfActivities)
                        {
                            string[] arrColumnHeader_1 = {"STT", "Tên doanh nghiệp", "Địa chỉ doanh nghiệp", "Mô tả hoạt động", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                        , "Thân nhân sống cùng", "Ghi chú" };
                            arrColumnHeader = arrColumnHeader_1;
                        }
                        else
                        {
                            string[] arrColumnHeader_1 = {"STT", "Tên doanh nghiệp", "Địa chỉ doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                        , "Thân nhân sống cùng", "Ghi chú" };
                            arrColumnHeader = arrColumnHeader_1;
                        }
                    }
                    else
                    {
                        if (IsColumnDiscriptionOfActivities)
                        {
                            string[] arrColumnHeader_2 = {"STT", "Tên doanh nghiệp", "Mô tả hoạt động", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                        , "Thân nhân sống cùng", "Ghi chú" };
                            arrColumnHeader = arrColumnHeader_2;
                        }
                        else
                        {
                            string[] arrColumnHeader_2 = {"STT", "Tên doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                        , "Thân nhân sống cùng", "Ghi chú" };
                            arrColumnHeader = arrColumnHeader_2;
                        }
                    }

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
                    // set title
                    DateTime today = DateTime.Today;
                    if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI " + SelectNationality.NationalityName.ToUpper() + " TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG\n TỪ NGÀY " + StartDate + " ĐẾN " + EndDate;
                    }
                    else if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI " + SelectNationality.NationalityName.ToUpper() + " TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG\n TỪ NGÀY " + StartDate + " ĐẾN " + today.ToString("dd-MM-yyyy");
                    }
                    else if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI " + SelectNationality.NationalityName.ToUpper() + " TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG\n TỪ TRƯỚC ĐẾN " + EndDate;
                    }
                    else
                    {
                        ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI " + SelectNationality.NationalityName.ToUpper() + " TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG";
                    }
                    ws.Cells[1, 1, 3, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 3, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 3, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[1, 1, 3, countColHeader].Style.WrapText = true;
                    //tạo các header từ column header đã tạo từ bên trên
                    int colIndex = 1, rowIndex = 5;
                    foreach (var item in arrColumnHeader)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];
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

                    // get list field from database
                    ObservableCollection<Field> listFields = getFields();
                    int sumCompany = 0, sumTotal = 0, sumAvailable = 0, sumExemption = 0, sumNotYet = 0, sumRelative = 0;
                    if (listFields != null && listFields.Count > 0)
                    {
                        // format export file
                        foreach (Field field in listFields)
                        {
                            // get company from database by field id
                            ObservableCollection<Company> listCompanyByNationalityByField = getCompanyByNationalityByField(field.IDField);
                            if (listCompanyByNationalityByField != null && listCompanyByNationalityByField.Count > 0)
                            {
                                // content company
                                int order = 0;
                                colIndex = 1;
                                // set title field name
                                ws.Cells[++rowIndex, colIndex].Value = "LĨNH VỰC " + field.FieldName.ToUpper();
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Merge = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Font.Bold = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.WrapText = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                var borderField = ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Border;
                                borderField.Bottom.Style =
                                borderField.Top.Style =
                                borderField.Left.Style =
                                borderField.Right.Style = ExcelBorderStyle.Thin;

                                foreach (Company item in listCompanyByNationalityByField)
                                {
                                    colIndex = 1;
                                    rowIndex++;
                                    ws.Cells[rowIndex, colIndex++].Value = ++order;
                                    ws.Cells[rowIndex, colIndex++].Value = item.CompanyName;
                                    if (IsColumnAddress)
                                    {
                                        ws.Cells[rowIndex, colIndex++].Value = item.Address;
                                    }
                                    if (IsColumnDiscriptionOfActivities)
                                    {
                                        ws.Cells[rowIndex, colIndex++].Value = item.DescriptionOfActivities;
                                    }
                                    ws.Cells[rowIndex, colIndex++].Value = item.TotalAmount;
                                    ws.Cells[rowIndex, colIndex++].Value = item.QuantityAvailable;
                                    ws.Cells[rowIndex, colIndex++].Value = item.AmountOfExemption;
                                    ws.Cells[rowIndex, colIndex++].Value = item.QuantityNotYet;
                                    ws.Cells[rowIndex, colIndex++].Value = item.NumberOfPersonalities;
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
                                // statistics after a field
                                ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listCompanyByNationalityByField);
                                colIndex = 1;
                                rowIndex++;
                                ws.Cells[rowIndex, colIndex++].Value = "Tổng";
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[0] + " doanh nghiệp/đơn vị trên lĩnh vực " + field.FieldName.ToLower();
                                if (IsColumnAddress)
                                {
                                    colIndex++;
                                }
                                if (IsColumnDiscriptionOfActivities)
                                {
                                    colIndex++;
                                }
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[1];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[2];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[3];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[4];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[5];
                                colIndex++;
                                for (int i = 1; i < colIndex; i++)
                                {
                                    var border = ws.Cells[rowIndex, i].Style.Border;
                                    border.Bottom.Style =
                                    border.Top.Style =
                                    border.Left.Style =
                                    border.Right.Style = ExcelBorderStyle.Thin;
                                }
                                sumCompany += listStatistics[0];
                                sumTotal += listStatistics[1];
                                sumAvailable += listStatistics[2];
                                sumExemption += listStatistics[3];
                                sumNotYet += listStatistics[4];
                                sumRelative += listStatistics[5];
                            }
                        }
                        // show statistics
                        string[] resultsColumnHeader_1 = { "Doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miễn GPLĐ", "Chưa có GPLĐ", "Thân nhân sống cùng" };
                        rowIndex++;
                        ws.Cells[++rowIndex, 2].Value = "Trong đó:";
                        ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                        ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Bold = true;
                        ws.Cells[rowIndex, 2, rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
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
                        ws.Cells[rowIndex, colIndex++].Value = sumCompany;
                        ws.Cells[rowIndex, colIndex++].Value = sumTotal;
                        ws.Cells[rowIndex, colIndex++].Value = sumAvailable;
                        ws.Cells[rowIndex, colIndex++].Value = sumExemption;
                        ws.Cells[rowIndex, colIndex++].Value = sumNotYet;
                        ws.Cells[rowIndex, colIndex++].Value = sumRelative;
                        for (int i = 3; i < colIndex; i++)
                        {
                            var border = ws.Cells[rowIndex, i].Style.Border;
                            border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }
                        // are employees doing many companies?
                        // national is 2
                        ObservableCollection<Employee> listEmployeeManyCompanies = getEmployeesManyCompanies(2);
                        if (listEmployeeManyCompanies != null && listEmployeeManyCompanies.Count > 0)
                        {
                            rowIndex++;
                            string[] resultsColumnHeader_EmployeeManyCompanies = { "Tên nhân viên", "Số hộ chiếu", "Số lượng công công ty đang làm" };
                            ws.Cells[++rowIndex, 2].Value = "Danh sách nhân viên làm nhiều công ty";
                            ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                            ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Bold = true;
                            ws.Cells[rowIndex, 2, rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            colIndex = 3;
                            rowIndex++;
                            foreach (var item in resultsColumnHeader_EmployeeManyCompanies)
                            {
                                var cell = ws.Cells[rowIndex, colIndex];
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
                            foreach (Employee item in listEmployeeManyCompanies)
                            {
                                rowIndex++;
                                colIndex = 3;
                                ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                                ws.Cells[rowIndex, colIndex++].Value = item.Passport;
                                // use IDCompany save number of company working
                                ws.Cells[rowIndex, colIndex++].Value = item.IDCompany;
                                for (int i = 3; i < colIndex; i++)
                                {
                                    var border = ws.Cells[rowIndex, i].Style.Border;
                                    border.Bottom.Style =
                                    border.Top.Style =
                                    border.Left.Style =
                                    border.Right.Style = ExcelBorderStyle.Thin;
                                }
                            }
                        }
                        // save file
                        Byte[] bin = package.GetAsByteArray();
                        File.WriteAllBytes(LinkFile, bin);
                        MessageBox.Show("Xuất excel thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Không có lĩnh vực\nVui lòng kiểm tra lại");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + ex.Message);
            }
            finally
            {
                _window.Close();
            }
        }
        private void ExportFileByNationalityOfInvestors()
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
            if (!string.IsNullOrWhiteSpace(TemporaryStay) && !MethodHandler.checkFormatDate(TemporaryStay))
            {
                MessageBox.Show("Định dạng ngày tháng là dd-mm-yyyy \n vui lòng kiểm tra lại");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectNationality.NationalityName))
            {
                MessageBox.Show("Vui lòng nhập một mã quốc tịch");
                return;
            }
            try
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    //Tạo một sheet để làm việc trên đó
                    package.Workbook.Worksheets.Add("Báo cáo");
                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = package.Workbook.Worksheets[1];
                    // đặt tên cho sheet
                    ws.Name = "BC theo quốc tịch pháp nhân";

                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    // Tạo danh sách các column header
                    string[] arrColumnHeader;
                    if (IsColumnAddress)
                    {
                        if (IsColumnDiscriptionOfActivities)
                        {
                            string[] arrColumnHeader_1 = {"STT", "Tên doanh nghiệp", "Địa chỉ doanh nghiệp","Số vốn góp", "Lĩnh vực", "Mô tả hoạt động", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                        , "Thân nhân sống cùng", "Ghi chú" };
                            arrColumnHeader = arrColumnHeader_1;
                        }
                        else
                        {
                            string[] arrColumnHeader_1 = {"STT", "Tên doanh nghiệp", "Địa chỉ doanh nghiệp","Số vốn góp", "Lĩnh vực", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                        , "Thân nhân sống cùng", "Ghi chú" };
                            arrColumnHeader = arrColumnHeader_1;
                        }
                    }
                    else
                    {
                        if (IsColumnDiscriptionOfActivities)
                        {
                            string[] arrColumnHeader_2 = {"STT", "Tên doanh nghiệp","Số vốn góp", "Lĩnh vực", "Mô tả hoạt động", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                        , "Thân nhân sống cùng", "Ghi chú" };
                            arrColumnHeader = arrColumnHeader_2;
                        }
                        else
                        {
                            string[] arrColumnHeader_2 = {"STT", "Tên doanh nghiệp","Số vốn góp", "Lĩnh vực", "Tổng số NNN", "Có GPLĐ", "Miến GPLĐ", "Chưa có GPLĐ"
                        , "Thân nhân sống cùng", "Ghi chú" };
                            arrColumnHeader = arrColumnHeader_2;
                        }
                    }

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
                    // set title
                    DateTime today = DateTime.Today;
                    if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "THỐNG KÊ DOANH NGHIỆP CÓ VỐN ĐẦU TƯ " + SelectNationality.NationalityName.ToUpper() + " \n TỪ NGÀY " + StartDate + " ĐẾN " + EndDate;
                    }
                    else if (!string.IsNullOrWhiteSpace(StartDate))
                    {
                        ws.Cells[1, 1].Value = "THỐNG KÊ DOANH NGHIỆP CÓ VỐN ĐẦU TƯ " + SelectNationality.NationalityName.ToUpper() + " \n TỪ NGÀY " + StartDate + " ĐẾN " + today.ToString("dd-MM-yyyy");
                    }
                    else if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        ws.Cells[1, 1].Value = "THỐNG KÊ DOANH NGHIỆP CÓ VỐN ĐẦU TƯ " + SelectNationality.NationalityName.ToUpper() + " \n TỪ TRƯỚC ĐẾN " + EndDate;
                    }
                    else
                    {
                        ws.Cells[1, 1].Value = "THỐNG KÊ DOANH NGHIỆP CÓ VỐN ĐẦU TƯ " + SelectNationality.NationalityName.ToUpper();
                    }
                    ws.Cells[1, 1, 3, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 3, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 3, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[1, 1, 3, countColHeader].Style.WrapText = true;
                    //tạo các header từ column header đã tạo từ bên trên
                    int colIndex = 1, rowIndex = 5;
                    foreach (var item in arrColumnHeader)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];
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

                    // get list ward from local
                    ObservableCollection<string> listWards = MethodHandler.getWards();
                    int sumCompany = 0, sumTotal = 0, sumAvailable = 0, sumExemption = 0, sumNotYet = 0, sumRelative = 0;
                    if (listWards != null && listWards.Count > 0)
                    {
                        // format export file
                        foreach (string ward in listWards)
                        {
                            // get company from database by ward
                            ObservableCollection<Company> listCompanyByWard = getCompanyByWard(ward);
                            if (listCompanyByWard != null && listCompanyByWard.Count > 0)
                            {
                                // content company
                                int order = 0;
                                colIndex = 1;
                                // set title field name
                                ws.Cells[++rowIndex, colIndex].Value = "Phường (Xã) " + ward.ToUpper();
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Merge = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Font.Bold = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.WrapText = true;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                var borderField = ws.Cells[rowIndex, 1, rowIndex, countColHeader].Style.Border;
                                borderField.Bottom.Style =
                                borderField.Top.Style =
                                borderField.Left.Style =
                                borderField.Right.Style = ExcelBorderStyle.Thin;

                                foreach (Company item in listCompanyByWard)
                                {
                                    colIndex = 1;
                                    rowIndex++;
                                    ws.Cells[rowIndex, colIndex++].Value = ++order;
                                    ws.Cells[rowIndex, colIndex++].Value = item.CompanyName;
                                    if (IsColumnAddress)
                                    {
                                        ws.Cells[rowIndex, colIndex++].Value = item.Address;
                                    }
                                    ws.Cells[rowIndex, colIndex++].Value = item.Investment;
                                    ws.Cells[rowIndex, colIndex++].Value = item.Field.FieldName;
                                    if (IsColumnDiscriptionOfActivities)
                                    {
                                        ws.Cells[rowIndex, colIndex++].Value = item.DescriptionOfActivities;
                                    }
                                    ws.Cells[rowIndex, colIndex++].Value = item.TotalAmount;
                                    ws.Cells[rowIndex, colIndex++].Value = item.QuantityAvailable;
                                    ws.Cells[rowIndex, colIndex++].Value = item.AmountOfExemption;
                                    ws.Cells[rowIndex, colIndex++].Value = item.QuantityNotYet;
                                    ws.Cells[rowIndex, colIndex++].Value = item.NumberOfPersonalities;
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
                                // statistics after a field
                                ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listCompanyByWard);
                                colIndex = 1;
                                rowIndex++;
                                ws.Cells[rowIndex, colIndex++].Value = "Tổng";
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[0] + " doanh nghiệp/đơn vị trên phường(xã) " + ward.ToLower();
                                if (IsColumnAddress)
                                {
                                    colIndex++;
                                }
                                colIndex++;
                                colIndex++;
                                if (IsColumnDiscriptionOfActivities)
                                {
                                    colIndex++;
                                }
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[1];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[2];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[3];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[4];
                                ws.Cells[rowIndex, colIndex++].Value = listStatistics[5];
                                colIndex++;
                                for (int i = 1; i < colIndex; i++)
                                {
                                    var border = ws.Cells[rowIndex, i].Style.Border;
                                    border.Bottom.Style =
                                    border.Top.Style =
                                    border.Left.Style =
                                    border.Right.Style = ExcelBorderStyle.Thin;
                                }
                                sumCompany += listStatistics[0];
                                sumTotal += listStatistics[1];
                                sumAvailable += listStatistics[2];
                                sumExemption += listStatistics[3];
                                sumNotYet += listStatistics[4];
                                sumRelative += listStatistics[5];
                            }
                        }
                        // show statistics
                        string[] resultsColumnHeader_1 = { "Doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miễn GPLĐ", "Chưa có GPLĐ", "Thân nhân sống cùng" };
                        rowIndex++;
                        ws.Cells[++rowIndex, 2].Value = "Trong đó:";
                        ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                        ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Bold = true;
                        ws.Cells[rowIndex, 2, rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
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
                        ws.Cells[rowIndex, colIndex++].Value = sumCompany;
                        ws.Cells[rowIndex, colIndex++].Value = sumTotal;
                        ws.Cells[rowIndex, colIndex++].Value = sumAvailable;
                        ws.Cells[rowIndex, colIndex++].Value = sumExemption;
                        ws.Cells[rowIndex, colIndex++].Value = sumNotYet;
                        ws.Cells[rowIndex, colIndex++].Value = sumRelative;
                        for (int i = 3; i < colIndex; i++)
                        {
                            var border = ws.Cells[rowIndex, i].Style.Border;
                            border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }
                        // are employees doing many companies?
                        // NationalityOfInvestors is 4
                        ObservableCollection<Employee> listEmployeeManyCompanies = getEmployeesManyCompanies(4);
                        if (listEmployeeManyCompanies != null && listEmployeeManyCompanies.Count > 0)
                        {
                            rowIndex++;
                            string[] resultsColumnHeader_EmployeeManyCompanies = { "Tên nhân viên", "Số hộ chiếu", "Số lượng công công ty đang làm" };
                            ws.Cells[++rowIndex, 2].Value = "Danh sách nhân viên làm nhiều công ty";
                            ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                            ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Bold = true;
                            ws.Cells[rowIndex, 2, rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            colIndex = 3;
                            rowIndex++;
                            foreach (var item in resultsColumnHeader_EmployeeManyCompanies)
                            {
                                var cell = ws.Cells[rowIndex, colIndex];
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
                            foreach (Employee item in listEmployeeManyCompanies)
                            {
                                rowIndex++;
                                colIndex = 3;
                                ws.Cells[rowIndex, colIndex++].Value = item.StaffName;
                                ws.Cells[rowIndex, colIndex++].Value = item.Passport;
                                // use IDCompany save number of company working
                                ws.Cells[rowIndex, colIndex++].Value = item.IDCompany;
                                for (int i = 3; i < colIndex; i++)
                                {
                                    var border = ws.Cells[rowIndex, i].Style.Border;
                                    border.Bottom.Style =
                                    border.Top.Style =
                                    border.Left.Style =
                                    border.Right.Style = ExcelBorderStyle.Thin;
                                }
                            }
                        }
                        // save file
                        Byte[] bin = package.GetAsByteArray();
                        File.WriteAllBytes(LinkFile, bin);
                        MessageBox.Show("Xuất excel thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Không có lĩnh vực\nVui lòng kiểm tra lại");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi xuất file \n" + ex.Message);
            }
            finally
            {
                _window.Close();
            }
        }

        private ObservableCollection<Field> getFields()
        {
            ObservableCollection<Field> listFields = new ObservableCollection<Field>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Fields where Fields.Delete_flag = 0", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listFields.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Field rowField = new Field();
                            rowField.IDField = int.Parse(dr["IDField"].ToString());
                            rowField.FieldName = dr["FieldName"].ToString();
                            rowField.Description = dr["Description"].ToString();
                            listFields.Add(rowField);
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
            return listFields;
        }

        private ObservableCollection<Company> getCompanyByField(int idField, bool isInvestment = false)
        {
            ObservableCollection<Company> listCompanies = new ObservableCollection<Company>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql;
                    if (!isInvestment)
                    {
                        sql = "select * from Companies where Delete_flag = 0 and IDField = " + idField;
                    }
                    else
                    {
                        sql = "select distinct IDCompany, CompanyName, Address, TotalAmount, AmountOfExemption,QuantityAvailable, QuantityNotYet, NumberOfPersonalities, Note, FieldName, DescriptionOfActivities "
                            + "from Companies, Fields where Companies.IDField = Fields.IDField and Companies.Delete_flag = 0 and Companies.IDField = "+ idField;
                        if (MinInvestment != 0 || MaxInvestment != 0)
                        {
                            if (MinInvestment != 0)
                            {
                                sql += " and IDCompany in (select Investment.IDCompany from Investment where Investment.AmountOfMoney >= " + MinInvestment;
                                if (MaxInvestment != 0)
                                {
                                    sql += " and Investment.AmountOfMoney<= " + MaxInvestment;
                                }
                                if (!string.IsNullOrWhiteSpace(SelectNationality.NationalityName))
                                {
                                    sql += " and Investment.Nationality like N'%" + SelectNationality.NationalityName + "%'";
                                }
                                // check investment
                                if (IsCheckInvestment)
                                {
                                    sql += " union select IDCompany from Companies where IDCompany not in (select IDCompany from Investment)";
                                }
                                sql += ")";
                            }
                            else
                            {
                                sql += " and IDCompany in (select Investment.IDCompany from Investment where Investment.AmountOfMoney <= " + MaxInvestment;
                                if (!string.IsNullOrWhiteSpace(SelectNationality.NationalityName))
                                {
                                    sql += " and Investment.Nationality like N'%" + SelectNationality.NationalityName + "%'";
                                }
                                // check investment
                                if (IsCheckInvestment)
                                {
                                    sql += " union select IDCompany from Companies where IDCompany not in (select IDCompany from Investment)";
                                }
                                sql += ")";
                            }
                        }
                        else
                        {
                            // MinInvestment ==0 && MaxInvestment == 0
                            if (!string.IsNullOrWhiteSpace(SelectNationality.NationalityName))
                            {
                                sql += " and IDCompany in (select Investment.IDCompany from Investment where Investment.Nationality like N'%" + SelectNationality.NationalityName + "%'";
                                // check investment
                                if (IsCheckInvestment)
                                {
                                    sql += " union select IDCompany from Companies where IDCompany not in (select IDCompany from Investment)";
                                }
                                sql += ")";
                            }
                            else
                            {
                                // check investment
                                if (IsCheckInvestment)
                                {
                                    sql += " and IDCompany not in (select IDCompany from Investment)";
                                }
                            }
                        }
                    }
                    sql += " order by IDCompany, Companies.CompanyName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listCompanies.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Company rowCompany = new Company();
                            rowCompany.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowCompany.CompanyName = dr["CompanyName"].ToString().Trim();
                            rowCompany.Address = dr["Address"].ToString().Trim();
                            rowCompany.DescriptionOfActivities = dr["DescriptionOfActivities"].ToString().Trim();
                            // can set amount when date changed
                            DataTable dtGetEmployee = new DataTable();
                            DateTime today = DateTime.Now;
                            string sqlEmployee = "select Employees.IDEmployee, Employees.StaffName, Employees.WorkPermit, Employees.SettlementResults from Employees"
                                + " inner join( select StaffName, Passport, IDCompany, MAX(DateCreated) as maxDate from Employees "
                                +"where DateCreated <= '"+(string.IsNullOrWhiteSpace(DateExportFile) ? today.ToString("yyyy-MM-dd") : MethodHandler.formatDatefromUsertoDatabase(DateExportFile)) +" 23:59:59' group by StaffName,Passport, IDCompany) t"
                                + " on Employees.StaffName = t.StaffName and Employees.Passport = t.Passport and Employees.IDCompany = t.IDCompany and t.maxDate = Employees.DateCreated"
                                + " where Employees.IDCompany = " + rowCompany.IDCompany;
                            // check date created employee
                            if (!string.IsNullOrWhiteSpace(DateExportFile))
                            {
                                sqlEmployee += " and Employees.DateCreated <= '"+ MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                            }
                            else
                            {
                                sqlEmployee += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                            }
                            // check card creation date is null
                            if ((!string.IsNullOrWhiteSpace(StartDate)) && (!string.IsNullOrWhiteSpace(EndDate)))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated between '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + " 00:00:00' and '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + " 23:59:59') or" : "")
                                    + " (Employees.CardCreationDate between '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "' and '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'))";
                            }
                            else if(!string.IsNullOrWhiteSpace(StartDate))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + " 00:00:00') or" : "")
                                    + " (Employees.CardCreationDate >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'))";
                            } else if (!string.IsNullOrWhiteSpace(EndDate))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + " 23:59:59') or" : "")
                                    + " (Employees.CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'))";
                            }
                            // check temporary stay
                            if (!string.IsNullOrWhiteSpace(TemporaryStay))
                            {
                                sqlEmployee += " and Employees.TemporaryStay >= '" + MethodHandler.formatDatefromUsertoDatabase(TemporaryStay) + "'";
                            }
                            // add condition working status
                            if (!string.IsNullOrWhiteSpace(DateExportFile))
                            {
                                sqlEmployee += " and (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + "')";
                            }
                            else
                            {
                                sqlEmployee += " and (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + today.ToString("yyyy-MM-dd") + "')";
                            }
                            adapter.SelectCommand = new SqlCommand(sqlEmployee, con);
                            adapter.Fill(dtGetEmployee);
                            ObservableCollection<Employee> listEmployeeByField = new ObservableCollection<Employee>();
                            if (dtGetEmployee.Rows.Count > 0)
                            {
                                listEmployeeByField.Clear();
                                foreach (DataRow row in dtGetEmployee.Rows)
                                {
                                    Employee rowEmployees = new Employee();
                                    rowEmployees.IDEmployee = int.Parse(row["IDEmployee"].ToString());
                                    rowEmployees.StaffName = row["StaffName"].ToString();
                                    rowEmployees.WorkPermit = int.Parse(row["WorkPermit"].ToString());
                                    rowEmployees.SettlementResults = int.Parse(row["SettlementResults"].ToString());
                                    listEmployeeByField.Add(rowEmployees);
                                }
                            }
                            ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeeByField);
                            rowCompany.TotalAmount = listEmployeeByField.Count;
                            if(listEmployeeByField.Count == 0)
                            {
                                continue;
                            }
                            rowCompany.QuantityAvailable = listStatistics[0];
                            rowCompany.AmountOfExemption = listStatistics[1];
                            rowCompany.QuantityNotYet = listStatistics[2];
                            rowCompany.NumberOfPersonalities = listStatistics[3];
                            rowCompany.Note = dr["Note"].ToString().Trim();
                            rowCompany.LineNumber = ++lineNumber;
                            if (isInvestment)
                            {
                                rowCompany.Field.FieldName = dr["FieldName"].ToString().Trim();
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
                            }
                            listCompanies.Add(rowCompany);
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
            return listCompanies;
        }

        private ObservableCollection<Company> getCompanyByWard(string ward)
        {
            ObservableCollection<Company> listCompanies = new ObservableCollection<Company>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql;
                    sql = "select distinct IDCompany, CompanyName, Address, FieldName, TotalAmount, QuantityAvailable, AmountOfExemption, QuantityNotYet, NumberOfPersonalities, Note, DescriptionOfActivities "
                        + "from Companies, Fields where Companies.Delete_flag = 0 and Companies.IDField = Fields.IDField "
                        + "and TypeOfBusiniess  like N'%" + SelectNationality.NationalityName + "%' "
                        + "and Address like N'%" + ward + "'";
                    sql += " order by Companies.CompanyName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listCompanies.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Company rowCompany = new Company();
                            rowCompany.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowCompany.CompanyName = dr["CompanyName"].ToString().Trim();
                            rowCompany.Address = dr["Address"].ToString().Trim();
                            rowCompany.DescriptionOfActivities = dr["DescriptionOfActivities"].ToString().Trim();
                            // can set amount when date changed
                            DataTable dtGetEmployee = new DataTable();
                            DateTime today = DateTime.Now;
                            string sqlEmployee = "select Employees.IDEmployee, Employees.StaffName, Employees.WorkPermit, Employees.SettlementResults from Employees"
                                + " inner join( select StaffName, Passport, IDCompany, MAX(DateCreated) as maxDate from Employees "
                                + "where DateCreated <= '" + (string.IsNullOrWhiteSpace(DateExportFile) ? today.ToString("yyyy-MM-dd") : MethodHandler.formatDatefromUsertoDatabase(DateExportFile)) + " 23:59:59' group by StaffName,Passport, IDCompany) t"
                                + " on Employees.StaffName = t.StaffName and Employees.Passport = t.Passport and Employees.IDCompany = t.IDCompany and t.maxDate = Employees.DateCreated"
                                + " where Employees.IDCompany = " + rowCompany.IDCompany;
                            // check date created employee
                            if (!string.IsNullOrWhiteSpace(DateExportFile))
                            {
                                sqlEmployee += " and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                            }
                            else
                            {
                                sqlEmployee += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                            }
                            // check card creation date is null
                            if ((!string.IsNullOrWhiteSpace(StartDate)) && (!string.IsNullOrWhiteSpace(EndDate)))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated between '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + " 00:00:00' and '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + " 23:59:59') or" : "")
                                    + " (Employees.CardCreationDate between '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "' and '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'))";
                            }
                            else if (!string.IsNullOrWhiteSpace(StartDate))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + " 00:00:00') or" : "")
                                    + " (Employees.CardCreationDate >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'))";
                            }
                            else if (!string.IsNullOrWhiteSpace(EndDate))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + " 23:59:59') or" : "")
                                    + " (Employees.CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'))";
                            }
                            // check temporary stay
                            if (!string.IsNullOrWhiteSpace(TemporaryStay))
                            {
                                sqlEmployee += " and Employees.TemporaryStay >= '" + MethodHandler.formatDatefromUsertoDatabase(TemporaryStay) + "'";
                            }
                            // add condition working status
                            if (!string.IsNullOrWhiteSpace(DateExportFile))
                            {
                                sqlEmployee += " and (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + "')";
                            }
                            else
                            {
                                sqlEmployee += " and (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + today.ToString("yyyy-MM-dd") + "')";
                            }
                            adapter.SelectCommand = new SqlCommand(sqlEmployee, con);
                            adapter.Fill(dtGetEmployee);
                            ObservableCollection<Employee> listEmployeeByField = new ObservableCollection<Employee>();
                            if (dtGetEmployee.Rows.Count > 0)
                            {
                                listEmployeeByField.Clear();
                                foreach (DataRow row in dtGetEmployee.Rows)
                                {
                                    Employee rowEmployees = new Employee();
                                    rowEmployees.IDEmployee = int.Parse(row["IDEmployee"].ToString());
                                    rowEmployees.StaffName = row["StaffName"].ToString();
                                    rowEmployees.WorkPermit = int.Parse(row["WorkPermit"].ToString());
                                    rowEmployees.SettlementResults = int.Parse(row["SettlementResults"].ToString());
                                    listEmployeeByField.Add(rowEmployees);
                                }
                            }
                            ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeeByField);
                            rowCompany.TotalAmount = listEmployeeByField.Count;
                            if (listEmployeeByField.Count == 0)
                            {
                                continue;
                            }
                            rowCompany.QuantityAvailable = listStatistics[0];
                            rowCompany.AmountOfExemption = listStatistics[1];
                            rowCompany.QuantityNotYet = listStatistics[2];
                            rowCompany.NumberOfPersonalities = listStatistics[3];
                            rowCompany.Note = dr["Note"].ToString().Trim();
                            rowCompany.LineNumber = ++lineNumber;
                            rowCompany.Field.FieldName = dr["FieldName"].ToString().Trim();
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
                            listCompanies.Add(rowCompany);
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
            return listCompanies;
        }

        private ObservableCollection<Company> getCompanyByNationalityByField(int idField)
        {
            ObservableCollection<Company> listCompanies = new ObservableCollection<Company>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql;
                    sql = "select distinct IDCompany, CompanyName, Address, TotalAmount, QuantityAvailable, AmountOfExemption, QuantityNotYet, NumberOfPersonalities, Note, DescriptionOfActivities "
                        + "from Companies where Delete_flag = 0 "
                        + "and IDCompany in (select distinct IDCompany from Employees where Nationality like N'%" + SelectNationality.NationalityCode + "%') "
                        + "and IDField = " + idField;
                    sql += " order by Companies.CompanyName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listCompanies.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Company rowCompany = new Company();
                            rowCompany.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowCompany.CompanyName = dr["CompanyName"].ToString().Trim();
                            rowCompany.Address = dr["Address"].ToString().Trim();
                            rowCompany.DescriptionOfActivities = dr["DescriptionOfActivities"].ToString().Trim();
                            // can set amount when date changed
                            // get employee by company and nationality
                            DataTable dtGet = new DataTable();
                            DateTime today = DateTime.Now;
                            string sqlEmployee = "select Employees.IDEmployee, Employees.StaffName, Employees.Nationality, Employees.WorkPermit, Employees.SettlementResults, NationalityCode, NationalityName from Nationality, Employees"
                                + " inner join( select StaffName, Passport, IDCompany, MAX(DateCreated) as maxDate from Employees "
                                + "where DateCreated <= '" + (string.IsNullOrWhiteSpace(DateExportFile) ? today.ToString("yyyy-MM-dd") : MethodHandler.formatDatefromUsertoDatabase(DateExportFile)) + " 23:59:59' group by StaffName,Passport, IDCompany) t"
                                + " on Employees.StaffName = t.StaffName and Employees.Passport = t.Passport and Employees.IDCompany = t.IDCompany and t.maxDate = Employees.DateCreated"
                                + " where Nationality.NationalityCode like Employees.Nationality and Employees.Nationality like N'%" + SelectNationality.NationalityCode + "%' and Employees.IDCompany = " + rowCompany.IDCompany;
                            // check date created employee
                            if (!string.IsNullOrWhiteSpace(DateExportFile))
                            {
                                sqlEmployee += " and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                            }
                            else
                            {
                                sqlEmployee += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                            }
                            // check card creation date is null
                            if ((!string.IsNullOrWhiteSpace(StartDate)) && (!string.IsNullOrWhiteSpace(EndDate)))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated between '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + " 00:00:00' and '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + " 23:59:59') or" : "")
                                    + " (Employees.CardCreationDate between '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "' and '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'))";
                            }
                            else if (!string.IsNullOrWhiteSpace(StartDate))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + " 00:00:00') or" : "")
                                    + " (Employees.CardCreationDate >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'))";
                            }
                            else if (!string.IsNullOrWhiteSpace(EndDate))
                            {
                                sqlEmployee += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + " 23:59:59') or" : "")
                                    + " (Employees.CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'))";
                            }
                            // check temporary stay
                            if (!string.IsNullOrWhiteSpace(TemporaryStay))
                            {
                                sqlEmployee += " and Employees.TemporaryStay >= '" + MethodHandler.formatDatefromUsertoDatabase(TemporaryStay) + "'";
                            }
                            // add condition working status
                            if (!string.IsNullOrWhiteSpace(DateExportFile))
                            {
                                sqlEmployee += " and (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + "')";
                            }
                            else
                            {
                                sqlEmployee += " and (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + today.ToString("yyyy-MM-dd") + "')";
                            }
                            adapter.SelectCommand = new SqlCommand(sqlEmployee, con);
                            adapter.Fill(dtGet);
                            ObservableCollection<Employee> listEmployeeByNational = new ObservableCollection<Employee>();
                            if (dtGet.Rows.Count > 0)
                            {
                                listEmployeeByNational.Clear();
                                foreach (DataRow row in dtGet.Rows)
                                {
                                    Employee rowEmployees = new Employee();
                                    rowEmployees.IDEmployee = int.Parse(row["IDEmployee"].ToString());
                                    rowEmployees.StaffName = row["StaffName"].ToString();
                                    rowEmployees.Nationality.NationalityCode = row["NationalityCode"].ToString();
                                    rowEmployees.Nationality.NationalityName = row["NationalityName"].ToString();
                                    rowEmployees.WorkPermit = int.Parse(row["WorkPermit"].ToString());
                                    rowEmployees.SettlementResults = int.Parse(row["SettlementResults"].ToString());
                                    listEmployeeByNational.Add(rowEmployees);
                                }
                            }
                            ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeeByNational);
                            rowCompany.TotalAmount = listEmployeeByNational.Count;
                            if(listEmployeeByNational.Count == 0)
                            {
                                continue;
                            }
                            rowCompany.QuantityAvailable = listStatistics[0];
                            rowCompany.AmountOfExemption = listStatistics[1];
                            rowCompany.QuantityNotYet = listStatistics[2];
                            rowCompany.NumberOfPersonalities = listStatistics[3];
                            rowCompany.Note = dr["Note"].ToString().Trim();
                            rowCompany.LineNumber = ++lineNumber;
                            listCompanies.Add(rowCompany);
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
            return listCompanies;
        }
        /// <summary>
        ///  Synthesis is 1
        ///  National is 2
        ///  Investment is 3
        ///  NationalityOfInvestors is 4
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<Employee> getEmployeesManyCompanies(int caseExport)
        {
            DateTime today = DateTime.Now;
            string sql = "";
            string sqlAllEmployees = "";
            string sqlCompany = "";
            ObservableCollection<Employee> listEmployees = new ObservableCollection<Employee>();
            switch (caseExport)
            {
                case 1:
                    sqlAllEmployees = "select StaffName, Passport, IDCompany, MAX(DateCreated) as maxDate from Employees where IDCompany in (select IDCompany from Companies where Delete_flag = 0)";
                    if (!string.IsNullOrWhiteSpace(DateExportFile))
                    {
                        sqlAllEmployees += " and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                    }
                    else
                    {
                        sqlAllEmployees += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                    }
                    sqlAllEmployees += " group by StaffName,Passport, IDCompany";
                    break;
                case 2:
                    sqlCompany = "select distinct IDCompany from Companies where Delete_flag = 0 "
                        + " and IDCompany in (select distinct IDCompany from Employees where Nationality like N'%" + SelectNationality.NationalityCode + "%')";
                    // set sql all employees
                    sqlAllEmployees = "select StaffName, Passport, IDCompany, MAX(DateCreated) as maxDate from Employees where"
                        + " IDCompany in (" + sqlCompany + ") and Nationality like N'%" + SelectNationality.NationalityCode + "%'";
                    // check date created employee
                    if (!string.IsNullOrWhiteSpace(DateExportFile))
                    {
                        sqlAllEmployees += " and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                    }
                    else
                    {
                        sqlAllEmployees += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                    }
                    sqlAllEmployees += " group by StaffName,Passport, IDCompany";
                    break;
                case 3:
                    sqlCompany = "select IDCompany from Companies where Delete_flag = 0";
                    if (MinInvestment != 0 || MaxInvestment != 0)
                    {
                        if (MinInvestment != 0)
                        {
                            sql += " and IDCompany in (select Investment.IDCompany from Investment where Investment.AmountOfMoney >= " + MinInvestment;
                            if (MaxInvestment != 0)
                            {
                                sql += " and Investment.AmountOfMoney<= " + MaxInvestment;
                            }
                            // check nationality code may be wrong
                            if(!string.IsNullOrWhiteSpace(SelectNationality.NationalityName))
                            {
                                sql += " and Investment.Nationality like N'%" + SelectNationality.NationalityName + "%'";
                            }
                            sql += ")";
                        }
                        else
                        {
                            sql += " and IDCompany in (select Investment.IDCompany from Investment where Investment.AmountOfMoney <= " + MaxInvestment;
                            if (!string.IsNullOrWhiteSpace(SelectNationality.NationalityName))
                            {
                                sql += " and Investment.Nationality like N'%" + SelectNationality.NationalityName + "%'";
                            }
                            sql += ")";
                        }
                    }
                    // set sql all employees
                    sqlAllEmployees = "select StaffName, Passport, IDCompany, MAX(DateCreated) as maxDate from Employees where"
                        + " IDCompany in (" + sqlCompany + ")";
                    // check date created employee
                    if (!string.IsNullOrWhiteSpace(DateExportFile))
                    {
                        sqlAllEmployees += " and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                    }
                    else
                    {
                        sqlAllEmployees += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                    }
                    sqlAllEmployees += " group by StaffName,Passport, IDCompany";
                    break;
                case 4:
                    sqlCompany = "select IDCompany from Companies where Companies.Delete_flag = 0"
                        + "and IDCompany in (select distinct IDCompany from Investment where Nationality like N'%" + SelectNationality.NationalityName + "%')";
                    sqlAllEmployees = "select StaffName, Passport, IDCompany, MAX(DateCreated) as maxDate from Employees where IDCompany in (" + sqlCompany + ")";
                    if (!string.IsNullOrWhiteSpace(DateExportFile))
                    {
                        sqlAllEmployees += " and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                    }
                    else
                    {
                        sqlAllEmployees += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                    }
                    sqlAllEmployees += " group by StaffName,Passport, IDCompany";
                    break;
                default:
                    return listEmployees;
            }
            // inner join
            sql = "select Employees.StaffName, Employees.Passport, COUNT(Employees.Passport) as countpassport from Employees inner join (" + sqlAllEmployees + ") t "
                + "on Employees.StaffName = t.StaffName and Employees.Passport = t.Passport and Employees.IDCompany = t.IDCompany and Employees.DateCreated = t.maxDate where";
            // add condition working status
            if (!string.IsNullOrWhiteSpace(DateExportFile))
            {
                sql += " (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + "')";
            }
            else
            {
                sql += " (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + today.ToString("yyyy-MM-dd") + "')";
            }
            // check card creation date is null
            if ((!string.IsNullOrWhiteSpace(StartDate)) && (!string.IsNullOrWhiteSpace(EndDate)))
            {
                sql += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated between '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + " 00:00:00' and '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + " 23:59:59') or" : "")
                    + " (Employees.CardCreationDate between '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "' and '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'))";
            }
            else if (!string.IsNullOrWhiteSpace(StartDate))
            {
                sql += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + " 00:00:00') or" : "")
                    + " (Employees.CardCreationDate >= '" + MethodHandler.formatDatefromUsertoDatabase(StartDate) + "'))";
            }
            else if (!string.IsNullOrWhiteSpace(EndDate))
            {
                sql += " and (" + ((IsColumnDateCreated) ? "(Employees.CardCreationDate is null and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + " 23:59:59') or" : "")
                    + " (Employees.CardCreationDate <= '" + MethodHandler.formatDatefromUsertoDatabase(EndDate) + "'))";
            }
            // check temporary stay
            if (!string.IsNullOrWhiteSpace(TemporaryStay))
            {
                sql += " and Employees.TemporaryStay >= '" + MethodHandler.formatDatefromUsertoDatabase(TemporaryStay) + "'";
            }
            sql += " group by Employees.StaffName, Employees.Passport having COUNT(Employees.Passport) > 1 order by Employees.StaffName";

            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listEmployees.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Employee rowEmployee = new Employee();
                            rowEmployee.StaffName = dr["StaffName"].ToString().Trim();
                            rowEmployee.Passport = dr["Passport"].ToString().Trim();
                            rowEmployee.IDCompany = int.Parse(dr["countpassport"].ToString());
                            listEmployees.Add(rowEmployee);
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
            return listEmployees;
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
