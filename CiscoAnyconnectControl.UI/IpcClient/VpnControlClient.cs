using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyconnectControl.UI.IpcClient
{
    class VpnControlClient : IDisposable, IVpnControlClient
    {
        private static readonly Lazy<VpnControlClient> _instance = new Lazy<VpnControlClient>(() => new VpnControlClient());

        public static VpnControlClient Instance => _instance.Value;

        private IVpnControlService _service;

        public VpnStatusModel VpnStatusModel { get; set; }
        public IVpnControlService Service => this._service;

        #region IDisposable Support
        private bool _disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    if (this._service != null)
                    {
                        try
                        {
                            this._service.UnSubscribeFromStatusModelChanges();
                            // ReSharper disable once SuspiciousTypeConversion.Global
                            ((System.ServiceModel.Channels.IChannel)this._service).Close();
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch (Exception) { }
                    }
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                _disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~Client() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }
        #endregion

        private VpnControlClient()
        {
            var binding = new WSDualHttpBinding("serviceEndpoint");
            var channelFactory = new DuplexChannelFactory<IVpnControlService>(new InstanceContext(this), binding);
            this._service = channelFactory.CreateChannel();
            this._service.SubscribeToStatusModelChanges();
            this.VpnStatusModel = this._service.GetStatusModel().ToModel();
        }

        public void StatusModelPropertyChanged(string propertyName, object value)
        {
            this.VpnStatusModel.GetType().GetProperty(propertyName)?.SetValue(this.VpnStatusModel, value);
        }
    }
}
