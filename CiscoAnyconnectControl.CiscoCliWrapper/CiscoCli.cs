using System;
using System.Collections.Generic;
using System.Diagnostics;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Utility;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.CiscoCliHelper
{
    public sealed class CiscoCli : IDisposable
    {
        private CliProcess _ciscoCli;
        private readonly Dictionary<string, string> _states;
        private readonly string _path;
        public VpnStatusModel VpnStatusModel { get; } = new VpnStatusModel();

        public CiscoCli(string path)
        {
            if (!File.Exists(path)) throw new ArgumentException("The executable doesnt exist!", nameof(path));
            this._states = new Dictionary<string, string>();
            this._path = path;
            CreateNewCli(path);
            this.UpdateStatus();
        }

        private void CreateNewCli(string path)
        {
            StopCurrentCli();
            this._ciscoCli = new CliProcess(path);
            this._ciscoCli.OutputDataReceived += _ciscoCli_ReceiveOutputData;
            this._ciscoCli.ErrorDataReceived += _ciscoCli_ErrorDataReceived;
            this._ciscoCli.Exited += _ciscoCli_Exited;
            this._ciscoCli.Start();
            this._ciscoCli.BeginOutputReadLine();
            this._ciscoCli.BeginErrorReadLine();
        }

        private void StopCurrentCli()
        {
            this._ciscoCli?.CancelOutputRead();
            this._ciscoCli?.CancelErrorRead();
            this._ciscoCli?.Kill();
            this._ciscoCli?.Dispose();
            this._ciscoCli = null;
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
                StopCurrentCli();
                var groups = new List<string>();
                try
                {
                    using (var cli = new CliProcess(this._path))
                    {
                        cli.Start();
                        try
                        {

                            await cli.SendCommand(CliProcess.Command.Connect, address);
                            TimeSpan localTimeout = this.Timeout;
                            var q = new Queue<string>();
                            try
                            {
                                while (true)
                                {
                                    string s = await cli.StandardOutput.ReadLineAsync().TimeoutAfter(localTimeout);
                                    Trace.TraceInformation("CLI_GROUPS: " + s);
                                    s = s.Trim(' ', '>').Trim();
                                    if (s.StartsWith("Awaiting user input."))
                                    {
                                        localTimeout = new TimeSpan(this.Timeout.Ticks / 10);
                                        q.Enqueue(s);
                                    }
                                    else if (s.StartsWith("error:")) throw new Exception(s.Replace("error: ", ""));
                                    else q.Enqueue(s);
                                }
                            }
                            catch (TimeoutException) { }

                            var searchingForGroups = true;
                            while (q.Count > 0 && searchingForGroups)
                            {
                                if (q.Dequeue().StartsWith("Awaiting user input.")) searchingForGroups = false;
                            }

                            while (q.Count > 0)
                            {
                                string line = q.Dequeue();
                                if (line.Length == 0) continue;
                                line = line.Remove(0, line.IndexOf(')') + 2);
                                Trace.TraceInformation($"Found group: {line}.");
                                groups.Add(line);
                            }

                        }
                        finally
                        {
                            cli.Kill();
                        }
                    }
                }
                finally
                {
                    CreateNewCli(this._path);
                }
                return groups;
            }
            throw new InvalidOperationException("The vpn needs to be disonnected for the profile list to be loaded.");
        }

        public async void Connect(string address, string username, string password, int groupId)
        {
            if (groupId < 0) throw new ArgumentException($"{nameof(groupId)} has to be > 0. Please use LoadGroups in case you dont have it.", nameof(groupId));
            await this.SendCompleteConnect(address, username, password, groupId);
        }

        private void _ciscoCli_Exited(object sender, EventArgs e)
        {
            Trace.TraceInformation("Cisco client exited.");
        }

        private void _ciscoCli_ReceiveOutputData(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"CISCO:\t{e.Data}");
            if (e.Data == null)
            {
                Trace.TraceWarning("Received null in _ciscoCli_ReceiveOutputData.");
                return;
            }
            
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

        public async void UpdateStatus()
        {
            if (this._ciscoCli == null) return;
            await this.SendToCli(CliProcess.Command.Stats);
            await this.SendToCli(CliProcess.Command.State);
        }

        public async void Disconnect()
        {
            if (this._ciscoCli == null) return;
            await this.SendToCli(CliProcess.Command.Disconnect);
        }

        private async Task SendToCli(CliProcess.Command command, string param = null)
        {
            if (this._ciscoCli == null) throw new InvalidOperationException("_ciscoCli cannot be null.");
            bool isValidCall = false;
            switch (command)
            {
                case CliProcess.Command.Connect:
                    if (this.VpnStatusModel.Status == VpnStatusModel.VpnStatus.Disconnected)
                    {
                        isValidCall = true;
                    }
                    break;
                case CliProcess.Command.Disconnect:
                    if (this.VpnStatusModel.Status == VpnStatusModel.VpnStatus.Connected)
                    {
                        isValidCall = true;
                    }
                    break;
                case CliProcess.Command.State:
                    isValidCall = true;
                    break;
                case CliProcess.Command.Stats:
                    if (this.VpnStatusModel.Status == VpnStatusModel.VpnStatus.Connected)
                    {
                        isValidCall = true;
                    }
                    break;
                default:
                    Trace.TraceError("wtf u doin?");
                    break;
            }
            if (isValidCall)
            {
                await this._ciscoCli.SendCommand(command, param);
            }
        }

        public async Task SendCompleteConnect(string host, string username, string password, int groupId)
        {
            string connectCommand = string.Format("{1}{0}{2}{0}{3}{0}{4}"
                , Environment.NewLine
                , host
                , groupId
                , username
                , password
            );
            await SendToCli(CliProcess.Command.Connect, connectCommand);
        }

        public void Dispose()
        {
            this._ciscoCli?.Kill();
            this._ciscoCli?.Dispose();
        }
    }
}
