using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CiscoAnyconnectControl.IPC.Contracts;

namespace CiscoAnyConnectControl.Service
{
    class PingableIVpnControlClient
    {
        private readonly IVpnControlClient _channel;
        private readonly IServiceChannel _convChannel;

        public PingableIVpnControlClient(IVpnControlClient channel)
        {
            this._channel = channel;
            // ReSharper disable once SuspiciousTypeConversion.Global
            this._convChannel = (IServiceChannel) channel;
        }

        public IVpnControlClient Channel => this._channel;

        public bool ReceivedPong { get; private set; } = true;

        public void Ping()
        {
            if (this._channel == null || this._convChannel.State != CommunicationState.Opened) return;
            Trace.TraceInformation("Pinging {0} ...", this.SessionId);
            this.ReceivedPong = false;
            this._channel.Ping();
        }

        public void ReceivePong()
        {
            Trace.TraceInformation("Received pong from {0}", this.SessionId);
            this.ReceivedPong = true;
        }

        public string SessionId => this._convChannel.SessionId;

        public override bool Equals(object obj)
        {
            if (obj is PingableIVpnControlClient client)
            {
                return this._channel.Equals(client.Channel);
            }
            else
            {
                return this._channel.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return this._channel.GetHashCode();
        }

        public void Abort()
        {
            this._convChannel.Abort();
        }
    }
}
