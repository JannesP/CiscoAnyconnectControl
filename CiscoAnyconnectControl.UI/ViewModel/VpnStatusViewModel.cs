using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CiscoAnyconnectControl.UI.Command;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Utility;
using CiscoAnyconnectControl.Model.Annotations;
using CiscoAnyconnectControl.Model.DAL;
using CiscoAnyconnectControl.UI.View;

namespace CiscoAnyconnectControl.UI.ViewModel
{
    class VpnStatusViewModel : INotifyPropertyChanged, IVpnStatusViewModel
    {
        private DateTime _connectLastClicked;
        public VpnStatusViewModel()
        {
            SetupCommands();
            this.CurrStatus = VpnStatusModel.Instance;
            this.CurrStatus.PropertyChanged += CurrStatus_PropertyChanged;
            this.CurrStatus.GroupRequested += CurrStatus_GroupRequested;
            this._connectLastClicked = DateTime.MinValue;
        }

        private void CurrStatus_GroupRequested(object sender, VpnStatusModel.GroupEventArgs e)
        {
            var selectBox = new SelectGroupModalWindow(e.AvailableGroups);
            bool? dr = selectBox.ShowDialog();
            if (dr == true)
            {
                e.SelectedGroup = selectBox.SelectedGroup;
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
                case nameof(VpnStatusModel.Status):
                    OnPropertyChanged(nameof(this.Color));
                    OnPropertyChanged(nameof(this.Status));
                    OnPropertyChanged(nameof(this.ActionButtonText));
                    OnPropertyChanged(nameof(this.ActionButtonEnabled));
                    OnPropertyChanged(nameof(this.TimeConnected));
                    break;
                case nameof(VpnStatusModel.Message):
                    OnPropertyChanged(e.PropertyName);
                    OnPropertyChanged(nameof(this.ActionButtonEnabled));
                    break;
                case nameof(VpnStatusModel.TimeConnected):
                    OnPropertyChanged(nameof(this.TimeConnected));
                    break;
            }
        }

        private VpnStatusModel CurrStatus { get; set; }

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
                    case VpnStatusModel.VpnStatus.Unknown:
                        color = "purple";
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
                    case VpnStatusModel.VpnStatus.Unknown:
                        text = "Not connected to backend ...";
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
                    case VpnStatusModel.VpnStatus.Unknown:
                        text = "Not connected to backend ...";
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
                    case VpnStatusModel.VpnStatus.Paused:
                    case VpnStatusModel.VpnStatus.SsoPolling:
                    case VpnStatusModel.VpnStatus.Unknown:
                        enabled = true;
                        break;
                    case VpnStatusModel.VpnStatus.Pausing:
                    case VpnStatusModel.VpnStatus.Disconnecting:
                    case VpnStatusModel.VpnStatus.Connecting:
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
            return this.ActionButtonEnabled;
        }

        private void SetupCommands()
        {
            this.CommandConnectVpn = new RelayCommand(this.CanExecuteAction,
                () =>
                {
                    this._connectLastClicked = DateTime.Now;
                    if (VpnDataFile.Instance.VpnDataModel.Group == null)
                    {
                        VpnStatusModel.Instance.Connect(VpnDataFile.Instance.VpnDataModel);
                    }
                    VpnDataFile.Instance.Save();
                    VpnStatusModel.Instance.Connect(VpnDataFile.Instance.VpnDataModel);
                });
            this.CommandDisconnectVpn = new RelayCommand(this.CanExecuteAction,
                () =>
                {
                    VpnStatusModel.Instance.Disconnect();
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
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
