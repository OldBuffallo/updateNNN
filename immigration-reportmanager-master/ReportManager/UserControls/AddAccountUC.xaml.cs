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
    /// Interaction logic for AddAccountUC.xaml
    /// </summary>
    public partial class AddAccountUC : UserControl
    {
        private AddAccountViewModel _addAccountVM;

        public AddAccountUC(ManagerWindow window)
        {
            InitializeComponent();
            _addAccountVM = new AddAccountViewModel(window);
            spAdd.Visibility = Visibility.Visible;
            spEdit.Visibility = Visibility.Hidden;
            this.DataContext = _addAccountVM;
        }
        public AddAccountUC(ManagerWindow window, Account selectedAccount)
        {
            InitializeComponent();
            _addAccountVM = new AddAccountViewModel(window, selectedAccount);
            spAdd.Visibility = Visibility.Hidden;
            spEdit.Visibility = Visibility.Visible;
            this.DataContext = _addAccountVM;
        }

        //private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        _addAccountVM.BtnCreateAccountCommand.Execute(true);
        //    }
        //}

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9a-zA-Z_.]+");
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text = textBox.Text.Trim();
        }
    }
}
