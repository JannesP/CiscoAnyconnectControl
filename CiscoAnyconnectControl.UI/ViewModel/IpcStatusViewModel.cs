using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CiscoAnyconnectControl.Model.Annotations;
using CiscoAnyconnectControl.UI.Command;
using CiscoAnyconnectControl.UI.IpcClient;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyconnectControl.UI.ViewModel
{
    class IpcStatusViewModel : INotifyPropertyChanged
    {
        private static int ReconnectSleepSeconds => 2;

        public IpcStatusViewModel()
        {
            _tokenSourceConnect = new CancellationTokenSource();
            VpnControlClient.Instance.PropertyChanged += VpnControlClient_Instance_PropertyChanged;
            VpnControlClient.Instance.ConnectionLost += Instance_ConnectionLost;
        }

        public bool ShowLoadingIndicator => !VpnControlClient.Instance.IsConnected;
        public bool IsInterfaceDisabled => !this.ShowLoadingIndicator;
        public Visibility LoadingIndicatorVisibility => this.ShowLoadingIndicator ? Visibility.Visible : Visibility.Collapsed;

        private CancellationTokenSource _tokenSourceConnect;
        
        private enum State
        {
            Connecting,
            Connected,
            Disconnecting,
            Disconnected
        }

        private volatile State _state = State.Disconnected;

        private void VpnControlClient_Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(VpnControlClient.IsConnected):
                    OnPropertyChanged(nameof(this.ShowLoadingIndicator));
                    OnPropertyChanged(nameof(this.IsInterfaceDisabled));
                    OnPropertyChanged(nameof(this.LoadingIndicatorVisibility));
                    break;
            }
        }

        private void Instance_ConnectionLost(object sender, EventArgs args)
        {
            _state = State.Connecting;
            VpnControlClient.Instance.ConnectAsync().ContinueWith(VpnControlClient_ConnectContinuation);
        }

        private void VpnControlClient_ConnectContinuation(Task<bool> t)
        {
            switch (t.Status)
            {
                case TaskStatus.RanToCompletion:
                    Trace.TraceInformation(t.Result ? "UI got IPC connected." : "UI got IPC connection failed.");
                    if (!t.Result)
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                if (_state == State.Connecting)
                                {
                                    Trace.TraceInformation("Reconnecting to IPC in 5s ...");
                                    await Task.Delay(TimeSpan.FromSeconds(ReconnectSleepSeconds), _tokenSourceConnect.Token);
                                    Task<bool> newTask = VpnControlClient.Instance.ConnectAsync();
                                    VpnControlClient_ConnectContinuation(newTask);
                                }
                            }
                            catch (TaskCanceledException ex)
                            {
                                Trace.TraceInformation("Reconnect cancelled.");
                            }
                        });
                    }
                    else
                    {
                        _state = State.Connected;
                    }
                    break;
                case TaskStatus.Faulted:
                    Util.TraceException("Uncaught exception while trying to connect to IPC server:", t.Exception?.InnerException);
                    VpnControlClient.Instance.Reset();
                    _state = State.Disconnected;
                    //TODO: inform user
                    break;
                default:
                    Trace.TraceError("Unexpected Task State after connect attempt ({0}).", t.Status);
                    VpnControlClient.Instance.Reset();
                    _state = State.Disconnected;
                    break;
            }
        }

        public void Disconnect()
        {
            if (_state == State.Disconnected || _state == State.Disconnecting) return;
            _state = State.Disconnecting;
            _tokenSourceConnect.Cancel();
            VpnControlClient.Instance.DisconnectAsync(TimeSpan.FromSeconds(1), true).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    Util.TraceException("Uncaught error disconnecting VpnControlClient:", t.Exception?.InnerException);
                }
                _state = State.Disconnected;
            });
        }

        public void Connect()
        {
            if (_state == State.Disconnected)
            {
                _state = State.Connecting;
                _tokenSourceConnect = new CancellationTokenSource();
                VpnControlClient.Instance.ConnectAsync().ContinueWith((t) =>
                {
                    if (t.IsFaulted)
                    {
                        _state = State.Disconnected;

                        Util.TraceException("Uncaught error connecting to VpnControlClient:",
                            t.Exception?.InnerException);
                    }
                    else
                    {
                        _state = State.Connected;
                    }
                });
            }
        }

        public RelayCommand ConnectCommand => new RelayCommand(() => true, Connect);

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
