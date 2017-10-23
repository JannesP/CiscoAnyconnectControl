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


        public bool CanExecute(object parameter)
        {
            return this._canExecuteInternal();
        }

        public void Execute(object parameter)
        {
            this._executeInternal();
        }

        private static ICommand _emptyCommand;
        public static ICommand Empty
        {
            get { return _emptyCommand ?? (_emptyCommand = new RelayCommand(() => true, () => { })); }
        }
    }
}
