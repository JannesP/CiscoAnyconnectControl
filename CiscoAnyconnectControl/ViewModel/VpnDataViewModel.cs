using CiscoAnyconnectControl.Command;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CiscoAnyconnectControl.ViewModel
{
    class VpnDataViewModel
    {
        public VpnDataViewModel()
        {
            try
            {
                Model = VpnDataFile.Instance.Load("");
                Console.WriteLine("VpnDataViewModel: Model loaded.");
            }
            catch (Exception e)
            {
                Console.WriteLine("VpnDataViewModel: Model failed to load.");
                Console.WriteLine(e.GetType().ToString());
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Model = new VpnDataModel();
            }
            SetupCommands();
        }

        private VpnDataModel Model;

        public string Address { get; set; } = "vpn.example.com";
        public string Username { get; set; } = "username";
        public SecureString SecurePassword { get; set; }
        public string Password
        {
            [SecurityCritical]
            get
            {
                if (this.SecurePassword == null) return "";
                using (SecureString securePassword = this.SecurePassword)
                {
                    IntPtr bstr = Marshal.SecureStringToBSTR(securePassword);
                    try
                    {
                        return Marshal.PtrToStringBSTR(bstr);
                    }
                    finally
                    {
                        Marshal.ZeroFreeBSTR(bstr);
                    }
                }
            }
        }
        public RelayCommand SaveToModel { get; private set; }

        private void SetupCommands()
        {
            SaveToModel = new RelayCommand(this.DataChanged, () => {
                Model.Address = this.Address;
                Model.SecurePassword = SecurePassword;
                Model.Username = Username;
            });
        }

        private bool DataChanged()
        {
            return true;
        }
    }

}
