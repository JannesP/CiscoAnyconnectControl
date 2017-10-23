using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.Model
{
    [Serializable]
    class VpnDataModel
    {
        public string Address { get; set; } = "vpn.example.com";

        public string Username { get; set; } = "username";

        public SecureString SecurePassword { [SecurityCritical]get; [SecurityCritical]set; }
    }
}
