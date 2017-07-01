using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public class CommandData
    {
        public CommandData([NotNull] TypeDefinition declaringType, [NotNull] string commandName)
        {
            DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
            OnExecuteMethods = new List<MethodDefinition>();
            CanExecuteMethods = new List<MethodDefinition>();
        }

        public TypeDefinition DeclaringType { get; }

        public string CommandName { get; }

        public bool PropertyInjectionRequired => CommandProperty == null;

        public List<MethodDefinition> OnExecuteMethods { get; }

        public List<MethodDefinition> CanExecuteMethods { get; }

        public bool PropertyInjectionApplied { get; set; }
        public bool CommandInitializationInjected { get; set; }
        public bool UsesNestedCommand { get; set; }
        public PropertyDefinition CommandProperty { get; set; }
        public TypeReference DelegateCommandTypeReference { get; set; }
        public MethodReference DelegateCommandConstructorReference { get; set; }        

        public override string ToString()
        {
            return string.Format(CommandName);
        }
    }
}