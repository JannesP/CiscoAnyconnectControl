using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            get => this._ciscoCliPath;
            set
            {
                if (this._ciscoCliPath == value) return;
                this._ciscoCliPath = value;
                OnPropertyChanged();
            }
        }

        public bool SavePassword
        {
            get => this._savePassword;
            set
            {
                if (this._savePassword == value) return;
                this._savePassword = value;
                OnPropertyChanged();
            }
        }

        public bool ConnectOnSystemStartup
        {
            get => this._connectOnSystemStartup;
            set
            {
                if (this._connectOnSystemStartup == value) return;
                this._connectOnSystemStartup = value;
                OnPropertyChanged();
            }
        }

        public bool ReconnectOnConnectionLoss
        {
            get => this._reconnectOnConnectionLoss;
            set
            {
                if (this._reconnectOnConnectionLoss == value) return;
                this._reconnectOnConnectionLoss = value;
                OnPropertyChanged();
            }
        }

        public bool StartGuiOnLogon
        {
            get => this._startGuiOnLogon;
            set
            {
                if (this._startGuiOnLogon == value) return;
                this._startGuiOnLogon = value;
                OnPropertyChanged();
            }
        }

        public bool NotifyAfterX
        {
            get => this._notifyAfterX;
            set
            {
                if (this._notifyAfterX == value) return;
                this._notifyAfterX = value;
                OnPropertyChanged();
            }
        }

        public int NotifyAfterHours
        {
            get => this._notifyAfterHours;
            set
            {
                if (this._notifyAfterHours == value) return;
                this._notifyAfterHours = value;
                OnPropertyChanged();
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
