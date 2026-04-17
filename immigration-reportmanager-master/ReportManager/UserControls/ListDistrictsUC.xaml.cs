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
    /// Interaction logic for ListDistrictsUC.xaml
    /// </summary>
    public partial class ListDistrictsUC : UserControl
    {
        private ListDistrictsViewModel _listDistrictsVM;
        public ListDistrictsUC(ManagerWindow window)
        {
            InitializeComponent();
            _listDistrictsVM = new ListDistrictsViewModel(window);
            this.DataContext = _listDistrictsVM;
        }

        private void dtgDataDistrictsInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _listDistrictsVM.BtnEditDistrictCommand.Execute(true);
        }

        private void dtgDataDistrictsInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if ("nhập tên quận/huyện".Equals(textBox.Text.ToString()))
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (String.IsNullOrWhiteSpace(textBox.Text.ToString()))
            {
                textBox.Text = "nhập tên quận/huyện";
            }
        }
    }
}
