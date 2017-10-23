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
    public class VpnStatusModel : INotifyPropertyChanged
    {
        private VpnStatus _status = VpnStatus.Disconnected;
        private string _message = null;

        public enum VpnStatus
        {
            Disconnected, Connecting, Connected, Disconnecting, Reconnecting
        }

        public VpnStatus Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                OnPropertyChanged(nameof(this.Status));
            }
        }

        public string Message
        {
            get { return this._message; }
            set
            {
                this._message = value;
                OnPropertyChanged(nameof(this.Message));
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
