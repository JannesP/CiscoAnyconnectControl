using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.IPC.Contracts
{
    interface IVpnControlClient
    {
        [OperationContract]
        void StatusModelPropertyChanged(string propertyName, object value);
    }
}
