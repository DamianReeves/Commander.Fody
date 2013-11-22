using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Commander.Commands
{
    /// <summary>
    /// Wraps a delegate as an <see cref="ICommand"/>.
    /// </summary>
    public class DelegateCommand : CommandBase
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public DelegateCommand(Action execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _execute = obj => execute();
            _canExecute = canExecute == null
                ? new Func<object, bool>(obj => true)
                : (obj => canExecute());
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _execute = execute;
            _canExecute = canExecute ?? (obj => true);
        }

        public override void Execute(object parameter)
        {
            _execute(parameter);
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
