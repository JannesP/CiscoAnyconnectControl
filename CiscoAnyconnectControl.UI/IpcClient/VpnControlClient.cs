using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.Model;

namespace CiscoAnyconnectControl.UI.IpcClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
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
                            ((IChannel)this._service).Abort();
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch (Exception) { }
                        this._service = null;
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
            ServiceModelSectionGroup group = ServiceModelSectionGroup.GetSectionGroup(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
            if (group == null) throw new Exception("Can't find pipe configuration.");
            var channelFactory = new DuplexChannelFactory<IVpnControlService>(this, group.Client.Endpoints[0].Name);
            this._service = channelFactory.CreateChannel();
            ((IChannel)this._service).Closed += VpnControlClient_Closed;
            ((IChannel)this._service).Faulted += VpnControlClient_Faulted;
            this._service.SubscribeToStatusModelChanges();
            this.VpnStatusModel = this._service.GetStatusModel().ToModel();
        }

        private void VpnControlClient_Faulted(object sender, EventArgs e)
        {
            Trace.TraceError("VpnControlClient: connection fauled.");
        }

        private void VpnControlClient_Closed(object sender, EventArgs e)
        {
            Trace.TraceWarning("VpnControlClient closed.");
        }

        public void StatusModelPropertyChanged(string propertyName, object value)
        {
            Console.WriteLine($"Got status for: {propertyName}:{value}");
            switch (propertyName)
            {
                case "Status":
                    if (Enum.TryParse(value.ToString(), out VpnStatusModel.VpnStatus ev))
                    {
                        this.VpnStatusModel.Status = ev;
                    }
                    break;
                default:
                    this.VpnStatusModel.GetType().GetProperty(propertyName)?.SetValue(this.VpnStatusModel, value);
                    break;
            }
            
        }
    }
}
