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
    class AddDistrictViewModel : INotifyPropertyChanged
    {
        public static readonly string NewDistrictProperty = "NewDistrict";
        private District _newDistrict;
        private District oldDistrict;
        private ICommand _btnCreateDistrictCommand;
        private ICommand _btnDeleteDistrictCommand;
        private ICommand _btnExitCommand;
        private ManagerWindow _window;
        private bool isEdit;

        #region set get method
        public AddDistrictViewModel(ManagerWindow window)
        {
            _window = window;
            _newDistrict = new District();
            isEdit = false;
        }
        public AddDistrictViewModel(ManagerWindow window, District selectedDistrict)
        {
            _window = window;
            _newDistrict = new District(selectedDistrict);
            oldDistrict = new District(selectedDistrict);
            isEdit = true;
        }
        public District NewDistrict
        {
            get
            {
                return _newDistrict;
            }
            set
            {
                this.SetValue(ref _newDistrict, value, NewDistrictProperty);
            }
        }
        #endregion

        #region method Icommand
        public ICommand BtnCreateDistrictCommand
        {
            get
            {
                return _btnCreateDistrictCommand ?? (_btnCreateDistrictCommand = new CommandHandler(CreateDistrict, true));
            }
        }
        private void CreateDistrict()
        {
            if (string.IsNullOrWhiteSpace(NewDistrict.DisTrictName))
            {
                MessageBox.Show("Không được để trống");
                return;
            }
            if (isEdit)
            {
                if (NewDistrict.DisTrictName.Trim().Equals(oldDistrict.DisTrictName))
                {
                    return;
                }
            }
            else
            {
                string sqlCheckExist = "select * from Districts where DisTrictName like N'" + MethodHandler.convertStringOwned(NewDistrict.DisTrictName) + "'";
                if (MethodHandler.checkExistInDatabase(sqlCheckExist))
                {
                    MessageBox.Show("Quận/huyện này đã có sẵn.\nVui lòng kiểm tra lại");
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
                        string updateSQL = "update Districts set DisTrictName = N'" + MethodHandler.convertStringOwned(NewDistrict.DisTrictName)
                            + "' where IDDistrict = " + NewDistrict.IDDistrict;
                        comm = new SqlCommand(updateSQL, con);
                    }
                    else
                    {
                        string insertSQL = "insert into Districts values(N'" + MethodHandler.convertStringOwned(NewDistrict.DisTrictName) + "', 0)";
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

        public ICommand BtnDeleteDistrictCommand
        {
            get
            {
                return _btnDeleteDistrictCommand ?? (_btnDeleteDistrictCommand = new CommandHandler(DeleteDistrict, true));
            }
        }
        private void DeleteDistrict()
        {
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa quận/huyện này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        string deleteSQL = "update Districts set Delete_flag = 1 where IDDistrict = " + NewDistrict.IDDistrict;
                        SqlCommand comm = new SqlCommand(deleteSQL, con);
                        comm.ExecuteNonQuery();
                        _window.ContentArea.Content = new ListDistrictsUC(_window);
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
            _window.ContentArea.Content = new ListDistrictsUC(_window);
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
