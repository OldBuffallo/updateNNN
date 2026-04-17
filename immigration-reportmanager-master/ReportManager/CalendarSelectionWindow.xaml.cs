using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for CalendarSelectionWindow.xaml
    /// </summary>
    public partial class CalendarSelectionWindow : Window
    {
        public CalendarSelectionWindow()
        {
            InitializeComponent();
        }

        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = Mouse.GetPosition(null).Y;
            this.Left = Mouse.GetPosition(null).X;
        }
    }
}
