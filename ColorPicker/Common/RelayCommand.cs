using System;
using System.Windows.Input;

namespace AutomatedPackagingSystem.Common
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _handler;
        private bool _isEnabled;
        public Key GestureKey { get; set; }
        //public Input MouseKey { get; set; }

        public RelayCommand(Action<object> handler, bool canExecute = true)
        {
            _handler = handler;
            _isEnabled = canExecute;
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    if (CanExecuteChanged != null)
                    {
                        CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _handler(parameter);
        }
    }
}
