using System;
using System.Windows.Input;

public class ExampleCommandClassWithInitializer
{
    public event EventHandler CommandsInitialized;

    protected virtual void OnCommandsInitialized()
    {
        EventHandler handler = CommandsInitialized;
        if (handler != null) handler(this, EventArgs.Empty);
    }

    public ExampleCommandClassWithInitializer()
    {
        this.InitializeCommands();
    }

    private void InitializeCommands()
    {
        if (TestCommand == null)
        {
            TestCommand = new DelegateCommand(OnTestCommand);
        }

        if (SubmitCommand == null)
        {
            SubmitCommand = new DelegateCommand(OnSubmit, CanSubmit);
        }

        if (NullCommand == null)
        {
            NullCommand = null;
        }
    }

    public ICommand TestCommand { get; set; }
    public ICommand SubmitCommand { get; set; }
    public ICommand NullCommand { get; set; }
    public ICommand NestedCommand { get; set; }

    public void OnTestCommand()
    {        
    }

    public bool CanSubmit()
    {
        return true;
    }

    public void OnSubmit()
    {        
    }

    public bool CanExecuteNestedCommand()
    {
        return true;
    }

    public void OnExecuteNestedCommand(object parameter)
    {        
    }

    private class NestedCommandImplementation : ICommand
    {
        private readonly ExampleCommandClassWithInitializer _owner;

        public NestedCommandImplementation(ExampleCommandClassWithInitializer owner)
        {
            _owner = owner;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _owner.OnExecuteNestedCommand(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _owner.CanExecuteNestedCommand();
        }        
    }
}