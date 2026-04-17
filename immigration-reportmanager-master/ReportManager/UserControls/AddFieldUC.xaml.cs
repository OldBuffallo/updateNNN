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
    /// Interaction logic for AddFieldUC.xaml
    /// </summary>
    public partial class AddFieldUC : UserControl
    {
        private AddFieldViewModel _addFieldVM;
        public AddFieldUC(ManagerWindow window)
        {
            InitializeComponent();
            _addFieldVM = new AddFieldViewModel(window);
            spAdd.Visibility = Visibility.Visible;
            spEdit.Visibility = Visibility.Hidden;
            this.DataContext = _addFieldVM;
        }
        public AddFieldUC(ManagerWindow window, Field editField)
        {
            InitializeComponent();
            _addFieldVM = new AddFieldViewModel(window, editField);
            spAdd.Visibility = Visibility.Hidden;
            spEdit.Visibility = Visibility.Visible;
            this.DataContext = _addFieldVM;
        }
    }
}
