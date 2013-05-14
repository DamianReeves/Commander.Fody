using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CommandClass
{
    public string Name { get; set; }

    public void NotCommandMethod()
    {
    }

    [OnCommand("TestCommand")]
    public void OnTestCommand()
    {

    }

    [OnCommand("TestCommand2")]
    [OnCommand("MixedParameterCommand")]
    public void OnTestCommandWithParameter(object parameter)
    {        
    }

    [OnCommandCanExecute("MixedParameterCommand")]
    public bool CanExecuteNoParameter()
    {
        return true;
    }
}