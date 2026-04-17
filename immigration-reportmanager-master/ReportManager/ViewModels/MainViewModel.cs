using ReportManager.Bases;
using ReportManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public static readonly string AccountLoginProperty = "AccountLogin";
        private Account _accountLogin = new Account();
        private ICommand _loadFileConfigCommand;
        private ICommand _btnCheckConnectCommand;
        private ICommand _btnSettingsCommand;
        private ICommand _btnExitCommand;
        private ICommand _btnLoginCommand;
        private ICommand _btnTutorialCommand;
        private ICommand _btnAboutCommand;
        #region set and get method
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
        public ICommand BtnCheckConnectCommand
        {
            get
            {
                return _btnCheckConnectCommand ?? (_btnCheckConnectCommand = new CommandHandler(BtnCheckConnect, true));
            }
        }
        private void BtnCheckConnect()
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                try
                {
                    con.Open();
                    MessageBox.Show("kết nối thành công");
                    con.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }

        public ICommand BtnSettingsCommand
        {
            get
            {
                return _btnSettingsCommand ?? (_btnSettingsCommand = new CommandHandler(OpenSettings, true));
            }
        }
        private void OpenSettings()
        {
            SettingsWindow settings = new SettingsWindow();
            settings.ShowDialog();
        }
        public ICommand BtnExitCommand
        {
            get
            {
                return _btnExitCommand ?? (_btnExitCommand = new CommandHandler(CloseWindow, true));
            }
        }

        public event EventHandler RequestClose;
        private void CloseWindow()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public ICommand BtnLoginCommand
        {
            get
            {
                return _btnLoginCommand ?? (_btnLoginCommand = new CommandHandler(Login, true));
            }
        }
        private void Login()
        {
            if (String.IsNullOrEmpty(AccountLogin.Username))
            {
                MessageBox.Show("Vui lòng nhập tài khoản");
                return;
            }
            if (String.IsNullOrEmpty(AccountLogin.Password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu");
                return;
            }
            AccountLogin.Username = AccountLogin.Username.Trim();
            AccountLogin.Password = AccountLogin.Password.Trim();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                DataTable dtOtherCareer = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Accounts where Delete_flag = 0 and Username = '" + AccountLogin.Username + "' and Password = '" + AccountLogin.Password + "'", con);
                    adapter.Fill(dt);
                    if(dt.Rows.Count > 0)
                    {
                        foreach( DataRow dr in dt.Rows)
                        {
                            Constants.IdUser = dr["IDUser"].ToString();
                            Constants.Username = dr["Username"].ToString();
                            Constants.Password = dr["Password"].ToString();
                            Constants.Permission = dr["Permission"].ToString();
                        }
                        SqlDataAdapter adapterOther = new SqlDataAdapter();
                        string sqlOtherCareer = "select IDCG from CareerGroups where CareerGroupName like N'khác'";
                        adapterOther.SelectCommand = new SqlCommand(sqlOtherCareer, con);
                        adapterOther.Fill(dtOtherCareer);
                        if(dtOtherCareer.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtOtherCareer.Rows)
                            {
                                Constants.OtherCareerGroup = dr["IDCG"].ToString();
                            }
                        } else
                        {
                            Constants.OtherCareerGroup = "-1";
                        }
                        //MessageBox.Show("đăng nhập thành công  " + Constants.Username);
                        ReportManagerWindow reportManager = new ReportManagerWindow(true);
                        BtnExitCommand.Execute(true);
                        reportManager.Show();
                        
                    }
                    else
                    {
                        MessageBox.Show("kiểm tra lại thông tin");
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

        public ICommand LoadFileConfigCommand
        {
            get
            {
                return _loadFileConfigCommand ?? (_loadFileConfigCommand = new CommandHandler(LoadFileConfig, true));
            }
        }
        private void LoadFileConfig()
        {
            var path = @Directory.GetCurrentDirectory() + "\\Resources\\server_config.txt";
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // input Source
                    if (line.IndexOf("<Source>") != -1)
                    {
                        int iBegin = line.IndexOf("<Source>") + "<Source>".Length;
                        int iEnd = line.LastIndexOf("</Source>");
                        Constants.Source = line.Substring(iBegin, iEnd - iBegin);
                        continue;
                    }
                    // input Catalog
                    if (line.IndexOf("<Catalog>") != -1)
                    {
                        int iBegin = line.IndexOf("<Catalog>") + "<Catalog>".Length;
                        int iEnd = line.LastIndexOf("</Catalog>");
                        Constants.Catalog = line.Substring(iBegin, iEnd - iBegin);
                        continue;
                    }
                    // input IDSQLServer
                    if (line.IndexOf("<IDSQLServer>") != -1)
                    {
                        int iBegin = line.IndexOf("<IDSQLServer>") + "<IDSQLServer>".Length;
                        int iEnd = line.LastIndexOf("</IDSQLServer>");
                        Constants.IDSQLServer = line.Substring(iBegin, iEnd - iBegin);
                        continue;
                    }
                    // input PasswordSQLServer
                    if (line.IndexOf("<PasswordSQLServer>") != -1)
                    {
                        int iBegin = line.IndexOf("<PasswordSQLServer>") + "<PasswordSQLServer>".Length;
                        int iEnd = line.LastIndexOf("</PasswordSQLServer>");
                        Constants.PasswordSQLServer = line.Substring(iBegin, iEnd - iBegin);
                    }
                    // input PathShare
                    if (line.IndexOf("<PathShare>") != -1)
                    {
                        int iBegin = line.IndexOf("<PathShare>") + "<PathShare>".Length;
                        int iEnd = line.LastIndexOf("</PathShare>");
                        Constants.PathShare = line.Substring(iBegin, iEnd - iBegin);
                    }
                    // input AccountShare
                    if (line.IndexOf("<AccountShare>") != -1)
                    {
                        int iBegin = line.IndexOf("<AccountShare>") + "<AccountShare>".Length;
                        int iEnd = line.LastIndexOf("</AccountShare>");
                        Constants.AccountShare = line.Substring(iBegin, iEnd - iBegin);
                    }
                    // input PasswordShare
                    if (line.IndexOf("<PasswordShare>") != -1)
                    {
                        int iBegin = line.IndexOf("<PasswordShare>") + "<PasswordShare>".Length;
                        int iEnd = line.LastIndexOf("</PasswordShare>");
                        Constants.PassShare = line.Substring(iBegin, iEnd - iBegin);
                    }
                }
            }
        }

        public ICommand BtnTutorialCommand
        {
            get
            {
                return _btnTutorialCommand ?? (_btnTutorialCommand = new CommandHandler(Tutorial, true));
            }
        }
        private void Tutorial()
        {
            var path = @Directory.GetCurrentDirectory() + "\\Tutorial.pdf";
            System.Diagnostics.Process.Start(path);
        }

        public ICommand BtnAboutCommand
        {
            get
            {
                return _btnAboutCommand ?? (_btnAboutCommand = new CommandHandler(About, true));
            }
        }
        private void About()
        {
            AboutWindow window = new AboutWindow();
            window.ShowDialog();
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

            field =value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
