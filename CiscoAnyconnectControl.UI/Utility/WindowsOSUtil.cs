using System;
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
        private NotifyIcon _trayIcon;

        public WindowsOSUtil() { }

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
