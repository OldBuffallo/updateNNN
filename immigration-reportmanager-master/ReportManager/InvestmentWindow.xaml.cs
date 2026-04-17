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
    /// Interaction logic for InvestmentWindow.xaml
    /// </summary>
    public partial class InvestmentWindow : Window
    {
        private AddInvestmentViewModel _addInvestmentVM;
        public InvestmentWindow(Investment selectInvestment, ObservableCollection<Investment> listInvestments, ObservableCollection<Investment> listUpdateInvestments = null)
        {
            InitializeComponent();
            _addInvestmentVM = new AddInvestmentViewModel(this, selectInvestment, listInvestments, listUpdateInvestments);
            this.DataContext = _addInvestmentVM;
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
    }
}
