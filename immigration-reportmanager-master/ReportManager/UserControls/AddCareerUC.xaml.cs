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
    /// Interaction logic for AddCareerUC.xaml
    /// </summary>
    public partial class AddCareerUC : UserControl
    {
        private AddCareerViewModel _addCareerVM;
        public AddCareerUC(ManagerWindow window)
        {
            InitializeComponent();
            _addCareerVM = new AddCareerViewModel(window);
            spAdd.Visibility = Visibility.Visible;
            spEdit.Visibility = Visibility.Hidden;
            this.DataContext = _addCareerVM;
        }
        public AddCareerUC(ManagerWindow window, Career editCareer)
        {
            InitializeComponent();
            _addCareerVM = new AddCareerViewModel(window, editCareer);
            spAdd.Visibility = Visibility.Hidden;
            spEdit.Visibility = Visibility.Visible;
            this.DataContext = _addCareerVM;
        }
    }
}
