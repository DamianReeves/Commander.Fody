using System;
using System.Collections.Concurrent;
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
        private readonly ITypeReferences _typeReferences;
        private readonly ITypeDefinitions _typeDefinitions;
        private readonly ConcurrentDictionary<string, CommandData> _commands;

        private readonly MethodReference _funcOfBoolConstructorReference;
        private readonly MethodReference _objectConstructorReference;
        private readonly MethodReference _commandManagerAddRequerySuggestedMethodReference;
        private readonly MethodReference _commandManagerRemoveRequerySuggestedMethodReference;
        private readonly MethodReference _actionOfTInvokeReference;
        private readonly MethodReference _actionOfTConstructorReference;
        private readonly MethodReference _predicateOfTInvokeReference;
        private readonly MethodReference _predicateOfTConstructorReference;
        private readonly MethodReference _argumentNullExceptionConstructorReference;
        private readonly MethodReference _delegateCombineMethodReference;
        private readonly MethodReference _delegateRemoveMethodReference;
        private readonly MethodReference _interlockedCompareExchangeOfT;
        private readonly GenericInstanceMethod _interlockedCompareExchangeOfEventHandler;
        private readonly IList<MethodReference> _commandImplementationConstructors;        

        public Assets([NotNull] ModuleWeaver moduleWeaver)
        {
            if (moduleWeaver == null)
            {
                throw new ArgumentNullException("moduleWeaver");
            }

            _commands = new ConcurrentDictionary<string, CommandData>();
            _moduleDefinition = moduleWeaver.ModuleDefinition;
            _log = moduleWeaver;
            var settings = moduleWeaver.Settings;
            _allClasses = new Lazy<List<TypeDefinition>>(()=> settings.GetAllTypes(moduleWeaver).ToList());
            var types = new Types(moduleWeaver);
            _typeReferences = types;
            _typeDefinitions = types;
            
            var constructorDefinition = TypeDefinitions.Object.Methods.First(x => x.IsConstructor);
            _objectConstructorReference = ModuleDefinition.ImportReference(constructorDefinition);
            var actionConstructor = TypeDefinitions.Action.Methods.First(x => x.IsConstructor);
            ActionConstructorReference = ModuleDefinition.ImportReference(actionConstructor);
            var actionOfTConstructor = TypeDefinitions.ActionOfT.GetConstructors().First();
            _actionOfTConstructorReference = ModuleDefinition.ImportReference(actionOfTConstructor);
            var actionOfTInvokerDefinition = TypeDefinitions.ActionOfT.Methods.First(x => x.Name == "Invoke");
            _actionOfTInvokeReference = ModuleDefinition.ImportReference(actionOfTInvokerDefinition);            
            var funcConstructor = TypeDefinitions.FuncOfT.Resolve().Methods.First(m => m.IsConstructor && m.Parameters.Count == 2);
            _funcOfBoolConstructorReference = ModuleDefinition.ImportReference(funcConstructor).MakeHostInstanceGeneric(TypeReferences.Boolean);
            var predicateOfTConstructor = TypeDefinitions.PredicateOfT.GetConstructors().First();
            _predicateOfTConstructorReference = ModuleDefinition.ImportReference(predicateOfTConstructor);
            var predicateOfTInvokerDefinition = TypeDefinitions.PredicateOfT.Methods.First(x => x.Name == "Invoke");
            _predicateOfTInvokeReference = ModuleDefinition.ImportReference(predicateOfTInvokerDefinition);
            var delegateCombineDefinition = TypeDefinitions.Delegate.Methods.First(x => x.Name == "Combine" && x.Parameters.Count == 2);
            _delegateCombineMethodReference = ModuleDefinition.ImportReference(delegateCombineDefinition);
            var delegateRemoveDefinition = TypeDefinitions.Delegate.Methods.First(x => x.Name == "Remove" && x.Parameters.Count == 2);
            _delegateRemoveMethodReference = ModuleDefinition.ImportReference(delegateRemoveDefinition);

            var interlockedCompareExchangeMethodDefinition = TypeDefinitions.Interlocked.Methods.First(
                x => x.Name == "CompareExchange"
                     && x.Parameters.Count == 3
                     && x.HasGenericParameters
            );
            _interlockedCompareExchangeOfT = ModuleDefinition.ImportReference(interlockedCompareExchangeMethodDefinition);
            _interlockedCompareExchangeOfEventHandler = new GenericInstanceMethod(_interlockedCompareExchangeOfT);
            _interlockedCompareExchangeOfEventHandler.GenericArguments.Add(TypeReferences.EventHandler);
            //_interlockedCompareExchangeOfEventHandler = 
            if (TypeDefinitions.CommandManager != null)
            {
                var requeryEvent = TypeDefinitions.CommandManager.Resolve().Events.Single(evt => evt.Name == "RequerySuggested");
                _commandManagerAddRequerySuggestedMethodReference = ModuleDefinition.ImportReference(requeryEvent.AddMethod);
                _commandManagerRemoveRequerySuggestedMethodReference = ModuleDefinition.ImportReference(requeryEvent.RemoveMethod);
            }
            _commandImplementationConstructors = GetCommandImplementationConstructors();

            constructorDefinition =
                TypeDefinitions.ArgumentNullException.Methods.Single(
                    x => x.IsConstructor && x.HasParameters && x.Parameters.Count == 1);
            _argumentNullExceptionConstructorReference = ModuleDefinition.ImportReference(constructorDefinition);
        }
        
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

        public ITypeReferences TypeReferences
        {
            get { return _typeReferences; }
        }

        public ITypeDefinitions TypeDefinitions
        {
            get { return _typeDefinitions; }
        }

        public MethodReference ActionConstructorReference { get; private set; }

        public IList<MethodReference> CommandImplementationConstructors
        {
            get { return _commandImplementationConstructors; }
        }        

        public MethodReference FuncOfBoolConstructorReference
        {
            get { return _funcOfBoolConstructorReference; }
        }

        public MethodReference ObjectConstructorReference
        {
            get { return _objectConstructorReference; }
        }        

        public MethodReference CommandManagerAddRequerySuggestedMethodReference
        {
            get { return _commandManagerAddRequerySuggestedMethodReference; }
        }

        public MethodReference CommandManagerRemoveRequerySuggestedMethodReference
        {
            get { return _commandManagerRemoveRequerySuggestedMethodReference; }
        }

        public MethodReference ActionOfTConstructorReference
        {
            get { return _actionOfTConstructorReference; }
        }

        public MethodReference ActionOfTInvokeReference
        {
            get { return _actionOfTInvokeReference; }
        }

        public MethodReference ArgumentNullExceptionConstructorReference
        {
            get { return _argumentNullExceptionConstructorReference; }
        }

        public MethodReference PredicateOfTInvokeReference
        {
            get { return _predicateOfTInvokeReference; }
        }

        public ConcurrentDictionary<string, CommandData> Commands
        {
            get { return _commands; }
        }

        public bool DelegateCommandImplementationWasInjected { get; set; }

        public MethodReference PredicateOfTConstructorReference
        {
            get { return _predicateOfTConstructorReference; }
        }

        public MethodReference DelegateCombineMethodReference
        {
            get { return _delegateCombineMethodReference; }
        }

        public MethodReference DelegateRemoveMethodReference
        {
            get { return _delegateRemoveMethodReference; }
        }

        public MethodReference InterlockedCompareExchangeOfEventHandler
        {
            get { return _interlockedCompareExchangeOfEventHandler; }
        }

        public MethodReference InterlockedCompareExchangeOfT
        {
            get { return _interlockedCompareExchangeOfT; }
        }

        internal IList<MethodReference> GetCommandImplementationConstructors()
        {
            var commandTypes =
                from @class in AllClasses
                where !@class.IsAbstract
                    && @class.Implements(TypeReferences.ICommand)
                select @class;

            // TODO: My goodness the implementation below is HACKY... gotta add some smarts
            var eligibleCtors =
                from type in commandTypes
                from ctor in type.GetConstructors()
                where (ctor.HasParameters
                && ctor.Parameters.Count == 1
                && ctor.Parameters[0].ParameterType.FullNameMatches(TypeReferences.Action))
                || (ctor.HasParameters
                && ctor.Parameters.Count == 2
                && ctor.Parameters[0].ParameterType.FullNameMatches(TypeReferences.Action)
                && ctor.Parameters[1].ParameterType.Name.StartsWith("Func") 
                && ctor.Parameters[1].ParameterType.IsGenericInstance
                )
                || (ctor.HasParameters
                && ctor.Parameters.Count == 2
                && ctor.Parameters[0].ParameterType.FullNameMatches(TypeReferences.Action)
                && ctor.Parameters[0].ParameterType.IsGenericInstance
                && ctor.Parameters[1].ParameterType.Name == "Predicate`1" 
                && ctor.Parameters[1].ParameterType.IsGenericInstance
                )
                select ModuleDefinition.ImportReference(ctor);

            var ctors = eligibleCtors.ToList();
            foreach (var ctor in ctors)
            {
                Log.Info("Found eligible ICommand implementation constructor: {0}", ctor);
            }
            return ctors;
        }     
    }
}