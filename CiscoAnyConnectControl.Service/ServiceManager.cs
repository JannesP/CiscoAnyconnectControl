using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CiscoAnyconnectControl.CiscoCliHelper;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.DAL;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyConnectControl.Service
{
    sealed class ServiceManager
    {
        private static readonly Lazy<ServiceManager> _instance = new Lazy<ServiceManager>(() => new ServiceManager());
        public static ServiceManager Instance => _instance.Value;

        private readonly object _syncRootClients = new object();

        private readonly List<PingableIVpnControlClient> _clients = new List<PingableIVpnControlClient>();
        private readonly CiscoCli _ciscoCli;
        private readonly Timer _clientsPingTimer;
        private readonly Timer _cliRefreshTimer;

        private ServiceManager()
        {
            if (!Util.CheckForCiscoProcesses(true))
            {
                Trace.TraceError("There are other cisco programs running. Aborting start!");
                throw new InvalidOperationException("There are other cisco programs running.");
            }
            this._ciscoCli = new CiscoCli(SettingsFile.Instance.SettingsModel.CiscoCliPath);
            this._ciscoCli.VpnStatusModel.PropertyChanged += VpnStatusModel_PropertyChanged;
            this._ciscoCli.UpdateStatus();
            this._clientsPingTimer = new Timer
            {
                Interval = 30000,
                Enabled = false,
                AutoReset = true
            };
            this._clientsPingTimer.Elapsed += _clientsPingTimer_Elapsed;
            this._clientsPingTimer = new Timer
            {
                Interval = 60000,
                Enabled = true,
                AutoReset = true
            };
            this._cliRefreshTimer.Elapsed += _cliRefreshTimer_Elapsed;
            

            //handle settings like autostart
            if (SettingsFile.Instance.SettingsModel.ConnectOnSystemStartup &&
                SettingsFile.Instance.SettingsModel.SavePassword)
            {
                Task.Run(async () =>
                {
                    VpnDataModel vpnData = VpnDataFile.Instance.VpnDataModel;
                    if (vpnData.Group == null)
                    {
                        Trace.TraceError("Error autostarting vpn. The group was not set.");
                    }
                    else
                    {
                        //give the cisco cli a little bit of time to start
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        Connect(vpnData);
                    }
                });
            }
        }

        private void _cliRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateStatus();
        }

        private void _clientsPingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var clientsToRemove = new List<IVpnControlClient>();
            lock (this._syncRootClients)
            {
                foreach (PingableIVpnControlClient client in this._clients)
                {
                    if (!client.ReceivedPong)
                    {
                        clientsToRemove.Add(client.Channel);
                        Trace.TraceInformation("Channel {0} didnt answer to ping. Removing ...", client.SessionId);
                    }
                    else
                    {
                        client.Ping();
                    }
                }
            }
            foreach (IVpnControlClient client in clientsToRemove)
            {
                UnSubscribeFromStatusModelChanges(client);
            }
        }

        public VpnStatusModel StatusModel => this._ciscoCli.VpnStatusModel;

        public void SubscribeToStatusModelChanges(IVpnControlClient client)
        {
            try
            {
                var pClient = new PingableIVpnControlClient(client);
                lock (this._syncRootClients)
                {
                    this._clients.Add(pClient);
                    this._clientsPingTimer.Enabled = true;
                }
                Trace.TraceInformation("Client {0} added.", pClient.SessionId);
            }
            catch (Exception ex) { HandleException(ex); }
        }

        public void UnSubscribeFromStatusModelChanges(IVpnControlClient client)
        {
            if (client == null) return;
            try
            {
                var pClient = new PingableIVpnControlClient(client);
                lock (this._syncRootClients)
                {
                    this._clients.Remove(pClient);
                    pClient.Abort();
                    if (this._clients.Count == 0) this._clientsPingTimer.Enabled = false;
                }
                Trace.TraceInformation("Client {0} removed.", pClient.SessionId);
            }
            catch (Exception ex) { HandleException(ex); }
        }

        private void VpnStatusModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (this._clients.Count == 0) return;
                string propName = e.PropertyName;
                object value = this._ciscoCli.VpnStatusModel.GetType().GetProperty(e.PropertyName)?.GetValue(sender);
                if (propName == "Status")
                {
                    value = value?.ToString();
                }
                this._clients.ForEach(c =>
                {
                    try
                    {
                        Trace.TraceInformation($"Sending PropertyChanged to client.({e.PropertyName})");
                        c.Channel.StatusModelPropertyChanged(propName, value);
                    }
                    catch (Exception ex)
                    {
                        UnSubscribeFromStatusModelChanges(c.Channel);
                        HandleException(ex);
                    }
                });
            }
            catch (Exception ex) { HandleException(ex); }
        }


        public void Connect(VpnDataModel vpnDataModel)
        {
            try
            {
                this._ciscoCli.Connect(vpnDataModel.Address, vpnDataModel.Username, vpnDataModel.Password, vpnDataModel.GroupId);
            }
            catch (Exception ex) { HandleException(ex); }
        }

        public void Disconnect()
        {
            try
            {
                this._ciscoCli.Disconnect();
            }
            catch (Exception ex) { HandleException(ex); }
        }

        public void UpdateStatus()
        {
            try
            {
                this._ciscoCli.UpdateStatus();
            }
            catch (Exception ex) { HandleException(ex); }
        }

        public async Task<IEnumerable<string>> GetGroupsForHost(string address)
        {
            try
            {
                return await this._ciscoCli.LoadGroups(address);
            }
            catch (Exception ex) { HandleException(ex); }
            return null;
        }

        private void HandleException(Exception ex)
        {
            Util.TraceException("Error in Service:", ex);
        }

        public void Pong(IVpnControlClient channel)
        {
            lock (this._syncRootClients)
            {
                foreach (PingableIVpnControlClient client in this._clients)
                {
                    if (client.Channel == channel)
                    {
                        client.ReceivePong();
                    }
                }
            }
        }
    }
}
