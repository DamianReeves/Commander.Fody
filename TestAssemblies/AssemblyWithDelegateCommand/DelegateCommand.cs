using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

public class DelegateCommand : ICommand
{
    private readonly Func<object, bool> _canExecute;
    private readonly Action<object> _execute;

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public DelegateCommand(Action execute):this(execute, null)
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

    public DelegateCommand(Action<object> execute, Func<object,bool> canExecute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException("execute");
        }
        _execute = execute;
        _canExecute = canExecute ?? (obj => true);
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute(parameter);
    }    
}

public class RelayCommand<T> : ICommand
{
    #region Fields

    readonly Action<T> _execute;
    readonly Predicate<T> _canExecute;

    #endregion // Fields

    #region Constructors

    public RelayCommand(Action<T> execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<T> execute, Predicate<T> canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");

        _execute = execute;
        _canExecute = canExecute;
    }

    #endregion // Constructors

    #region ICommand Members

    [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute((T)parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }

    #endregion // ICommand Members
}
