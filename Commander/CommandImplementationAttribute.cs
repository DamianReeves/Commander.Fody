using System;

namespace Commander
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = true)]
    public class CommandImplementationAttribute : Attribute
    {
        private readonly Type _commandImplementationType;

        public CommandImplementationAttribute(Type commandImplementationType)
        {
            _commandImplementationType = commandImplementationType;
        }

        public Type CommandImplementationType
        {
            get { return _commandImplementationType; }
        }
    }
}