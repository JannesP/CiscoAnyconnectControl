using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CiscoAnyconnectControl.Command;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyconnectControl.ViewModel
{
    class VpnStatusViewModel
    {
        public VpnStatusViewModel()
        {
            this.CurrStatus = new VpnStatusModel();
            SetupCommands();
        }

        public VpnStatusModel CurrStatus { get; set; }

        public string Color
        {
            get
            {
                var color = "";
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
                string text = null;

                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Disconnected:
                        text = this.CurrStatus.Error == null ? "Disconnected." : $"Error: {this.CurrStatus.Error}";
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
                    default:
                        text = "Error ...";
                        break;
                }

                return text;
            }
        }

        public string ActionButtonText
        {
            get
            {
                string text = null;

                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Disconnected:
                        text = "Connect";
                        break;
                    case VpnStatusModel.VpnStatus.Connecting:
                        text = "Connecting ...";
                        break;
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
                var enabled = false;
                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Disconnected:
                    case VpnStatusModel.VpnStatus.Connected:
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

        private bool CanExecuteAction()
        {
            return this.CurrStatus.Status == VpnStatusModel.VpnStatus.Connected ||
                   this.CurrStatus.Status == VpnStatusModel.VpnStatus.Disconnected;
        }

        private void SetupCommands()
        {
            this.CommandConnectVpn = new RelayCommand(this.CanExecuteAction,
            () =>
            {
                Console.WriteLine("Connect VPN.");      
            });
            this.CommandDisconnectVpn = new RelayCommand(this.CanExecuteAction,
            () =>
            {
                Console.WriteLine("Disconnect VPN.");
            });
        }

        public ICommand CurrentActionCommand
        {
            get
            {
                ICommand command = null;
                switch (this.CurrStatus.Status)
                {
                    case VpnStatusModel.VpnStatus.Connected:
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

        private ICommand CommandConnectVpn { get; set; }
        private ICommand CommandDisconnectVpn { get; set; }
    }
}
