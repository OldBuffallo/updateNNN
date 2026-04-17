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
    class ListNationalityViewModel : INotifyPropertyChanged
    {
        public static readonly string ListNationalitiesDataProperty = "ListNationalitiesData";
        public static readonly string SelectNationalityProperty = "SelectNationality";
        public static readonly string SearchTextProperty = "SearchText";
        private ObservableCollection<Nationality> _listNationalities;
        private Nationality _selectNationality;
        private String _searchText;
        private ICommand _btnAddNationalityCommand;
        private ICommand _btnEditNationalityCommand;
        private ICommand _btnSearchNationalityCommand;
        private ICommand _loadDataCommand;
        private ManagerWindow _window;

        #region set get method
        public ListNationalityViewModel(ManagerWindow window)
        {
            _window = window;
            _listNationalities = new ObservableCollection<Nationality>();
            LoadDataCommand.Execute(true);
        }
        public ObservableCollection<Nationality> ListNationalitiesData
        {
            get
            {
                return _listNationalities;
            }

            set
            {
                this.SetValue(ref _listNationalities, value, ListNationalitiesDataProperty);
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
        public ICommand BtnAddNationalityCommand
        {
            get
            {
                return _btnAddNationalityCommand ?? (_btnAddNationalityCommand = new CommandHandler(AddNationality, true));
            }
        }
        private void AddNationality()
        {
            _window.ContentArea.Content = new AddNationalityUC(_window);
        }

        public ICommand BtnEditNationalityCommand
        {
            get
            {
                return _btnEditNationalityCommand ?? (_btnEditNationalityCommand = new CommandHandler(EditNationality, true));
            }
        }
        private void EditNationality()
        {
            _window.ContentArea.Content = new AddNationalityUC(_window, SelectNationality);
        }

        public ICommand BtnSearchNationalityCommand
        {
            get
            {
                return _btnSearchNationalityCommand ?? (_btnSearchNationalityCommand = new CommandHandler(SearchNationality, true));
            }
        }
        private void SearchNationality()
        {
            if (String.IsNullOrWhiteSpace(SearchText))
            {
                MessageBox.Show("Vui lòng nhập mã quốc tịch hoặc tên quốc tịch");
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
                    String sql = "select * from Nationality where Delete_flag = 0"
                        + " and IDNationality in (select IDNationality from Nationality where NationalityCode like N'%" + MethodHandler.convertStringOwned(SearchText) + "%' or NationalityName like N'%" + MethodHandler.convertStringOwned(SearchText) + "%')"
                        + " order by NationalityCode, NationalityName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListNationalitiesData.Clear();
                        int _line = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Nationality rowNationality = new Nationality();
                            rowNationality.IDNationality = int.Parse(dr["IDNationality"].ToString());
                            rowNationality.NationalityCode = dr["NationalityCode"].ToString();
                            rowNationality.NationalityName = dr["NationalityName"].ToString();
                            rowNationality.LineNumber = ++_line;
                            ListNationalitiesData.Add(rowNationality);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có mã quốc tịch hoặc tên quốc tịch muốn tìm");
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
                    adapter.SelectCommand = new SqlCommand("select * from Nationality where Delete_flag = 0 order by NationalityCode, NationalityName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListNationalitiesData.Clear();
                        int _line = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Nationality rowNationality = new Nationality();
                            rowNationality.IDNationality = int.Parse(dr["IDNationality"].ToString());
                            rowNationality.NationalityCode = dr["NationalityCode"].ToString();
                            rowNationality.NationalityName = dr["NationalityName"].ToString();
                            rowNationality.LineNumber = ++_line;
                            ListNationalitiesData.Add(rowNationality);
                        }
                    }
                    SearchText = "nhập mã/tên quốc tịch";
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
