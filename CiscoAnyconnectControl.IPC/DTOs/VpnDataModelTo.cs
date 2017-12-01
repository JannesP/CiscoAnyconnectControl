using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyconnectControl.IPC.DTOs
{
    [DataContract]
    public class VpnDataModelTo
    {
        public static VpnDataModelTo FromModel(VpnDataModel mdl)
        {
            return new VpnDataModelTo
            {
                Address = mdl.Address,
                Username = mdl.Username,
                Password = mdl.Password,
                Group = mdl.Group,
                GroupId = mdl.GroupId
            };
        }

        public VpnDataModel ToModel()
        {
            return new VpnDataModel
            {
                Address = this.Address,
                Username = this.Username,
                Password = this.Password,
                Group = this.Group,
                GroupId = this.GroupId
            };
        }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Group { get; set; }

        [DataMember]
        public int GroupId { get; set; }
    }
}
