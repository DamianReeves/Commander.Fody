using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commander
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class OnCommandAttribute : Attribute
    {
        public OnCommandAttribute(string commandName)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class OnCommandCanExecuteAttribute : Attribute
    {
        public OnCommandCanExecuteAttribute(string commandName)
        {
        }
    }
}
