using System;

namespace Commander
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class OnCommandCanExecuteAttribute : Attribute
    {
        private readonly string _commandName;

        public OnCommandCanExecuteAttribute(string commandName)
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
    }
}