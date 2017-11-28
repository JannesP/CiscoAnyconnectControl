using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private const string AutostartKeyName = "CiscoAnyconnectControl";

        public override bool AddUiToSystemStart()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    key.SetValue(AutostartKeyName, $"\"{Util.FullAssemblyPath}\" -tray");
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
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    if (key.GetValue(AutostartKeyName) != null)
                    {
                        key.DeleteValue(AutostartKeyName);
                    }
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

        public override void AddTrayIcon()
        {
            throw new NotImplementedException();
        }

        public override void RemoveTrayIcon()
        {
            throw new NotImplementedException();
        }
    }
}
