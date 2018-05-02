using CiscoAnyconnectControl.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using CiscoAnyconnectControl.UI.ViewModel.DesignMocks;

namespace CiscoAnyconnectControl.UI.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VpnDataViewModel _vpnDataViewModel = null;
        private VpnStatusViewModel _vpnStatusViewModel = null;
        private SettingsViewModel _settingsViewModel = null;
        private PasswordBox _pwdBox = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vpnDataViewModel = (VpnDataViewModel) FindResource("VpnData");
            _vpnStatusViewModel = (VpnStatusViewModel) FindResource("VpnStatus");
            _settingsViewModel = (SettingsViewModel) FindResource("Settings");
            _pwdBox = (PasswordBox)FindName("PwdVpnPassword");
            if (_pwdBox != null) _pwdBox.Password = _vpnDataViewModel.Password;
        }

        private void PwdVpnPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _vpnDataViewModel.SecurePassword = ((PasswordBox)sender).SecurePassword;
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            _vpnDataViewModel.SaveToModel.Execute();
            _vpnStatusViewModel?.CurrentActionCommand?.Execute();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _settingsViewModel.CommandSaveToPersistentStorage.Execute();
        }

        private void CbSavePassword_Unchecked(object sender, RoutedEventArgs e)
        {
            this._pwdBox.Password = "";
        }
    }
}
