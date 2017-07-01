using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commander
{
    /// <summary>
    /// Marks a method as a command handler which will be bound to an ICommand.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class OnCommandAttribute : Attribute
    {
        /// <summary>
        /// Creates the attribute with a command with the given name.
        /// </summary>
        /// <param name="commandName"></param>
        public OnCommandAttribute(string commandName)
        {
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
        }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string CommandName { get; }
    }
}
