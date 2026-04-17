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
    /// Interaction logic for ListCompaniesUC.xaml
    /// </summary>
    public partial class ListCompaniesUC : UserControl
    {
        private ListCompaniesViewModel _listCompaniesVM;
        public ListCompaniesUC(ReportManagerWindow window, bool isLogin = false)
        {
            InitializeComponent();
            _listCompaniesVM = new ListCompaniesViewModel(window, this);
            this.DataContext = _listCompaniesVM;
            if (isLogin)
            {
                _listCompaniesVM.BtnAlmostExpiredCommand.Execute(true);
            }
        }

        private void dtgDataCompaniesInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _listCompaniesVM.BtnListEmployeesCommand.Execute(true);
        }

        private void dtgDataCompaniesInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
