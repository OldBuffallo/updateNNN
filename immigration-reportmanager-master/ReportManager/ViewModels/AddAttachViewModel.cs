using Microsoft.Win32;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using ReportManager.Bases;
using ReportManager.Models;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class AddAttachViewModel : INotifyPropertyChanged
    {
        public static readonly string AddAttachProperty = "AddAttach";
        public static readonly string LinkFileProperty = "LinkFile";
        public static readonly string FileNameProperty = "FileName";
        public static readonly string ExtensionFileProperty = "ExtensionFile";
        private Attach _addAttach;
        private Attach oldAttach;
        private int trackerID;
        private string _linkFile;
        private string _fileName;
        private string _extensionFile;
        private ICommand _btnChooseLinkCommand;
        private ICommand _btnCreateAttachCommand;
        private ICommand _btnEditAttachCommand;
        private ICommand _btnDeleteAttachCommand;
        private ICommand _btnExitCommand;
        private AttachWindow _window;
        private bool isEdit;

        #region set get method
        public AddAttachViewModel(AttachWindow window, int idCompany = 0, int trackerID = 0)
        {
            _window = window;
            _addAttach= new Attach();
            _addAttach.IDCompany = idCompany;
            this.trackerID= trackerID;
            isEdit = false;
        }

        public AddAttachViewModel(AttachWindow window, Attach attach, int idCompany = 0, int trackerID = 0)
        {
            _window = window;
            _addAttach = new Attach(attach);
            oldAttach = attach;
            _addAttach.IDCompany = idCompany;
            this.trackerID = trackerID;
            LinkFile = _addAttach.Folder +"\\"+ _addAttach.Name;
            setName(LinkFile);
            isEdit= true;
        }

        public Attach AddAttach
        {
            get
            {
                return _addAttach;
            }
            set
            {
                this.SetValue(ref _addAttach, value, AddAttachProperty);
                OnPropertyChanged("AddAttach");
            }
        }

        public string LinkFile
        {
            get
            {
                return _linkFile;
            }
            set
            {
                this.SetValue(ref _linkFile, value, LinkFileProperty);
                OnPropertyChanged("LinkFile");
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                this.SetValue(ref _fileName, value, FileNameProperty);
                OnPropertyChanged("FileName");
            }
        }

        public string ExtensionFile
        {
            get
            {
                return _extensionFile;
            }
            set
            {
                this.SetValue(ref _extensionFile, value, ExtensionFileProperty);
                OnPropertyChanged("ExtensionFile");
            }
        }
        #endregion

        #region method ICommand
        public ICommand BtnChooseLinkCommand
        {
            get
            {
                return _btnChooseLinkCommand ?? (_btnChooseLinkCommand = new CommandHandler(ChooseLink, true));
            }
        }
        private void ChooseLink()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();
            if (result == false)
            {
                return;
            }
            LinkFile = ofd.FileName;
            setName(LinkFile);
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
            _window.ContentArea.Content = new ListAttachCompanyUC(_window, AddAttach.IDCompany, trackerID);
        }

        public ICommand BtnCreateAttachCommand
        {
            get
            {
                return _btnCreateAttachCommand ?? (_btnCreateAttachCommand = new CommandHandler(CreateAttach, true));
            }
        }
        private void CreateAttach()
        {
            if ((string.IsNullOrWhiteSpace(LinkFile)) || (string.IsNullOrWhiteSpace(FileName))
                || (string.IsNullOrWhiteSpace(ExtensionFile)))
            {
                MessageBox.Show("Không được để trống");
                return;
            }
            // create data
            AddAttach.Name = FileName.Trim() + "." + ExtensionFile;
            AddAttach.Folder = Constants.PathShare + "\\" + AddAttach.IDCompany + "\\" + DateTime.Today.ToString("yyyy-MM-dd") + "\\" + AddAttach.TypeString;

            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    // copy file
                    MethodHandler.CopyFile(LinkFile, AddAttach.Folder, AddAttach.Name);
                    // insert data
                    DateTime today = DateTime.Now;
                    con.Open();
                    SqlCommand comm;
                    string insertSQL = "insert into Attach values(" + AddAttach.IDCompany + ", " + AddAttach.Type + ", N'"
                        + MethodHandler.convertStringOwned(AddAttach.Name) + "', N'"
                        + MethodHandler.convertStringOwned(AddAttach.Folder) + "', '" + today.ToString("yyyy-MM-dd HH:mm:ss") + "', 0, null, null)";
                    comm = new SqlCommand(insertSQL, con);
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

        public ICommand BtnEditAttachCommand
        {
            get
            {
                return _btnEditAttachCommand ?? (_btnEditAttachCommand = new CommandHandler(EditAttach, true));
            }
        }
        private void EditAttach()
        {
            if (isEdit && trackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Bạn chỉ có quyền xem thông tin nhân viên.\nChỉ có người theo dõi công ty của nhân viên này mới được sửa thông tin.\nVui lòng lòng đăng nhập đúng tài khoản.");
                return;
            }
            
            // update data
            AddAttach.Name = FileName.Trim() + "." + ExtensionFile;
            AddAttach.Folder = Constants.PathShare + "\\" + AddAttach.IDCompany + "\\" + MethodHandler.formatDatefromUsertoDatabase(AddAttach.DateCreated) + "\\" + AddAttach.TypeString;
            AddAttach.DateModified = DateTime.Today.ToString("yyyy-MM-dd");

            //check
            if (!checkDeff(oldAttach, AddAttach))
            {
                MessageBox.Show("Thông tin không thay đổi.\nVui lòng lòng kiểm tra lại.");
                return;
            }

            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    // copy file
                    MethodHandler.CopyFile(LinkFile, AddAttach.Folder, AddAttach.Name);
                    // update
                    DateTime today = DateTime.Now;
                    con.Open();
                    SqlCommand comm;
                    string updateSQL = "update Attach set type = " + AddAttach.Type + ", Name = N'"
                        + MethodHandler.convertStringOwned(AddAttach.Name) + "', Folder = N'"
                        + MethodHandler.convertStringOwned(AddAttach.Folder) + "', DateModified = '" + today.ToString("yyyy-MM-dd HH:mm:ss")
                        + "' where IDAttach = " + oldAttach.IDAttach;
                    comm = new SqlCommand(updateSQL, con);
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

        public ICommand BtnDeleteAttachCommand
        {
            get
            {
                return _btnDeleteAttachCommand ?? (_btnDeleteAttachCommand = new CommandHandler(DeleteAttach, true));
            }
        }
        private void DeleteAttach()
        {
            if (!isEdit)
            {
                return;
            }
            if (trackerID != int.Parse(Constants.IdUser))
            {
                MessageBox.Show("Bạn không có quyền xóa file trong công ty này.\nVui lòng kiểm tra lại hoặc đăng nhập tài khoản khác");
                return;
            }
            MessageBoxResult result = MessageBox.Show("Bạn chắc chắn muốn xóa file này?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        SqlCommand comm = new SqlCommand("update Attach set Delete_flag = 1, DateDelete = '"+ DateTime.Today.ToString("yyyy-MM-dd") + "' where IDAttach = " + AddAttach.IDAttach, con);
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
        }
        #endregion

        #region method
        private void setName(string linkFile)
        {
            int index = linkFile.LastIndexOf('\\');
            string fullPath = linkFile.Substring(index + 1);
            int idx = fullPath.LastIndexOf('.');
            FileName = fullPath.Substring(0, idx);
            ExtensionFile= fullPath.Substring(idx + 1);
        }

        private bool checkDeff(Attach oldAttach, Attach newAttach)
        {
            if (oldAttach == null || newAttach == null)
            {
                return false;
            }
            if (!oldAttach.Folder.Equals(newAttach.Folder))
            {
                return true;
            }
            if (!oldAttach.Name.Equals(newAttach.Name.Trim()))
            {
                return true;
            }
            if (oldAttach.Type != newAttach.Type)
            {
                return true;
            }
            return false;
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
