using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.ViewModel.Converters;

namespace CiscoAnyconnectControl.ViewModel
{
    class SettingsViewModel
    {
        public string CiscoCliPath { get; set; } =
            @"C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client\vpncli.exe";

        public bool SavePassword { get; set; } = true;

        public bool ConnectOnSystemStartup { get; set; } = true;

        public bool ReconnectOnConnectionLoss { get; set; } = true;

        public bool StartGuiOnLogon { get; set; } = false;

        public bool NotifyAfterX { get; set; } = true;
        [TypeConverter(typeof(IntConverter))]
        public int NotifyAfterHours { get; set; } = 9;
    }
}
