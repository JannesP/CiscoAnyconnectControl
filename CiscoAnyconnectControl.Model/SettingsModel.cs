using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Model.Annotations;

namespace CiscoAnyconnectControl.Model
{
    [Serializable]
    public class SettingsModel : INotifyPropertyChanged
    {
        private string _ciscoCliPath = @"C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client\vpncli.exe";
        private bool _savePassword = true;
        private bool _connectOnSystemStartup = true;
        private bool _reconnectOnConnectionLoss = true;
        private bool _startGuiOnLogon = false;
        private bool _notifyAfterX = true;
        private int _notifyAfterHours = 9;

        public string CiscoCliPath
        {
            get { return this._ciscoCliPath; }
            set
            {
                this._ciscoCliPath = value;
                OnPropertyChanged(nameof(this.CiscoCliPath));
            }
        }

        public bool SavePassword
        {
            get { return this._savePassword; }
            set
            {
                this._savePassword = value;
                OnPropertyChanged(nameof(this.SavePassword));
            }
        }

        public bool ConnectOnSystemStartup
        {
            get { return this._connectOnSystemStartup; }
            set
            {
                this._connectOnSystemStartup = value;
                OnPropertyChanged(nameof(this.ConnectOnSystemStartup));
            }
        }

        public bool ReconnectOnConnectionLoss
        {
            get { return this._reconnectOnConnectionLoss; }
            set
            {
                this._reconnectOnConnectionLoss = value;
                OnPropertyChanged(nameof(this.ReconnectOnConnectionLoss));
            }
        }

        public bool StartGuiOnLogon
        {
            get { return this._startGuiOnLogon; }
            set
            {
                this._startGuiOnLogon = value;
                OnPropertyChanged(nameof(this.StartGuiOnLogon));
            }
        }

        public bool NotifyAfterX
        {
            get { return this._notifyAfterX; }
            set
            {
                this._notifyAfterX = value;
                OnPropertyChanged(nameof(this.NotifyAfterX));
            }
        }

        public int NotifyAfterHours
        {
            get { return this._notifyAfterHours; }
            set
            {
                this._notifyAfterHours = value;
                OnPropertyChanged(nameof(this.NotifyAfterHours));
            }
        }

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
