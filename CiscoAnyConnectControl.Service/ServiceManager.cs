using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private readonly List<IVpnControlClient> _clients = new List<IVpnControlClient>();
        private readonly CiscoCli _ciscoCli;

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
        }

        public VpnStatusModel StatusModel => this._ciscoCli.VpnStatusModel;

        public void SubscribeToStatusModelChanges(IVpnControlClient client)
        {
            try
            {
                lock (this._syncRootClients)
                {
                    this._clients.Add(client);
                }
                Trace.TraceInformation("Client added.");
            }
            catch (Exception ex) { HandleException(ex); }
        }

        public void UnSubscribeFromStatusModelChanges(IVpnControlClient client)
        {
            try
            {
                lock (this._syncRootClients)
                {
                    this._clients.Remove(client);
                }
                Trace.TraceInformation("Client removed.");
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
                        c.StatusModelPropertyChanged(propName, value);
                    }
                    catch (Exception ex)
                    {
                        UnSubscribeFromStatusModelChanges(c);
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
                this._ciscoCli.Connect(vpnDataModel.Address, vpnDataModel.Username, vpnDataModel.Password, vpnDataModel.Group);
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
    }
}
