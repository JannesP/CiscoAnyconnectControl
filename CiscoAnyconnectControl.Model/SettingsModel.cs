using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CiscoAnyconnectControl.Model.Annotations;

namespace CiscoAnyconnectControl.Model
{
    [Serializable]
    public class SettingsModel : INotifyPropertyChanged
    {
        private bool _savePassword = true;
        private bool _connectOnSystemStartup = true;
        private bool _reconnectOnConnectionLoss = true;
        private bool _startGuiOnLogon = false;
        private bool _notifyAfterX = true;
        private int _notifyAfterHours = 9;
        private bool _closeToTray;

        public bool SavePassword
        {
            get => _savePassword;
            set
            {
                if (_savePassword == value) return;
                _savePassword = value;
                OnPropertyChanged();
            }
        }

        public bool ConnectOnSystemStartup
        {
            get => this._connectOnSystemStartup;
            set
            {
                if (_connectOnSystemStartup == value) return;
                _connectOnSystemStartup = value;
                OnPropertyChanged();
            }
        }

        public bool ReconnectOnConnectionLoss
        {
            get => _reconnectOnConnectionLoss;
            set
            {
                if (_reconnectOnConnectionLoss == value) return;
                _reconnectOnConnectionLoss = value;
                OnPropertyChanged();
            }
        }

        public bool StartGuiOnLogon
        {
            get => _startGuiOnLogon;
            set
            {
                if (_startGuiOnLogon == value) return;
                _startGuiOnLogon = value;
                OnPropertyChanged();
            }
        }

        public bool NotifyAfterX
        {
            get => _notifyAfterX;
            set
            {
                if (_notifyAfterX == value) return;
                _notifyAfterX = value;
                OnPropertyChanged();
            }
        }

        public int NotifyAfterHours
        {
            get => _notifyAfterHours;
            set
            {
                if (_notifyAfterHours == value) return;
                _notifyAfterHours = value;
                OnPropertyChanged();
            }
        }

        public bool CloseToTray
        {
            get => _closeToTray;
            set
            {
                if (_closeToTray == value) return;
                _closeToTray = value;
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
