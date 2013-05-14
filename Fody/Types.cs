using System;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public interface ITypeReferences
    {
        TypeReference Action { get; }
        TypeReference Boolean { get; }
        TypeReference CommandManager { get; }
        TypeReference EventHandler { get; }
        TypeReference FuncOfBool { get; }
//// ReSharper disable InconsistentNaming
        TypeReference ICommand { get; }
//// ReSharper restore InconsistentNaming
        TypeReference Object { get; }
        TypeReference String { get; }
        TypeReference Void { get; }        
    }

    public interface ITypeDefinitions
    {
        TypeDefinition Action { get; }
        TypeDefinition CommandManager { get; }
        TypeDefinition EventHandler { get; }
        TypeDefinition FuncOfBool { get; }
//// ReSharper disable InconsistentNaming
        TypeDefinition ICommand { get; }
//// ReSharper restore InconsistentNaming
        TypeDefinition Object { get; }
    }

    public class Types : ITypeReferences, ITypeDefinitions
    {
        private readonly ModuleDefinition _moduleDefinition;

        private readonly TypeReference _action;
        private readonly TypeDefinition _actionDef;
        private readonly TypeReference _boolean;
        private readonly TypeReference _commandManager;
        private readonly TypeDefinition _commandManagerDef;
        private readonly TypeReference _eventHandler;
        private readonly TypeDefinition _eventHandlerDef;
        private readonly TypeReference _funcOfBool;
        private readonly TypeDefinition _funcOfBoolDef;
        private readonly TypeReference _iCommand;
        private readonly TypeDefinition _iCommandDef;
        private readonly TypeReference _object;
        private readonly TypeDefinition _objectDef;
        private readonly TypeReference _string;
        private readonly TypeReference _void;        

        public Types([NotNull] ModuleWeaver moduleWeaver)
        {
            if (moduleWeaver == null)
            {
                throw new ArgumentNullException("moduleWeaver");
            }
            var moduleDefinition = _moduleDefinition = moduleWeaver.ModuleDefinition;

            _string = moduleDefinition.TypeSystem.String;
            _void = moduleDefinition.TypeSystem.Void;
            _object = moduleDefinition.TypeSystem.Object;
            _boolean = moduleDefinition.TypeSystem.Boolean;

            var assemblyResolver = ModuleDefinition.AssemblyResolver;
            var msCoreLibDefinition = assemblyResolver.Resolve("mscorlib");
            var msCoreTypes = msCoreLibDefinition.MainModule.Types;

            var systemDefinition = assemblyResolver.Resolve("System");
            var systemTypes = systemDefinition.MainModule.Types;

            var objectDefinition = msCoreTypes.FirstOrDefault(x => x.Name == "Object");
            if (objectDefinition == null)
            {
                //ExecuteWinRT();
                //return;
            }
            _objectDef = objectDefinition;
            _eventHandlerDef = msCoreTypes.First(x => x.Name == "EventHandler");
            _eventHandler = ModuleDefinition.Import(_eventHandlerDef);

            var actionDefinition = msCoreTypes.FirstOrDefault(x => x.Name == "Action");
            if (actionDefinition == null)
            {
                actionDefinition = systemTypes.FirstOrDefault(x => x.Name == "Action");
            }
            var systemCoreDefinition = GetSystemCoreDefinition();
            if (actionDefinition == null)
            {
                actionDefinition = systemCoreDefinition.MainModule.Types.First(x => x.Name == "Action");
            }
            _actionDef = actionDefinition;
            _action = ModuleDefinition.Import(actionDefinition);

            var funcDefinition = msCoreTypes.FirstOrDefault(x => x.Name.StartsWith("Func") && x.HasGenericParameters);
            if (funcDefinition == null)
            {
                funcDefinition = systemTypes.FirstOrDefault(x => x.Name == "Func");
            }
            if (funcDefinition == null)
            {
                funcDefinition = systemCoreDefinition.MainModule.Types.First(x => x.Name == "Func");
            }
            _funcOfBoolDef = funcDefinition;
            _funcOfBool = ModuleDefinition.Import(funcDefinition);

            var presentationCoreDefinition = GetPresentationCoreDefinition();
            var presentationCoreTypes = presentationCoreDefinition.MainModule.Types;
            var iCommandDefinition = presentationCoreTypes.FirstOrDefault(x => x.Name == "ICommand");
            if (iCommandDefinition == null)
            {
                iCommandDefinition = systemTypes.FirstOrDefault(x => x.Name == "ICommand");
            }
            _iCommandDef = iCommandDefinition;
            _iCommand = ModuleDefinition.Import(iCommandDefinition);
            if (_iCommand == null)
            {
                const string message = "Could not find type System.Windows.Input.ICommand.";
                throw new WeavingException(message);
            }
            var commandManagerDefinition = presentationCoreTypes.FirstOrDefault(x => x.Name == "CommandManager");
            if (commandManagerDefinition == null)
            {
                commandManagerDefinition = systemTypes.FirstOrDefault(x => x.Name == "CommandManager");
            }
            _commandManagerDef = commandManagerDefinition;
            _commandManager = ModuleDefinition.Import(commandManagerDefinition);            
        }

        TypeReference ITypeReferences.Action
        {
            get { return _action; }
        }        

        TypeReference ITypeReferences.Boolean
        {
            get { return _boolean; }
        }

        TypeReference ITypeReferences.CommandManager
        {
            get { return _commandManager; }
        }        

        TypeReference ITypeReferences.EventHandler
        {
            get { return _eventHandler; }
        }

        TypeReference ITypeReferences.FuncOfBool
        {
            get { return _funcOfBool; }
        }

        TypeReference ITypeReferences.ICommand
        {
            get { return _iCommand; }
        }

        TypeReference ITypeReferences.Object
        {
            get { return _object; }
        }

        TypeReference ITypeReferences.String
        {
            get { return _string; }
        }

        TypeReference ITypeReferences.Void
        {
            get { return _void; }
        }

        #region ITypeDefinitions Implementation
        TypeDefinition ITypeDefinitions.Action
        {
            get { return _actionDef; }
        }

        TypeDefinition ITypeDefinitions.CommandManager
        {
            get { return _commandManagerDef; }
        }

        TypeDefinition ITypeDefinitions.EventHandler
        {
            get { return _eventHandlerDef; }
        }

        TypeDefinition ITypeDefinitions.FuncOfBool
        {
            get { return _funcOfBoolDef; }
        }

        TypeDefinition ITypeDefinitions.ICommand
        {
            get { return _iCommandDef; }
        }

        TypeDefinition ITypeDefinitions.Object
        {
            get { return _objectDef; }
        }
        #endregion ITypeDefinitions Implementation

        public ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
        }        

        private AssemblyDefinition GetPresentationCoreDefinition()
        {
            try
            {
                return ModuleDefinition.AssemblyResolver.Resolve("PresentationCore");
            }
            catch (Exception exception)
            {
                var message = string.Format(@"Could not resolve PresentationCore. Please ensure you are using .net 3.5 or higher.{0}Inner message:{1}.", Environment.NewLine, exception.Message);
                message += " AssemblyResolver is: " + ModuleDefinition.AssemblyResolver.GetType().FullName;
                throw new WeavingException(message);
            }
        }

        private AssemblyDefinition GetSystemCoreDefinition()
        {
            try
            {
                return ModuleDefinition.AssemblyResolver.Resolve("System.Core");
            }
            catch (Exception exception)
            {
                var message = string.Format(@"Could not resolve System.Core. Please ensure you are using .net 3.5 or higher.{0}Inner message:{1}.", Environment.NewLine, exception.Message);
                throw new WeavingException(message);
            }
        }  
    }
}