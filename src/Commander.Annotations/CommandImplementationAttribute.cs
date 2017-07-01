using System;

namespace Commander
{
    /// <summary>
    /// An attribute used to denote the command implementation type to generate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = true)]
    public class CommandImplementationAttribute : Attribute
    {
        private readonly Type _commandImplementationType;

        /// <summary>
        /// Creates the command implementation type attribute.
        /// </summary>
        /// <param name="commandImplementationType"></param>
        public CommandImplementationAttribute(Type commandImplementationType)
        {
            _commandImplementationType = commandImplementationType;
        }

        /// <summary>
        /// The type that is used for the command implementation.
        /// </summary>
        public Type CommandImplementationType
        {
            get { return _commandImplementationType; }
        }
    }
}