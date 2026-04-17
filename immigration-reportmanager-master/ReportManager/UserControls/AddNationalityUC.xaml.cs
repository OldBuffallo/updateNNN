using ReportManager.Models;
using ReportManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for AddNationalityUC.xaml
    /// </summary>
    public partial class AddNationalityUC : UserControl
    {
        private AddNationalityViewModel _addNationalityVM;
        public AddNationalityUC(ManagerWindow window)
        {
            InitializeComponent();
            _addNationalityVM = new AddNationalityViewModel(window);
            spAdd.Visibility = Visibility.Visible;
            spEdit.Visibility = Visibility.Hidden;
            this.DataContext = _addNationalityVM;
        }
        public AddNationalityUC(ManagerWindow window, Nationality selectNationality)
        {
            InitializeComponent();
            _addNationalityVM = new AddNationalityViewModel(window, selectNationality);
            spAdd.Visibility = Visibility.Hidden;
            spEdit.Visibility = Visibility.Visible;
            this.DataContext = _addNationalityVM;
        }
        private void NationalityCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^a-zA-Z]+");
        }
    }
}
