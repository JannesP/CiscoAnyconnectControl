using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.IPC.DTOs;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyConnectControl.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class VpnControlService : IVpnControlService
    {
        public VpnControlService()
        {
            var ins = ServiceManager.Instance;
        }

        public VpnStatusModelTo GetStatusModel()
        {
            return VpnStatusModelTo.FromModel(ServiceManager.Instance.StatusModel);
        }

        public void Connect(VpnDataModelTo vpnData)
        {
            try
            {
                ServiceManager.Instance.Connect(vpnData.ToModel());
            } catch (Exception ex) { HandleException(ex); }
        }

        public void Disconnect()
        {
            try
            {
                ServiceManager.Instance.Disconnect();
            }
            catch (Exception ex) { HandleException(ex); }
        }

        public void SubscribeToStatusModelChanges()
        {
            OperationContext.Current.Channel.Closing += Channel_Closing;
            OperationContext.Current.Channel.Closed += Channel_Closed;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;
            try
            {
                var channel = OperationContext.Current.GetCallbackChannel<IVpnControlClient>();
                if (channel != null)
                {
                    ServiceManager.Instance.SubscribeToStatusModelChanges(channel);
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    Trace.TraceInformation("Added channel {0} to subscriber list.", ((IServiceChannel)channel).SessionId);
                }
                else Trace.TraceInformation("VpnControlService Channel that should've subscribed was null.");
            }
            catch (Exception ex) { HandleException(ex); }
        }

        private void Channel_Closing(object sender, EventArgs e)
        {
            Trace.TraceInformation("Sender {0} closing ...", ((IServiceChannel)sender).SessionId);
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            Trace.TraceWarning("Sender {0} faulted.", ((IServiceChannel)sender).SessionId);
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Trace.TraceInformation("Sender {0} closed.", ((IServiceChannel)sender).SessionId);
            ServiceManager.Instance.UnSubscribeFromStatusModelChanges((IVpnControlClient)sender);
        }

        public void UnSubscribeFromStatusModelChanges()
        {
            try
            {
                var channel = OperationContext.Current.GetCallbackChannel<IVpnControlClient>();
                if (channel != null) ServiceManager.Instance.UnSubscribeFromStatusModelChanges(channel);
                else Trace.TraceInformation("VpnControlService Channel that should've been unsubscribed was null.");
            }
            catch (Exception ex) { HandleException(ex); }
        }

        public async Task<string[]> GetGroupsForHost(string address)
        {
            try
            {
                return (await ServiceManager.Instance.GetGroupsForHost(address)).ToArray();
            }
            catch (Exception ex) { HandleException(ex); }
            return null;
        }

        public void UpdateStatus()
        {
            try
            {
                ServiceManager.Instance.UpdateStatus();
            }
            catch (Exception ex) { HandleException(ex); }
        }

        public void Pong()
        {
            try
            {
                var channel = OperationContext.Current.GetCallbackChannel<IVpnControlClient>();
                if (channel != null) ServiceManager.Instance.Pong(channel);
                else Trace.TraceInformation("VpnControlService Channel that should've been unsubscribed was null.");
            }
            catch (Exception ex) { HandleException(ex); }
            
        }

        private void HandleException(Exception ex)
        {
            Util.TraceException("Error in Service:", ex);
        }
    }
}
