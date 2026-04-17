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
    /// Interaction logic for PhoneNumberWindow.xaml
    /// </summary>
    public partial class PhoneNumberWindow : Window
    {
        private AddPhoneNumberViewModel _addPhoneNumbersVM;
        public PhoneNumberWindow(ObservableCollection<PhoneNumber> listPhoneNumbers, ObservableCollection<PhoneNumber> listUpdatePhoneNumbers = null)
        {
            InitializeComponent();
            _addPhoneNumbersVM = new AddPhoneNumberViewModel(this, listPhoneNumbers, listUpdatePhoneNumbers);
            this.DataContext = _addPhoneNumbersVM;
            this.addPhone.Visibility = Visibility.Visible;
            this.editPhone.Visibility = Visibility.Collapsed;
        }

        public PhoneNumberWindow(PhoneNumber selectphone, ObservableCollection<PhoneNumber> listPhoneNumbers, ObservableCollection<PhoneNumber> listUpdatePhoneNumbers = null)
        {
            InitializeComponent();
            _addPhoneNumbersVM = new AddPhoneNumberViewModel(this,selectphone,  listPhoneNumbers, listUpdatePhoneNumbers);
            this.DataContext = _addPhoneNumbersVM;
            this.addPhone.Visibility = Visibility.Collapsed;
            this.editPhone.Visibility = Visibility.Visible;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }
    }
}
