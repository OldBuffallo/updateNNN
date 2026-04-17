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
    /// Interaction logic for ListWardsUC.xaml
    /// </summary>
    public partial class ListWardsUC : UserControl
    {
        private ListWardsViewModel _listWardsVM;
        public ListWardsUC(ManagerWindow window)
        {
            InitializeComponent();
            _listWardsVM = new ListWardsViewModel(window);
            this.DataContext = _listWardsVM;
        }

        private void dtgDataWardsInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _listWardsVM.BtnEditWardCommand.Execute(true);
        }

        private void dtgDataWardsInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if ("nhập tên phường/xã".Equals(textBox.Text.ToString()))
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (String.IsNullOrWhiteSpace(textBox.Text.ToString()))
            {
                textBox.Text = "nhập tên phường/xã";
            }
        }
    }
}
