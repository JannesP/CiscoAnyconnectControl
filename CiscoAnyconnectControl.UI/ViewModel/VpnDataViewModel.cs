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
        private int _groupId = 0;

        public VpnDataViewModel()
        {
            Model = VpnDataFile.Instance.VpnDataModel;
            Address = Model.Address;
            Username = Model.Username;
            Password = Model.Password;
            Group = Model.Group;
            GroupId = Model.GroupId;
            Model.PropertyChanged += Model_PropertyChanged;
            
            SetupCommands();
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GetType().GetProperty(e.PropertyName)?.SetValue(this, sender.GetType().GetProperty(e.PropertyName)?.GetValue(sender));
        }

        private VpnDataModel Model { get; set; }

        public string Address
        {
            get => _address;
            set
            {
                if (_address == value) return;
                _address = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username == value) return;
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public string Group
        {
            get => $"({_groupId}) {_group}";
            set
            {
                if (_group == value) return;
                _group = value;
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
                OnPropertyChanged(nameof(Group));
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
                Password = pwd;
            }
        }
        
        public RelayCommand SaveToModel { get; private set; }

        public RelayCommand RemoveGroup { get; private set; }

        public bool IsRemoveProfileButtonEnabled => this.Group != null;

        private void SetupCommands()
        {
            SaveToModel = new RelayCommand(DataChanged, () => {
                Model.Address = Address;
                if (SettingsFile.Instance.SettingsModel.SavePassword)
                    Model.Password = _password;
                Model.Username = _username;
                Model.Group = _group;
                Model.GroupId = _groupId;
            });
            RemoveGroup = new RelayCommand(() => IsRemoveProfileButtonEnabled, () => {
                Group = null;
                Model.GroupId = 0;
                Model.Group = null;
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
