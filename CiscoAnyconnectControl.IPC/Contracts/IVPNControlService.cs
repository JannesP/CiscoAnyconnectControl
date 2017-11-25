using System.ServiceModel;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.DTOs;

namespace CiscoAnyconnectControl.IPC.Contracts
{
    /// <summary>
    /// Contract that defines the methods the service will provide.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IVpnControlClient))]
    public interface IVpnControlService
    {
        [OperationContract]
        [ServiceKnownType(typeof(VpnStatusModelTo))]
        VpnStatusModelTo GetStatusModel();

        [OperationContract]
        [ServiceKnownType(typeof(VpnDataModelTo))]
        void Connect(VpnDataModelTo vpnData);

        [OperationContract]
        void Disconnect();

        [OperationContract]
        void SubscribeToStatusModelChanges();

        [OperationContract]
        void UnSubscribeFromStatusModelChanges();

        [OperationContract]
        Task<string[]> GetGroupsForHost(string address);

        [OperationContract]
        void UpdateStatus();
    }
}
