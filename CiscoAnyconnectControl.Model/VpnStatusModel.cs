using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CiscoAnyconnectControl.Model.Annotations;
using VpnApiLib;

namespace CiscoAnyconnectControl.Model
{
    public class VpnStatusModel : INotifyPropertyChanged
    {
        public class GroupEventArgs : EventArgs
        {
            public GroupEventArgs(List<string> availableGroups)
            {
                AvailableGroups = availableGroups;
            }

            public List<string> AvailableGroups;
            public string SelectedGroup { get; set; } = null;
        }

        public enum NoticeType
        {
            Error, Warning, Info, Status
        } 
    
        public class NoticeEventArgs : EventArgs
        {
            public NoticeEventArgs(string notice, NoticeType noticeType)
            {
                Notice = notice;
                NoticeType = noticeType;
            }

            public string Notice { get; set; }
            public NoticeType NoticeType { get; set; }
        }

        private static Lazy<VpnStatusModel> _instance = new Lazy<VpnStatusModel>(() => new VpnStatusModel());
        public static VpnStatusModel Instance => _instance.Value;

        private VpnStatus _status = VpnStatus.Unknown;
        private string _message = null;
        private TimeSpan? _timeConnected = null;
        private VpnApi _vpnApi = null;
        private VpnDataModel _connectData;

        private VpnStatusModel()
        {
            _vpnApi = new VpnApi();
            _vpnApi.Register(new VpnApiEventListener(this));
            _vpnApi.Attach();
        }

        public enum VpnStatus
        {
            Disconnected, Connecting, Connected, Disconnecting, Reconnecting, Pausing, Paused, SsoPolling, Unknown
        }

        public VpnStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                switch (value)
                {
                    case VpnStatus.Disconnected:
                        //this.Message = $"{value.ToString()}.";
                        this.TimeConnected = null;
                        break;
                }
                OnPropertyChanged();
            }
        }

        public void Connect(VpnDataModel vpnData)
        {
            _connectData = vpnData;
            try
            {
                if (_vpnApi.IsVPNServiceAvailable) _vpnApi.ConnectVpn(vpnData.Address);
            }
            catch (Exception ex)
            {
                OnNotice($"Error connecting to VPN, you probybly need to restart the program or hit the debug menu.\n{ex.Message}",
                    NoticeType.Error);
            }
        }

        public void Disconnect()
        {
            if (_vpnApi.IsVPNServiceAvailable) _vpnApi.DisconnectVpn();
        }

        [CanBeNull]
        public TimeSpan? TimeConnected
        {
            get
            {
                if (this.Status == VpnStatus.Connected || this.Status == VpnStatus.Disconnecting)
                {
                    return _timeConnected;
                }
                return null;
            }
            set
            {
                this._timeConnected = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => this._message;
            set
            {
                if (this._message == value) return;
                this._message = value;
                OnPropertyChanged();
            }
        }

        private class VpnApiEventListener : IVpnApiEvents
        {
            private readonly VpnStatusModel _model;

            public VpnApiEventListener(VpnStatusModel model)
            {
                _model = model;
            }

            public void VpnStatsNotification(IVpnStats pVpnStats)
            {
                TimeSpan.TryParseExact(pVpnStats[VPNStatsTag.TimeConnected], @"hh\:mm\:ss", CultureInfo.CurrentCulture,
                    out TimeSpan timeConnected);
                _model.TimeConnected = timeConnected;
            }

            public void VpnStateNotification(VPNState eState, VPNSubState eSubState, string strState)
            {
                switch (eState)
                {
                    case VPNState.CONNECTED:
                        _model.Status = VpnStatus.Connected;
                        break;
                    case VPNState.DISCONNECTED:
                        _model.Status = VpnStatus.Disconnected;
                        break;
                    case VPNState.RECONNECTING:
                        _model.Status = VpnStatus.Reconnecting;
                        break;
                    case VPNState.CONNECTING:
                        _model.Status = VpnStatus.Connecting;
                        break;
                    case VPNState.DISCONNECTING:
                        _model.Status = VpnStatus.Disconnecting;
                        break;
                    case VPNState.PAUSING:
                        _model.Status = VpnStatus.Pausing;
                        break;
                    case VPNState.PAUSED:
                        _model.Status = VpnStatus.Paused;
                        break;
                    case VPNState.SSOPOLLING:
                        _model.Status = VpnStatus.SsoPolling;
                        break;
                    case VPNState.UNKNOWN:
                        _model.Status = VpnStatus.Unknown;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eState), eState, null);
                }
            }

            public void VpnBannerNotification(string strBannerMessage)
            {
                Trace.TraceInformation("VpnBannerNotification: " + strBannerMessage);
                _model.Message = strBannerMessage;
            }

            public void VpnNoticeNotification(string strNoticeMessage, MessageType eMessageType)
            {
                Trace.TraceInformation($"VpnNoticeNotification: {strNoticeMessage}: {eMessageType.ToString()}");
                NoticeType nt = NoticeType.Info;
                switch (eMessageType)
                {
                    case MessageType.MsgType_Info:
                        _model.Message = strNoticeMessage;
                        break;
                    case MessageType.MsgType_Error:
                        nt = NoticeType.Error;
                        break;
                    case MessageType.MsgType_Warn:
                        nt = NoticeType.Warning;
                        break;
                    case MessageType.MsgType_Status:
                        nt = NoticeType.Status;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eMessageType), eMessageType, null);
                }
                if (nt != NoticeType.Info) _model.OnNotice(strNoticeMessage, nt);
            }

            public void VpnExitNotification(string strExitMessage, int exitCode)
            {
                Trace.TraceError($"VpnExitNotification: '{strExitMessage}' exit code: {exitCode}");
            }

            public void VpnServiceReadyNotification()
            {
                Trace.TraceError($"VpnServiceReadyNotification");
            }

            public void VpnUserPromptNotification(IConnectPromptInfo pConnectPromptInfo)
            {
                switch (pConnectPromptInfo.ConnectPromptType)
                {
                    case ConnectPromptType.CERTIFICATE:
                        Trace.TraceError("VpnUserPromptNotification: CERTIFICATE not implemented testing with just submitting");
                        break;
                    case ConnectPromptType.CREDENTIALS:
                        foreach (dynamic promptEntry in pConnectPromptInfo.PromptEntries)
                        {
                            switch (promptEntry.PromptName)
                            {
                                case "group_list":
                                    var entries = new List<string>();
                                    foreach (dynamic o in promptEntry.ValueOptions)
                                    {
                                        entries.Add(o);
                                    }
                                    if (entries.Count == 1)
                                    {
                                        _model._connectData.Group = entries[0];
                                    }
                                    if (_model._connectData.Group == null && entries.Count > 1)
                                    {
                                        _model._connectData.Group =_model.OnGroupRequested(entries);
                                        if (_model._connectData.Group == null)
                                        {
                                            pConnectPromptInfo.Canceled = true;
                                        }
                                    }
                                    Trace.TraceInformation("Setting group to " + _model._connectData.Group);
                                    promptEntry.Value = _model._connectData.Group;
                                    break;
                                case "password":
                                    promptEntry.Value = _model._connectData.Password;
                                    break;
                                case "username":
                                    promptEntry.Value = _model._connectData.Username;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(promptEntry.PromptName);
                            }
                        }
                        _model._vpnApi.UserSubmit();
                        break;
                    case ConnectPromptType.PROXY:
                        Trace.TraceError("VpnUserPromptNotification: PROXY not implemented setting cancel and submitting");
                        pConnectPromptInfo.Canceled = true;
                        break;
                    case ConnectPromptType.STATUS:
                        Trace.TraceError("VpnUserPromptNotification: STATUS not implemented setting cancel and submitting");
                        pConnectPromptInfo.Canceled = true;
                        break;
                    case ConnectPromptType.SINGLESIGNON:
                        Trace.TraceError("VpnUserPromptNotification: SINGLESIGNON not implemented setting cancel and submitting");
                        pConnectPromptInfo.Canceled = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _model._vpnApi.UserSubmit();
            }

            public void VpnWMHintNotification(WMHint eHint, WMHintReason eReason)
            {
                Trace.TraceError($"VpnWMHintNotification not implemented: {eHint.ToString()}: {eReason.ToString()}");
            }

            public void VpnWebLaunchHostNotification(string strActiveHost)
            {
                Trace.TraceError($"VpnWebLaunchHostNotification not implemented: {strActiveHost}");
            }

            public void VpnEventAvailableNotification()
            {
                Trace.TraceError($"VpnEventAvailableNotification not implemented ... trying VpnApi.ProcessEvents()");
                _model._vpnApi.ProcessEvents();
            }

            public void VpnCertBlockedNotification(string strUntrustedServer)
            {
                Trace.TraceError($"VpnCertBlockedNotification: {strUntrustedServer}");
            }

            public void VpnCertWarningNotification(string strUntrustedServer, IStringCollection pCertErrors, bool bImportAllowed)
            {
                Trace.TraceError($"VpnCertWarningNotification: {strUntrustedServer}");
                foreach (string pCertError in pCertErrors)
                {
                    Trace.TraceError(pCertError);
                }
            }
        }

        public virtual void OnNotice(string notice, NoticeType noticeType)
        {
            Notice?.Invoke(this, new NoticeEventArgs(notice, noticeType));
        }
        public event EventHandler<NoticeEventArgs> Notice; 

        private string OnGroupRequested(List<string> availableGroups)
        {
            GroupEventArgs gea = new GroupEventArgs(availableGroups);
            GroupRequested?.Invoke(this, gea);
            return gea.SelectedGroup;
        }
        public event EventHandler<GroupEventArgs> GroupRequested;
        
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
