using ReportManager.ViewModels;
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
    /// Interaction logic for ReportManagerWindow.xaml
    /// </summary>
    public partial class ReportManagerWindow : Window
    {
        private ReportManagerViewModel _reportManagerVM;
        public ReportManagerWindow(bool isLogin = false)
        {
            InitializeComponent();
            _reportManagerVM = new ReportManagerViewModel(this, isLogin);
            _reportManagerVM.RequestClose += (s, e) => this.Close();
            this.DataContext = _reportManagerVM;
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
