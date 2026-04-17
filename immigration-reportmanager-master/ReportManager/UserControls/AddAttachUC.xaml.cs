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
    /// Interaction logic for AddAttachUC.xaml
    /// </summary>
    public partial class AddAttachUC : UserControl
    {
        private AddAttachViewModel _addAttachVM;
        public AddAttachUC(AttachWindow window, int idCompany, int trackerID)
        {
            InitializeComponent();
            _addAttachVM = new AddAttachViewModel(window, idCompany, trackerID);
            this.DataContext = _addAttachVM;
            spAdd.Visibility = Visibility.Visible;
            spEdit.Visibility = Visibility.Hidden;
        }

        public AddAttachUC(AttachWindow window,Attach attach, int idCompany, int trackerID)
        {
            InitializeComponent();
            _addAttachVM = new AddAttachViewModel(window, attach, idCompany, trackerID);
            this.DataContext = _addAttachVM;
            spAdd.Visibility = Visibility.Hidden;
            spEdit.Visibility = Visibility.Visible;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //var textBox = sender as TextBox;
            //e.Handled = Regex.IsMatch(e.Text, "[^0-9a-zA-Z_.]+");
        }
    }
}
