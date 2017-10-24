using System;
using System.Collections.Generic;
using System.Diagnostics;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyconnectControl.CiscoCliHelper
{
    class CiscoCli : IDisposable
    {
        private Process _ciscoCli;
        private Dictionary<string, string> _states;
        public VpnStatusModel VpnStatusModel { get; } = new VpnStatusModel();

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
            this.UpdateStatus();
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
            string[] parts = e.Data.Split(new[] { ':' }, 2);
            if (parts.Length == 2)
            {
                parts[0] = parts[0].TrimStart('>', ' ').Trim();
                parts[1] = parts[1].TrimStart('>', ' ').Trim();
                this._states[parts[0]] = parts[1];
                ParseState(parts[0], parts[1]);
            }
        }

        private void ParseState(string state, string value)
        {
            switch (state)
            {
                case "notice":
                    this.VpnStatusModel.Message = value;
                    break;
                case "state":
                    try
                    {
                        this.VpnStatusModel.Status = (VpnStatusModel.VpnStatus)Enum.Parse(typeof(VpnStatusModel.VpnStatus), value, true);
                    } catch { Trace.TraceWarning($"CiscoCli: Enum value {value} is not defined for VpnStatus while parsing {state}"); }
                    break;
                case "Time Connected":
                    if (value == "Not Available")
                    {
                        Trace.TraceError("Time Connected is not available.");
                        this.VpnStatusModel.TimeConnected = null;
                    }
                    else
                    {
                        this.VpnStatusModel.TimeConnected = TimeSpan.ParseExact(value, @"h\:mm\:ss", null);
                    }
                    
                    break;
            }
        }

        private void ParseStates()
        {
            var keys = this._states.Keys;
            foreach (var key in keys)
            {
                ParseState(key, this._states[key]);
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
