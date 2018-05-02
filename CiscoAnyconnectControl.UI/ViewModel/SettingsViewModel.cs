using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CiscoAnyconnectControl.UI.ViewModel.Converters;
using CiscoAnyconnectControl.UI.Command;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.Annotations;
using CiscoAnyconnectControl.Model.DAL;
using CiscoAnyconnectControl.UI.Utility;
using CiscoAnyconnectControl.Utility;
using Microsoft.Win32;

namespace CiscoAnyconnectControl.UI.ViewModel
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            this.CommandSaveToPersistentStorage = new RelayCommand(() => true, () =>
            {
                SettingsFile.Instance.Save();
            });
            SettingsFile.Instance.SettingsModel.PropertyChanged += SettingsModel_PropertyChanged;
        }

        private void SettingsModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.GetType().GetProperty(e.PropertyName) != null)
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        public bool SavePassword
        {
            get => SettingsFile.Instance.SettingsModel.SavePassword;
            set => SettingsFile.Instance.SettingsModel.SavePassword = value;
        }
        public bool ConnectOnSystemStartup
        {
            get => SettingsFile.Instance.SettingsModel.ConnectOnSystemStartup;
            set => SettingsFile.Instance.SettingsModel.ConnectOnSystemStartup = value;
        }
        public bool ReconnectOnConnectionLoss
        {
            get => SettingsFile.Instance.SettingsModel.ReconnectOnConnectionLoss;
            set => SettingsFile.Instance.SettingsModel.ReconnectOnConnectionLoss = value;
        }

        public bool StartGuiOnLogon
        {
            get => SettingsFile.Instance.SettingsModel.StartGuiOnLogon;
            set => SettingsFile.Instance.SettingsModel.StartGuiOnLogon = value;
        }

        public bool NotifyAfterX
        {
            get => SettingsFile.Instance.SettingsModel.NotifyAfterX;
            set => SettingsFile.Instance.SettingsModel.NotifyAfterX = value;
        }
        [TypeConverter(typeof(IntConverter))]
        public int NotifyAfterHours
        {
            get => SettingsFile.Instance.SettingsModel.NotifyAfterHours;
            set => SettingsFile.Instance.SettingsModel.NotifyAfterHours = value;
        }
        public bool CloseToTray
        {
            get => SettingsFile.Instance.SettingsModel.CloseToTray;
            set => SettingsFile.Instance.SettingsModel.CloseToTray = value;
        }

        public RelayCommand CommandSelectCiscoCli { get; private set; }

        public RelayCommand CommandSaveToPersistentStorage { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            switch (propertyName)
            {
                case nameof(StartGuiOnLogon):
                    if (StartGuiOnLogon)
                    {
                        OSUtil.Instance.AddUiToSystemStart();
                    }
                    else
                    {
                        OSUtil.Instance.RemoveUiFromSystemStart();
                    }
                    break;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
