using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Utility;
using Microsoft.Win32;

namespace CiscoAnyconnectControl.UI.Utility
{
    class WindowsOSUtil : OSUtil
    {
        public override bool AddUiToSystemStart()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                if (key != null)
                {
                    key.SetValue("CiscoAnyconnectControl", Assembly.GetExecutingAssembly().CodeBase);
                    return true;
                }
                else
                {
                    Trace.TraceError("Autostart key couldn't be opened!");
                }
            }
            catch (SecurityException ex)
            {
                Util.TraceException("SecuriyException in AddUiToSystemStart:", ex);
            }
            return false;
        }

        public override bool RemoveUiFromSystemStart()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                if (key != null)
                {
                    key.DeleteValue("CiscoAnyconnectControl");
                    return true;
                }
                else
                {
                    Trace.TraceError("Autostart key couldn't be opened!");
                }
            }
            catch (SecurityException ex)
            {
                Util.TraceException("SecuriyException in RemoveUiFromSystemStart:", ex);
            }
            return false;
        }
        
    }
}
