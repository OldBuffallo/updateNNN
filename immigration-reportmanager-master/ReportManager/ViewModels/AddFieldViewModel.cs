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
    class AddFieldViewModel : INotifyPropertyChanged
    {
        public static readonly string NewFieldProperty = "NewField";
        private Field _newField = new Field();
        private string oldFieldName;
        private ICommand _btnCreateFieldCommand;
        private ICommand _btnDeleteFieldCommand;
        private ICommand _btnExitCommand;
        private ManagerWindow _window;
        private bool isEdit;

        #region set get method
        public AddFieldViewModel(ManagerWindow window)
        {
            _window = window;
            isEdit = false;
        }
        public AddFieldViewModel(ManagerWindow window, Field selectedField)
        {
            _window = window;
            NewField = selectedField;
            oldFieldName = selectedField.FieldName;
            isEdit = true;
        }
        public Field NewField
        {
            get
            {
                return _newField;
            }
            set
            {
                this.SetValue(ref _newField, value, NewFieldProperty);
                OnPropertyChanged("NewField");
            }
        }
        #endregion

        #region method Icommand
        public ICommand BtnCreateFieldCommand
        {
            get
            {
                return _btnCreateFieldCommand ?? (_btnCreateFieldCommand = new CommandHandler(CreateField, true));
            }
        }
        private void CreateField()
        {
            if (string.IsNullOrWhiteSpace(NewField.FieldName))
            {
                MessageBox.Show("Không được để trống");
                return;
            }
            if (string.IsNullOrWhiteSpace(NewField.Description))
            {
                NewField.Description = " ";
            }
            if(isEdit && NewField.FieldName.Trim().Equals(oldFieldName))
            {
                // no check exist
            }
            else
            {
                string sqlCheckExist = "select * from Fields where FieldName like N'" + MethodHandler.convertStringOwned(NewField.FieldName) + "'";
                if (MethodHandler.checkExistInDatabase(sqlCheckExist))
                {
                    MessageBox.Show("Lĩnh vực này đã có sẵn. \nVui lòng kiểm tra lại.");
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
                        string updateSQL = "Update Fields Set FieldName = N'" + MethodHandler.convertStringOwned(NewField.FieldName) + "' , Description = N'" + MethodHandler.convertStringOwned(NewField.Description) + "' where IDField = " + NewField.IDField;
                        comm = new SqlCommand(updateSQL, con);
                    }
                    else
                    {
                        string insertSQL = "Insert into Fields values(N'" + MethodHandler.convertStringOwned(NewField.FieldName) + "', N'" + MethodHandler.convertStringOwned(NewField.Description) + "', 0)";
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

        public ICommand BtnDeleteFieldCommand
        {
            get
            {
                return _btnDeleteFieldCommand ?? (_btnDeleteFieldCommand = new CommandHandler(DeleteField, true));
            }
        }
        private void DeleteField()
        {
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa lĩnh vực này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        string deleteSQL = "update Fields set Delete_flag = 1 where IDField = " + NewField.IDField;
                        SqlCommand comm = new SqlCommand(deleteSQL, con);
                        comm.ExecuteNonQuery();
                        _window.ContentArea.Content = new ListFieldsUC(_window);
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
            _window.ContentArea.Content = new ListFieldsUC(_window);
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
