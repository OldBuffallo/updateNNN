using ReportManager.Models;
using ReportManager.UserControls;
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
    /// Interaction logic for AlmostExpiredWindow.xaml
    /// </summary>
    public partial class AlmostExpiredWindow : Window
    {
        private AlmostExpiredViewModel _almostExpiredVM;
        public AlmostExpiredWindow(ReportManagerWindow reportManagerWindow, ObservableCollection<Employee> listAlmostExpired)
        {
            InitializeComponent();
            _almostExpiredVM = new AlmostExpiredViewModel(this, reportManagerWindow, listAlmostExpired);
            this.DataContext = _almostExpiredVM;
        }

        private void dtgDataEmployeesInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _almostExpiredVM.BtnEmployeeInfoCommand.Execute(true);
        }

        private void dtgDataEmployeesInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
