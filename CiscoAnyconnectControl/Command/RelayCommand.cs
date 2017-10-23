using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CiscoAnyconnectControl.Command
{
    class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged { add { } remove { } }

        private readonly Action _executeInternal;
        private readonly Func<bool> _canExecuteInternal;

        public RelayCommand(Func<bool> canExecute, Action action)
        {
            this._executeInternal = action;
            this._canExecuteInternal = canExecute;
        }


        public bool CanExecute(object parameter = null)
        {
            return this._canExecuteInternal();
        }

        public void Execute(object parameter = null)
        {
            this._executeInternal();
        }

        private static RelayCommand _emptyCommand;
        public static RelayCommand Empty
        {
            get { return _emptyCommand ?? (_emptyCommand = new RelayCommand(() => true, () => { })); }
        }
    }
}
