using CiscoAnyconnectControl.Model;
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
        private VpnStatusModel VpnStatusModel;

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
            VpnStatusModel = new VpnStatusModel();
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
            if (e.Data.StartsWith("  >>"))
            {
                string[] parts = e.Data.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    parts[0] = parts[0].TrimStart('>', ' ').Trim();
                    parts[1] = parts[1].TrimStart('>', ' ').Trim();
                    this._states[parts[0]] = parts[1];
                }
                ParseState(parts[0], parts[1]);
            }
        }

        private void ParseState(string state, string value)
        {
            switch (state)
            {
                case "notice":
                    VpnStatusModel.Message = value;
                    break;
                case "state":
                    try
                    {
                        VpnStatusModel.Status = (VpnStatusModel.VpnStatus)Enum.Parse(typeof(VpnStatusModel.VpnStatus), value, true);
                    } catch { Trace.TraceWarning($"CiscoCli: Enum value {value} is not defined for VpnStatus while parsing {state}"); }
                    break;
                case "Time Connected":
                    VpnStatusModel.TimeConnected = TimeSpan.ParseExact(value, new[] { "hh:mm:ss", "hhh:mm:ss", "hhhh:mm:ss" }, null);
                    break;
            }
        }

        private void ParseStates()
        {
            var keys = _states.Keys;
            foreach (var key in keys)
            {
                ParseState(key, _states[key]);
            }
        }

        private void SendCommand(CliCommand command)
        {
            string cmd = command.ToString().ToLower();
            this._ciscoCli.StandardInput.WriteLine(cmd);
        }

        public void UpdateStatus()
        {
            SendCommand(CliCommand.Stats);
            SendCommand(CliCommand.State);
        }

        public void Disconnect()
        {
            SendCommand(CliCommand.Disconnect);
        }

        public void Dispose()
        {
            this._ciscoCli.OutputDataReceived -= _ciscoCli_OutputDataReceived;
            this._ciscoCli?.Kill();
            this._ciscoCli?.Dispose();
        }
    }
}
