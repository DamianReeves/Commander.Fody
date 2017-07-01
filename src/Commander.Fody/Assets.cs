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

        private readonly GenericInstanceMethod _interlockedCompareExchangeOfEventHandler;

        public Assets([NotNull] ModuleWeaver moduleWeaver)
        {
            if (moduleWeaver == null)
            {
                throw new ArgumentNullException(nameof(moduleWeaver));
            }

            Commands = new ConcurrentDictionary<string, CommandData>();
            ModuleDefinition = moduleWeaver.ModuleDefinition;
            Log = moduleWeaver;
            var settings = moduleWeaver.Settings;
            _allClasses = new Lazy<List<TypeDefinition>>(()=> settings.GetAllTypes(moduleWeaver).ToList());
            var types = new Types(moduleWeaver);
            TypeReferences = types;
            TypeDefinitions = types;
            
            var constructorDefinition = TypeDefinitions.Object.Methods.First(x => x.IsConstructor);
            ObjectConstructorReference = ModuleDefinition.ImportReference(constructorDefinition);
            var actionConstructor = TypeDefinitions.Action.Methods.First(x => x.IsConstructor);
            ActionConstructorReference = ModuleDefinition.ImportReference(actionConstructor);
            var actionOfTConstructor = TypeDefinitions.ActionOfT.GetConstructors().First();
            ActionOfTConstructorReference = ModuleDefinition.ImportReference(actionOfTConstructor);
            var actionOfTInvokerDefinition = TypeDefinitions.ActionOfT.Methods.First(x => x.Name == "Invoke");
            ActionOfTInvokeReference = ModuleDefinition.ImportReference(actionOfTInvokerDefinition);            
            var funcConstructor = TypeDefinitions.FuncOfT.Resolve().Methods.First(m => m.IsConstructor && m.Parameters.Count == 2);
            FuncOfBoolConstructorReference = ModuleDefinition.ImportReference(funcConstructor).MakeHostInstanceGeneric(TypeReferences.Boolean);
            var predicateOfTConstructor = TypeDefinitions.PredicateOfT.GetConstructors().First();
            PredicateOfTConstructorReference = ModuleDefinition.ImportReference(predicateOfTConstructor);
            var predicateOfTInvokerDefinition = TypeDefinitions.PredicateOfT.Methods.First(x => x.Name == "Invoke");
            PredicateOfTInvokeReference = ModuleDefinition.ImportReference(predicateOfTInvokerDefinition);
            var delegateCombineDefinition = TypeDefinitions.Delegate.Methods.First(x => x.Name == "Combine" && x.Parameters.Count == 2);
            DelegateCombineMethodReference = ModuleDefinition.ImportReference(delegateCombineDefinition);
            var delegateRemoveDefinition = TypeDefinitions.Delegate.Methods.First(x => x.Name == "Remove" && x.Parameters.Count == 2);
            DelegateRemoveMethodReference = ModuleDefinition.ImportReference(delegateRemoveDefinition);

            var interlockedCompareExchangeMethodDefinition = TypeDefinitions.Interlocked.Methods.First(
                x => x.Name == "CompareExchange"
                     && x.Parameters.Count == 3
                     && x.HasGenericParameters
            );
            InterlockedCompareExchangeOfT = ModuleDefinition.ImportReference(interlockedCompareExchangeMethodDefinition);
            _interlockedCompareExchangeOfEventHandler = new GenericInstanceMethod(InterlockedCompareExchangeOfT);
            _interlockedCompareExchangeOfEventHandler.GenericArguments.Add(TypeReferences.EventHandler);
            //_interlockedCompareExchangeOfEventHandler = 
            if (TypeDefinitions.CommandManager != null)
            {
                var requeryEvent = TypeDefinitions.CommandManager.Resolve().Events.Single(evt => evt.Name == "RequerySuggested");
                CommandManagerAddRequerySuggestedMethodReference = ModuleDefinition.ImportReference(requeryEvent.AddMethod);
                CommandManagerRemoveRequerySuggestedMethodReference = ModuleDefinition.ImportReference(requeryEvent.RemoveMethod);
            }
            CommandImplementationConstructors = GetCommandImplementationConstructors();

            constructorDefinition =
                TypeDefinitions.ArgumentNullException.Methods.Single(
                    x => x.IsConstructor && x.HasParameters && x.Parameters.Count == 1);
            ArgumentNullExceptionConstructorReference = ModuleDefinition.ImportReference(constructorDefinition);
        }
        
        public ModuleDefinition ModuleDefinition { get; }

        public IFodyLogger Log { get; }

        public List<TypeDefinition> AllClasses => _allClasses.Value;

        public ITypeReferences TypeReferences { get; }

        public ITypeDefinitions TypeDefinitions { get; }

        public MethodReference ActionConstructorReference { get; }

        public IList<MethodReference> CommandImplementationConstructors { get; }

        public MethodReference FuncOfBoolConstructorReference { get; }

        public MethodReference ObjectConstructorReference { get; }

        public MethodReference CommandManagerAddRequerySuggestedMethodReference { get; }

        public MethodReference CommandManagerRemoveRequerySuggestedMethodReference { get; }

        public MethodReference ActionOfTConstructorReference { get; }

        public MethodReference ActionOfTInvokeReference { get; }

        public MethodReference ArgumentNullExceptionConstructorReference { get; }

        public MethodReference PredicateOfTInvokeReference { get; }

        public ConcurrentDictionary<string, CommandData> Commands { get; }

        public bool DelegateCommandImplementationWasInjected { get; set; }

        public MethodReference PredicateOfTConstructorReference { get; }

        public MethodReference DelegateCombineMethodReference { get; }

        public MethodReference DelegateRemoveMethodReference { get; }

        public MethodReference InterlockedCompareExchangeOfEventHandler => _interlockedCompareExchangeOfEventHandler;

        public MethodReference InterlockedCompareExchangeOfT { get; }

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