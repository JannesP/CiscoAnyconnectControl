using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CiscoAnyconnectControl.Util;

namespace CiscoAnyconnectControl.Model.DAL
{
    public sealed class SettingsFile
    {
        private static SettingsFile _instance;
        public static SettingsFile Instance => _instance ?? (_instance = new SettingsFile(Path.Combine(Util.AssemblyDirectory, "settings.xml")));

        private readonly string _path;

        public SettingsModel SettingsModel { get; set; }

        private SettingsFile(string path)
        {
            Console.WriteLine($"Initializing SettingsFile with {path}.");
            this._path = path;
            this.SettingsModel = Load(path);
        }

        private SettingsModel Load(string path = "")
        {
            bool fileExists = File.Exists(path);
            if (!fileExists)
            {
                this.SettingsModel = new SettingsModel();
            }
            else
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    var xmlSerializer = new XmlSerializer(typeof(SettingsModel));
                    this.SettingsModel = (SettingsModel)xmlSerializer.Deserialize(stream);
                }
            }
            return this.SettingsModel;
        }

        public void Save()
        {
            var xmlSerializer = new XmlSerializer(this.SettingsModel.GetType());
            using (FileStream fs = File.OpenWrite(this._path))
            {
                xmlSerializer.Serialize(fs, this.SettingsModel);
            }
            
        }
    }
}
