using CiscoAnyconnectControl.Command;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CiscoAnyconnectControl.ViewModel
{
    class VpnDataViewModel
    {
        private string _password = "";

        public VpnDataViewModel()
        {
            this.Model = VpnDataFile.Instance.VpnDataModel;
            this.Address = this.Model.Address;
            this.Username = this.Model.Username;
            this.Password = this.Model.Password;
            this.Model.PropertyChanged += Model_PropertyChanged;
            
            SetupCommands();
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PropertyInfo propertyInfo = sender.GetType().GetProperty(e.PropertyName);
            if (propertyInfo != null)
            {
                if (e.PropertyName == nameof(this.Profile)) return;
                PropertyInfo memberInfo = this.GetType().GetProperty(e.PropertyName);
                if (memberInfo != null)
                    memberInfo.SetValue(this, propertyInfo.GetValue(sender));
            }
        }

        private VpnDataModel Model { get; set; }

        public string Address { get; set; } = "vpn.example.com";
        public string Username { get; set; } = "username";

        public string Password
        {
            get { return this._password; }
            set
            {
                this._password = value;
                if (!SettingsFile.Instance.SettingsModel.SavePassword)
                {
                    this.Model.Password = "";
                }
            }
        }

        public string Profile
        {
            get { return this.Model.Group; }
            set { this.Model.Group = value; }
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

        public RelayCommand RemoveProfile { get; private set; }

        public bool IsRemoveProfileButtonEnabled => this.Profile != "";

        private void SetupCommands()
        {
            this.SaveToModel = new RelayCommand(this.DataChanged, () => {
                this.Model.Address = this.Address;
                if (SettingsFile.Instance.SettingsModel.SavePassword)
                    this.Model.Password = this.Password;
                this.Model.Username = this.Username;
            });
            this.RemoveProfile = new RelayCommand(() => this.IsRemoveProfileButtonEnabled, () => {
                this.Profile = "";
            });
        }

        private bool DataChanged()
        {
            return true;
        }
    }

}
