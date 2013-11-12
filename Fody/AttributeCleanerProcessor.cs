using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Commander.Fody
{
    public class AttributeCleanerProcessor : IModuleProcessor
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly IFodyLogger _logger;

        private readonly List<string> _propertyAttributeNames = new List<string>
        {
            "Commander.OnCommandAttribute",
            "Commander.OnCommandCanExecuteAttribute",
        };

        public AttributeCleanerProcessor(ModuleDefinition moduleDefinition, IFodyLogger logger)
        {
            _moduleDefinition = moduleDefinition;
            _logger = logger;
        }

        public IFodyLogger Logger
        {
            get { return _logger; }
        }

        public ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
        }

        public void Execute()
        {
            CleanAttributes();
        }        

        void ProcessType(TypeDefinition type)
        {
            RemoveAttributes(type.CustomAttributes);
            foreach (var property in type.Properties)
            {
                RemoveAttributes(property.CustomAttributes);
            }
            foreach (var field in type.Fields)
            {
                RemoveAttributes(field.CustomAttributes);
            }
        }

        void RemoveAttributes(Collection<CustomAttribute> customAttributes)
        {
            var attributes = customAttributes
                .Where(attribute => _propertyAttributeNames.Contains(attribute.Constructor.DeclaringType.FullName));

            foreach (var customAttribute in attributes.ToList())
            {
                customAttributes.Remove(customAttribute);
            }
        }

        public void CleanAttributes()
        {
            foreach (var type in ModuleDefinition.GetTypes())
            {
                ProcessType(type);
            }
        }
    }
}