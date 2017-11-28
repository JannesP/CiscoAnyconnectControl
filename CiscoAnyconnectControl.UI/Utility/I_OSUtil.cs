using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.UI.Utility
{
    interface I_OSUtil
    {
        bool AddUiToSystemStart();
        bool RemoveUiFromSystemStart();
        void AddTrayIcon();
        void RemoveTrayIcon();
        
        event EventHandler TrayIconDoubleClick;
    }
}
