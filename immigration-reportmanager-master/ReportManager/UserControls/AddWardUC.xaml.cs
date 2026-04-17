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
    /// Interaction logic for AddWardUC.xaml
    /// </summary>
    public partial class AddWardUC : UserControl
    {
        private AddWardViewModel _addWardVM;

        public AddWardUC(ManagerWindow window)
        {
            InitializeComponent();
            _addWardVM = new AddWardViewModel(window);
            this.DataContext = _addWardVM;
            spAdd.Visibility = Visibility.Visible;
            spEdit.Visibility = Visibility.Collapsed;
        }
        public AddWardUC(ManagerWindow window, Ward selectedWard)
        {
            InitializeComponent();
            _addWardVM = new AddWardViewModel(window, selectedWard);
            this.DataContext = _addWardVM;
            spAdd.Visibility = Visibility.Collapsed;
            spEdit.Visibility = Visibility.Visible;
        }
    }
}
