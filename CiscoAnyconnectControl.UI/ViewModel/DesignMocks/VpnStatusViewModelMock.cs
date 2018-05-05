using System.ComponentModel;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.UI.Command;

namespace CiscoAnyconnectControl.UI.ViewModel.DesignMocks
{
    class VpnStatusViewModelMock : IVpnStatusViewModel
    {
        public bool ActionButtonEnabled => true;
        public string ActionButtonText => "Connect";
        public string Color => "purple";
        public RelayCommand CurrentActionCommand => RelayCommand.Empty;
        public string Message => "This is a design mock.";
        public string Status => "DesignStatus";
        public string TimeConnected => "00:00:00";
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
