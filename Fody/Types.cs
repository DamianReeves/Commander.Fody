using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Commander.Fody
{
    public interface ITypeReferences
    {
        TypeReference Action { get; }
        TypeReference ActionOfT { get; }
        TypeReference ArgumentNullException { get; }
        TypeReference Boolean { get; }
        TypeReference CommandManager { get; }
        TypeReference Delegate { get; }
        TypeReference EventHandler { get; }
        TypeReference FuncOfT { get; }
//// ReSharper disable InconsistentNaming
        TypeReference ICommand { get; }
//// ReSharper restore InconsistentNaming
        TypeReference Interlocked { get; }
        TypeReference Object { get; }
        TypeReference String { get; }
        TypeReference Void { get; }
        TypeReference PredicateOfT { get; }
    }

    public interface ITypeDefinitions
    {
        TypeDefinition Action { get; }
        TypeDefinition ActionOfT { get; }
        TypeDefinition ArgumentNullException { get; }
        TypeDefinition CommandManager { get; }
        TypeDefinition Delegate { get; }
        TypeDefinition EventHandler { get; }
        TypeDefinition FuncOfT { get; }
//// ReSharper disable InconsistentNaming
        TypeDefinition ICommand { get; }
//// ReSharper restore InconsistentNaming
        TypeDefinition Interlocked { get; }
        TypeDefinition Object { get; }
        TypeDefinition PredicateOfT { get; }
    }

    public class Types : ITypeReferences, ITypeDefinitions
    {
        private readonly ModuleWeaver _moduleWeaver;
        private readonly ModuleDefinition _moduleDefinition;

        private TypeReference _action;
        private TypeDefinition _actionDef;
        private TypeReference _actionOfT;
        private TypeDefinition _actionOfTDef;
        private TypeReference _argumentNullException;
        private TypeDefinition _argumentNullExceptionDef;
        private TypeReference _boolean;
        private TypeReference _commandManager;
        private TypeDefinition _commandManagerDef;
        private TypeReference _eventHandler;
        private TypeDefinition _eventHandlerDef;
        private TypeReference _funcOfT;
        private TypeDefinition _funcOfTDef;
        private TypeReference _iCommand;
        private TypeDefinition _iCommandDef;
        private TypeReference _interlocked;
        private TypeDefinition _interlockedDef;
        private TypeReference _object;
        private TypeDefinition _objectDef;
        private TypeReference _string;
        private TypeReference _void;
        private TypeReference _predicateOfT;
        private TypeDefinition _predicateOfTDef;
        private TypeReference _delegate;
        private TypeDefinition _delegateDef;

        public Types([NotNull] ModuleWeaver moduleWeaver)
        {            
            if (moduleWeaver == null)
            {
                throw new ArgumentNullException("moduleWeaver");
            }

            _moduleWeaver = moduleWeaver;
            var moduleDefinition = _moduleDefinition = moduleWeaver.ModuleDefinition;

            _string = moduleDefinition.TypeSystem.String;
            _void = moduleDefinition.TypeSystem.Void;
            _object = moduleDefinition.TypeSystem.Object;
            _boolean = moduleDefinition.TypeSystem.Boolean;

            var targetFramework = moduleDefinition.Assembly.GetTargetFramework();
            var assemblyResolver = ModuleDefinition.AssemblyResolver;
            var msCoreTypes = GetMscorlibTypes(targetFramework);

            var systemTypes = GetSystemTypes(targetFramework);            

            var objectDefinition = msCoreTypes.FirstOrDefault(x => x.Name == "Object")
                ?? systemTypes.FirstOrDefault(x => x.Name == "Object");
            if (objectDefinition == null)
            {
                ExecuteWinRT();
                return;
            }
            _objectDef = objectDefinition;
            _eventHandlerDef = msCoreTypes.First(x => x.Name == "EventHandler");
            _eventHandler = ModuleDefinition.Import(_eventHandlerDef);
            _delegateDef = msCoreTypes.First(x => x.Name == "Delegate");
            _delegate = ModuleDefinition.Import(_delegateDef);

            _interlockedDef = msCoreTypes.First(x => x.FullName == "System.Threading.Interlocked");
            _interlocked = ModuleDefinition.Import(_interlockedDef);

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

            actionDefinition = msCoreTypes.FirstOrDefault(x => x.Name == "Action`1");
            if (actionDefinition == null)
            {
                actionDefinition = systemTypes.FirstOrDefault(x => x.Name == "Action`1");
            }
            if (actionDefinition == null)
            {
                actionDefinition = systemCoreDefinition.MainModule.Types.First(x => x.Name == "Action`1");
            }
            _actionOfTDef = actionDefinition;
            _actionOfT = ModuleDefinition.Import(actionDefinition);

            var funcFilter = new Func<TypeDefinition, bool>(x => x.Name.StartsWith("Func") && x.HasGenericParameters && x.GenericParameters.Count == 1);
            var funcDefinition = msCoreTypes.FirstOrDefault(funcFilter);
            if (funcDefinition == null)
            {
                funcDefinition = systemTypes.FirstOrDefault(funcFilter);
            }
            if (funcDefinition == null)
            {
                funcDefinition = systemCoreDefinition.MainModule.Types.First(funcFilter);
            }
            _funcOfTDef = funcDefinition;
            _funcOfT = ModuleDefinition.Import(funcDefinition);

            var predicateFilter = new Func<TypeDefinition, bool>(x => x.Name.StartsWith("Predicate") && x.HasGenericParameters && x.GenericParameters.Count == 1);
            var predicateDefinition = msCoreTypes.FirstOrDefault(predicateFilter);
            if (predicateDefinition == null)
            {
                predicateDefinition = systemTypes.FirstOrDefault(predicateFilter);
            }
            if (predicateDefinition == null)
            {
                predicateDefinition = systemCoreDefinition.MainModule.Types.First(predicateFilter);
            }
            _predicateOfTDef = predicateDefinition;
            _predicateOfT = ModuleDefinition.Import(predicateDefinition);

            var argumentNullException = msCoreTypes.FirstOrDefault(x => x.Name == "ArgumentNullException");
            if (argumentNullException == null)
            {
                argumentNullException = systemTypes.First(x => x.Name == "ArgumentNullException");
            }
            _argumentNullExceptionDef = argumentNullException;
            _argumentNullException = ModuleDefinition.Import(argumentNullException);

            var commandPrimaryAssemblyDef = GetPrimaryICommandSearchLocation(targetFramework);
            var presentationCoreTypes = commandPrimaryAssemblyDef.MainModule.Types;
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
            if (commandManagerDefinition != null)
            {
                _commandManager = ModuleDefinition.Import(commandManagerDefinition);                       
            }            
        }

        TypeReference ITypeReferences.Action
        {
            get { return _action; }
        }        

        TypeReference ITypeReferences.ActionOfT
        {
            get { return _actionOfT; }
        }        

        TypeReference ITypeReferences.ArgumentNullException
        {
            get { return _argumentNullException; }
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

        TypeReference ITypeReferences.FuncOfT
        {
            get { return _funcOfT; }
        }

        TypeReference ITypeReferences.ICommand
        {
            get { return _iCommand; }
        }

        TypeReference ITypeReferences.Interlocked
        {
            get { return _interlocked; }
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

        TypeReference ITypeReferences.PredicateOfT
        {
            get { return _predicateOfT; }
        }

        TypeReference ITypeReferences.Delegate
        {
            get { return _delegate; }
        }

        #region ITypeDefinitions Implementation
        TypeDefinition ITypeDefinitions.Action
        {
            get { return _actionDef; }
        }

        TypeDefinition ITypeDefinitions.ActionOfT
        {
            get { return _actionOfTDef; }
        }

        TypeDefinition ITypeDefinitions.ArgumentNullException
        {
            get { return _argumentNullExceptionDef; }
        }

        TypeDefinition ITypeDefinitions.CommandManager
        {
            get { return _commandManagerDef; }
        }

        TypeDefinition ITypeDefinitions.EventHandler
        {
            get { return _eventHandlerDef; }
        }

        TypeDefinition ITypeDefinitions.FuncOfT
        {
            get { return _funcOfTDef; }
        }

        TypeDefinition ITypeDefinitions.ICommand
        {
            get { return _iCommandDef; }
        }

        TypeDefinition ITypeDefinitions.Interlocked
        {
            get { return _interlockedDef; }
        }

        TypeDefinition ITypeDefinitions.Object
        {
            get { return _objectDef; }
        }

        TypeDefinition ITypeDefinitions.PredicateOfT
        {
            get { return _predicateOfTDef; }
        }

        TypeDefinition ITypeDefinitions.Delegate
        {
            get { return _delegateDef; }
        }
        #endregion ITypeDefinitions Implementation

        public ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
        }                

        public void ExecuteWinRT()
        {
            var targetFramework = ModuleDefinition.Assembly.GetTargetFramework();
            var assemblyResolver = ModuleDefinition.AssemblyResolver;
            var systemRuntime = assemblyResolver.Resolve("System.Runtime");
            var systemRuntimeTypes = systemRuntime.MainModule.Types;

            var systemDefinition = assemblyResolver.Resolve("System");
            var systemTypes = systemDefinition.MainModule.Types;

            var objectDefinition = systemRuntimeTypes.First(x => x.Name == "Object");
            
            _objectDef = objectDefinition;
            _eventHandlerDef = systemRuntimeTypes.First(x => x.Name == "EventHandler");
            _eventHandler = ModuleDefinition.Import(_eventHandlerDef);
            _delegateDef = systemRuntimeTypes.First(x => x.Name == "Delegate");
            _delegate = ModuleDefinition.Import(_delegateDef);

            _interlockedDef = systemRuntimeTypes.First(x => x.FullName == "System.Threading.Interlocked");
            _interlocked = ModuleDefinition.Import(_interlockedDef);

            var actionDefinition = systemRuntimeTypes.FirstOrDefault(x => x.Name == "Action");
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

            actionDefinition = systemRuntimeTypes.FirstOrDefault(x => x.Name == "Action`1");
            if (actionDefinition == null)
            {
                actionDefinition = systemTypes.FirstOrDefault(x => x.Name == "Action`1");
            }
            if (actionDefinition == null)
            {
                actionDefinition = systemCoreDefinition.MainModule.Types.First(x => x.Name == "Action`1");
            }
            _actionOfTDef = actionDefinition;
            _actionOfT = ModuleDefinition.Import(actionDefinition);

            var funcFilter = new Func<TypeDefinition, bool>(x => x.Name.StartsWith("Func") && x.HasGenericParameters && x.GenericParameters.Count == 1);
            var funcDefinition = systemRuntimeTypes.FirstOrDefault(funcFilter);
            if (funcDefinition == null)
            {
                funcDefinition = systemTypes.FirstOrDefault(funcFilter);
            }
            if (funcDefinition == null)
            {
                funcDefinition = systemCoreDefinition.MainModule.Types.First(funcFilter);
            }
            _funcOfTDef = funcDefinition;
            _funcOfT = ModuleDefinition.Import(funcDefinition);

            var predicateFilter = new Func<TypeDefinition, bool>(x => x.Name.StartsWith("Predicate") && x.HasGenericParameters && x.GenericParameters.Count == 1);
            var predicateDefinition = systemRuntimeTypes.FirstOrDefault(predicateFilter);
            if (predicateDefinition == null)
            {
                predicateDefinition = systemTypes.FirstOrDefault(predicateFilter);
            }
            if (predicateDefinition == null)
            {
                predicateDefinition = systemCoreDefinition.MainModule.Types.First(predicateFilter);
            }
            _predicateOfTDef = predicateDefinition;
            _predicateOfT = ModuleDefinition.Import(predicateDefinition);

            var argumentNullException = systemRuntimeTypes.FirstOrDefault(x => x.Name == "ArgumentNullException");
            if (argumentNullException == null)
            {
                argumentNullException = systemTypes.First(x => x.Name == "ArgumentNullException");
            }
            _argumentNullExceptionDef = argumentNullException;
            _argumentNullException = ModuleDefinition.Import(argumentNullException);

            var systemObjectModelDef = assemblyResolver.Resolve("System.ObjectModel");
            var objectModelTypes  = systemObjectModelDef.MainModule.Types;
            var iCommandDefinition = objectModelTypes.FirstOrDefault(x => x.Name == "ICommand");
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
        }

        private IList<TypeDefinition> GetMscorlibTypes(string targetFramework)
        {
            targetFramework = targetFramework ?? string.Empty;
            //if (targetFramework.StartsWith("WindowsPhone"))
            //{
            //    return new TypeDefinition[] { };
            //}
            var assemblyResolver = ModuleDefinition.AssemblyResolver;
            var corlibFullName = (AssemblyNameReference)ModuleDefinition.TypeSystem.Corlib;
            try
            {
                var msCoreLibDefinition = assemblyResolver.Resolve(corlibFullName);
                var msCoreTypes = msCoreLibDefinition.MainModule.Types;
                return msCoreTypes;
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to load corlib assembly [{0}].", corlibFullName)
                    + Environment.NewLine
                    + "Error was:"+ex.ToString();
                throw new WeavingException(message);
            }
        }

        private IList<TypeDefinition> GetSystemTypes(string targetFramework)
        {
            targetFramework = targetFramework ?? string.Empty;
            if (targetFramework.StartsWith("WindowsPhone"))
            {
                //string message = "Could not find the System assembly for target framework ("+targetFramework+").";
                //throw new WeavingException(message);
                return new TypeDefinition[] { };
            }
            var assemblyResolver = ModuleDefinition.AssemblyResolver;
            var systemDef = assemblyResolver.Resolve("System");
            var types = systemDef.MainModule.Types;
            return types;
        } 

        private AssemblyDefinition GetPrimaryICommandSearchLocation(string targetFramework)
        {
            try
            {
                if (targetFramework.Contains("Portable") || targetFramework.Contains("Silverlight"))
                {                    
                    return ModuleDefinition.AssemblyResolver.Resolve("System.Windows");
                }

                if (targetFramework.Contains("WindowsPhone"))
                {
                    return ModuleDefinition.AssemblyResolver.Resolve("System");
                }
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