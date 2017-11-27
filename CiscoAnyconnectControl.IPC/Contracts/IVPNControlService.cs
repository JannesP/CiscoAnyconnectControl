using System.ServiceModel;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.DTOs;

namespace CiscoAnyconnectControl.IPC.Contracts
{
    /// <summary>
    /// Contract that defines the methods the service will provide.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IVpnControlClient))]
    public interface IVpnControlService
    {
        [OperationContract]
        [ServiceKnownType(typeof(VpnStatusModelTo))]
        VpnStatusModelTo GetStatusModel();

        [OperationContract]
        [ServiceKnownType(typeof(VpnDataModelTo))]
        void Connect(VpnDataModelTo vpnData);

        [OperationContract(IsOneWay = true)]
        void Disconnect();

        [OperationContract(IsOneWay = true)]
        void SubscribeToStatusModelChanges();

        [OperationContract(IsOneWay = true)]
        void UnSubscribeFromStatusModelChanges();

        [OperationContract]
        Task<string[]> GetGroupsForHost(string address);

        [OperationContract(IsOneWay = true)]
        void UpdateStatus();

        [OperationContract(IsOneWay = true)]
        void Pong();
    }
}
