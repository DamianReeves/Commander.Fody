using System;
using System.Threading;
using System.Windows.Input;

namespace Commander.Commands
{
    public abstract class CommandBase : ICommand
    {
        private readonly WeakEvent<EventHandler> _canExecuteChanged = new WeakEvent<EventHandler>();

        internal CommandBase() { }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { _canExecuteChanged.Add(value); }
            remove { _canExecuteChanged.Remove(value); }
        }

        /// <summary>
        /// Called when <see cref="CanExecute"/> should be queried again.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            _canExecuteChanged.Invoke(h => h(this, EventArgs.Empty));
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public abstract bool CanExecute(object parameter);

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public abstract void Execute(object parameter);

        /// <summary>
        /// Attempts to convert a parameter to a typed value.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="parameter">The parameter.</param>
        /// <param name="typedParameter">The converted value.</param>
        /// <returns>Whether the conversion was successful.</returns>
        protected static bool TryConvert<T>(object parameter, out T typedParameter)
        {
            if (parameter is T || typeof(T).IsClass && parameter == null)
            {
                typedParameter = (T)parameter;
                return true;
            }

            if (typeof(IConvertible).IsAssignableFrom(typeof(T)) && parameter is IConvertible)
                try
                {
                    typedParameter = (T)Convert.ChangeType(parameter, typeof(T), Thread.CurrentThread.CurrentCulture);
                    return true;
                }
                catch { }

            typedParameter = default(T);
            return false;
        }
    }
}