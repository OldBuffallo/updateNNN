using ReportManager.Bases;
using ReportManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace ReportManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainVM;
        public MainWindow()
        {
            InitializeComponent();
            _mainVM = new MainViewModel();
            _mainVM.LoadFileConfigCommand.Execute(true);
            _mainVM.RequestClose += (s, e) => this.Close();
            this.DataContext = _mainVM;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _mainVM.BtnLoginCommand.Execute(true);
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9a-zA-Z_.]+");
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                _mainVM.AccountLogin.Password = ((PasswordBox)sender).Password.ToString();
            }
        }
    }
}
