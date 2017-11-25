using CiscoAnyconnectControl.UI.Command;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CiscoAnyconnectControl.Model.Annotations;

namespace CiscoAnyconnectControl.UI.ViewModel
{
    class VpnDataViewModel : INotifyPropertyChanged
    {
        private string _address = "vpn.example.com";
        private string _username = "username";
        private string _password = "";
        private string _group = null;

        public VpnDataViewModel()
        {
            this.Model = VpnDataFile.Instance.VpnDataModel;
            this.Address = this.Model.Address;
            this.Username = this.Model.Username;
            this.Password = this.Model.Password;
            this.Group = this.Model.Group;
            this.Model.PropertyChanged += Model_PropertyChanged;
            
            SetupCommands();
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.GetType().GetProperty(e.PropertyName)?.SetValue(this, sender.GetType().GetProperty(e.PropertyName)?.GetValue(sender));
        }

        private VpnDataModel Model { get; set; }

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

        public SecureString SecurePassword
        {
            set
            {
                string pwd = "";
                if (value != null)
                {
                    using (SecureString securePassword = value)
                    {
                        IntPtr bstr = Marshal.SecureStringToBSTR(securePassword);
                        try
                        {
                            pwd = Marshal.PtrToStringBSTR(bstr);
                        }
                        finally
                        {
                            Marshal.ZeroFreeBSTR(bstr);
                        }
                    }
                }
                this.Password = pwd;
            }
        }
        
        public RelayCommand SaveToModel { get; private set; }

        public RelayCommand RemoveGroup { get; private set; }

        public bool IsRemoveProfileButtonEnabled => this.Group != null;

        private void SetupCommands()
        {
            this.SaveToModel = new RelayCommand(this.DataChanged, () => {
                this.Model.Address = this.Address;
                if (SettingsFile.Instance.SettingsModel.SavePassword)
                    this.Model.Password = this.Password;
                this.Model.Username = this.Username;
                this.Model.Group = this.Group;
            });
            this.RemoveGroup = new RelayCommand(() => this.IsRemoveProfileButtonEnabled, () => {
                this.Group = null;
                this.Model.Group = null;
            });
        }

        private bool DataChanged()
        {
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
