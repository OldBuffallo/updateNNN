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
    /// Interaction logic for ListNationalityUC.xaml
    /// </summary>
    public partial class ListNationalityUC : UserControl
    {
        private ListNationalityViewModel _listNationalitiesVM;
        public ListNationalityUC(ManagerWindow window)
        {
            InitializeComponent();
            _listNationalitiesVM = new ListNationalityViewModel(window);
            this.DataContext = _listNationalitiesVM;
        }

        private void dtgDataNationalitiesInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _listNationalitiesVM.BtnEditNationalityCommand.Execute(true);
        }

        private void dtgDataNationalitiesInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if ("nhập mã/tên quốc tịch".Equals(textBox.Text.ToString()))
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (String.IsNullOrWhiteSpace(textBox.Text.ToString()))
            {
                textBox.Text = "nhập mã/tên quốc tịch";
            }
        }
    }
}
