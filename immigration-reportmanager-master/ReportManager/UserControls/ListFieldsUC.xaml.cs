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
    /// Interaction logic for ListFieldsUC.xaml
    /// </summary>
    public partial class ListFieldsUC : UserControl
    {
        private ListFieldsViewModel _listFieldVM;
        public ListFieldsUC(ManagerWindow window)
        {
            InitializeComponent();
            _listFieldVM = new ListFieldsViewModel(window);
            this.DataContext = _listFieldVM;
        }

        private void dtgDataFielsInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _listFieldVM.BtnEditFieldCommand.Execute(true);
        }

        private void dtgDataFielsInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if ("nhập tên lĩnh vực".Equals(textBox.Text.ToString()))
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (String.IsNullOrWhiteSpace(textBox.Text.ToString()))
            {
                textBox.Text = "nhập tên lĩnh vực";
            }
        }
    }
}
