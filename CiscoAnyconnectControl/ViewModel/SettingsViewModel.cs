using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.ViewModel.Converters;
using CiscoAnyconnectControl.Command;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.DAL;
using Microsoft.Win32;

namespace CiscoAnyconnectControl.ViewModel
{
    class SettingsViewModel
    {
        public SettingsViewModel()
        {
            this.CommandSelectCiscoCli = new RelayCommand(() => true, () =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.InitialDirectory = @"C:\Program Files (x86)\";
                ofd.DefaultExt = "*.exe";
                ofd.Multiselect = false;
                bool success = ofd.ShowDialog().Value;
                if (success)
                {
                    this.CiscoCliPath = ofd.FileName;
                }
            });

            this.CommandSaveToPersistentStorage = new RelayCommand(() => true, () =>
            {
                SettingsFile.Instance.Save();
            });

            SettingsFile s = SettingsFile.Instance;
        }

        public string CiscoCliPath
        {
            get { return SettingsFile.Instance.SettingsModel.CiscoCliPath; }
            set { SettingsFile.Instance.SettingsModel.CiscoCliPath = value; }
        }

        public bool SavePassword
        {
            get { return SettingsFile.Instance.SettingsModel.SavePassword; }
            set { SettingsFile.Instance.SettingsModel.SavePassword = value; }
        }
        public bool ConnectOnSystemStartup
        {
            get { return SettingsFile.Instance.SettingsModel.ConnectOnSystemStartup; }
            set { SettingsFile.Instance.SettingsModel.ConnectOnSystemStartup = value; }
        }
        public bool ReconnectOnConnectionLoss
        {
            get { return SettingsFile.Instance.SettingsModel.ReconnectOnConnectionLoss; }
            set { SettingsFile.Instance.SettingsModel.ReconnectOnConnectionLoss = value; }
        }

        public bool StartGuiOnLogon
        {
            get { return SettingsFile.Instance.SettingsModel.StartGuiOnLogon; }
            set { SettingsFile.Instance.SettingsModel.StartGuiOnLogon = value; }
        }

        public bool NotifyAfterX
        {
            get { return SettingsFile.Instance.SettingsModel.NotifyAfterX; }
            set { SettingsFile.Instance.SettingsModel.NotifyAfterX = value; }
        }
        [TypeConverter(typeof(IntConverter))]
        public int NotifyAfterHours
        {
            get { return SettingsFile.Instance.SettingsModel.NotifyAfterHours; }
            set { SettingsFile.Instance.SettingsModel.NotifyAfterHours = value; }
        }

        public RelayCommand CommandSelectCiscoCli { get; private set; }

        public RelayCommand CommandSaveToPersistentStorage { get; private set; }
    }
}
