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
    /// Interaction logic for ExportFileCompanyWindow.xaml
    /// </summary>
    public partial class ExportFileCompanyWindow : Window
    {
        private ExportFileCompanyViewModel _exportFileCompanyVM;
        public ExportFileCompanyWindow(ObservableCollection<Company> listCompanies)
        {
            InitializeComponent();
            _exportFileCompanyVM = new ExportFileCompanyViewModel(this, listCompanies);
            this.DataContext = _exportFileCompanyVM;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9-]+");
        }
        private void number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string str = textBox.Text.ToString();
            if (string.IsNullOrWhiteSpace(str))
            {
                textBox.Text = "0";
            }
        }
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            CalendarSelectionWindow calendarWin = new CalendarSelectionWindow();
            calendarWin.ShowDialog();
            if (calendarWin.calendar.SelectedDate != null)
            {
                DateTime dateSelect = (DateTime)calendarWin.calendar.SelectedDate;
                textBox.Text = dateSelect.ToString("dd-MM-yyyy");
            }
        }

        private void txtStartDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtStartDate.Text.ToString()) && string.IsNullOrWhiteSpace(txtEndDate.Text.ToString()))
            {
                checkboxSettlementResults.Visibility = Visibility.Collapsed;
            }
            else
            {
                checkboxSettlementResults.Visibility = Visibility.Visible;
            }
        }

        private void Upper_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^a-zA-Z]+");
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _exportFileCompanyVM.BtnSetNationalityCommand.Execute(true);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var uie = e.OriginalSource as UIElement;

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                uie.MoveFocus(
                new TraversalRequest(
                FocusNavigationDirection.Next));
            }
        }
    }
}
