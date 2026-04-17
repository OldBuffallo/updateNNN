using ReportManager.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        public static readonly string SourceProperty = "Source";
        public static readonly string CatalogProperty = "Catalog";
        public static readonly string IDSQLServerProperty = "IDSQLServer";
        public static readonly string PasswordSQLServerProperty = "PasswordSQLServer";
        public static readonly string PathShareProperty = "PathShare";
        public static readonly string AccountShareProperty = "AccountShare";
        public static readonly string PasswordShareProperty = "PasswordShare";
        private string _source = Constants.Source;
        private string _catalog = Constants.Catalog;
        private string _idSQLServer = Constants.IDSQLServer;
        private string _passwordSQLServer = Constants.PasswordSQLServer;
        private string _pathShare = Constants.PathShare;
        private string _accountShare = Constants.AccountShare;
        private string _passwordShare = Constants.PassShare;
        private ICommand _saveCommand;
        private ICommand _btnExitCommand;
        #region method set get
        public string Source
        {
            get
            {
                return _source;
            }

            set
            {
                this.SetValue(ref _source, value, SourceProperty);
            }
        }
        public string Catalog
        {
            get
            {
                return _catalog;
            }

            set
            {
                this.SetValue(ref _catalog, value, CatalogProperty);
            }
        }
        public string IDSQLServer
        {
            get
            {
                return _idSQLServer;
            }

            set
            {
                this.SetValue(ref _idSQLServer, value, IDSQLServerProperty);
            }
        }
        public string PasswordSQLServer
        {
            get
            {
                return _passwordSQLServer;
            }

            set
            {
                this.SetValue(ref _passwordSQLServer, value, PasswordSQLServerProperty);
            }
        }

        public string PathShare
        {
            get
            {
                return _pathShare;
            }

            set
            {
                this.SetValue(ref _pathShare, value, PathShareProperty);
            }
        }

        public string AccountShare
        {
            get
            {
                return _accountShare;
            }

            set
            {
                this.SetValue(ref _accountShare, value, AccountShareProperty);
            }
        }

        public string PasswordShare
        {
            get
            {
                return _passwordShare;
            }

            set
            {
                this.SetValue(ref _passwordShare, value, PasswordShareProperty);
            }
        }
        #endregion

        #region method icommand
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new CommandHandler(SaveConfig, true));
            }
        }
        public event EventHandler RequestClose;
        private void SaveConfig()
        {
            Source = Source.Trim();
            Catalog = Catalog.Trim();
            IDSQLServer = IDSQLServer.Trim();
            PasswordSQLServer = PasswordSQLServer.Trim();
            if (String.IsNullOrEmpty(Source) || String.IsNullOrEmpty(Catalog) || String.IsNullOrEmpty(IDSQLServer) || String.IsNullOrEmpty(PasswordSQLServer))
            {
                MessageBox.Show("Vui lòng không để trống các trường *");
                return;
            }
            if (String.IsNullOrEmpty(PathShare))
            {
                MessageBox.Show("Vui lòng không để trống các trường *");
                return;
            }
            var path = @Directory.GetCurrentDirectory() + "\\Resources\\server_config.txt";
            File.WriteAllText(path, string.Empty);
            using (StreamWriter stream = new FileInfo(path).AppendText())
            {
                stream.WriteLine("<Source>"+ Source +"</Source>");
                stream.WriteLine("<Catalog>"+ Catalog +"</Catalog>");
                stream.WriteLine("<IDSQLServer>"+ IDSQLServer +"</IDSQLServer>");
                stream.WriteLine("<PasswordSQLServer>"+ PasswordSQLServer +"</PasswordSQLServer>");
                stream.WriteLine("<PathShare>" + PathShare + "</PathShare>");
                stream.WriteLine("<AccountShare>" + AccountShare + "</AccountShare>");
                stream.WriteLine("<PasswordShare>" + PasswordShare + "</PasswordShare>");
            }
            Constants.Source = Source;
            Constants.Catalog = Catalog;
            Constants.IDSQLServer = IDSQLServer;
            Constants.PasswordSQLServer = PasswordSQLServer;
            Constants.PathShare = PathShare;
            Constants.AccountShare = AccountShare;
            Constants.PassShare = PasswordShare;
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        public ICommand BtnExitCommand
        {
            get
            {
                return _btnExitCommand ?? (_btnExitCommand = new CommandHandler(BtnExit, true));
            }
        }
        private void BtnExit()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
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
