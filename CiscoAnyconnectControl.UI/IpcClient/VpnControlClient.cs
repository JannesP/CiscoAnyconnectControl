using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.Contracts;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.Annotations;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyconnectControl.UI.IpcClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    class VpnControlClient : IDisposable, IVpnControlClient, INotifyPropertyChanged
    {
        private static Lazy<VpnControlClient> _instance = new Lazy<VpnControlClient>(() => new VpnControlClient());

        public static VpnControlClient Instance => _instance.Value;

        private readonly object _syncRoot = new object();

        private IVpnControlService _service;
        private volatile bool _isConnected = false;
        private ServiceModelSectionGroup _serviceModelSectionGroup;
        private bool _manuallyDisconnected = false;
        private bool _connecting = false;

        public VpnStatusModel VpnStatusModel { get; } = new VpnStatusModel();

        private enum State
        {
            Connecting,
            Connected,
            Disconnecting,
            Disconnected
        }

        private volatile State _state = State.Disconnected;

        [CanBeNull]
        public IVpnControlService Service
        {
            get
            {
                return this._service;
            } 
            private set
            {
                if (value != this._service)
                {
                    this._service = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsConnected
        {
            get => this._isConnected;
            private set
            {
                if (value != this._isConnected)
                {
                    this._isConnected = value;
                    OnPropertyChanged();
                }
            }
        }

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
            this._serviceModelSectionGroup = ServiceModelSectionGroup.GetSectionGroup(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
            if (this._serviceModelSectionGroup == null) throw new Exception("Can't find pipe configuration. Make sure the .exe.config has a correct endpoint.");
        }

        private async Task<bool> CreateNewChannelAsync()
        {
            if (_state == State.Connected) return true;
            this.IsConnected = false;

            await DisconnectAsync(TimeSpan.FromMilliseconds(100), true);
            this._manuallyDisconnected = false;
            _state = State.Connecting;
            Trace.TraceInformation("Connecting to IPC server ...");
            var channelFactory = new DuplexChannelFactory<IVpnControlService>(this, this._serviceModelSectionGroup.Client.Endpoints[0].Name);
            try
            {
                this.Service = channelFactory.CreateChannel();
            }
            catch (Exception ex)
            {
                Util.TraceException($"Cannot create pipe for endpoint {channelFactory.Endpoint.Address}:", ex);
                _state = State.Disconnected;
                return false;
            }
            try
            {
                this.Service?.SubscribeToStatusModelChanges();
            }
            catch (Exception ex)
            {
                Util.TraceException("Error connecting to IPC server:", ex);
                _state = State.Disconnected;
                return false;
            }
            _state = State.Connected;
            Trace.TraceInformation("Connected to IPC server.");
            lock (this._syncRoot)
            {
                this.Service?.GetStatusModel().ToModel().CopyTo(this.VpnStatusModel);
            }
            ((IChannel)this.Service).Closing += VpnControlClient_Closing;
            ((IChannel)this.Service).Closed += VpnControlClient_Closed;
            ((IChannel)this.Service).Faulted += VpnControlClient_Faulted;
            Trace.TraceInformation("Refreshed model from remote.");
            this.IsConnected = true;
            return true;
        }
        
        /// <summary>
        /// Tries to connect to the first endpoint defined in the .config file.
        /// </summary>
        /// <returns>True if the connection was successful or False otherwise.</returns>
        public async Task<bool> ConnectAsync()
        {
            this._manuallyDisconnected = false;
            return await CreateNewChannelAsync();
        }

        public async Task<bool> DisconnectAsync(TimeSpan timeout, bool forceAfterTimeout = true)
        {
            if (_state == State.Disconnected) return true;
            _manuallyDisconnected = true;
            _state = State.Disconnecting;
            return await Task.Run(() =>
            {
                var closed = false;
                try
                {
                    
                    IChannel ch = this.Service as IChannel;
                    if (ch != null)
                    {
                        if (ch.State != CommunicationState.Faulted && ch.State != CommunicationState.Closed &&
                            ch.State != CommunicationState.Closing)
                        {
                            ch?.Close(timeout);
                            _state = State.Disconnected;
                        }
                        if (this.Service != null) Trace.TraceInformation("Closed old IPC connection.");
                    }
                    closed = true;
                }
                catch (TimeoutException)
                {
                    if (forceAfterTimeout)
                    {
                        (this.Service as IChannel).Abort();
                        if (this.Service != null) Trace.TraceWarning("Aborted old IPC connection.");
                        _state = State.Disconnected;
                        closed = true;
                    }
                }
                return closed;
            });
        }

        private void ResetModel()
        {
            VpnStatusModel.Status = VpnStatusModel.VpnStatus.Unknown;
            VpnStatusModel.TimeConnected = null;
            VpnStatusModel.Message = "";
        }

        private void VpnControlClient_Faulted(object sender, EventArgs e)
        {
            _state = State.Disconnected;
            ResetModel();
            this.IsConnected = false;
            this.Service = null;
            Trace.TraceError("VpnControlClient: connection fauled.");
            if (!_manuallyDisconnected)
            {
                OnConnectionLost();
            }
        }

        private void VpnControlClient_Closed(object sender, EventArgs e)
        {
            _state = State.Disconnected;
            ResetModel();
            this.IsConnected = false;
            this.Service = null;
            Trace.TraceWarning("VpnControlClient closed.");
            if (!this._manuallyDisconnected)
            {
                OnConnectionLost();
            }
        }

        private void VpnControlClient_Closing(object sender, EventArgs e)
        {
            _state = State.Disconnecting;
            this.IsConnected = false;
            Trace.TraceWarning("VpnControlClient closing ...");
        }

        public void StatusModelPropertyChanged(string propertyName, object value)
        {
            lock (this._syncRoot)
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

        public void Ping()
        {
            this.Service?.Pong();
        }

        public event ConnectionLostEventHandler ConnectionLost;
        public delegate void ConnectionLostEventHandler(object sender, EventArgs args);

        protected virtual void OnConnectionLost()
        {
            ConnectionLost?.Invoke(this, EventArgs.Empty);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Reset()
        {
            Trace.TraceError("VpnControlClient got reset.");
            _instance = new Lazy<VpnControlClient>(() => new VpnControlClient());
            this.Dispose();
        }
    }
}
