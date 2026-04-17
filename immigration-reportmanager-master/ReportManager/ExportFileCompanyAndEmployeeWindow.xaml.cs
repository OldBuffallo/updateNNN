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
using System.Windows.Shapes;

namespace ReportManager
{
    /// <summary>
    /// Interaction logic for ExportFileCompanyAndEmployeeWindow.xaml
    /// </summary>
    public partial class ExportFileCompanyAndEmployeeWindow : Window
    {
        private ExportFileCompanyAndEmployeeViewModel _exportFileCompanyAndEmployeeVM;
        public ExportFileCompanyAndEmployeeWindow()
        {
            InitializeComponent();
            _exportFileCompanyAndEmployeeVM = new ExportFileCompanyAndEmployeeViewModel(this);
            this.DataContext = _exportFileCompanyAndEmployeeVM;
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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9-]+");
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

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _exportFileCompanyAndEmployeeVM.BtnSetNationalityCommand.Execute(true);
        }
    }
}
