﻿using CiscoAnyconnectControl.ViewModel;
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
using System.Windows.Threading;

namespace CiscoAnyconnectControl.View
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
        private DispatcherTimer _dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this._vpnDataViewModel = (VpnDataViewModel) FindResource("VpnData");
            this._vpnStatusViewModel = (VpnStatusViewModel) FindResource("VpnStatus");
            this._settingsViewModel = (SettingsViewModel) FindResource("Settings");
            this._pwdBox = (PasswordBox)FindName("PwdVpnPassword");
            this._pwdBox.Password = this._vpnDataViewModel.Password;

            this._dispatcherTimer = new DispatcherTimer(new TimeSpan(TimeSpan.TicksPerSecond * 5), DispatcherPriority.Normal, this._dispatcherTimer_Tick, Dispatcher.CurrentDispatcher);
            this._dispatcherTimer.Start();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this._vpnStatusViewModel.RefreshVpnStatus();
        }

        private void PwdVpnPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this._vpnDataViewModel.SecurePassword = ((PasswordBox)sender).SecurePassword;
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            this._vpnDataViewModel.SaveToModel.Execute();
            this._vpnStatusViewModel.CurrentActionCommand.Execute();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._dispatcherTimer.Stop();
            this._settingsViewModel.CommandSaveToPersistentStorage.Execute();
            this._vpnStatusViewModel.Dispose();
        }
    }
}
