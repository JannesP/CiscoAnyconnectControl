using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.ViewModel
{
    class VpnStatusViewModel
    {
        public string Color => "#FFBF0000";
        public string Text => "Disconnected ...";

        public string ActionButtonText => "Connect";
        public bool ActionButtonEnabled => false;
    }
}
