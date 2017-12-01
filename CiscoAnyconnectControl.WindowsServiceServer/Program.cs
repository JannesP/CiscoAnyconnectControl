using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyconnectControl.WindowsServiceServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            CiscoAnyconnectControlServer service = new CiscoAnyconnectControlServer();
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-uninstall":
                        ServiceUtil.Uninstall(service.ServiceName);
                        return;
                    case "-installIfNotInstalled":
                        if (!ServiceUtil.IsInstalled(service.ServiceName))
                        {
                            ServiceUtil.InstallAndStart(service.ServiceName, service.DisplayName, Util.FullAssemblyPath);
                        }
                        return;
                }
            }
            ServiceBase.Run(service);
            
        }
    }
}
