using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public class CommandData
    {
        private readonly string _commandName;
        private readonly TypeDefinition _declaringType;
        private readonly List<MethodDefinition> _onExecuteMethods;
        private readonly List<MethodDefinition> _canExecuteMethods;        

        public CommandData([NotNull] TypeDefinition declaringType, [NotNull] string commandName)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }

            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }

            _declaringType = declaringType;
            _commandName = commandName;
            _onExecuteMethods = new List<MethodDefinition>();
            _canExecuteMethods = new List<MethodDefinition>();
        }

        public TypeDefinition DeclaringType
        {
            get { return _declaringType; }
        }

        public string CommandName
        {
            get { return _commandName; }
        }

        public bool PropertyInjectionRequired
        {
            get { return CommandProperty == null; }
        }        

        public List<MethodDefinition> OnExecuteMethods
        {
            get { return _onExecuteMethods; }
        }

        public List<MethodDefinition> CanExecuteMethods
        {
            get { return _canExecuteMethods; }
        }

        public bool PropertyInjectionApplied { get; set; }
        public PropertyDefinition CommandProperty { get; set; }
        public TypeReference DelegateCommandTypeReference { get; set; }
        public MethodReference DelegateCommandConstructorReference { get; set; }        

        public override string ToString()
        {
            return string.Format(CommandName);
        }
    }
}