using ReportManager.Bases;
using ReportManager.Models;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ReportManager.ViewModels
{
    class ListWardsViewModel : INotifyPropertyChanged
    {
        public static readonly string ListWardsDataProperty = "ListWardsData";
        public static readonly string SelectWardProperty = "SelectWard";
        public static readonly string SearchTextProperty = "SearchText";
        private ObservableCollection<Ward> _listWardsData;
        private Ward _selectWard;
        private String _searchText;
        private ICommand _btnAddWardCommand;
        private ICommand _btnEditWardCommand;
        private ICommand _btnSearchWardCommand;
        private ICommand _loadDataCommand;
        private ManagerWindow _window;

        #region set get method
        public ListWardsViewModel(ManagerWindow window)
        {
            _window = window;
            _listWardsData = new ObservableCollection<Ward>();
            LoadDataCommand.Execute(true);
        }
        public ObservableCollection<Ward> ListWardsData
        {
            get
            {
                return _listWardsData;
            }

            set
            {
                this.SetValue(ref _listWardsData, value, ListWardsDataProperty);
            }
        }
        public Ward SelectWard
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
                    adapter.SelectCommand = new SqlCommand("select IDWard, WardName from Wards where Wards.Delete_flag = 0 order by WardName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListWardsData.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Ward rowWard = new Ward();
                            rowWard.IDWard = int.Parse(dr["IDWard"].ToString());
                            rowWard.WardName = dr["WardName"].ToString();
                            rowWard.LineNumber = ++lineNumber;
                            ListWardsData.Add(rowWard);
                        }
                    }
                    SearchText = "nhập tên phường/xã";
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

        public ICommand BtnSearchWardCommand
        {
            get
            {
                return _btnSearchWardCommand ?? (_btnSearchWardCommand = new CommandHandler(SearchWard, true));
            }
        }
        private void SearchWard()
        {
            if (String.IsNullOrWhiteSpace(SearchText))
            {
                MessageBox.Show("Vui lòng nhập tên phường/ xã");
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
                    String sql = "select IDWard, WardName"
                        + " from Wards where Delete_flag = 0"
                        + " and WardName like N'%" + MethodHandler.convertStringOwned(SearchText) + "%' order by WardName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListWardsData.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Ward rowWard = new Ward();
                            rowWard.IDWard = int.Parse(dr["IDWard"].ToString());
                            rowWard.WardName = dr["WardName"].ToString();
                            rowWard.LineNumber = ++lineNumber;
                            ListWardsData.Add(rowWard);
                        }
                    }
                    else
                    {
                        MessageBox.Show("không có tên phường/ xã muốn tìm");
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

        public ICommand BtnAddWardCommand
        {
            get
            {
                return _btnAddWardCommand ?? (_btnAddWardCommand = new CommandHandler(AddWard, true));
            }
        }
        private void AddWard()
        {
            _window.ContentArea.Content = new AddWardUC(_window);
        }

        public ICommand BtnEditWardCommand
        {
            get
            {
                return _btnEditWardCommand ?? (_btnEditWardCommand = new CommandHandler(EditWard, true));
            }
        }
        private void EditWard()
        {
            _window.ContentArea.Content = new AddWardUC(_window, SelectWard);
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
