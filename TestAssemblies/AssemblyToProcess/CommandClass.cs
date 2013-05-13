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
    public void OnTestCommandWithParameter(object parameter)
    {
        
    }
}