using ReportManager.Bases;
using ReportManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ReportManager.UserControls;
using Microsoft.Win32;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.IO;

namespace ReportManager.ViewModels
{
    public class ListAttachCompanyViewModel : INotifyPropertyChanged
    {
        public static readonly string ListAttachDataProperty = "ListAttachData";
        public static readonly string SelectAttachProperty = "SelectAttach";
        public static readonly string IDCompanyProperty = "IDCompany";
        private ObservableCollection<Attach> _listAttachData = new ObservableCollection<Attach>();
        private Attach _selectAttach;
        private int _idCompany;
        private int trackerID;
        private ICommand _btnAddAttachCommand;
        private ICommand _btnEditAttachCommand;
        private ICommand _loadDataCommand;
        private ICommand _btnDownloadAttachCommand;
        private AttachWindow _window;

        #region set get method
        public ListAttachCompanyViewModel(AttachWindow window, int idCompany, int trackerID)
        {
            _idCompany= idCompany;
            _window = window;
            this.trackerID = trackerID;
            _listAttachData = new ObservableCollection<Attach>();
            LoadDataCommand.Execute(true);
        }
        public ObservableCollection<Attach> ListAttachData
        {
            get
            {
                return _listAttachData;
            }

            set
            {
                this.SetValue(ref _listAttachData, value, ListAttachDataProperty);
            }
        }
        public Attach SelectAttach
        {
            get
            {
                return _selectAttach;
            }

            set
            {
                this.SetValue(ref _selectAttach, value, SelectAttachProperty);
            }
        }
        public int IDCompany
        {
            get
            {
                return _idCompany;
            }

            set
            {
                this.SetValue(ref _idCompany, value, IDCompanyProperty);
            }
        }
        #endregion

        #region method ICommand
        public ICommand LoadDataCommand
        {
            get
            {
                return _loadDataCommand ?? (_loadDataCommand = new CommandHandler(LoadData, true));
            }
        }

        private void LoadData()
        {
            if (IDCompany== 0)
            {
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
                    string sql = "select IDAttach, IDCompany, Type, Name, Folder, CONVERT(varchar, DateCreated, 105) date_created, CONVERT(varchar, DateModified, 105) date_modified from Attach where Delete_flag = 0 and IDCompany = " + IDCompany + " order by date_created DESC, Type ASC, Name ASC";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ListAttachData.Clear();
                        int lineNumber = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            Attach rowAttach = new Attach();
                            rowAttach.LineNumber = ++lineNumber;
                            rowAttach.IDAttach = int.Parse(dr["IDAttach"].ToString());
                            rowAttach.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowAttach.Type = int.Parse(dr["Type"].ToString());
                            rowAttach.Name = dr["Name"].ToString();
                            rowAttach.Folder = dr["Folder"].ToString();
                            rowAttach.DateCreated = dr["date_created"].ToString();
                            rowAttach.DateModified = dr["date_modified"].ToString();
                            ListAttachData.Add(rowAttach);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có file đính kèm");
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


        public ICommand BtnAddAttachCommand
        {
            get
            {
                return _btnAddAttachCommand ?? (_btnAddAttachCommand = new CommandHandler(AddAttach, true));
            }
        }

        private void AddAttach()
        {
            _window.ContentArea.Content = new AddAttachUC(_window, IDCompany, trackerID);
        }

        public ICommand BtnEditAttachCommand
        {
            get
            {
                return _btnEditAttachCommand ?? (_btnEditAttachCommand = new CommandHandler(EditAttach, true));
            }
        }
        private void EditAttach()
        {
            if (SelectAttach != null)
            {
                _window.ContentArea.Content = new AddAttachUC(_window, SelectAttach, IDCompany, trackerID);
            }
        }

        public ICommand BtnDownloadAttachCommand
        {
            get
            {
                return _btnDownloadAttachCommand ?? (_btnDownloadAttachCommand = new CommandHandler(DownloadAttach, true));
            }
        }
        private void DownloadAttach()
        {
            if (SelectAttach == null)
            {
                return;
            }
            // save path
            //var dialog = new System.Windows.Forms.FolderBrowserDialog();

            //if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    string fullPath = SelectAttach.Folder + "\\" + SelectAttach.Name;
            //    string folderName = dialog.SelectedPath;
            //    MethodHandler.CopyFile(fullPath, folderName, SelectAttach.Name);
            //    MessageBox.Show("Đã tải thành công");
            //}

            SaveFileDialog sf = new SaveFileDialog();
            sf.FileName = SelectAttach.Name;
            var result = sf.ShowDialog();
            if (result == true)
            {
                // Now here's our save folder
                string savePath = Path.GetDirectoryName(sf.FileName);
                string newName = Path.GetFileName(sf.FileName);
                string fullPath = SelectAttach.Folder + "\\" + SelectAttach.Name;
                MethodHandler.CopyFile(fullPath, savePath, newName);
                MessageBox.Show("Đã tải thành công");
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
