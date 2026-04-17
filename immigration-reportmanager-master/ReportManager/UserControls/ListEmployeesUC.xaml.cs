using ReportManager.Models;
using ReportManager.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReportManager.UserControls
{
    /// <summary>
    /// Interaction logic for ListEmployeesUC.xaml
    /// </summary>
    public partial class ListEmployeesUC : UserControl
    {
        private ListEmployeesViewModel _listEmployeesVM;
        public ListEmployeesUC(ReportManagerWindow window, Company company, UserControl userControl)
        {
            InitializeComponent();
            _listEmployeesVM = new ListEmployeesViewModel(window, userControl, company, this);
            this.DataContext = _listEmployeesVM;
        }

        private void dtgDataEmployeesInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _listEmployeesVM.BtnOpenEditEmployeeCommand.Execute(true);
        }

        private void dtgDataEmployeesInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
