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
using System.Windows;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class AddNationalityViewModel : INotifyPropertyChanged
    {
        public static readonly string NewNationalityProperty = "NewNationality";
        private Nationality _newNationality;
        private Nationality oldNationality;
        private ICommand _btnCreateNationalityCommand;
        private ICommand _btnDeleteNationalityCommand;
        private ICommand _btnExitCommand;
        private ManagerWindow _window;
        private bool isEdit;

        #region set get method
        public AddNationalityViewModel(ManagerWindow window)
        {
            _window = window;
            _newNationality = new Nationality();
            isEdit = false;
        }
        public AddNationalityViewModel(ManagerWindow window, Nationality selectedNationality)
        {
            _window = window;
            _newNationality = new Nationality();
            NewNationality = selectedNationality;
            oldNationality = new Nationality(selectedNationality);
            isEdit = true;
        }
        public Nationality NewNationality
        {
            get
            {
                return _newNationality;
            }
            set
            {
                this.SetValue(ref _newNationality, value, NewNationalityProperty);
            }
        }
        #endregion

        #region method Icommand
        public ICommand BtnCreateNationalityCommand
        {
            get
            {
                return _btnCreateNationalityCommand ?? (_btnCreateNationalityCommand = new CommandHandler(CreateNationality, true));
            }
        }
        private void CreateNationality()
        {
            if (string.IsNullOrWhiteSpace(NewNationality.NationalityCode) || string.IsNullOrWhiteSpace(NewNationality.NationalityName))
            {
                MessageBox.Show("Không được để trống");
                return;
            }
            if (isEdit)
            {
                if(NewNationality.NationalityCode.Trim().Equals(oldNationality.NationalityCode) && NewNationality.NationalityName.Trim().Equals(oldNationality.NationalityName))
                {
                    return;
                }
            }
            string sqlCheckExist = "select * from Nationality where NationalityCode like N'" + MethodHandler.convertStringOwned(NewNationality.NationalityCode) + "'";
            if (MethodHandler.checkExistInDatabase(sqlCheckExist))
            {
                MessageBox.Show("Quốc tịch này đã có sẵn.\nVui lòng kiểm tra lại");
                return;
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
                        string updateSQL = "update Nationality set NationalityName = N'" + MethodHandler.convertStringOwned(NewNationality.NationalityName) + "' where IDNationality = " + NewNationality.IDNationality;
                        comm = new SqlCommand(updateSQL, con);
                    }
                    else
                    {
                        string insertSQL = "Insert into Nationality values(N'" + MethodHandler.convertStringOwned(NewNationality.NationalityCode) + "',N'" + MethodHandler.convertStringOwned(NewNationality.NationalityName) + "', 0)";
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

        public ICommand BtnDeleteNationalityCommand
        {
            get
            {
                return _btnDeleteNationalityCommand ?? (_btnDeleteNationalityCommand = new CommandHandler(DeleteNationality, true));
            }
        }
        private void DeleteNationality()
        {
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa quốc tịch này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        string deleteSQL = "update Nationality set Delete_flag = 1 where IDNationality = " + NewNationality.IDNationality;
                        SqlCommand comm = new SqlCommand(deleteSQL, con);
                        comm.ExecuteNonQuery();
                        _window.ContentArea.Content = new ListNationalityUC(_window);
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
            _window.ContentArea.Content = new ListNationalityUC(_window);
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
