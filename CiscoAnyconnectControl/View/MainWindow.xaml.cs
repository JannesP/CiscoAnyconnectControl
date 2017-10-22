using CiscoAnyconnectControl.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CiscoAnyconnectControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VpnDataViewModel vpnDataViewModel = null;
        private PasswordBox pwdBox = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vpnDataViewModel = (VpnDataViewModel) FindResource("VpnData");
            pwdBox = (PasswordBox)FindName("PwdVpnPassword");

            pwdBox.Password = vpnDataViewModel.Password;
        }

        private void PwdVpnPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            vpnDataViewModel.SecurePassword = ((PasswordBox)sender).SecurePassword;
        }
    }
}
