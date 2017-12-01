using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.Utility;
using CiscoAnyConnectControl.Service;

namespace CiscoAnyconnectControl.WindowsServiceServer
{
    public sealed partial class CiscoAnyconnectControlServer : ServiceBase
    {
        public string DisplayName => this.ServiceName;
        private string EventLogName => ServiceName + "Log";

        private ServiceHost _serviceHost;
        private SemaphoreSlim _semaphoreStopServer;

        public CiscoAnyconnectControlServer()
        {
            InitializeComponent();
            _semaphoreStopServer = new SemaphoreSlim(1, 1);
            this.ServiceName = "CiscoAnyconnectControlServer";
            this.CanPauseAndContinue = false;
            this.CanHandlePowerEvent = true;
            this.CanStop = true;

            if (!EventLog.SourceExists(ServiceName))
            {
                EventLog.CreateEventSource(ServiceName, EventLogName);
            }
            this.EventLog.Source = this.ServiceName;
            this.EventLog.Log = this.EventLogName;
            Trace.Listeners.Add(new EventLogTraceListener(this.EventLog));
            Trace.TraceInformation("Constructor ended.");
            
        }

        private void RunWcfServiceAndWaitForExit()
        {
            ServiceModelSectionGroup group = ServiceModelSectionGroup.GetSectionGroup(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
            if (group == null)
            {
                Trace.TraceError("The configuration group is null. Not starting server. Please make sure the configuration is correct.");
                return;
            }
            ServiceElement service = group.Services.Services[0];
            string baseAddress = service.Endpoints[0].Address.AbsoluteUri.Replace(service.Endpoints[0].Address.AbsolutePath, String.Empty);
            Trace.TraceInformation("Binding name: " + service.Endpoints[0].Binding);
            try
            {
                using (var host = new ServiceHost(typeof(VpnControlService), new Uri(baseAddress)))
                {
                    foreach (ServiceEndpointElement endpoint in service.Endpoints)
                    {
                        Binding binding;
                        switch (endpoint.Binding)
                        {
                            case "netNamedPipeBinding":
                                if (!endpoint.Address.AbsoluteUri.StartsWith("net.pipe:"))
                                {
                                    throw new ConfigurationErrorsException($"The configured address {endpoint.Address.AbsoluteUri} doesn't fit the specified binding {endpoint.Binding}.");
                                }
                                binding = new NetNamedPipeBinding();
                                break;
                            default:
                                throw new ConfigurationErrorsException($"The configured binding {endpoint.Binding} is not supported.");
                        }
                        host.AddServiceEndpoint(typeof(IVpnControlService), binding, endpoint.Address.AbsolutePath);
                    }
                    host.Closed += Host_Closed;
                    host.Closing += Host_Closing;
                    host.Faulted += Host_Faulted;
                    host.Opened += Host_Opened;
                    host.Opening += Host_Opening;
                    // Open the host and start listening for incoming messages.
                    host.Open();
                    PrintBindingInfo(host);
                    Trace.TraceInformation("The service is ready.");
                    _semaphoreStopServer.Wait();

                    host.Abort();
                }
            }
            catch (Exception e)
            {
                throw new Exception("The server crashed.", e);
            }
        }

        private static void PrintBindingInfo(ServiceHost sh)
        {
            // Iterate through the endpoints contained in the ServiceDescription 
            System.Text.StringBuilder sb = new System.Text.StringBuilder(string.Format("Active Service Endpoints:{0}", Environment.NewLine), 128);
            foreach (ServiceEndpoint se in sh.Description.Endpoints)
            {
                sb.Append($"Endpoint:{Environment.NewLine}");
                sb.Append($"\tAddress: {se.Address}{Environment.NewLine}");
                sb.Append($"\tBinding: {se.Binding}{Environment.NewLine}");
                sb.Append($"\tContract: {se.Contract.Name}{Environment.NewLine}");
                foreach (IEndpointBehavior behavior in se.Behaviors)
                {
                    sb.Append($"Behavior: {behavior}{Environment.NewLine}");
                }
            }
            Trace.TraceInformation(sb.ToString());
        }

        private static void Host_Opening(object sender, EventArgs e)
        {
            Trace.TraceInformation("Host_Opening");
        }

        private static void Host_Opened(object sender, EventArgs e)
        {
            Trace.TraceInformation("Host_Opened");
        }

        private static void Host_Faulted(object sender, EventArgs e)
        {
            Trace.TraceInformation("Host_Faulted");
        }

        private static void Host_Closing(object sender, EventArgs e)
        {
            Trace.TraceInformation("Host_Closing");
        }

        private static void Host_Closed(object sender, EventArgs e)
        {
            Trace.TraceInformation("Host_Closed");
        }

        private void StopServer()
        {
            _semaphoreStopServer.Release();
        }

        private void StartServer()
        {
            _semaphoreStopServer.Wait();
            Task.Run(() => RunWcfServiceAndWaitForExit()).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    Util.TraceException("Error running server:", t.Exception?.InnerException);
                }
            });
        }

        #region ServiceLifecycle

        protected override void OnStart(string[] args)
        {
            Trace.TraceInformation("Service OnStart");
            StartServer();
        }

        protected override void OnStop()
        {
            Trace.TraceInformation("Service OnStop");
            StopServer();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.Suspend:
                    StopServer();
                    break;
                case PowerBroadcastStatus.ResumeSuspend:
                case PowerBroadcastStatus.ResumeCritical:
                case PowerBroadcastStatus.ResumeAutomatic:
                    StartServer();
                    break;
            }
            return base.OnPowerEvent(powerStatus);
        }

        #endregion
    }
}
