using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CiscoAnyconnectControl.CiscoCliWrapper;
using CiscoAnyconnectControl.Command;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.DAL;

namespace CiscoAnyconnectControl.ViewModel
{
    class VpnStatusViewModel : IDisposable
    {
        private CliWrapper _ciscoCli; 
        public VpnStatusViewModel()
        {
            this.CurrStatus = new VpnStatusModel();
            SetupCommands();
            SetupCli();
        }

        private void SetupCli()
        {
            this._ciscoCli = new CliWrapper(SettingsFile.Instance.SettingsModel.CiscoCliPath);
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

        public string Text
        {
            get
            {
                string text;
                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Disconnected:
                        text = this.CurrStatus.Message ?? "Disconnected.";
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
            () =>
            {
                VpnDataFile.Instance.Save();
                Console.WriteLine("Connect VPN.");      
            });
            this.CommandDisconnectVpn = new RelayCommand(this.CanExecuteAction,
            () =>
            {
                Console.WriteLine("Disconnect VPN.");
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
            this._ciscoCli.UpdateStats();
        }
    }
}
