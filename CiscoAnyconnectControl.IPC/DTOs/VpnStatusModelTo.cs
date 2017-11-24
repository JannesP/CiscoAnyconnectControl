using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.Annotations;

namespace CiscoAnyconnectControl.IPC.DTOs
{
    [DataContract]
    public class VpnStatusModelTo
    {
        public static VpnStatusModelTo FromModel(VpnStatusModel mdl)
        {
            return new VpnStatusModelTo
            {
                Status = mdl.Status,
                ConnectedSince = DateTime.Now - mdl.TimeConnected,
                Message = mdl.Message
            };
        }

        public VpnStatusModel ToModel()
        {
            return new VpnStatusModel
            {
                Status = this.Status,
                TimeConnected = DateTime.Now - this.ConnectedSince,
                Message = this.Message
            };
        }

        [DataMember]
        public VpnStatusModel.VpnStatus Status { get; set; }
        
        [CanBeNull]
        [DataMember]
        public DateTime? ConnectedSince { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
