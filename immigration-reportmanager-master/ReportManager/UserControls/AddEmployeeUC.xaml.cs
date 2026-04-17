using ReportManager.Bases;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReportManager.UserControls
{
    /// <summary>
    /// Interaction logic for AddEmployeeUC.xaml
    /// </summary>
    public partial class AddEmployeeUC : UserControl
    {
        private AddEmployeeViewModel _addEmployeeVM;
        private ObservableCollection<string> wards;
        public AddEmployeeUC(ReportManagerWindow window, UserControl preUserControl, Company company)
        {
            InitializeComponent();
            _addEmployeeVM = new AddEmployeeViewModel(window, preUserControl, company);
            this.DataContext = _addEmployeeVM;
            this.addEmployee.Visibility = Visibility.Visible;
            this.editEmployee.Visibility = Visibility.Hidden;
            wards = MethodHandler.getWards();
        }

        public AddEmployeeUC(ReportManagerWindow window, UserControl preUserControl, Company company, Employee selectEmployee)
        {
            InitializeComponent();
            _addEmployeeVM = new AddEmployeeViewModel(window, preUserControl, company, selectEmployee);
            this.DataContext = _addEmployeeVM;
            this.addEmployee.Visibility = Visibility.Hidden;
            this.editEmployee.Visibility = Visibility.Visible;
            // check old data
            if (selectEmployee.Hidden != 0)
            {
                this.btnUpdateAndCreate.Visibility = Visibility.Hidden;
                this.btnUpdate.Visibility = Visibility.Hidden;
                this.btnDelete.Visibility = Visibility.Hidden;

            }
            wards = MethodHandler.getWards();
        }

        private void Date_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9-]+");
        }
        private void TextNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(workPermitcmb.SelectedIndex > 1)
            //{
            //    workPermitText.Text = "";
            //    workPermitText.Visibility = Visibility.Hidden;
            //}
            //else
            //{
            //    workPermitText.Visibility = Visibility.Visible;
            //}
            switch (workPermitcmb.SelectedIndex)
            {
                case 0:
                case 1:
                case 3:
                case 4:
                    workPermitText.Visibility = Visibility.Visible;
                    break;
                default:
                    workPermitText.Text = "";
                    workPermitText.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void cbbWorkingStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtDateOfLeave.Text = "";
            if (cbbWorkingStatus.SelectedIndex == 0)
            {
                gridDateOfLeave.Visibility = Visibility.Hidden;
            }
            else
            {
                gridDateOfLeave.Visibility = Visibility.Visible;
            }
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            CalendarSelectionWindow calendarWin = new CalendarSelectionWindow();
            calendarWin.ShowDialog();
            if(calendarWin.calendar.SelectedDate != null)
            {
                DateTime dateSelect = (DateTime)calendarWin.calendar.SelectedDate;
                textBox.Text = dateSelect.ToString("dd-MM-yyyy");
            }
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _addEmployeeVM.BtnSetNationalityCommand.Execute(true);
        }
        private void Upper_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^a-zA-Z]+");
        }

        private void ComboBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            cmb.IsDropDownOpen = true;

            string pastedText = (string)e.DataObject.GetData(typeof(string));
            string fullText = cmb.Text.Insert(MethodHandler.GetChildOfType<TextBox>(cmb).CaretIndex, pastedText);

            if (!string.IsNullOrEmpty(fullText))
            {
                ObservableCollection<string> filteredDistricts = new ObservableCollection<string>(wards.Where(s => s.IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) >= 0));
                cmb.ItemsSource = filteredDistricts;
            }
            else
            {
                cmb.ItemsSource = wards;
            }
        }

        private void ComboBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                ComboBox cmb = (ComboBox)sender;
                cmb.IsDropDownOpen = true;

                if (!string.IsNullOrEmpty(cmb.Text))
                {
                    ObservableCollection<string> filteredDistricts = new ObservableCollection<string>(wards.Where(s => s.IndexOf(cmb.Text, StringComparison.InvariantCultureIgnoreCase) >= 0));

                    cmb.ItemsSource = filteredDistricts;
                }
                else
                {
                    cmb.ItemsSource = wards;
                }
            }
        }

        private void ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            cmb.IsDropDownOpen = true;

            if (!string.IsNullOrEmpty(cmb.Text))
            {
                string fullText = cmb.Text.Insert(MethodHandler.GetChildOfType<TextBox>(cmb).CaretIndex, e.Text);
                ObservableCollection<string> filteredDistricts = new ObservableCollection<string>(wards.Where(s => s.IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) >= 0));
                cmb.ItemsSource = filteredDistricts;
            }
            else if (!string.IsNullOrEmpty(e.Text))
            {
                ObservableCollection<string> filteredDistricts = new ObservableCollection<string>(wards.Where(s => s.IndexOf(e.Text, StringComparison.InvariantCultureIgnoreCase) >= 0));
                cmb.ItemsSource = filteredDistricts;
            }
            else
            {
                cmb.ItemsSource = wards;
            }
        }
    }
}
