using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Model;
using System.IO;
using CiscoAnyconnectControl.Util;
using System.Runtime.Serialization.Formatters.Binary;

namespace CiscoAnyconnectControl.Model.DAL
{
    public class VpnDataFile
    {
        private static VpnDataFile _instance;
        public static VpnDataFile Instance => _instance ?? (_instance = new VpnDataFile());

        public VpnDataModel VpnDataModel { get; private set; }

        public VpnDataFile()
        {
            this.VpnDataModel = new VpnDataModel();
        }

        public VpnDataModel Load(string path)
        {
            bool fileExists = File.Exists(path);
            if (!fileExists) return Instance.VpnDataModel;

            byte[] file = File.ReadAllBytes(path);
            file = ProtectedDataHelper.Unprotect(file);
            using (MemoryStream ms = new MemoryStream(file))
            {
                BinaryFormatter bf = new BinaryFormatter();
                Instance.VpnDataModel = (VpnDataModel)bf.Deserialize(ms);
            }
            return Instance.VpnDataModel;
        }

        public void Save(string path)
        {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, ms);
                data = ms.ToArray();
            }
            data = ProtectedDataHelper.Protect(data);
            File.WriteAllBytes(path, data);
        }
        
    }
}
