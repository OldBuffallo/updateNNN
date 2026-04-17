using ReportManager.Bases;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class ReportManagerViewModel : INotifyPropertyChanged
    {
        public static readonly string CurrentViewProperty = "CurrentView";
        private UserControl _currentView;
        private ICommand _btnHomeCommand;
        private ICommand _btnAddAccountCommand;
        private ICommand _btnFieldsCommand;
        private ICommand _btnCareersCommand;
        private ICommand _btnNationalitiesCommand;
        private ICommand _btnDistrictsCommand;
        private ICommand _btnWardsCommand;
        private ICommand _btnLogOutCommand;
        private ICommand _btnTutorialCommand;
        private ICommand _btnAboutCommand;
        private ReportManagerWindow _window;

        public ReportManagerViewModel(ReportManagerWindow window, bool isLogin = false)
        {
            _window = window;
            CurrentView = new ListCompaniesUC(window, isLogin);
        }
        public UserControl CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                this.SetValue(ref _currentView, value, CurrentViewProperty);
                OnPropertyChanged("CurrentView");
            }
        }

        #region method ICommand
        public ICommand BtnHomeCommand
        {
            get
            {
                return _btnHomeCommand ?? (_btnHomeCommand = new CommandHandler(Home, true));
            }
        }

        private void Home()
        {
            _window.ContentArea.Content = new ListCompaniesUC(_window);
        }
        public ICommand BtnAddAccountCommand
        {
            get
            {
                return _btnAddAccountCommand ?? (_btnAddAccountCommand = new CommandHandler(AddAccount, true));
            }
        }
        private void AddAccount()
        {
            if(int.Parse(Constants.Permission) != 1)
            {
                MessageBox.Show("Bạn không có quyền này");
                return;
            }
            ManagerWindow window = new ManagerWindow(Constants.OpenListAccountsUC);
            window.ShowDialog();
        }

        public ICommand BtnFieldsCommand
        {
            get
            {
                return _btnFieldsCommand ?? (_btnFieldsCommand = new CommandHandler(Fields, true));
            }
        }
        private void Fields()
        {
            if (int.Parse(Constants.Permission) != 1)
            {
                MessageBox.Show("Bạn không có quyền này");
                return;
            }
            ManagerWindow window = new ManagerWindow(Constants.OpenListFieldUC);
            window.ShowDialog();
        }

        public ICommand BtnCareersCommand
        {
            get
            {
                return _btnCareersCommand ?? (_btnCareersCommand = new CommandHandler(Careers, true));
            }
        }
        private void Careers()
        {
            if (int.Parse(Constants.Permission) != 1)
            {
                MessageBox.Show("Bạn không có quyền này");
                return;
            }
            ManagerWindow window = new ManagerWindow(Constants.OpenListCareersUC);
            window.ShowDialog();
        }

        public ICommand BtnNationalitiesCommand
        {
            get
            {
                return _btnNationalitiesCommand ?? (_btnNationalitiesCommand = new CommandHandler(Nationalities, true));
            }
        }
        private void Nationalities()
        {
            if (int.Parse(Constants.Permission) != 1)
            {
                MessageBox.Show("Bạn không có quyền này");
                return;
            }
            ManagerWindow window = new ManagerWindow(Constants.OpenListNationalitiesUC);
            window.ShowDialog();
        }

        public ICommand BtnDistrictsCommand
        {
            get
            {
                return _btnDistrictsCommand ?? (_btnDistrictsCommand = new CommandHandler(Districts, true));
            }
        }
        private void Districts()
        {
            if (int.Parse(Constants.Permission) != 1)
            {
                MessageBox.Show("Bạn không có quyền này");
                return;
            }
            ManagerWindow window = new ManagerWindow(Constants.OpenListDistrictsUC);
            window.ShowDialog();
        }

        public ICommand BtnWardsCommand
        {
            get
            {
                return _btnWardsCommand ?? (_btnWardsCommand = new CommandHandler(Wards, true));
            }
        }
        private void Wards()
        {
            if (int.Parse(Constants.Permission) != 1)
            {
                MessageBox.Show("Bạn không có quyền này");
                return;
            }
            ManagerWindow window = new ManagerWindow(Constants.OpenListWardsUC);
            window.ShowDialog();
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

        public ICommand BtnLogOutCommand
        {
            get
            {
                return _btnLogOutCommand ?? (_btnLogOutCommand = new CommandHandler(LogOut, true));
            }
        }
        public event EventHandler RequestClose;
        private void LogOut()
        {
            Constants.IdUser = "";
            Constants.Username = "";
            Constants.Password = "";
            Constants.Permission = "";
            MainWindow main = new MainWindow();
            main.Show();
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
