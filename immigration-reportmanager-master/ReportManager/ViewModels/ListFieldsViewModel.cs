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
    class ListFieldsViewModel : INotifyPropertyChanged
    {
        public static readonly string ListFieldsDataProperty = "ListFieldsData";
        public static readonly string SelectFieldProperty = "SelectField";
        public static readonly string SearchTextProperty = "SearchText";
        private ObservableCollection<Field> _listFieldsData;
        private Field _selectField;
        private String _searchText;
        private ICommand _btnAddFieldCommand;
        private ICommand _btnEditFieldCommand;
        private ICommand _btnSearchFieldCommand;
        private ICommand _loadDataCommand;
        private ManagerWindow _window;

        #region set get method
        public ListFieldsViewModel(ManagerWindow window)
        {
            _window = window;
            _listFieldsData = new ObservableCollection<Field>();
            LoadDataCommand.Execute(true);
        }
        public ObservableCollection<Field> ListFieldsData
        {
            get
            {
                return _listFieldsData;
            }

            set
            {
                this.SetValue(ref _listFieldsData, value, ListFieldsDataProperty);
            }
        }
        public Field SelectField
        {
            get
            {
                return _selectField;
            }

            set
            {
                this.SetValue(ref _selectField, value, SelectFieldProperty);
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
        public ICommand BtnAddFieldCommand
        {
            get
            {
                return _btnAddFieldCommand ?? (_btnAddFieldCommand = new CommandHandler(AddField, true));
            }
        }
        private void AddField()
        {
            _window.ContentArea.Content = new AddFieldUC(_window);
        }

        public ICommand BtnEditFieldCommand
        {
            get
            {
                return _btnEditFieldCommand ?? (_btnEditFieldCommand = new CommandHandler(EditField, true));
            }
        }
        private void EditField()
        {
            _window.ContentArea.Content = new AddFieldUC(_window, SelectField);
        }

        public ICommand BtnSearchFieldCommand
        {
            get
            {
                return _btnSearchFieldCommand ?? (_btnSearchFieldCommand = new CommandHandler(SearchField, true));
            }
        }
        private void SearchField()
        {
            if (String.IsNullOrWhiteSpace(SearchText))
            {
                MessageBox.Show("Vui lòng nhập tên lĩnh vực");
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
                    String sql = "select * from Fields where Fields.Delete_flag = 0 and FieldName like N'%" + MethodHandler.convertStringOwned(SearchText) + "%' order by FieldName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListFieldsData.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Field rowField = new Field();
                            rowField.IDField = int.Parse(dr["IDField"].ToString());
                            rowField.FieldName = dr["FieldName"].ToString();
                            rowField.Description = dr["Description"].ToString();
                            ListFieldsData.Add(rowField);
                        }
                    }
                    else
                    {
                        MessageBox.Show("không có lĩnh vực muốn tìm");
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
                    adapter.SelectCommand = new SqlCommand("select * from Fields where Fields.Delete_flag = 0 order by FieldName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListFieldsData.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Field rowField = new Field();
                            rowField.IDField = int.Parse(dr["IDField"].ToString());
                            rowField.FieldName = dr["FieldName"].ToString();
                            rowField.Description = dr["Description"].ToString();
                            ListFieldsData.Add(rowField);
                        }
                    }
                    else
                    {
                        MessageBox.Show("không có lĩnh vực");
                    }
                    SearchText = "nhập tên lĩnh vực";
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

        public void NavigateToEditField(Field selectedField)
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
