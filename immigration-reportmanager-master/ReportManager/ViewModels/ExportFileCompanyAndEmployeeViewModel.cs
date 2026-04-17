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
    class ExportFileCompanyAndEmployeeViewModel : INotifyPropertyChanged
    {
        #region properties
        public static readonly string LinkFileProperty = "LinkFile";
        public static readonly string SelectFormatFileProperty = "SelectFormatFile";
        public static readonly string ListFieldsProperty = "ListFields";
        public static readonly string SelectFieldProperty = "SelectField";
        public static readonly string DateExportFileProperty = "DateExportFile";
        public static readonly string TemporaryStayProperty = "TemporaryStay";
        public static readonly string ListNationalitiesProperty = "ListNationalities";
        public static readonly string SelectNationalityProperty = "SelectNationality";
        private string _linkFile;
        private int _selectFormatFile;
        private ObservableCollection<Field> _listFields;
        private Field _selectField;
        private string _dateExportFile;
        private string _temporaryStay;
        private ObservableCollection<Nationality> _listNationalities;
        private Nationality _selectNationality;
        private ExportFileCompanyAndEmployeeWindow _window;
        private ICommand _btnSaveLinkCommand;
        private ICommand _loadDataFieldsCommamnd;
        private ICommand _btnSetNationalityCommand;
        private ICommand _btnExportFileCommand;
        private ICommand _btnCancelCommand;
        #endregion
        #region set get method
        public ExportFileCompanyAndEmployeeViewModel(ExportFileCompanyAndEmployeeWindow window)
        {
            _window = window;
            // load list field
            _listFields = new ObservableCollection<Field>();
            LoadDataFieldsCommand.Execute(true);
            SelectFormatFile = 0;
            _selectField = _listFields[0];
            ListNationalities = MethodHandler.getNationalities();
            SelectNationality = new Nationality();
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
                switch (_selectFormatFile)
                {
                    case 0:
                        _window.gridcbbField.Visibility = Visibility.Visible;
                        _window.DateExportFile.Visibility = Visibility.Collapsed;
                        _window.TemporaryStay.Visibility = Visibility.Collapsed;
                        _window.gridnationality.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        _window.gridcbbField.Visibility = Visibility.Collapsed;
                        _window.DateExportFile.Visibility = Visibility.Visible;
                        _window.TemporaryStay.Visibility = Visibility.Visible;
                        _window.gridnationality.Visibility = Visibility.Visible;
                        break;
                }
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
                    adapter.SelectCommand = new SqlCommand("select * from Fields order by FieldName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListFields.Clear();
                        Field allField = new Field();
                        allField.IDField = -1;
                        allField.FieldName = "Tất cả";
                        ListFields.Add(allField);
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
            switch (SelectFormatFile)
            {
                case 0:
                    ExportByField();
                    break;
                case 1:
                    ExportFileByNational();
                    break;
            }
        }
        private void ExportByField()
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
                    ws.Name = "Báo cáo nhân viên theo lĩnh vực công ty";

                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    // Tạo danh sách các column header
                    string[] arrColumnHeader = {"Stt", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu", "Địa chỉ tạm trú"
                            , "Nghề nghiệp (Chức danh)", "Số GPLĐ/Thời hạn", "Số visa/Thẻ tạm trú", "Thời hạn tạm trú"};

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
                    ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI NƯỚC NGOÀI TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG" + (SelectField.IDField != -1 ? "THEO LĨNH VỰC " + SelectField.FieldName : "") + "\nTẠI THỜI ĐIỂM " + today.ToString("dd-MM-yyyy");
                    
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

                    // set list field
                    ObservableCollection<Field> listFields = new ObservableCollection<Field>();
                    if (SelectField.IDField != -1)
                    {
                        listFields.Add(SelectField);
                    } else
                    {
                        foreach (Field field in ListFields)
                        {
                            if (field.IDField != -1)
                            {
                                listFields.Add(field);
                            }
                        }
                    }
                    int sumCompany = 0, sumTotal = 0, sumAvailable = 0, sumExemption = 0, sumNotYet = 0, sumRelative = 0, sumGHTT = 0, sumVisa = 0, sumTTT = 0, sumOther = 0;
                    if (listFields != null && listFields.Count > 0)
                    {
                        // format export file
                        int indexCompany = 0;
                        foreach (Field field in listFields)
                        {
                            // get company from database by field id
                            ObservableCollection<Company> listCompanyByField = getCompanyByField(field.IDField);
                            foreach (Company company in listCompanyByField)
                            {
                                //get employee from company
                                ObservableCollection<Employee> listEmployeeByCompany = getEmployeeByCompany(company.IDCompany);
                                if (listEmployeeByCompany == null || listEmployeeByCompany.Count == 0)
                                {
                                    continue;
                                }
                                int order = 0;
                                colIndex = 1;
                                // set title field name
                                ws.Cells[++rowIndex, colIndex].Value = ++indexCompany + "." + company.CompanyName.ToUpper();
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
                                // set employee
                                foreach (Employee employee in listEmployeeByCompany)
                                {
                                    colIndex = 1;
                                    rowIndex++;
                                    ws.Cells[rowIndex, colIndex++].Value = ++order;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.StaffName;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.GenderString;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.Birthday;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.Nationality.NationalityName;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.Passport;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.Address;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.Career.CareerName;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.WorkPermitDisplay;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.VisaNumber;
                                    ws.Cells[rowIndex, colIndex++].Value = employee.TemporaryStay;
                                    for (int i = 1; i < colIndex; i++)
                                    {
                                        var border = ws.Cells[rowIndex, i].Style.Border;
                                        border.Bottom.Style =
                                        border.Top.Style =
                                        border.Left.Style =
                                        border.Right.Style = ExcelBorderStyle.Thin;
                                    }
                                }
                                // statistics after a company
                                ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeeByCompany);
                                sumCompany++;
                                sumTotal += listEmployeeByCompany.Count;
                                sumAvailable += listStatistics[0];
                                sumExemption += listStatistics[1];
                                sumNotYet += listStatistics[2];
                                sumRelative += listStatistics[3];
                                sumGHTT += listStatistics[4];
                                sumVisa += listStatistics[5];
                                sumTTT += listStatistics[6];
                                sumOther += listStatistics[7];
                            }
                        }
                        // show statistics
                        string[] resultsColumnHeader_1 = { "Doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miễn GPLĐ", "Chưa có GPLĐ", "Thân nhân sống cùng" };
                        string[] resultsColumnHeader_2 = { "GHTT", "Visa", "TTT", "Khác" };
                        //show company
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
                        //show employee
                        colIndex = 3;
                        rowIndex++;
                        //tạo các header từ column header đã tạo từ bên trên
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
                        ws.Cells[rowIndex, colIndex++].Value = sumGHTT;
                        ws.Cells[rowIndex, colIndex++].Value = sumVisa;
                        ws.Cells[rowIndex, colIndex++].Value = sumTTT;
                        ws.Cells[rowIndex, colIndex++].Value = sumOther;
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
        private void ExportFileByNational()
        {
            if (!string.IsNullOrWhiteSpace(DateExportFile) && !MethodHandler.checkFormatDate(DateExportFile))
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
                    ws.Name = "Báo cáo nhân viên theo NNN";

                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    // Tạo danh sách các column header
                    string[] arrColumnHeader = {"Stt", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu", "Địa chỉ tạm trú"
                            , "Nghề nghiệp (Chức danh)", "Số GPLĐ/Thời hạn", "Số visa/Thẻ tạm trú", "Thời hạn tạm trú"};

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
                    ws.Cells[1, 1].Value = "THỐNG KÊ SỐ LIỆU NGƯỜI NƯỚC NGOÀI TRÊN ĐỊA BÀN THÀNH PHỐ ĐÀ NẴNG THEO QUỐC TỊCH " + SelectNationality.NationalityName + "\nTẠI THỜI ĐIỂM " + (String.IsNullOrWhiteSpace(DateExportFile) ? today.ToString("dd-MM-yyyy") : DateExportFile);

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

                    //set list company
                    ObservableCollection<Company> listCompanies = getCompanies(); // get from database
                    if (listCompanies != null && listCompanies.Count > 0)
                    {
                        int indexCompany = 0;
                        int sumCompany = 0, sumTotal = 0, sumAvailable = 0, sumExemption = 0, sumNotYet = 0, sumRelative = 0, sumGHTT = 0, sumVisa = 0, sumTTT = 0, sumOther = 0;
                        foreach (Company company in listCompanies)
                        {

                            // get list employee by company
                            ObservableCollection<Employee> listEmployeesByCompany = getEmployeeByCompany(company);
                            if (listEmployeesByCompany == null || listEmployeesByCompany.Count == 0)
                            {
                                continue;
                            }
                            colIndex = 1;
                            // set title company name
                            ws.Cells[++rowIndex, colIndex].Value = ++indexCompany + "." + company.CompanyName.ToUpper();
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
                            // set employee
                            int order = 0;
                            foreach (Employee employee in listEmployeesByCompany)
                            {
                                colIndex = 1;
                                rowIndex++;
                                ws.Cells[rowIndex, colIndex++].Value = ++order;
                                ws.Cells[rowIndex, colIndex++].Value = employee.StaffName;
                                ws.Cells[rowIndex, colIndex++].Value = employee.GenderString;
                                ws.Cells[rowIndex, colIndex++].Value = employee.Birthday;
                                ws.Cells[rowIndex, colIndex++].Value = employee.Nationality.NationalityName;
                                ws.Cells[rowIndex, colIndex++].Value = employee.Passport;
                                ws.Cells[rowIndex, colIndex++].Value = employee.Address;
                                ws.Cells[rowIndex, colIndex++].Value = employee.Career.CareerName;
                                ws.Cells[rowIndex, colIndex++].Value = employee.WorkPermitDisplay;
                                ws.Cells[rowIndex, colIndex++].Value = employee.VisaNumber;
                                ws.Cells[rowIndex, colIndex++].Value = employee.TemporaryStay;
                                for (int i = 1; i < colIndex; i++)
                                {
                                    var border = ws.Cells[rowIndex, i].Style.Border;
                                    border.Bottom.Style =
                                    border.Top.Style =
                                    border.Left.Style =
                                    border.Right.Style = ExcelBorderStyle.Thin;
                                }
                            }
                            // statistics after a company
                            ObservableCollection<int> listStatistics = MethodHandler.getStatistics(listEmployeesByCompany);
                            sumCompany++;
                            sumTotal += listEmployeesByCompany.Count;
                            sumAvailable += listStatistics[0];
                            sumExemption += listStatistics[1];
                            sumNotYet += listStatistics[2];
                            sumRelative += listStatistics[3];
                            sumGHTT += listStatistics[4];
                            sumVisa += listStatistics[5];
                            sumTTT += listStatistics[6];
                            sumOther += listStatistics[7];

                        }
                        // show statistics
                        string[] resultsColumnHeader_1 = { "Doanh nghiệp", "Tổng số NNN", "Có GPLĐ", "Miễn GPLĐ", "Chưa có GPLĐ", "Thân nhân sống cùng" };
                        string[] resultsColumnHeader_2 = { "GHTT", "Visa", "TTT", "Khác" };
                        //show company
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
                        //show employee
                        colIndex = 3;
                        rowIndex++;
                        //tạo các header từ column header đã tạo từ bên trên
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
                        ws.Cells[rowIndex, colIndex++].Value = sumGHTT;
                        ws.Cells[rowIndex, colIndex++].Value = sumVisa;
                        ws.Cells[rowIndex, colIndex++].Value = sumTTT;
                        ws.Cells[rowIndex, colIndex++].Value = sumOther;
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
                        MessageBox.Show("Xuất excel thành công!");
                    } else
                    {
                        MessageBox.Show("Không có NNN thỏa điều kiện xuất file\nVui lòng kiểm tra lại");
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
        #endregion
        private ObservableCollection<Company>  getCompanyByField(int idField)
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
                    string sql = "select * from Companies where IDField = " + idField + " order by Companies.CompanyName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listCompanies.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Company rowCompany = new Company();
                            rowCompany.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowCompany.CompanyName = dr["CompanyName"].ToString().Trim();
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

        private ObservableCollection<Employee> getEmployeeByCompany(int idCompany)
        {
            ObservableCollection<Employee> list = new ObservableCollection<Employee>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                DateTime today = DateTime.Now;
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select IDEmployee, StaffName, Gender, CONVERT(varchar, Birthday, 105) birth, NationalityCode,"
                        + " NationalityName, Passport, Employees.Address diachi, CareerName, WorkPermit, WorkPermitNumber, VisaNumber,"
                        + " CONVERT(varchar, TemporaryStay, 105) temporary, Employees.SettlementResults settlementResults from Employees, Careers, Nationality "
                        + "where Employees.IDCareer = Careers.IDCareer and Nationality.NationalityCode like Employees.Nationality  and Employees.Hidden_flag = 0 and Employees.IDCompany = " + idCompany ;
                    // check date created employee
                    sql += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                    // check temporary stay
                    sql += " and Employees.TemporaryStay >= '" + today.ToString("yyyy-MM-dd") + "'";
                    // add condition working status
                    sql += " and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + today.ToString("yyyy-MM-dd") + "')";
                    // order by
                    sql += " order by StaffName, NationalityName";
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
                            rowEmployees.Career.CareerName = dr["CareerName"].ToString();
                            rowEmployees.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployees.WorkPermitNumber = dr["WorkPermitNumber"].ToString();
                            rowEmployees.VisaNumber = dr["VisaNumber"].ToString();
                            rowEmployees.TemporaryStay = dr["temporary"].ToString();
                            rowEmployees.SettlementResults = int.Parse(dr["settlementResults"].ToString());
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

        private ObservableCollection<Company> getCompanies()
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
                    sql = "select IDCompany, CompanyName from Companies where Delete_flag = 0 "
                        + "and IDCompany in (select distinct IDCompany from Employees where Nationality like N'%" + SelectNationality.NationalityCode + "%' "
                        + "and Hidden_flag = 0";
                    if (!string.IsNullOrWhiteSpace(TemporaryStay))
                    {
                        sql += " and TemporaryStay >= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + "'";
                    }
                    sql += ") order by Companies.IDField, Companies.CompanyName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listCompanies.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Company rowCompany = new Company();
                            rowCompany.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowCompany.CompanyName = dr["CompanyName"].ToString().Trim();
                            // can set amount when date changed
                            // get employee by company and nationality
                            DataTable dtGet = new DataTable();
                            DateTime today = DateTime.Now;
                            string sqlEmployee = "select * from Employees where Hidden_flag = 0 and Nationality like N'%" + SelectNationality.NationalityCode + "%' and IDCompany = " + rowCompany.IDCompany;
                            // check date created employee
                            if (!string.IsNullOrWhiteSpace(DateExportFile))
                            {
                                sqlEmployee += " and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                            }
                            else
                            {
                                sqlEmployee += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
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
                                    listEmployeeByNational.Add(rowEmployees);
                                }
                                listCompanies.Add(rowCompany);
                            }
                            else
                            {
                                continue;
                            }
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

        private ObservableCollection<Employee> getEmployeeByCompany(Company Company)
        {
            ObservableCollection<Employee> list = new ObservableCollection<Employee>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                DateTime today = DateTime.Now;
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select IDEmployee, StaffName, Gender, CONVERT(varchar, Birthday, 105) birth, NationalityCode,"
                        + " NationalityName, Passport, Employees.Address diachi, CareerName, WorkPermit, WorkPermitNumber, VisaNumber,"
                        + " CONVERT(varchar, TemporaryStay, 105) temporary, Employees.SettlementResults settlementResults from Employees, Careers, Nationality "
                        + "where Employees.IDCareer = Careers.IDCareer and Nationality.NationalityCode like Employees.Nationality  and Employees.Hidden_flag = 0"
                        + " and Employees.Nationality like N'%" + SelectNationality.NationalityCode + "%' and Employees.IDCompany = " + Company.IDCompany;
                    // check date created employee
                    if (!string.IsNullOrWhiteSpace(DateExportFile))
                    {
                        sql += " and Employees.DateCreated <= '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + " 23:59:59'";
                    }
                    else
                    {
                        sql += " and Employees.DateCreated <= '" + today.ToString("yyyy-MM-dd") + " 23:59:59'";
                    }
                    // check temporary stay
                    if (!string.IsNullOrWhiteSpace(TemporaryStay))
                    {
                        sql += " and Employees.TemporaryStay >= '" + MethodHandler.formatDatefromUsertoDatabase(TemporaryStay) + "'";
                    }
                    // add condition working status
                    if (!string.IsNullOrWhiteSpace(DateExportFile))
                    {
                        sql += " and (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + MethodHandler.formatDatefromUsertoDatabase(DateExportFile) + "')";
                    }
                    else
                    {
                        sql += " and (Employees.DateOfLeave is null or Employees.DateOfLeave > '" + today.ToString("yyyy-MM-dd") + "')";
                    }
                    // order by
                    sql += " order by StaffName, NationalityName";
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
                            rowEmployees.Career.CareerName = dr["CareerName"].ToString();
                            rowEmployees.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployees.WorkPermitNumber = dr["WorkPermitNumber"].ToString();
                            rowEmployees.VisaNumber = dr["VisaNumber"].ToString();
                            rowEmployees.TemporaryStay = dr["temporary"].ToString();
                            rowEmployees.SettlementResults = int.Parse(dr["settlementResults"].ToString());
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
        #region method
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
