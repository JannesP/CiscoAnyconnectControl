using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
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
            OperationContext.Current.Channel.Closed += Channel_Closed;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;
            try
            {
                ServiceManager.Instance.SubscribeToStatusModelChanges(OperationContext.Current.GetCallbackChannel<IVpnControlClient>());
            }
            catch (Exception ex) { HandleException(ex); }
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            UnSubscribeFromStatusModelChanges();
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            UnSubscribeFromStatusModelChanges();
        }

        public void UnSubscribeFromStatusModelChanges()
        {
            try
            {
                ServiceManager.Instance.UnSubscribeFromStatusModelChanges(OperationContext.Current.GetCallbackChannel<IVpnControlClient>());
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

        private void HandleException(Exception ex)
        {
            Util.TraceException("Error in Service:", ex);
        }
    }
}
