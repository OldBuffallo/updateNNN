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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReportManager.UserControls
{
    /// <summary>
    /// Interaction logic for SearchResultsUC.xaml
    /// </summary>
    public partial class SearchResultsUC : UserControl
    {
        private SearchResultsViewModel _searchResultsVM;
        public SearchResultsUC(ReportManagerWindow window, ObservableCollection<Employee> listEmployees)
        {
            InitializeComponent();
            _searchResultsVM = new SearchResultsViewModel(window, listEmployees, this);
            this.DataContext = _searchResultsVM;
        }

        private void dtgDataEmployeesInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _searchResultsVM.BtnOpenEditEmployeeCommand.Execute(true);
        }
    }
}
