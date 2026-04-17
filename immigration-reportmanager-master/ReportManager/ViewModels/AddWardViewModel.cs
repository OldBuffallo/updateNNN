using ReportManager.Bases;
using ReportManager.Models;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ReportManager.ViewModels
{
    class AddWardViewModel : INotifyPropertyChanged
    {
        public static readonly string NewWardProperty = "NewWard";
        private Ward _newWard;
        private Ward oldWard;
        private ICommand _btnCreateWardCommand;
        private ICommand _btnDeleteWardCommand;
        private ICommand _btnExitCommand;
        private ManagerWindow _window;
        private bool isEdit;

        #region set get method
        public AddWardViewModel(ManagerWindow window)
        {
            _window = window;
            _newWard = new Ward();
            isEdit = false;
        }
        public AddWardViewModel(ManagerWindow window, Ward selectedWard)
        {
            _window = window;
            _newWard = new Ward(selectedWard);
            oldWard = new Ward(selectedWard);
            isEdit = true;
        }
        public Ward NewWard
        {
            get
            {
                return _newWard;
            }
            set
            {
                this.SetValue(ref _newWard, value, NewWardProperty);
            }
        }
        #endregion

        #region method Icommand
        public ICommand BtnCreateWardCommand
        {
            get
            {
                return _btnCreateWardCommand ?? (_btnCreateWardCommand = new CommandHandler(CreateWard, true));
            }
        }
        private void CreateWard()
        {
            if (string.IsNullOrWhiteSpace(NewWard.WardName))
            {
                MessageBox.Show("Không được để trống");
                return;
            }
            if (isEdit)
            {
                if (NewWard.WardName.Trim().Equals(oldWard.WardName))
                {
                    return;
                }
            }
            else
            {
                string sqlCheckExist = "select * from Wards where WardName like N'" + MethodHandler.convertStringOwned(NewWard.WardName) + "'";
                if (MethodHandler.checkExistInDatabase(sqlCheckExist))
                {
                    MessageBox.Show("Phường/xã này đã có sẵn.\nVui lòng kiểm tra lại");
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
                        string updateSQL = "update Wards set WardName = N'" + MethodHandler.convertStringOwned(NewWard.WardName)
                            + "' where IDWard = " + NewWard.IDWard;
                        comm = new SqlCommand(updateSQL, con);
                    }
                    else
                    {
                        string insertSQL = "insert into Wards values(N'" + MethodHandler.convertStringOwned(NewWard.WardName) + "', 0)";
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

        public ICommand BtnDeleteWardCommand
        {
            get
            {
                return _btnDeleteWardCommand ?? (_btnDeleteWardCommand = new CommandHandler(DeleteWard, true));
            }
        }
        private void DeleteWard()
        {
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa phường/xã này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        string deleteSQL = "update Wards set Delete_flag = 1 where IDWard = " + NewWard.IDWard;
                        SqlCommand comm = new SqlCommand(deleteSQL, con);
                        comm.ExecuteNonQuery();
                        _window.ContentArea.Content = new ListWardsUC(_window);
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
            _window.ContentArea.Content = new ListWardsUC(_window);
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
