using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.CiscoCliHelper
{
    class CliProcess : Process
    {
        public CliProcess(string path)
        {
            this.StartInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = "-s",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                ErrorDialog = true
            };
        }

        public enum Command
        {
            Connect, Disconnect, Stats, State
        }

        public void SendCommand(Command command, string param = null)
        {
            string cmd = command.ToString().ToLower();
            if (param != null)
            {
                cmd += $" {param}";
            }
            this.StandardInput.WriteLine(cmd);
        }
    }
}
