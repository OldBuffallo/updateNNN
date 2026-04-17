using ReportManager.Bases;
using ReportManager.Models;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
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
    class AddAccountViewModel : INotifyPropertyChanged
    {
        public static readonly string AccountLoginProperty = "AccountLogin";
        private Account _accountLogin = new Account();
        private ICommand _btnCreateAccountCommand;
        private ICommand _btnDeleteAccountCommand;
        private ICommand _btnExitCommand;
        private ManagerWindow _window;
        private bool isEdit;
        private string oldUsername; // for function edit
        #region set get method
        public AddAccountViewModel(ManagerWindow window)
        {
            _window = window;
            isEdit = false;
            AccountLogin.PermissionIndex = 0;
        }
        public AddAccountViewModel(ManagerWindow window, Account selectedAccount)
        {
            _window = window;
            AccountLogin = selectedAccount;
            oldUsername = selectedAccount.Username;
            AccountLogin.PermissionIndex = AccountLogin.Permission - 1;
            isEdit = true;
        }
        public Account AccountLogin
        {
            get
            {
                return _accountLogin;
            }
            set
            {
                this.SetValue(ref _accountLogin, value, AccountLoginProperty);
                OnPropertyChanged("AccountLogin");
            }
        }
        #endregion

        #region method Icommand
        public ICommand BtnCreateAccountCommand
        {
            get
            {
                return _btnCreateAccountCommand ?? (_btnCreateAccountCommand = new CommandHandler(CreateAccount, true));
            }
        }
        private void CreateAccount()
        {
            if ((string.IsNullOrWhiteSpace(AccountLogin.Username)) || (string.IsNullOrWhiteSpace(AccountLogin.Password)) || (string.IsNullOrWhiteSpace(AccountLogin.Name)))
            {
                MessageBox.Show("Không được để trống");
                return;
            }
            if(isEdit && AccountLogin.Username.Trim().Equals(oldUsername))
            {
                // no checks exist
            }
            else
            {
                string sqlCheckExist = "select * from Accounts where Username like '" + MethodHandler.convertStringOwned(AccountLogin.Username) + "'";
                if (MethodHandler.checkExistInDatabase(sqlCheckExist))
                {
                    MessageBox.Show("Tên tài khoản đã tồn tại");
                    return;
                }
            }
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    con.Open();
                    SqlCommand comm;
                    if (isEdit)
                    {
                        string updateSQL = "Update Accounts Set Username = '" + MethodHandler.convertStringOwned(AccountLogin.Username) + "', Password = '"
                            + AccountLogin.Password.Trim() + "' , Permission = " + AccountLogin.Permission + ", Name = N'" + MethodHandler.convertStringOwned(AccountLogin.Name) + "' where IDUser = " + AccountLogin.IDUser;
                        comm = new SqlCommand(updateSQL, con);
                    }
                    else
                    {
                        string insertSQL = "Insert into Accounts values('" + MethodHandler.convertStringOwned(AccountLogin.Username) + "','" + AccountLogin.Password.Trim() + "'," + AccountLogin.Permission + ", N'" + MethodHandler.convertStringOwned(AccountLogin.Name) + "', 0)";
                        comm = new SqlCommand(insertSQL, con);
                    }
                    comm.ExecuteNonQuery();
                    BtnExitCommand.Execute(true);
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

        public ICommand BtnDeleteAccountCommand
        {
            get
            {
                return _btnDeleteAccountCommand ?? (_btnDeleteAccountCommand = new CommandHandler(DeleteAccount, true));
            }
        }
        private void DeleteAccount()
        {
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa tài khoản này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (AccountLogin.IDUser == int.Parse(Constants.IdUser))
                {
                    MessageBox.Show("Không cho phép xóa tài khoản này.");
                    return;
                }
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        string deleteSQL = "update Accounts set Delete_flag = 1 where IDUser = " + AccountLogin.IDUser;
                        SqlCommand comm = new SqlCommand(deleteSQL, con);
                        comm.ExecuteNonQuery();
                        _window.ContentArea.Content = new ListAccountsUC(_window);
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
        }

        public ICommand BtnExitCommand
        {
            get
            {
                return _btnExitCommand ?? (_btnExitCommand = new CommandHandler(Exit, true));
            }
        }
        private void Exit()
        {
            _window.ContentArea.Content = new ListAccountsUC(_window);
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
