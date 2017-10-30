using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Model.Annotations;

namespace CiscoAnyconnectControl.Model
{
    [Serializable]
    public class VpnDataModel : INotifyPropertyChanged
    {
        private string _username = "username";
        private string _address = "vpn.example.com";
        private string _password = "";
        private string _profile = null;

        public string Address
        {
            get { return this._address; }
            set
            {
                this._address = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get { return this._username; }
            set
            {
                this._username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return this._password; }
            set
            {
                this._password = value;
                OnPropertyChanged();
            }
        }

        public string Group
        {
            get { return this._profile; }
            set
            {
                this._profile = value;
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
