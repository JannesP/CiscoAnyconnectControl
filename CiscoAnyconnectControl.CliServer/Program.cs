using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.CliServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(string)))
            {
                // Open the host and start listening for incoming messages.
                host.Open();
                // Keep the service running until the Enter key is pressed.
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press the Enter key to terminate service.");
                Console.ReadLine();
            }
        }
    }
}
