using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.ViewModel
{
    class VpnDataViewModel
    {
        public string Address { get; set; } = "vpn.example.com";

        public string Username { get; set; } = "username";

        public SecureString SecurePassword { [SecurityCritical]get; [SecurityCritical]set; }


        private static SecureString GetSecureString(string password)
        {
            var secureString = new SecureString();

            foreach (char c in password)
            {
                secureString.AppendChar(c);
            }

            secureString.MakeReadOnly();
            return secureString;
        }

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
            [SecurityCritical] set
            {
                this.SecurePassword = GetSecureString(value);
            }
        }
    }
}
