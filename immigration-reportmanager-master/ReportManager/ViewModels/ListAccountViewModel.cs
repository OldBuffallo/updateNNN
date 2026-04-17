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
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class ListAccountViewModel : INotifyPropertyChanged
    {
        public static readonly string ListAccountDataProperty = "ListAccountData";
        public static readonly string SearchTextProperty = "SearchText";
        private ObservableCollection<Account> _listAccountData = new ObservableCollection<Account>();
        private String _searchText;
        private ICommand _btnAddAccountCommand;
        private ICommand _btnSearchAccountCommand;
        private ICommand _loadDataCommand;
        private ManagerWindow _window;

        #region set get method
        public ListAccountViewModel(ManagerWindow window)
        {
            _window = window;
            LoadDataCommand.Execute(true);
        }
        public ObservableCollection<Account> ListAccountData
        {
            get
            {
                return _listAccountData;
            }

            set
            {
                this.SetValue(ref _listAccountData, value, ListAccountDataProperty);
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

        public void NavigateToEditAccount(Account selectedAccount)
        {
            _window.ContentArea.Content = new AddAccountUC(_window, selectedAccount);
        }

        #region method ICommand
        public ICommand BtnAddAccountCommand
        {
            get
            {
                return _btnAddAccountCommand ?? (_btnAddAccountCommand = new CommandHandler(AddAccount, true));
            }
        }
        private void AddAccount()
        {
            _window.ContentArea.Content = new AddAccountUC(_window);
        }

        public ICommand BtnSearchAccountCommand
        {
            get
            {
                return _btnSearchAccountCommand ?? (_btnSearchAccountCommand = new CommandHandler(SearchAccount, true));
            }
        }
        private void SearchAccount()
        {
            if (String.IsNullOrWhiteSpace(SearchText))
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản hoặc tên cán bộ");
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
                    String sql = "select * from Accounts where Delete_flag = 0 and IDUser in (select IDUser from Accounts where Username like N'%"+ MethodHandler.convertStringOwned(SearchText) + "%' or Name like N'%"+ MethodHandler.convertStringOwned(SearchText) + "%') order by Username";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListAccountData.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Account rowAccount = new Account();
                            rowAccount.IDUser = int.Parse(dr["IDUser"].ToString());
                            rowAccount.Username = dr["Username"].ToString();
                            rowAccount.Password = dr["Password"].ToString();
                            rowAccount.Name = dr["Name"].ToString();
                            rowAccount.Permission = int.Parse(dr["Permission"].ToString());
                            ListAccountData.Add(rowAccount);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có tên tài khoản hay tên cán bộ muốn tìm");
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
                    adapter.SelectCommand = new SqlCommand("select * from Accounts where Delete_flag = 0 order by Username", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListAccountData.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Account rowAccount = new Account();
                            rowAccount.IDUser = int.Parse(dr["IDUser"].ToString());
                            rowAccount.Username = dr["Username"].ToString();
                            rowAccount.Password = dr["Password"].ToString();
                            rowAccount.Name = dr["Name"].ToString();
                            rowAccount.Permission = int.Parse(dr["Permission"].ToString());
                            ListAccountData.Add(rowAccount);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có tài khoản");
                    }
                    SearchText = "nhập tên tài khoản hoặc tên cán bộ";
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
