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
            var x = new VpnStatusModelTo
            {
                ConnectedSince = DateTime.Now - mdl.TimeConnected,
                Message = mdl.Message,
                Status = mdl.Status.ToString()
            };
            return x;
        }

        public VpnStatusModel ToModel()
        {
            var x = new VpnStatusModel
            {
                TimeConnected = DateTime.Now - this.ConnectedSince,
                Message = this.Message
            };
            Enum.TryParse(this.Status, out VpnStatusModel.VpnStatus st);
            x.Status = st;
            return x;
        }

        [DataMember]
        public string Status { get; set; }
        
        [CanBeNull]
        [DataMember]
        public DateTime? ConnectedSince { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
