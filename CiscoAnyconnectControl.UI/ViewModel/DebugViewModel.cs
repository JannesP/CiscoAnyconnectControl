using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CiscoAnyconnectControl.UI.Command;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyconnectControl.UI.ViewModel
{
    class DebugViewModel
    {
        public RelayCommand OpenExplorerInProcDir => new RelayCommand(() => true, () => Process.Start(Util.AssemblyDirectory));
        public RelayCommand RestartServiceCommand => new RelayCommand(() => true, () =>
        {
            IsAdministrator();
            ServiceUtil.Restart(IPC.SharedSettings.ServiceName, 10000);
        });
        public RelayCommand StopServiceCommand => new RelayCommand(() => true, () =>
        {
            IsAdministrator();
            ServiceUtil.Stop(IPC.SharedSettings.ServiceName, 10000);
        });
        public RelayCommand StartServiceCommand => new RelayCommand(() => true, () =>
        {
            IsAdministrator();
            ServiceUtil.Start(IPC.SharedSettings.ServiceName, 10000);
        });

        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdmin) MessageBox.Show("This process isnt started as admin, but is required for this command.");
            return isAdmin;
        }
    }
}
