using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Commander.Fody
{
    public class Assets
    {                

        private readonly Lazy<List<TypeDefinition>> _allClasses;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly IFodyLogger _log;

        private readonly TypeReference _stringTypeReference;
        private readonly TypeReference _voidTypeReference;
        private readonly TypeReference _objectTypeReference;
        private readonly TypeReference _booleanTypeReference;
        private readonly TypeReference _iCommandTypeReference;
        private readonly TypeReference _commandManagerTypeReference;
        private readonly TypeReference _funcOfBoolTypeReference;
        private readonly TypeReference _eventHandlerTypeReference;
        private readonly MethodReference _funcOfBoolConstructorReference;
        private readonly MethodReference _objectConstructorReference;
        private readonly MethodReference _commandManagerAddRequerySuggestedMethodReference;
        private readonly MethodReference _commandManagerRemoveRequerySuggestedMethodReference;
        private readonly IList<MethodReference> _commandImplementationConstructors;

        public Assets([NotNull] ModuleDefinition moduleDefinition, [NotNull] IFodyLogger log)
        {
            if (moduleDefinition == null)
            {
                throw new ArgumentNullException("moduleDefinition");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            _moduleDefinition = moduleDefinition;
            _log = log;
            _allClasses = new Lazy<List<TypeDefinition>>(()=> ModuleDefinition.GetTypes().Where(x => x.IsClass).ToList());

            _stringTypeReference = moduleDefinition.TypeSystem.String;
            _voidTypeReference = moduleDefinition.TypeSystem.Void;
            _objectTypeReference = moduleDefinition.TypeSystem.Object;            
            _booleanTypeReference = moduleDefinition.TypeSystem.Boolean;

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
            var constructorDefinition = objectDefinition.Methods.First(x => x.IsConstructor);
            _objectConstructorReference = ModuleDefinition.Import(constructorDefinition);

            var eventHandlerDefinition = msCoreTypes.First(x => x.Name == "EventHandler");
            _eventHandlerTypeReference = ModuleDefinition.Import(eventHandlerDefinition);

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
            ActionTypeReference = ModuleDefinition.Import(actionDefinition);

            var actionConstructor = actionDefinition.Methods.First(x => x.IsConstructor);
            ActionConstructorReference = ModuleDefinition.Import(actionConstructor);

            var funcDefinition = msCoreTypes.FirstOrDefault(x => x.Name.StartsWith("Func") && x.HasGenericParameters);
            if (funcDefinition == null)
            {
                funcDefinition = systemTypes.FirstOrDefault(x => x.Name == "Func");
            }
            if (funcDefinition == null)
            {
                funcDefinition = systemCoreDefinition.MainModule.Types.First(x => x.Name == "Func");
            }
            _funcOfBoolTypeReference = ModuleDefinition.Import(funcDefinition);
            var funcConstructor = funcDefinition.Resolve().Methods.First(m => m.IsConstructor && m.Parameters.Count == 2);
            _funcOfBoolConstructorReference = ModuleDefinition.Import(funcConstructor).MakeHostInstanceGeneric(BooleanTypeReference);
            var presentationCoreDefinition = GetPresentationCoreDefinition();
            var presentationCoreTypes = presentationCoreDefinition.MainModule.Types;
            var iCommandDefinition = presentationCoreTypes.FirstOrDefault(x => x.Name == "ICommand");
            if (iCommandDefinition == null)
            {
                iCommandDefinition = systemTypes.FirstOrDefault(x => x.Name == "ICommand");
            }
            _iCommandTypeReference = ModuleDefinition.Import(iCommandDefinition);
            if (_iCommandTypeReference == null)
            {
                const string message = "Could not find type System.Windows.Input.ICommand.";
                throw new WeavingException(message);
            }
            var commandManagerDefinition = presentationCoreTypes.FirstOrDefault(x => x.Name == "CommandManager");
            if (commandManagerDefinition == null)
            {
                commandManagerDefinition = systemTypes.FirstOrDefault(x => x.Name == "CommandManager");
            }
            _commandManagerTypeReference = ModuleDefinition.Import(commandManagerDefinition);
            if (commandManagerDefinition != null && _commandManagerTypeReference != null)
            {
                var requeryEvent = commandManagerDefinition.Resolve().Events.Single(evt => evt.Name == "RequerySuggested");
                _commandManagerAddRequerySuggestedMethodReference = ModuleDefinition.Import(requeryEvent.AddMethod);
                _commandManagerRemoveRequerySuggestedMethodReference = ModuleDefinition.Import(requeryEvent.RemoveMethod);
            }
            _commandImplementationConstructors = GetCommandImplementationConstructors();
        }

        public MethodReference ActionConstructorReference { get; private set; }

        public TypeReference ActionTypeReference { get; private set; }

        public ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
        }

        public IFodyLogger Log
        {
            get { return _log; }
        }

        public List<TypeDefinition> AllClasses
        {
            get { return _allClasses.Value; }
        }

        public TypeReference StringTypeReference
        {
            get { return _stringTypeReference; }
        }

        public TypeReference VoidTypeReference
        {
            get { return _voidTypeReference; }
        }

        public TypeReference ObjectTypeReference
        {
            get { return _objectTypeReference; }
        }

        public TypeReference BooleanTypeReference
        {
            get { return _booleanTypeReference; }
        }

        public TypeReference ICommandTypeReference
        {
            get { return _iCommandTypeReference; }
        }

        public IList<MethodReference> CommandImplementationConstructors
        {
            get { return _commandImplementationConstructors; }
        }

        public TypeReference FuncOfBoolTypeReference
        {
            get { return _funcOfBoolTypeReference; }
        }

        public MethodReference FuncOfBoolConstructorReference
        {
            get { return _funcOfBoolConstructorReference; }
        }

        public MethodReference ObjectConstructorReference
        {
            get { return _objectConstructorReference; }
        }

        public TypeReference EventHandlerTypeReference
        {
            get { return _eventHandlerTypeReference; }
        }

        public TypeReference CommandManagerTypeReference
        {
            get { return _commandManagerTypeReference; }
        }

        public MethodReference CommandManagerAddRequerySuggestedMethodReference
        {
            get { return _commandManagerAddRequerySuggestedMethodReference; }
        }

        public MethodReference CommandManagerRemoveRequerySuggestedMethodReference
        {
            get { return _commandManagerRemoveRequerySuggestedMethodReference; }
        }

        internal IList<MethodReference> GetCommandImplementationConstructors()
        {
            var commandTypes =
                from @class in AllClasses
                where !@class.IsAbstract
                    //&& @class.Implements(ICommandTypeReference)
                    && @class.Name.EndsWith("Command")
                select @class;

            // TODO: My goodness the implementation below is HACKY... gotta add some smarts
            var eligibleCtors =
                from type in commandTypes
                from ctor in type.GetConstructors()
                where (ctor.HasParameters
                && ctor.Parameters.Count == 1
                && ctor.Parameters[0].ParameterType.FullNameMatches(ActionTypeReference))
                || (ctor.HasParameters
                && ctor.Parameters.Count == 2
                && ctor.Parameters[0].ParameterType.FullNameMatches(ActionTypeReference)
                && ctor.Parameters[1].ParameterType.Name.StartsWith("Func") 
                && ctor.Parameters[1].ParameterType.IsGenericInstance
                )
                select ModuleDefinition.Import(ctor);

            var ctors = eligibleCtors.ToList();
            foreach (var ctor in ctors)
            {
                Log.Info("Found eligible ICommand implementation constructor: {0}", ctor);
            }
            return ctors;
        }

        AssemblyDefinition GetPresentationCoreDefinition()
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

        AssemblyDefinition GetSystemCoreDefinition()
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