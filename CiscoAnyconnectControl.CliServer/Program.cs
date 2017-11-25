using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.Utility;
using CiscoAnyConnectControl.Service;

namespace CiscoAnyconnectControl.CliServer
{
    class Program
    {
        /*private static string BaseAddress = "http://localhost:18375/AnyconnectControlServer";
        private static string Address = "net.pipe://localhost/AnyconnectControlServer";*/
        static void Main(string[] args)
        {
            //redirect to standard output for now.
            Trace.Listeners.Add(new ConsoleTraceListener());
            bool run = true;
            while (run)
            {
                ServiceModelSectionGroup group = ServiceModelSectionGroup.GetSectionGroup(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
                if (group == null)
                {
                    Console.WriteLine("Group is null");
                    Console.ReadLine();
                    return;
                }
                ServiceElement service = group.Services.Services[0];
                string baseAddress = service.Endpoints[0].Address.AbsoluteUri.Replace(service.Endpoints[0].Address.AbsolutePath, String.Empty);
                try
                {
                    using (var host = new ServiceHost(typeof(VpnControlService), new Uri(baseAddress)))
                    {
                        host.AddServiceEndpoint(typeof(IVpnControlService), new NetNamedPipeBinding(), service.Endpoints[0].Address.AbsolutePath);
                        host.Closed += Host_Closed;
                        host.Closing += Host_Closing;
                        host.Faulted += Host_Faulted;
                        host.Opened += Host_Opened;
                        host.Opening += Host_Opening;
                        // Open the host and start listening for incoming messages.
                        host.Open();
                        // Keep the service running until the Enter key is pressed.
                        foreach (var chDisp in host.ChannelDispatchers)
                        {
                            Console.WriteLine(chDisp.Listener?.Uri);
                        }
                        PrintBindingInfo(host);
                        Console.WriteLine("The service is ready.");
                        while (run)
                        {
                            Console.WriteLine("Press Escape to terminate service.");
                            ConsoleKeyInfo ki = Console.ReadKey();
                            if (ki.Key == ConsoleKey.Escape) run = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.TraceException("Error in service host:", e);
                    Console.WriteLine("The process errored. Hit Esc to kill or any other key to restart.");
                    ConsoleKeyInfo ki = Console.ReadKey();
                    if (ki.Key == ConsoleKey.Escape) run = false;
                }
                
            }
        }

        private static void PrintBindingInfo(ServiceHost sh)
        {
            // Iterate through the endpoints contained in the ServiceDescription 
            System.Text.StringBuilder sb = new System.Text.StringBuilder(string.Format("Active Service Endpoints:{0}", Environment.NewLine), 128);
            foreach (ServiceEndpoint se in sh.Description.Endpoints)
            {
                sb.Append(String.Format("Endpoint:{0}", Environment.NewLine));
                sb.Append(String.Format("\tAddress: {0}{1}", se.Address, Environment.NewLine));
                sb.Append(String.Format("\tBinding: {0}{1}", se.Binding, Environment.NewLine));
                sb.Append(String.Format("\tContract: {0}{1}", se.Contract.Name, Environment.NewLine));
                foreach (IEndpointBehavior behavior in se.Behaviors)
                {
                    sb.Append(String.Format("Behavior: {0}{1}", behavior, Environment.NewLine));
                }
            }

            Console.WriteLine(sb.ToString());
        }

        private static void Host_Opening(object sender, EventArgs e)
        {
            Console.WriteLine("Host_Opening");
        }

        private static void Host_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Host_Opened");
        }

        private static void Host_Faulted(object sender, EventArgs e)
        {
            Console.WriteLine("Host_Faulted");
        }

        private static void Host_Closing(object sender, EventArgs e)
        {
            Console.WriteLine("Host_Closing");
        }

        private static void Host_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Host_Closed");
        }
    }
}
