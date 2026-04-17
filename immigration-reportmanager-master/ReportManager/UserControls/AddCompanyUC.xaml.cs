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
    /// Interaction logic for AddCompanyUC.xaml
    /// </summary>
    public partial class AddCompanyUC : UserControl
    {
        private AddCompanyViewModel _addCompanyVM;
        private ObservableCollection<string> wards;
        public AddCompanyUC(ReportManagerWindow window, UserControl userControl)
        {
            InitializeComponent();
            _addCompanyVM = new AddCompanyViewModel(window, userControl);
            this.DataContext = _addCompanyVM;
            this.AddCompany.Visibility = Visibility.Visible;
            this.LabelAddCompany.Visibility = Visibility.Visible;
            this.EditCompany.Visibility = Visibility.Hidden;
            this.LabelEditCompany.Visibility = Visibility.Hidden;
            this.gridTracker.Visibility = Visibility.Hidden;
            this.btnAttach.Visibility = Visibility.Hidden;
            wards = MethodHandler.getWards();
        }
        public AddCompanyUC(ReportManagerWindow window, Company selectCompany, UserControl userControl)
        {
            InitializeComponent();
            _addCompanyVM = new AddCompanyViewModel(window, selectCompany, userControl);
            this.DataContext = _addCompanyVM;
            this.AddCompany.Visibility = Visibility.Hidden;
            this.LabelAddCompany.Visibility = Visibility.Hidden;
            this.EditCompany.Visibility = Visibility.Visible;
            this.LabelEditCompany.Visibility = Visibility.Visible;
            this.gridTracker.Visibility = Visibility.Visible;
            this.btnAttach.Visibility = Visibility.Visible;
            wards = MethodHandler.getWards();
        }

        private void dtgDataInvestmentInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _addCompanyVM.BtnEditInvestmentCommand.Execute(true);
        }

        private void number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void uptime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9-/]+");
        }

        private void dtgDataEmailsInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _addCompanyVM.BtnEditEmailCommand.Execute(true);
        }

        private void dtgDataPhoneNumbersInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _addCompanyVM.BtnEditPhoneNumberCommand.Execute(true);
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

        private void cbbRegistrationProfileIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbbRegistrationProfileIndex.SelectedIndex)
            {
                case 0:
                    this.txtRegistrationProfile.Visibility = Visibility.Hidden;
                    this.txtRegistrationProfile.Text = "không";
                    break;
                case 1:
                    this.txtRegistrationProfile.Visibility = Visibility.Visible;
                    this.txtRegistrationProfile.Text = "";
                    break;
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
            else if(!string.IsNullOrEmpty(e.Text))
            {
                ObservableCollection<string> filteredDistricts = new ObservableCollection<string>(wards.Where(s => s.IndexOf(e.Text, StringComparison.InvariantCultureIgnoreCase) >= 0));
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
    }
}
