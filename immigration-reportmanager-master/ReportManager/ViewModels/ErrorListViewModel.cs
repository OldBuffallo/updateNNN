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
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class ErrorListViewModel : INotifyPropertyChanged
    {
        public static readonly string ListErrorEmployeesProperty = "ListErrorEmployees";
        private ObservableCollection<Employee> _listErrorEmployees;
        private ICommand _btnCloseCommand;
        private ErrorListWindow _window;

        #region set get method
        public ErrorListViewModel(ErrorListWindow window, ObservableCollection<Employee> listErrorEmployees)
        {
            _window = window;
            ListErrorEmployees = listErrorEmployees;
        }
        public ObservableCollection<Employee> ListErrorEmployees
        {
            get
            {
                return _listErrorEmployees;
            }
            set
            {
                this.SetValue(ref _listErrorEmployees, value, ListErrorEmployeesProperty);
            }
        }
        #endregion

        #region ICommand method
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
