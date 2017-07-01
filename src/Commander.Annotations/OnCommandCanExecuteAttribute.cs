using System;

namespace Commander
{
    /// <summary>
    /// Creates an attribute that is used to conditionally allow command execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class OnCommandCanExecuteAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandName"></param>
        public OnCommandCanExecuteAttribute(string commandName)
        {
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
        }

        /// <summary>
        /// Gets the command name that is associated.
        /// </summary>
        public string CommandName { get; }
    }
}