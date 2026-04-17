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
    /// Interaction logic for AddDistrictUC.xaml
    /// </summary>
    public partial class AddDistrictUC : UserControl
    {
        private AddDistrictViewModel _addDistrictVM;

        public AddDistrictUC(ManagerWindow window)
        {
            InitializeComponent();
            _addDistrictVM = new AddDistrictViewModel(window);
            this.DataContext = _addDistrictVM;
            spAdd.Visibility = Visibility.Visible;
            spEdit.Visibility = Visibility.Collapsed;
        }
        public AddDistrictUC(ManagerWindow window, District selectedDistrict)
        {
            InitializeComponent();
            _addDistrictVM = new AddDistrictViewModel(window, selectedDistrict);
            this.DataContext = _addDistrictVM;
            spAdd.Visibility = Visibility.Collapsed;
            spEdit.Visibility = Visibility.Visible;
        }
    }
}
