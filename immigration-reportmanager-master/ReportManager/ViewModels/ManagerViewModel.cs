using ReportManager.Bases;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ReportManager.ViewModels
{
    class ManagerViewModel : INotifyPropertyChanged
    {
        public static readonly string CurrentViewProperty = "CurrentView";
        private UserControl _currentView;

        public ManagerViewModel(ManagerWindow window, int state)
        {
            switch (state)
            {
                case Constants.OpenListFieldUC:
                    CurrentView = new ListFieldsUC(window);
                    break;
                case Constants.OpenListCareersUC:
                    CurrentView = new ListCareersUC(window);
                    break;
                case Constants.OpenListAccountsUC:
                    CurrentView = new ListAccountsUC(window);
                    break;
                case Constants.OpenListNationalitiesUC:
                    CurrentView = new ListNationalityUC(window);
                    break;
                case Constants.OpenListDistrictsUC:
                    CurrentView = new ListDistrictsUC(window);
                    break;
                case Constants.OpenListWardsUC:
                    CurrentView = new ListWardsUC(window);
                    break;
            }
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
