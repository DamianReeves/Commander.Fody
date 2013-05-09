using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CommandClass
{
    public void NotCommandMethod()
    {
    }

    [OnCommand("TestCommand")]
    public void OnTestCommand()
    {

    }
}