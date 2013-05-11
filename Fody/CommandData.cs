using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public class CommandData
    {
        private readonly string _commandName;

        public bool InjectionRequired;    
        public PropertyDefinition CommandProperty;
        private readonly List<MethodDefinition> _onExecuteMethods;
        private readonly List<MethodDefinition> _canExecuteMethods;
        public MethodDefinition CanExecuteMethod; 
        public TypeReference DelegateCommandTypeReference;
        public MethodReference DelegateCommandConstructorReference;

        public CommandData([NotNull] string commandName)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }

            _commandName = commandName;
            _onExecuteMethods = new List<MethodDefinition>();
            _canExecuteMethods = new List<MethodDefinition>();
        }

        public string CommandName
        {
            get { return _commandName; }
        }

        public List<MethodDefinition> OnExecuteMethods
        {
            get { return _onExecuteMethods; }
        }

        public List<MethodDefinition> CanExecuteMethods
        {
            get { return _canExecuteMethods; }
        }

        public override string ToString()
        {
            return string.Format(CommandName);
        }
    }
}