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
    class AddCareerViewModel : INotifyPropertyChanged
    {
        public static readonly string NewCareerProperty = "NewCareer";
        public static readonly string ListCareersGroupProperty = "ListCareersGroup";
        public static readonly string SelectCareerGroupProperty = "SelectCareerGroup";
        private Career _newCareer = new Career();
        private ObservableCollection<CareerGroup> _listCareersGroup;
        private CareerGroup _selectCareerGroup = new CareerGroup();
        private Career oldCareer;
        private ICommand _btnCreateCareerCommand;
        private ICommand _btnDeleteCareerCommand;
        private ICommand _loadDataCommand;
        private ICommand _btnExitCommand;
        private ManagerWindow _window;
        private bool isEdit;

        #region set get method
        public AddCareerViewModel(ManagerWindow window)
        {
            _window = window;
            _listCareersGroup = new ObservableCollection<CareerGroup>();
            LoadDataCommand.Execute(true);
            SelectCareerGroup = ListCareersGroup[0];
            isEdit = false;
        }
        public AddCareerViewModel(ManagerWindow window, Career selectedCareer)
        {
            _window = window;
            _listCareersGroup = new ObservableCollection<CareerGroup>();
            LoadDataCommand.Execute(true);
            foreach(CareerGroup cg in ListCareersGroup)
            {
                if(cg.IDCG == selectedCareer.IDCG)
                {
                    SelectCareerGroup = cg;
                    break;
                }
            }
            NewCareer = selectedCareer;
            oldCareer = new Career(selectedCareer);
            isEdit = true;
        }
        public Career NewCareer
        {
            get
            {
                return _newCareer;
            }
            set
            {
                this.SetValue(ref _newCareer, value, NewCareerProperty);
                OnPropertyChanged("NewCareer");
            }
        }
        public ObservableCollection<CareerGroup> ListCareersGroup
        {
            get
            {
                return _listCareersGroup;
            }
            set
            {
                this.SetValue(ref _listCareersGroup, value, ListCareersGroupProperty);
                OnPropertyChanged("ListCareersGroup");
            }
        }
        public CareerGroup SelectCareerGroup
        {
            get
            {
                return _selectCareerGroup;
            }
            set
            {
                this.SetValue(ref _selectCareerGroup, value, SelectCareerGroupProperty);
                OnPropertyChanged("SelectCareerGroup");
            }
        }
        #endregion

        #region method Icommand
        public ICommand BtnCreateCareerCommand
        {
            get
            {
                return _btnCreateCareerCommand ?? (_btnCreateCareerCommand = new CommandHandler(CreateCareer, true));
            }
        }
        private void CreateCareer()
        {
            if (string.IsNullOrWhiteSpace(NewCareer.CareerName))
            {
                MessageBox.Show("Không được để trống");
                return;
            }
            if(isEdit)
            {
                if (NewCareer.CareerName.Trim().Equals(oldCareer.CareerName) && NewCareer.IDCG == oldCareer.IDCG)
                {
                    return;
                }
            }
            else
            {
                string sqlCheckExist = "select * from Careers where CareerName like N'" + MethodHandler.convertStringOwned(NewCareer.CareerName) + "' and IDCG = " + SelectCareerGroup.IDCG;
                if (MethodHandler.checkExistInDatabase(sqlCheckExist))
                {
                    MessageBox.Show("Nghề nghiệp này đã có sẵn.\nVui lòng kiểm tra lại");
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
                        string updateSQL = "Update Careers Set CareerName = N'" + MethodHandler.convertStringOwned(NewCareer.CareerName) + "' , IDCG = " + SelectCareerGroup.IDCG + " where IDCareer = " + NewCareer.IDCareer;
                        comm = new SqlCommand(updateSQL, con);
                    }
                    else
                    {
                        string insertSQL = "Insert into Careers values(N'" + MethodHandler.convertStringOwned(NewCareer.CareerName) + "'," + SelectCareerGroup.IDCG + ", 0)";
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

        public ICommand BtnDeleteCareerCommand
        {
            get
            {
                return _btnDeleteCareerCommand ?? (_btnDeleteCareerCommand = new CommandHandler(DeleteCareer, true));
            }
        }
        private void DeleteCareer()
        {
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa nghề nghiệp này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        string deleteSQL = "update Careers set Delete_flag = 1 where IDCareer = " + NewCareer.IDCareer;
                        SqlCommand comm = new SqlCommand(deleteSQL, con);
                        comm.ExecuteNonQuery();
                        _window.ContentArea.Content = new ListCareersUC(_window);
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
                    adapter.SelectCommand = new SqlCommand("select * from CareerGroups", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListCareersGroup.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            CareerGroup rowCG = new CareerGroup();
                            rowCG.IDCG = int.Parse(dr["IDCG"].ToString());
                            rowCG.CareerGroupName = dr["CareerGroupName"].ToString();
                            if(rowCG.IDCG == 1)
                            {
                                rowCG.CareerGroupName = " ----- ";
                            }
                            ListCareersGroup.Add(rowCG);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có dữ liệu");
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

        public ICommand BtnExitCommand
        {
            get
            {
                return _btnExitCommand ?? (_btnExitCommand = new CommandHandler(Exit, true));
            }
        }
        private void Exit()
        {
            _window.ContentArea.Content = new ListCareersUC(_window);
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
