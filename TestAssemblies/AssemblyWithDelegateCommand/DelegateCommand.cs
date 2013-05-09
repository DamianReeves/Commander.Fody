using System;
using System.Collections.Generic;
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

    public void Execute(object parameter)
    {
        
    }

    public bool CanExecute(object parameter)
    {
        throw new NotImplementedException();
    }    
}
