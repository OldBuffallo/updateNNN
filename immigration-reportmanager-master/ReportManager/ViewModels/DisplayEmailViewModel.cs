using ReportManager.Bases;
using ReportManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class DisplayEmailViewModel : INotifyPropertyChanged
    {
        #region properties
        public static readonly string ListCompaniesProperty = "ListCompanies";
        public static readonly string ShowEmailProperty = "ShowEmail";
        private ObservableCollection<Company> _listCompanies;
        private string _showEmail;
        private DisplayEmailWindow _window;
        private ICommand _btnCloseCommand;
        private ICommand _loadEmailCommand;
        #endregion

        #region set get method
        public DisplayEmailViewModel(DisplayEmailWindow window, ObservableCollection<Company> listCompanies)
        {
            _window = window;
            ListCompanies = listCompanies;
            LoadEmailCommand.Execute(true);
        }
        public ObservableCollection<Company> ListCompanies
        {
            get
            {
                return _listCompanies;
            }
            set
            {
                this.SetValue(ref _listCompanies, value, ListCompaniesProperty);
            }
        }
        public string ShowEmail
        {
            get
            {
                return _showEmail;
            }
            set
            {
                this.SetValue(ref _showEmail, value, ShowEmailProperty);
            }
        }
        #endregion

        #region ICommand method
        public ICommand LoadEmailCommand
        {
            get
            {
                return _loadEmailCommand ?? (_loadEmailCommand = new CommandHandler(LoadEmail, true));
            }
        }
        private void LoadEmail()
        {
            if(ListCompanies != null && ListCompanies.Count > 0)
            {
                ShowEmail = "";
                foreach(Company company in ListCompanies)
                {
                    if (company.TrackerID == int.Parse(Constants.IdUser))
                    {
                        foreach (Email item in company.ListEmail)
                        {
                            if (string.IsNullOrWhiteSpace(ShowEmail))
                            {
                                ShowEmail = item.Mail;
                            }
                            else
                            {
                                ShowEmail += ";" + item.Mail;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng kiểm tra lại danh sách công ty");
            }
        }

        public ICommand BtnCloseCommand
        {
            get
            {
                return _btnCloseCommand ?? (_btnCloseCommand = new CommandHandler(Close, true));
            }
        }
        private void Close()
        {
            _window.Close();
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
