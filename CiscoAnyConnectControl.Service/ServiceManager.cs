using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.CiscoCliHelper;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.DAL;

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
            this._ciscoCli = new CiscoCli(SettingsFile.Instance.SettingsModel.CiscoCliPath);
            this._ciscoCli.VpnStatusModel.PropertyChanged += VpnStatusModel_PropertyChanged;
        }

        public VpnStatusModel StatusModel => this._ciscoCli.VpnStatusModel;

        public void SubscribeToStatusModelChanges(IVpnControlClient client)
        {
            lock (this._syncRootClients)
            {
                this._clients.Add(client);
            }
        }

        public void UnSubscribeFromStatusModelChanges(IVpnControlClient client)
        {
            lock (this._syncRootClients)
            {
                this._clients.Remove(client);
            }
        }

        private void VpnStatusModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this._clients.Count == 0) return;
            string propName = e.PropertyName;
            object value = this._ciscoCli.VpnStatusModel.GetType().GetProperty(e.PropertyName)?.GetValue(sender);
            this._clients.AsParallel().ForAll(c => c.StatusModelPropertyChanged(propName, value));
        }


        public void Connect(VpnDataModel vpnDataModel)
        {
            this._ciscoCli.Connect(vpnDataModel.Address, vpnDataModel.Username, vpnDataModel.Password, vpnDataModel.Group);
        }

        public void Disconnect()
        {
            this._ciscoCli.Disconnect();
        }

        public void UpdateStatus()
        {
            this._ciscoCli.UpdateStatus();
        }

        public async Task<IEnumerable<string>> GetGroupsForHost(string address)
        {
            return await this._ciscoCli.LoadGroups(address);
        }
    }
}
