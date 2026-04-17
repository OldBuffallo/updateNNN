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
    /// Interaction logic for ListCareersUC.xaml
    /// </summary>
    public partial class ListCareersUC : UserControl
    {
        private ListCareersViewModel _listCareerVM;
        public ListCareersUC(ManagerWindow window)
        {
            InitializeComponent();
            _listCareerVM = new ListCareersViewModel(window);
            this.DataContext = _listCareerVM;
        }

        private void dtgDataCareersInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _listCareerVM.BtnEditCareerCommand.Execute(true);
        }

        private void dtgDataCareersInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if ("nhập tên nghề hoặc tên nhóm nghề".Equals(textBox.Text.ToString()))
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (String.IsNullOrWhiteSpace(textBox.Text.ToString()))
            {
                textBox.Text = "nhập tên nghề hoặc tên nhóm nghề";
            }
        }
    }
}
