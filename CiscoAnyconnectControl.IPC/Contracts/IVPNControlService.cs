using System.ServiceModel;
using CiscoAnyconnectControl.IPC.DTOs;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyconnectControl.IPC.Contracts
{
    /// <summary>
    /// Contract that defines the methods the service will provide.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IVpnControlClient))]
    public interface IVpnControlService
    {
        [OperationContract]
        VpnStatusModelTo GetStatusModel();

        [OperationContract]
        void SetLoginData(VpnDataModelTo vpnData);

        [OperationContract]
        void Connect();

        [OperationContract]
        void Disconnect();
    }
}
