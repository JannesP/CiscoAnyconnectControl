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
        private string _group = null;
        private int _groupId = 0;

        public string Address
        {
            get => this._address;
            set
            {
                if (this._address == value) return;
                this._address = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => this._username;
            set
            {
                if (this._username == value) return;
                this._username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => this._password;
            set
            {
                if (this._password == value) return;
                this._password = value;
                OnPropertyChanged();
            }
        }

        public string Group
        {
            get => this._group;
            set
            {
                if (this._group == value) return;
                this._group = value;
                OnPropertyChanged();
            }
        }

        public int GroupId
        {
            get => _groupId;
            set
            {
                if (_groupId == value) return;
                _groupId = value;
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
