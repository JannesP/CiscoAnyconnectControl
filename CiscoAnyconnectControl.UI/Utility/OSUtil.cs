using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.UI.Utility
{
    abstract class OSUtil : I_OSUtil
    {
        private static Lazy<OSUtil> _instance = new Lazy<OSUtil>(() =>
        {
            Trace.TraceInformation("Environment.OSVersion: {0}", Environment.OSVersion);
            return new WindowsOSUtil();
        });

        public static OSUtil Instance => _instance.Value;

        public abstract bool AddUiToSystemStart();
        public abstract bool RemoveUiFromSystemStart();
        public abstract void ShowTrayIcon();
        public abstract void HideTrayIcon();
        public abstract void DisableCiscoAutostart();
        public abstract void RestoreCiscoAutostart();
        public abstract bool IsCiscoAutostartEnabled();

        public event EventHandler TrayIconDoubleClick;

        protected virtual void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            TrayIconDoubleClick?.Invoke(sender, e);
        }

        public event EventHandler TrayExitClick;

        protected virtual void OnTrayExitClick(object sender, EventArgs e)
        {
            TrayExitClick?.Invoke(sender, e);
        }
    }
}
