using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.Model.DAL
{
    public sealed class SettingsFile
    {
        private static SettingsFile _instance = null;
        public static SettingsFile Instance => _instance ?? (_instance = new SettingsFile());

        public SettingsModel SettingsModel { get; set; }

        private SettingsFile()
        {
            SettingsModel = new SettingsModel();
        }

        public SettingsFile Load(string path)
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
        
    }
}
