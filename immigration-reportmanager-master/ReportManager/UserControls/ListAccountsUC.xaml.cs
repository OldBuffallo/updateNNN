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
    /// Interaction logic for ListAccountsUC.xaml
    /// </summary>
    public partial class ListAccountsUC : UserControl
    {
        private ListAccountViewModel _listAccountVM;
        private Account _selectedItem;
        private ManagerWindow _window;
        public ListAccountsUC(ManagerWindow window)
        {
            InitializeComponent();
            _window = window;
            _listAccountVM = new ListAccountViewModel(window);
            this.DataContext = _listAccountVM;
        }
        private void dtgDataAccountsInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // navigate to edit screen
            _listAccountVM.NavigateToEditAccount(_selectedItem);
        }

        private void dtgDataAccountsInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = this.dtgDataAccountsInfo.SelectedItem as Account;
            if(item != null)
            {
                _selectedItem = item;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if ("nhập tên tài khoản hoặc tên cán bộ".Equals(textBox.Text.ToString()))
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (String.IsNullOrWhiteSpace(textBox.Text.ToString()))
            {
                textBox.Text = "nhập tên tài khoản hoặc tên cán bộ";
            }
        }
    }
}
