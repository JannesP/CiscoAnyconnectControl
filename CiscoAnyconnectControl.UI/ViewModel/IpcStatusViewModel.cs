﻿using System;
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
using CiscoAnyconnectControl.UI.IpcClient;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyconnectControl.UI.ViewModel
{
    class IpcStatusViewModel : INotifyPropertyChanged
    {
        public IpcStatusViewModel()
        {
            VpnControlClient.Instance.PropertyChanged += VpnControlClient_Instance_PropertyChanged;
            VpnControlClient.Instance.ConnectionLost += Instance_ConnectionLost;
            VpnControlClient.Instance.ConnectAsync().ContinueWith(VpnControlClient_ConnectContinuation);
        }

        public bool ShowLoadingIndicator => !VpnControlClient.Instance.IsConnected;
        public bool IsInterfaceDisabled => !this.ShowLoadingIndicator;
        public Visibility LoadingIndicatorVisibility => this.ShowLoadingIndicator ? Visibility.Visible : Visibility.Collapsed;

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
                        Task.Run(() =>
                        {
                            Trace.TraceInformation("Reconnecting to IPC in 5s ...");
                            Thread.Sleep(5000);
                            VpnControlClient.Instance.ConnectAsync().ContinueWith(VpnControlClient_ConnectContinuation);
                        });
                    }
                    break;
                case TaskStatus.Faulted:
                    Util.TraceException("Uncaught exception while trying to connect to IPC server:", t.Exception?.InnerException);
                    //TODO: inform user
                    break;
                default:
                    Trace.TraceError("Unexpected Task State after connect attempt ({0}).", t.Status);
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}