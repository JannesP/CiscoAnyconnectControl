using System;
using System.Collections.Generic;
using System.Diagnostics;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Utility;
using System.IO;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.CiscoCliHelper
{
    public class CiscoCli : IDisposable
    {
        private CliProcess _ciscoCli;
        private Dictionary<string, string> _states;
        private string path;
        public VpnStatusModel VpnStatusModel { get; } = new VpnStatusModel();

        public CiscoCli(string path)
        {
            if (!File.Exists(path)) throw new ArgumentException("The executable doesnt exist!", nameof(path));
            this._states = new Dictionary<string, string>();
            this._ciscoCli = new CliProcess(path);
            this._ciscoCli.OutputDataReceived += _ciscoCli_OutputDataReceived;
            this._ciscoCli.ErrorDataReceived += _ciscoCli_ErrorDataReceived;
            this._ciscoCli.Exited += _ciscoCli_Exited;
            this._ciscoCli.Start();
            this._ciscoCli.BeginOutputReadLine();
            this._ciscoCli.BeginErrorReadLine();
            this.UpdateStatus();
        }

        public TimeSpan Timeout { get; set; } = new TimeSpan(TimeSpan.TicksPerSecond * 5);

        private void _ciscoCli_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"CISCO_ERROR:\t{e.Data}");
        }

        public async Task<IEnumerable<string>> LoadGroups(string address)
        {
            if (this.VpnStatusModel.Status == VpnStatusModel.VpnStatus.Disconnected)
            {
                var groups = new List<string>();
                using (var cli = new CliProcess(this._ciscoCli.StartInfo.FileName))
                {
                    cli.Start();
                    cli.BeginOutputReadLine();

                    cli.SendCommand(CliProcess.Command.Connect, address);
                    var finished = false;
                    var q = new Queue<string>();
                    while (!finished)
                    {
                        string s = await cli.StandardOutput.ReadLineAsync().TimeoutAfter(this.Timeout);
                        s = s.Trim(' ', '>').Trim();
                        if (s.StartsWith("Group:")) finished = true;
                        else if (s.StartsWith("error:")) throw new Exception(s.Replace("error: ", ""));
                        else q.Enqueue(s);
                    }

                    var searchingForGroups = true;
                    while (q.Count > 0 && searchingForGroups)
                    {
                        if (q.Dequeue().StartsWith("Awaiting user input.")) searchingForGroups = false;
                    }

                    while (q.Count > 0)
                    {
                        string line = q.Dequeue();
                        line = line.Remove(0, line.IndexOf(')') + 2);
                        Trace.TraceInformation($"Found group: {line}.");
                        groups.Add(line);
                    }

                    cli.Kill();
                }
                return groups;
            }
            throw new InvalidOperationException("The vpn needs to be disonnected for the profile list to be loaded.");
        }

        public void Connect(string address, string username, string password, string profile)
        {
            if (profile == null) throw new ArgumentException($"{nameof(profile)} cannot be null!", nameof(profile));
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

        public void UpdateStatus()
        {
            this._ciscoCli.SendCommand(CliProcess.Command.Stats);
            this._ciscoCli.SendCommand(CliProcess.Command.State);
        }

        public void Disconnect()
        {
            this._ciscoCli.SendCommand(CliProcess.Command.Disconnect);
        }

        public void Dispose()
        {
            this._ciscoCli.OutputDataReceived -= _ciscoCli_OutputDataReceived;
            this._ciscoCli?.Kill();
            this._ciscoCli?.Dispose();
        }
    }
}
