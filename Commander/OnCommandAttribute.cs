using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commander
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class OnCommandAttribute : Attribute
    {
        private readonly string _commandName;

        public OnCommandAttribute(string commandName)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }
            _commandName = commandName;
        }

        public string CommandName
        {
            get { return _commandName; }
        }

        //public Type CommandImplementationType { get; set; }
    }
}
