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
        private readonly TypeReference _funcOfBoolTypeReference;
        private readonly MethodReference _funcOfBoolConstructorReference;
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
            _commandImplementationConstructors = GetCommandImplementationConstructors().ToList();
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

        internal IEnumerable<MethodReference> GetCommandImplementationConstructors()
        {
            var commandTypes =
                from @class in AllClasses
                where !@class.IsAbstract
                    //&& @class.Implements(ICommandTypeReference)
                    && @class.Name.EndsWith("Command")
                select @class;

            // TODO: My goodness the implementation below is HACKY... gotta add some smarts
            var elligibleCtors =
                from type in commandTypes
                from ctor in type.GetConstructors()
                where (ctor.HasParameters
                && ctor.Parameters.Count == 1
                && ctor.Parameters[0].ParameterType.FullNameMatches(ActionTypeReference))
                || (ctor.HasParameters
                && ctor.Parameters.Count == 2
                && ctor.Parameters[0].ParameterType.FullNameMatches(ActionTypeReference)
                && ctor.Parameters[1].ParameterType.Name.StartsWith("Func")
                )
                select ModuleDefinition.Import(ctor);

            return elligibleCtors;
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