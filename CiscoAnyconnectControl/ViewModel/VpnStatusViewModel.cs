using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using CiscoAnyconnectControl.Command;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Utility;
using CiscoAnyconnectControl.Model.Annotations;
using CiscoAnyconnectControl.Model.DAL;
using CiscoAnyconnectControl.CiscoCliHelper;
using CiscoAnyconnectControl.View;

namespace CiscoAnyconnectControl.ViewModel
{
    class VpnStatusViewModel : IDisposable, INotifyPropertyChanged
    {
        private CiscoCli _ciscoCli;
        private DispatcherTimer _timeChangedTimer;
        public VpnStatusViewModel()
        {
            SetupCommands();
            SetupCli();
            this.CurrStatus = this._ciscoCli.VpnStatusModel;
            this.CurrStatus.PropertyChanged += CurrStatus_PropertyChanged;
            this._timeChangedTimer = new DispatcherTimer
            {
                IsEnabled = false,
                Interval = TimeSpan.FromSeconds(1)
            };
            this._timeChangedTimer.Tick += _timeChangedTimer_Tick;
        }

        private void _timeChangedTimer_Tick(object sender, EventArgs e)
        {
            if (this.CurrStatus.Status == VpnStatusModel.VpnStatus.Connected ||
                this.CurrStatus.Status == VpnStatusModel.VpnStatus.Disconnecting)
            {
                if (this.CurrStatus.TimeConnected == null) this._ciscoCli.UpdateStatus();
                OnPropertyChanged(nameof(this.TimeConnected));
            }
        }
        
        public string TimeConnected
        {
            get
            {
                if (this.CurrStatus.Status == VpnStatusModel.VpnStatus.Connected ||
                    this.CurrStatus.Status == VpnStatusModel.VpnStatus.Disconnecting)
                {
                    return this.CurrStatus.TimeConnected == null ? "loading ..." : $"({this.CurrStatus.TimeConnected:h\\:mm\\:ss})";
                }
                return "";
            }
        }

        private void CurrStatus_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.Status):
                    OnPropertyChanged(nameof(this.Color));
                    OnPropertyChanged(nameof(this.Status));
                    OnPropertyChanged(nameof(this.ActionButtonText));
                    OnPropertyChanged(nameof(this.ActionButtonEnabled));
                    OnPropertyChanged(nameof(this.TimeConnected));
                    if (this.CurrStatus.Status == VpnStatusModel.VpnStatus.Connected ||
                        this.CurrStatus.Status == VpnStatusModel.VpnStatus.Disconnecting)
                    {
                        this._timeChangedTimer.Start();
                    }
                    else
                    {
                        this._timeChangedTimer.Stop();
                    }
                    
                    break;
                case nameof(this.Message):
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }

        private void SetupCli()
        {
            this._ciscoCli = new CiscoCli(SettingsFile.Instance.SettingsModel.CiscoCliPath);
        }

        public VpnStatusModel CurrStatus { get; set; }

        public string Color
        {
            get
            {
                string color;
                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Disconnected:
                        color = "red";
                        break;
                    case VpnStatusModel.VpnStatus.Connected:
                        color = "green";
                        break;
                    case VpnStatusModel.VpnStatus.Disconnecting:
                    case VpnStatusModel.VpnStatus.Connecting:
                    case VpnStatusModel.VpnStatus.Reconnecting:
                        color = "yellow";
                        break;
                    default:
                        color = "orange";
                        break;
                }
                return color;
            }
        }

        public string Status
        {
            get
            {
                string text;
                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Disconnected:
                        text = "Disconnected.";
                        break;
                    case VpnStatusModel.VpnStatus.Connecting:
                        text = "Connecting ...";
                        break;
                    case VpnStatusModel.VpnStatus.Connected:
                        text = "Connected.";
                        break;
                    case VpnStatusModel.VpnStatus.Disconnecting:
                        text = "Disconnecting ...";
                        break;
                    case VpnStatusModel.VpnStatus.Reconnecting:
                        text = "Reconnecting ...";
                        break;
                    default:
                        text = $"Error ... {this.CurrStatus.Status} is not defined.";
                        break;
                }

                return text;
            }
        }

        public string Message => this.CurrStatus.Message ?? "No messages yet :(";

        public string ActionButtonText
        {
            get
            {
                string text;
                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Disconnected:
                        text = "Connect";
                        break;
                    case VpnStatusModel.VpnStatus.Connecting:
                        text = "Connecting ...";
                        break;
                    case VpnStatusModel.VpnStatus.Reconnecting:
                    case VpnStatusModel.VpnStatus.Connected:
                        text = "Disconnect";
                        break;
                    case VpnStatusModel.VpnStatus.Disconnecting:
                        text = "Disconnecting ...";
                        break;
                    default:
                        text = "Error ...";
                        break;
                }

                return text;
            }
        }

        public bool ActionButtonEnabled
        {
            get
            {
                bool enabled;
                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Disconnected:
                    case VpnStatusModel.VpnStatus.Connected:
                    case VpnStatusModel.VpnStatus.Reconnecting:
                        enabled = true;
                        break;
                    case VpnStatusModel.VpnStatus.Disconnecting:
                    case VpnStatusModel.VpnStatus.Connecting:
                        enabled = false;
                        break;
                    default:
                        enabled = false;
                        break;
                }
                return enabled;
            }
        }

        private RelayCommand CommandConnectVpn { get; set; }
        private RelayCommand CommandDisconnectVpn { get; set; }

        private bool CanExecuteAction()
        {
            return this.CurrStatus.Status == VpnStatusModel.VpnStatus.Connected ||
                   this.CurrStatus.Status == VpnStatusModel.VpnStatus.Disconnected ||
                   this.CurrStatus.Status == VpnStatusModel.VpnStatus.Reconnecting;
        }

        private void SetupCommands()
        {
            this.CommandConnectVpn = new RelayCommand(this.CanExecuteAction,
            async () =>
            {
                //TODO test this command
                if (VpnDataFile.Instance.VpnDataModel.Group == null)
                {
                    try
                    {
                        IEnumerable<string> groups = await this._ciscoCli.LoadGroups(VpnDataFile.Instance.VpnDataModel.Address);
                        var selectBox = new SelectGroupModalWindow(groups);
                        bool? dr = selectBox.ShowDialog();
                        if (dr == true)
                        {
                            VpnDataFile.Instance.VpnDataModel.Group = selectBox.SelectedGroup;
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.TraceException($"Error finding groups for host {VpnDataFile.Instance.VpnDataModel.Address}:", ex);
                    }
                }
                VpnDataFile.Instance.Save();
                this._ciscoCli.Connect(VpnDataFile.Instance.VpnDataModel.Address, VpnDataFile.Instance.VpnDataModel.Username, VpnDataFile.Instance.VpnDataModel.Password, VpnDataFile.Instance.VpnDataModel.Group);
            });
            this.CommandDisconnectVpn = new RelayCommand(this.CanExecuteAction,
            () =>
            {
                this._ciscoCli.Disconnect();
            });
        }

        public RelayCommand CurrentActionCommand
        {
            get
            {
                RelayCommand command = null;
                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Connected:
                    case VpnStatusModel.VpnStatus.Reconnecting:
                        command = this.CommandDisconnectVpn;
                        break;
                        case VpnStatusModel.VpnStatus.Disconnected:
                        command = this.CommandConnectVpn;
                        break;
                    default:
                        command = RelayCommand.Empty;
                        break;
                } 
                return command;
            }
        }

        public void Dispose()
        {
            this._ciscoCli?.Dispose();
        }

        public void RefreshVpnStatus()
        {
            this._ciscoCli.UpdateStatus();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
