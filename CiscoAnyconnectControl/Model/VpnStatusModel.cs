using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.Model
{
    class VpnStatusModel
    {
        public enum VpnStatus
        {
            Disconnected, Connecting, Connected, Disconnecting
        }

        public VpnStatus Status { get; set; } = VpnStatus.Disconnected;

        public string Error { get; set; } = null;

    }
}
