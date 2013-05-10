using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Commander.Fody
{
    public class TypeProcessor : TypeProcessorBase
    {
        // TODO: Eventually change this to be configurable
        public const string OnCommandAttributeName = "OnCommandAttribute";
        public const string OnCommandCanExecuteAttributeName = "OnCommandCanExecuteAttribute";
        private readonly ConcurrentDictionary<string, CommandData> _commands;

        public TypeProcessor(TypeDefinition type, ModuleWeaver moduleWeaver) : base(type, moduleWeaver)
        {
            _commands = new ConcurrentDictionary<string, CommandData>();
        }

        public ConcurrentDictionary<string, CommandData> Commands
        {
            get { return _commands; }
        }

        public override void Execute()
        {
            ScanForOnCommandAttribute();
            ScanForOnCommandCanExecuteAttribute();
            InjectCommandProperties();
        }

        public IEnumerable<MethodDefinition> FindOnCommandMethods(TypeDefinition type)
        {
            return type.Methods.Where(method => method.CustomAttributes.ContainsAttribute(OnCommandAttributeName));
        }

        public IEnumerable<MethodDefinition> FindCommandCanExecuteMethods(TypeDefinition type)
        {
            return type.Methods.Where(method => method.CustomAttributes.ContainsAttribute(OnCommandCanExecuteAttributeName));
        }

        public bool IsValidOnExecuteMethod(MethodDefinition method)
        {
            return method.ReturnType.Matches(Assets.BooleanTypeReference)
                && (!method.HasParameters
                    || (method.Parameters.Count == 1 
                    && !method.Parameters[0].IsOut
                    && method.Parameters[0].ParameterType.Matches(Assets.ObjectTypeReference)));
        }

        public bool IsValidCanExecuteMethod(MethodDefinition method)
        {
            return method.ReturnType.Matches(Assets.VoidTypeReference)
                && (!method.HasParameters
                    || (method.Parameters.Count == 1
                    && !method.Parameters[0].IsOut
                    && method.Parameters[0].ParameterType.Matches(Assets.ObjectTypeReference)));
        }

        internal void ScanForOnCommandAttribute()
        {
            var methods = FindOnCommandMethods(Type);
            foreach (var method in methods)
            {
                if (!IsValidOnExecuteMethod(method))
                {
                    Assets.Log.Warning("Method: {0} is not a valid OnExecute method for ICommand binding..", method);
                }

                // Find OnCommand methods where name is given
                var attributes =
                    method.CustomAttributes
                    .Where(x => x.IsCustomAttribute(OnCommandAttributeName))
                    .Where(x => x.HasConstructorArguments 
                        && x.ConstructorArguments.First().Type.FullNameMatches(Assets.StringTypeReference));

                foreach (var attribute in attributes)
                {                    
                    var commandName = (string)attribute.ConstructorArguments[0].Value;
                    Assets.Log.Info("Found OnCommand method {0} for command {1} on type {2}"
                    , method
                    , commandName
                    , Type.Name);
                    var command = Commands.GetOrAdd(commandName, name => new CommandData(name));    
                    command.OnExecuteMethods.Add(method);
                }
            }
        }

        internal void ScanForOnCommandCanExecuteAttribute()
        {
            var methods = FindCommandCanExecuteMethods(Type);
            foreach (var method in methods)
            {
                if (!IsValidOnExecuteMethod(method))
                {
                    Assets.Log.Warning("Method: {0} is not a valid CanExecute method for ICommand binding.", method);
                }


                // Find OnCommandCanExecute methods where name is given
                var attributes =
                    method.CustomAttributes
                    .Where(x => x.IsCustomAttribute(OnCommandCanExecuteAttributeName))
                    .Where(x => x.HasConstructorArguments
                        && x.ConstructorArguments.First().Type.FullNameMatches(Assets.StringTypeReference));

                foreach (var attribute in attributes)
                {
                    var commandName = (string)attribute.ConstructorArguments[0].Value;
                    var command = Commands.GetOrAdd(commandName, name => new CommandData(name));
                    command.CanExecuteMethods.Add(method);
                }
            }
        }  
   
        internal void InjectCommandProperties()
        {
            var commandTypeReference = Assets.ICommandTypeReference;
            foreach (var commandData in Commands.Values)
            {
                try
                {
                    PropertyDefinition propertyDefinition;
                    if (Type.TryAddCommandProperty(commandTypeReference, commandData.CommandName, out propertyDefinition)) ;
                }
                catch (Exception ex)
                {
                    Assets.Log.Error("Error while adding property {0} to {1}: {2}", commandData.CommandName, Type, ex);
                }
            }
        }
    }
}