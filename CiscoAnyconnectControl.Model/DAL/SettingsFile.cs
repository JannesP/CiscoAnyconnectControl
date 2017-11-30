using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using CiscoAnyconnectControl.Utility;

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
            _path = path;
            SettingsModel = Load(path);
        }

        private SettingsModel Load(string path = "")
        {
            bool fileExists = File.Exists(path);
            if (!fileExists)
            {
                SettingsModel = new SettingsModel();
            }
            else
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    var xmlSerializer = new XmlSerializer(typeof(SettingsModel));
                    try
                    {
                        SettingsModel = (SettingsModel) xmlSerializer.Deserialize(stream);
                    }
                    catch (Exception ex)
                    {
                        Util.TraceException("Error parsing xml settings file.", ex);
                        MessageBoxResult mbr = MessageBox.Show(
                            "There seems to be an error in the settings.xml\nDo you want to reset the settings?\nIm lazy af so if you fixed it and want to retry hit 'Cancel'.",
                            "Cisco Anyconnect Control", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                        switch (mbr)
                        {
                            case MessageBoxResult.Cancel:
                                return Load(path);
                            
                            case MessageBoxResult.No:
                                Environment.Exit(100);
                                break;
                            case MessageBoxResult.Yes:
                            default:
                                stream.Dispose();
                                File.Delete(path);
                                SettingsModel = new SettingsModel();
                                break;
                        }
                    }
                }
            }
            return this.SettingsModel;
        }

        public void Save()
        {
            try
            {
                var xmlSerializer = new XmlSerializer(this.SettingsModel.GetType());
                using (FileStream fs = File.Create(_path))
                {
                    xmlSerializer.Serialize(fs, SettingsModel);
                }
            }
            catch (Exception ex)
            {
                Util.TraceException("Error serializing settings to xml:", ex);
            }
        }
    }
}
