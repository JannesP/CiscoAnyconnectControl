using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyconnectControl.DAL
{
    class VpnDataFile
    {
        private static VpnDataFile _instance;
        public static VpnDataFile Instance => _instance ?? (_instance = new VpnDataFile());

        public VpnDataModel VpnDataModel { get; set; }

        public VpnDataFile()
        {
            this.VpnDataModel = new VpnDataModel();
        }


        public void Load(string path)
        {
            
        }

        public void Save(string path)
        {
            
        }
        
    }
}
