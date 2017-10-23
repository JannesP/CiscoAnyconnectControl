using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.ViewModel.Converters;
using CiscoAnyconnectControl.Command;
using Microsoft.Win32;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyconnectControl.ViewModel
{
    class SettingsViewModel
    {
        private SettingsModel Model { get; set; }

        public SettingsViewModel()
        {
            CommandSelectCiscoCli = new RelayCommand(() => true, () =>
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
                    CiscoCliPath = ofd.FileName;
                }
            });
        }

        public string CiscoCliPath { get; set; } =
            @"C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client\vpncli.exe";

        public bool SavePassword { get; set; } = true;

        public bool ConnectOnSystemStartup { get; set; } = true;

        public bool ReconnectOnConnectionLoss { get; set; } = true;

        public bool StartGuiOnLogon { get; set; } = false;

        public bool NotifyAfterX { get; set; } = true;
        [TypeConverter(typeof(IntConverter))]
        public int NotifyAfterHours { get; set; } = 9;

        public RelayCommand CommandSelectCiscoCli { get; private set; }
    }
}
