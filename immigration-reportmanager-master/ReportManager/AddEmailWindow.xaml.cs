using ReportManager.Models;
using ReportManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace ReportManager
{
    /// <summary>
    /// Interaction logic for AddEmailWindow.xaml
    /// </summary>
    public partial class AddEmailWindow : Window
    {
        private AddEmailViewModel _addEmailVM;
        public AddEmailWindow(ObservableCollection<Email> listData, ObservableCollection<Email> listUPdateData = null)
        {
            InitializeComponent();
            _addEmailVM = new AddEmailViewModel(this, listData, listUPdateData);
            this.DataContext = _addEmailVM;
            this.addEmail.Visibility = Visibility.Visible;
            this.editEmail.Visibility = Visibility.Collapsed;
        }

        public AddEmailWindow(Email selectEmail, ObservableCollection<Email> listEmails, ObservableCollection<Email> listUpdateEmails)
        {
            InitializeComponent();
            _addEmailVM = new AddEmailViewModel(this, selectEmail, listEmails, listUpdateEmails);
            this.DataContext = _addEmailVM;
            this.addEmail.Visibility = Visibility.Collapsed;
            this.editEmail.Visibility = Visibility.Visible;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9a-zA-Z_.@]+");
        }
    }
}
