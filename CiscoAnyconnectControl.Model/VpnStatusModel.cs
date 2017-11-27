using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
            get => this._status;
            set
            {
                if (this._status == value) return;
                this._status = value;
                switch (value)
                {
                    case VpnStatus.Disconnected:
                        this.Message = $"{value.ToString()}.";
                        this.TimeConnected = null;
                        break;
                }
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
                }
                else
                {
                    return null;
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
            get => this._message;
            set
            {
                if (this._message == value) return;
                this._message = value;
                OnPropertyChanged();
            }
        }

        public void CopyTo(VpnStatusModel target)
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                this.GetType().GetProperty(prop.Name)?.SetValue(target, prop.GetValue(this));
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
