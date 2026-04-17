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
    class ListDistrictsViewModel : INotifyPropertyChanged
    {
        public static readonly string ListDistrictsDataProperty = "ListDistrictsData";
        public static readonly string SelectDistrictProperty = "SelectDistrict";
        public static readonly string SearchTextProperty = "SearchText";
        private ObservableCollection<District> _listDistrictsData;
        private District _selectDistrict;
        private String _searchText;
        private ICommand _btnAddDistrictCommand;
        private ICommand _btnEditDistrictCommand;
        private ICommand _btnSearchDistrictCommand;
        private ICommand _loadDataCommand;
        private ManagerWindow _window;

        #region set get method
        public ListDistrictsViewModel(ManagerWindow window)
        {
            _window = window;
            _listDistrictsData = new ObservableCollection<District>();
            LoadDataCommand.Execute(true);
        }
        public ObservableCollection<District> ListDistrictsData
        {
            get
            {
                return _listDistrictsData;
            }

            set
            {
                this.SetValue(ref _listDistrictsData, value, ListDistrictsDataProperty);
            }
        }
        public District SelectDistrict
        {
            get
            {
                return _selectDistrict;
            }

            set
            {
                this.SetValue(ref _selectDistrict, value, SelectDistrictProperty);
            }
        }
        public String SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                this.SetValue(ref _searchText, value, SearchTextProperty);
            }
        }
        #endregion

        #region method ICommand
        public ICommand LoadDataCommand
        {
            get
            {
                return _loadDataCommand ?? (_loadDataCommand = new CommandHandler(LoadData, true));
            }
        }
        private void LoadData()
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select IDDistrict, DisTrictName from Districts where Districts.Delete_flag = 0 order by DisTrictName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListDistrictsData.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            District rowDistrict = new District();
                            rowDistrict.IDDistrict = int.Parse(dr["IDDistrict"].ToString());
                            rowDistrict.DisTrictName = dr["DisTrictName"].ToString();
                            rowDistrict.LineNumber = ++lineNumber;
                            ListDistrictsData.Add(rowDistrict);
                        }
                    }
                    SearchText = "nhập tên quận/huyện";
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

        public ICommand BtnSearchDistrictCommand
        {
            get
            {
                return _btnSearchDistrictCommand ?? (_btnSearchDistrictCommand = new CommandHandler(SearchDistrict, true));
            }
        }
        private void SearchDistrict()
        {
            if (String.IsNullOrWhiteSpace(SearchText))
            {
                MessageBox.Show("Vui lòng nhập tên quận/ huyện");
                return;
            }
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    String sql = "select IDDistrict, DisTrictName"
                        + " from Districts where Delete_flag = 0"
                        +" and DisTrictName like N'%" + MethodHandler.convertStringOwned(SearchText) + "%' order by DisTrictName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListDistrictsData.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            District rowDistrict = new District();
                            rowDistrict.IDDistrict = int.Parse(dr["IDDistrict"].ToString());
                            rowDistrict.DisTrictName = dr["DisTrictName"].ToString();
                            rowDistrict.LineNumber = ++lineNumber;
                            ListDistrictsData.Add(rowDistrict);
                        }
                    }
                    else
                    {
                        MessageBox.Show("không có tên quận/ huyện muốn tìm");
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

        public ICommand BtnAddDistrictCommand
        {
            get
            {
                return _btnAddDistrictCommand ?? (_btnAddDistrictCommand = new CommandHandler(AddDistrict, true));
            }
        }
        private void AddDistrict()
        {
            _window.ContentArea.Content = new AddDistrictUC(_window);
        }

        public ICommand BtnEditDistrictCommand
        {
            get
            {
                return _btnEditDistrictCommand ?? (_btnEditDistrictCommand = new CommandHandler(EditDistrict, true));
            }
        }
        private void EditDistrict()
        {
            _window.ContentArea.Content = new AddDistrictUC(_window, SelectDistrict);
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
