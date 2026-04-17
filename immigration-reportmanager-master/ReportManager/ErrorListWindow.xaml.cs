using ReportManager.Models;
using ReportManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReportManager
{
    /// <summary>
    /// Interaction logic for ErrorListWindow.xaml
    /// </summary>
    public partial class ErrorListWindow : Window
    {
        private ErrorListViewModel _errorListVM;
        public ErrorListWindow(ObservableCollection<Employee> listErrorEmployees)
        {
            InitializeComponent();
            _errorListVM = new ErrorListViewModel(this, listErrorEmployees);
            this.DataContext = _errorListVM;
        }
    }
}
