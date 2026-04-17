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
    class ListCareersViewModel : INotifyPropertyChanged
    {
        public static readonly string ListCareersDataProperty = "ListCareersData";
        public static readonly string SelectCareerProperty = "SelectCareer";
        public static readonly string SearchTextProperty = "SearchText";
        private ObservableCollection<Career> _listCareersData;
        private Career _selectCareer;
        private String _searchText;
        private ICommand _btnAddCareerCommand;
        private ICommand _btnEditCareerCommand;
        private ICommand _btnSearchCareerCommand;
        private ICommand _loadDataCommand;
        private ManagerWindow _window;

        #region set get method
        public ListCareersViewModel(ManagerWindow window)
        {
            _window = window;
            _listCareersData = new ObservableCollection<Career>();
            LoadDataCommand.Execute(true);
        }
        public ObservableCollection<Career> ListCareersData
        {
            get
            {
                return _listCareersData;
            }

            set
            {
                this.SetValue(ref _listCareersData, value, ListCareersDataProperty);
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
        public ICommand BtnAddCareerCommand
        {
            get
            {
                return _btnAddCareerCommand ?? (_btnAddCareerCommand = new CommandHandler(AddCareer, true));
            }
        }
        private void AddCareer()
        {
            _window.ContentArea.Content = new AddCareerUC(_window);
        }

        public ICommand BtnEditCareerCommand
        {
            get
            {
                return _btnEditCareerCommand ?? (_btnEditCareerCommand = new CommandHandler(EditCareer, true));
            }
        }
        private void EditCareer()
        {
            _window.ContentArea.Content = new AddCareerUC(_window, SelectCareer);
        }

        public ICommand BtnSearchCareerCommand
        {
            get
            {
                return _btnSearchCareerCommand ?? (_btnSearchCareerCommand = new CommandHandler(SearchCareer, true));
            }
        }
        private void SearchCareer()
        {
            if (String.IsNullOrWhiteSpace(SearchText))
            {
                MessageBox.Show("Vui lòng nhập tên nghề hoặc nhóm nghề");
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
                    string sql = "select * from Careers, CareerGroups where Careers.Delete_flag = 0 and Careers.IDCG= CareerGroups.IDCG"
                        +" and IDCareer in ((select IDCareer from Careers where CareerName like N'%"+ MethodHandler.convertStringOwned(SearchText) + "%')"
                        +" union (select IDCareer from Careers, CareerGroups where Careers.IDCG= CareerGroups.IDCG and CareerGroupName like N'%"+ MethodHandler.convertStringOwned(SearchText) + "%'))"
                        +" order by Careers.IDCG, CareerGroups.CareerGroupName, CareerName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListCareersData.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Career rowAccount = new Career();
                            rowAccount.IDCareer = int.Parse(dr["IDCareer"].ToString());
                            rowAccount.CareerName = dr["CareerName"].ToString();
                            rowAccount.IDCG = int.Parse(dr["IDCG"].ToString());
                            rowAccount.CareerGroupName = dr["CareerGroupName"].ToString();
                            ListCareersData.Add(rowAccount);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có ngành nghề hay nhóm nghề muốn tìm");
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
                    adapter.SelectCommand = new SqlCommand("select * from Careers, CareerGroups where Careers.Delete_flag = 0 and Careers.IDCG= CareerGroups.IDCG order by Careers.IDCG, CareerGroups.CareerGroupName, CareerName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListCareersData.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Career rowAccount = new Career();
                            rowAccount.IDCareer = int.Parse(dr["IDCareer"].ToString());
                            rowAccount.CareerName = dr["CareerName"].ToString();
                            rowAccount.IDCG = int.Parse(dr["IDCG"].ToString());
                            rowAccount.CareerGroupName = dr["CareerGroupName"].ToString();
                            ListCareersData.Add(rowAccount);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có ngành nghề");
                    }
                    SearchText = "nhập tên nghề hoặc tên nhóm nghề";
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

        public void NavigateToEditCareer(Career selectCareer)
        {
            //_window.ContentArea.Content = new AddAccountUC(_window, selectedAccount);
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
