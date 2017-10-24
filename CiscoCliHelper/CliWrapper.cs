using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Model;
using CiscoCliHelper;

// ReSharper disable once CheckNamespace
namespace CiscoAnyconnectControl.CiscoCliWrapper
{
    public class CliWrapper : IDisposable
    {
        private CiscoCli Cli { get; set; }
        public CliWrapper(string executablePath)
        {
            if (!File.Exists(executablePath)) throw new ArgumentException("The executable doesnt exist!", nameof(executablePath));
            this.Cli = new CiscoCli(executablePath);
        }

        public void UpdateStatus()
        {
            this.Cli.UpdateStatus();
        }
        public VpnStatusModel VpnStatusModel { get; set; }

        public void Dispose()
        {
            this.Cli?.Dispose();
        }
    }
}
