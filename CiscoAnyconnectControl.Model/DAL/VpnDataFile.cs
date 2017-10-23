using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static VpnDataFile Instance => _instance ?? (_instance = new VpnDataFile(Path.Combine(Util.AssemblyDirectory, "credentials.data")));

        private readonly string _path;

        public VpnDataModel VpnDataModel { get; private set; }

        private VpnDataFile(string path)
        {
            this._path = path;
            this.VpnDataModel = Load(path);
        }

        private VpnDataModel Load(string path)
        {
            bool fileExists = File.Exists(path);
            if (!fileExists) return new VpnDataModel();

            VpnDataModel model = null;
            try
            {
                byte[] file = File.ReadAllBytes(path);
                file = ProtectedDataHelper.Unprotect(file);
                using (var ms = new MemoryStream(file))
                {
                    var bf = new BinaryFormatter();
                    model = (VpnDataModel) bf.Deserialize(ms);
                }
            }
            catch (Exception e)
            {
                Util.TraceException("Error loading VpnData, restoring defaults ...", e);
            }
            return model ?? new VpnDataModel();
        }

        public void Save()
        {
            try
            {
                byte[] data;
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, this.VpnDataModel);
                    data = ms.ToArray();
                }
                data = ProtectedDataHelper.Protect(data);
                File.WriteAllBytes(this._path, data);
            }
            catch (Exception e)
            {
                Util.TraceException("Error while saving VpnData", e);
            }
        }
        
    }
}
