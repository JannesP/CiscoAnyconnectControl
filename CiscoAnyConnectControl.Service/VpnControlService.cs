using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.IPC.DTOs;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyConnectControl.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class VpnControlService : IVpnControlService
    {
        public VpnStatusModelTo GetStatusModel()
        {
            return VpnStatusModelTo.FromModel(ServiceManager.Instance.StatusModel);
        }

        public void Connect(VpnDataModelTo vpnData)
        {
            ServiceManager.Instance.Connect(vpnData.ToModel());
        }

        public void Disconnect()
        {
            ServiceManager.Instance.Disconnect();
        }

        public void SubscribeToStatusModelChanges()
        {
            OperationContext.Current.Channel.Closed += Channel_Closed;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;
            ServiceManager.Instance.SubscribeToStatusModelChanges(OperationContext.Current.GetCallbackChannel<IVpnControlClient>());
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
            ServiceManager.Instance.UnSubscribeFromStatusModelChanges(OperationContext.Current.GetCallbackChannel<IVpnControlClient>());
        }

        public async Task<string[]> GetGroupsForHost(string address)
        {
            return (await ServiceManager.Instance.GetGroupsForHost(address)).ToArray();
        }

        public void UpdateStatus()
        {
            ServiceManager.Instance.UpdateStatus();
        }
    }
}
