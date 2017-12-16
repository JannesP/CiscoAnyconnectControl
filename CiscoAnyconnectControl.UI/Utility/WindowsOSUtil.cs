using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CiscoAnyconnectControl.Utility;
using Microsoft.Win32;

namespace CiscoAnyconnectControl.UI.Utility
{
    class WindowsOSUtil : OSUtil
    {
        private const string AutostartKeyName = "CiscoAnyconnectControl";
        private string CiscoAutostartKeyName => "Cisco AnyConnect Secure Mobility Agent for Windows";
        private string RegistryAutostartKey => @"Software\Microsoft\Windows\CurrentVersion\Run";
        private string RegistryDisabledAutostartKey => "AutorunsDisabled";

        private NotifyIcon _trayIcon;

        public WindowsOSUtil() { }

        public override bool AddUiToSystemStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryAutostartKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue(AutostartKeyName, $"\"{Util.FullAssemblyPath}\" -tray -unsafeRunAsUser");
                        return true;
                    }
                    else
                    {
                        Trace.TraceError("Autostart key couldn't be opened!");
                    }
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
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryAutostartKey, true))
                {
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
            }
            catch (SecurityException ex)
            {
                Util.TraceException("SecuriyException in RemoveUiFromSystemStart:", ex);
            }
            return false;
        }

        public override void ShowTrayIcon()
        {
            if (_trayIcon == null)
            {
                _trayIcon = CreateTrayIcon();
            }
            _trayIcon.Visible = true;
        }

        private void _trayIcon_DoubleClick(object sender, EventArgs e)
        {
            OnTrayIconDoubleClick(sender, e);
        }

        public override void HideTrayIcon()
        {
            if (_trayIcon != null) _trayIcon.Visible = false;
        }

        public override void DisableCiscoAutostart()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryAutostartKey, true))
                {
                    if (key != null)
                    {
                        object ciscoAutostartValue = key.GetValue(CiscoAutostartKeyName);
                        if (ciscoAutostartValue != null)
                        {
                            using (RegistryKey backupKey = key.CreateSubKey(RegistryDisabledAutostartKey, true))
                            {
                                backupKey.SetValue(CiscoAutostartKeyName, ciscoAutostartValue);
                            }
                            key.DeleteValue(CiscoAutostartKeyName);
                        }
                    }
                    else
                    {
                        Trace.TraceError("Autostart key couldn't be opened!");
                    }
                }
            }
            catch (SecurityException ex)
            {
                Util.TraceException("SecuriyException in DisableCiscoAutostart:", ex);
            }
        }

        public override void RestoreCiscoAutostart()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryAutostartKey, true))
                {
                    if (key != null)
                    {
                        using (RegistryKey backupKey = key.OpenSubKey(RegistryDisabledAutostartKey, true))
                        {
                            if (backupKey != null)
                            {
                                object backupVal = backupKey.GetValue(CiscoAutostartKeyName);
                                if (backupVal != null)
                                {
                                    key.SetValue(CiscoAutostartKeyName, backupVal);
                                    backupKey.DeleteValue(CiscoAutostartKeyName);
                                }
                                else Trace.TraceError("Cannot find backup key. Nothing to restore from.");
                            }
                            else
                            {
                                Trace.TraceError("Backup key couldn't be opened!");
                            }
                        }
                    }
                    else
                    {
                        Trace.TraceError("Autostart key couldn't be opened!");
                    }
                }
            }
            catch (SecurityException ex)
            {
                Util.TraceException("SecuriyException in DisableCiscoAutostart:", ex);
            }
        }

        public override bool IsCiscoAutostartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryAutostartKey, true))
                {
                    if (key != null)
                    {
                        return key.GetValue(CiscoAutostartKeyName) != null;
                    }
                    else
                    {
                        Trace.TraceError("Autostart key couldn't be opened!");
                        return false;
                    }
                }
            }
            catch (SecurityException ex)
            {
                Util.TraceException("SecuriyException in DisableCiscoAutostart:", ex);
            }
            throw new Exception("Windows:IsCiscoAutostartEnabled ran to end without reaching a return.");
        }

        public override bool IsElevatedProcess()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true))
                {
                    return true;
                }
            }
            catch (SecurityException ex)
            {
                Util.TraceException("SecuriyException in DisableCiscoAutostart:", ex);
                return false;
            }
        }

        private NotifyIcon CreateTrayIcon()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.DoubleClick += _trayIcon_DoubleClick;
            try
            {
                _trayIcon.Icon = new Icon(Assembly.GetExecutingAssembly()
                                              .GetManifestResourceStream(
                                                  "CiscoAnyconnectControl.UI.Resources.trayIcon.ico") ??
                                          throw new InvalidOperationException());
            }
            catch (Exception ex)
            {
                Util.TraceException("Error loading trayIcon:", ex);
            }
            var menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("Exit", OnTrayExitClick));
            _trayIcon.ContextMenu = menu;
            return _trayIcon;
        }
    }
}
