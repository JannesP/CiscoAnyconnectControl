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
        private DateTime _timeConnectedLastValueSetAt = DateTime.MinValue;
        private TimeSpan? _timeConnectedLastValue = null;

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
                OnPropertyChanged();
            }
        }
        
        [CanBeNull]
        public TimeSpan? TimeConnected {
            get
            {
                if (this.Status == VpnStatus.Connected || this.Status == VpnStatus.Disconnecting)
                {
                    if (this._timeConnectedLastValue == null) return null;
                    return this._timeConnectedLastValue + (DateTime.Now - this._timeConnectedLastValueSetAt);
                } else
                {
                    return TimeSpan.MinValue;
                }
            }
            set
            {
                this._timeConnectedLastValueSetAt = DateTime.Now;
                this._timeConnectedLastValue = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return this._message; }
            set
            {
                this._message = value;
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
