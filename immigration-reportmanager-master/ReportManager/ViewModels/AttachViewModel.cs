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
    public class AttachViewModel : INotifyPropertyChanged
    {
        public static readonly string CurrentViewProperty = "CurrentView";
        private UserControl _currentView;

        public AttachViewModel(AttachWindow window, int idCompany, int trackerID)
        {
            CurrentView = new ListAttachCompanyUC(window, idCompany, trackerID);
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
            PropertyChangedEventHandler handler = PropertyChanged;
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
