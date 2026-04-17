using ReportManager.Models;
using ReportManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReportManager.UserControls
{
    /// <summary>
    /// Interaction logic for ListAttachCompanyUC.xaml
    /// </summary>
    public partial class ListAttachCompanyUC : UserControl
    {
        private ListAttachCompanyViewModel _listAttachCompanyVM;
        public ListAttachCompanyUC(AttachWindow window, int idCompany, int trackerID)
        {
            InitializeComponent();
            _listAttachCompanyVM = new ListAttachCompanyViewModel(window, idCompany, trackerID);
            this.DataContext = _listAttachCompanyVM;
        }

        private void dtgDataAttachInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _listAttachCompanyVM.BtnEditAttachCommand.Execute(true);
        }

        private void dtgDataAttachInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _listAttachCompanyVM.BtnDownloadAttachCommand.Execute(true);
        }
    }
}
