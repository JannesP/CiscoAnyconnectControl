using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoCliHelper
{
    class CiscoCli : IDisposable
    {
        private Process _ciscoCli;
        private Dictionary<string, string> _states;

        public CiscoCli(string path)
        {
            this._states = new Dictionary<string, string>();
            this._ciscoCli = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "-s",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    ErrorDialog = true
                }
            };
            this._ciscoCli.OutputDataReceived += _ciscoCli_OutputDataReceived;
            this._ciscoCli.ErrorDataReceived += _ciscoCli_ErrorDataReceived;
            this._ciscoCli.Exited += _ciscoCli_Exited;
            this._ciscoCli.Start();
            this._ciscoCli.BeginOutputReadLine();
            this._ciscoCli.BeginErrorReadLine();
        }

        private void _ciscoCli_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"CISCO_ERROR:\t{e.Data}");
        }

        public enum CliCommand
        {
            Connect, Disconnect, Stats, State
        }

        private void _ciscoCli_Exited(object sender, EventArgs e)
        {
            Trace.TraceInformation("Cisco client exited.");
        }

        private void _ciscoCli_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"CISCO:\t{e.Data}");
            string[] parts = e.Data.Split(new[]{':'}, 2);
            if (parts.Length == 2)
            {
                this._states[parts[0]] = parts[1];
            }
        }

        public void SendCommand(CliCommand command)
        {
            string cmd = command.ToString().ToLower();
            this._ciscoCli.StandardInput.WriteLine(cmd);
        }

        public void Dispose()
        {
            this._ciscoCli.OutputDataReceived -= _ciscoCli_OutputDataReceived;
            this._ciscoCli?.Kill();
            this._ciscoCli?.Dispose();
        }
    }
}
