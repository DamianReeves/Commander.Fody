using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Commander.Fody
{
    public class CommandInjectionTypeProcessor : TypeProcessorBase
    {
        // TODO: Eventually change this to be configurable
        private const string InitializerMethodName = "<Commander_Fody>InitializeCommands";

        public CommandInjectionTypeProcessor(TypeDefinition type, ModuleWeaver moduleWeaver) : base(type, moduleWeaver)
        {
            Commands = Assets.Commands.Values.Where(cmd => cmd.DeclaringType.FullName == type.FullName).ToList();
        }        

        public List<CommandData> Commands { get; }

        public override void Execute()
        {
            InjectCommandProperties();

            if (Commands.Count > 0)
            {
                InjectCommandInitialization();
            }            
        }        
   
        internal void InjectCommandProperties()
        {
            var commandTypeReference = Assets.TypeReferences.ICommand;
            foreach (var commandData in Commands)
            {
                try
                {
                    if (Type.TryAddCommandProperty(commandTypeReference, commandData.CommandName, out PropertyDefinition propertyDefinition))
                    {
                        Assets.Log.Info("Successfully added a property for command: {0}.", commandData.CommandName);
                    }
                    commandData.CommandProperty = propertyDefinition;
                }
                catch (Exception ex)
                {
                    Assets.Log.Error("Error while adding property {0} to {1}: {2}", commandData.CommandName, Type, ex);
                }
            }
        }

        internal void InjectCommandInitialization()
        {
            if (Commands.Count == 0)
            {
                Assets.Log.Info("Command initialization for type: {0} skipped since there were no commands to bind.", Type.FullName);
                return;
            }            

            var initializeMethod = CreateCommandInitializerMethod();
            if (Assets.CommandImplementationConstructors.Count == 0 && ModuleWeaver.Settings.FallbackToNestedCommands)
            {
                Assets.Log.Info("Opting for nested command injection for type: {0} since there were no eligible command implementations.", Type.FullName);
                //Assets.Log.Info("Command initialization for type: {0} skipped since there were no eligible command implementations.", Type.FullName);
                InjectCommandInitializationWithNestedCommand(initializeMethod);             
            }
            else
            {
                InjectCommandInitializationWithDelegateCommand(initializeMethod);     
            }
            var wasCommandInitializationInjected = Commands.Any(x => x.CommandInitializationInjected);
            if (wasCommandInitializationInjected)
            {
                Type.Methods.Add(initializeMethod);
                AddInitializationToConstructors(initializeMethod);
            }            
        }        

        public MethodDefinition CreateCommandInitializerMethod()
        {
            var initializeMethod = 
                Type.Methods.FirstOrDefault(x => x.Name == InitializerMethodName);
            if (initializeMethod != null)
            {
                return initializeMethod;
            }

            initializeMethod = new MethodDefinition(
                InitializerMethodName
                , MethodAttributes.Private | MethodAttributes.SpecialName
                , Assets.TypeReferences.Void)
            {
                HasThis = true,
                Body = { InitLocals = true }
            };

            initializeMethod.Body.Instructions.Append(
                Instruction.Create(OpCodes.Ret)
                );

            return initializeMethod;
        }

        public bool TryAddCommandPropertyInitialization(MethodDefinition initializeMethod, CommandData commandData)
        {
            if (!Assets.CommandImplementationConstructors.Any())
            {
                if (!Assets.DelegateCommandImplementationWasInjected)
                {
                    Assets.Log.Info("Skipped command initialization for command {0}, because there is no eligible command implementation to bind to.", commandData);
                    return false;
                }                
            }

            if (!initializeMethod.Body.Variables.Any(vDef => vDef.VariableType.IsBoolean()))// && vDef.Name == "isNull"))
            {
                var vDef = new VariableDefinition(Type.Module.TypeSystem.Boolean);
                initializeMethod.Body.Variables.Add(vDef);
            }
            var instructions = initializeMethod.Body.Instructions;
            GetOrCreateLastReturnInstruction(initializeMethod);
            var instructionsToAdd = GetCommandInitializationInstructions(commandData).ToArray();
            instructions.Prepend(instructionsToAdd);
            commandData.CommandInitializationInjected = true;
            return true;
        }

        public static Instruction GetOrCreateLastReturnInstruction(MethodDefinition initializeMethod)
        {
            var instructions = initializeMethod.Body.Instructions;
            Instruction returnInst;
            if (instructions.Count == 0)
            {
                returnInst = Instruction.Create(OpCodes.Ret);
                instructions.Add(returnInst);
            }
            else
            {
                returnInst = instructions.GetLastInstructionWhere(inst => inst.OpCode == OpCodes.Ret);
                if (returnInst == null)
                {
                    returnInst = Instruction.Create(OpCodes.Ret);
                    instructions.Add(returnInst);
                }
            }
            return returnInst;
        }

        public void InjectCommandInitializationWithNestedCommand(MethodDefinition initializeMethod)
        {
            foreach (var commandData in Commands)
            {
                try
                {
                    var processor = new NestedCommandInjectionTypeProcessor(commandData, initializeMethod,  Type, ModuleWeaver);
                    processor.Execute();
                }
                catch (Exception ex)
                {
                    Assets.Log.Error(ex);                    
                }
            }
        }

        public void InjectCommandInitializationWithDelegateCommand(MethodDefinition initializeMethod)
        {
            foreach (var commandData in Commands)
            {
                try
                {
                    Assets.Log.Info("Trying to add initialization for command: {0}.", commandData.CommandName);
                    if (TryAddCommandPropertyInitialization(initializeMethod, commandData))
                    {
                        Assets.Log.Info("Successfully added initialization for command: {0}.",
                            commandData.CommandName);
                    }
                    else
                    {
                        Assets.Log.Warning("Failed to add initialization for command: {0}.", commandData.CommandName);
                    }
                }
                catch (Exception ex)
                {
                    Assets.Log.Error(ex);
                }
            }
        }

        public void AddInitializationToConstructors(MethodDefinition initMethod)
        {
            var constructors =
                from constructor in Type.GetConstructors()
                where !constructor.IsStatic
                select constructor;

            foreach (var constructor in constructors)
            {
                var instructions = constructor.Body.Instructions;
                var returnInst = instructions.GetLastInstructionWhere(inst => inst.OpCode == OpCodes.Ret);
                instructions.BeforeInstruction(inst => inst == returnInst
                    , Instruction.Create(OpCodes.Nop)
                    , Instruction.Create(OpCodes.Ldarg_0)
                    , Instruction.Create(OpCodes.Call, initMethod)
                );
            }
        }

        private IEnumerable<Instruction> GetCommandInitializationInstructions(CommandData commandData)
        {
            Instruction blockEnd = Instruction.Create(OpCodes.Nop);
            //// Null check
            //// if (Command == null) { ... }
            yield return Instruction.Create(OpCodes.Nop);
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Call, commandData.CommandProperty.GetMethod);
            yield return Instruction.Create(OpCodes.Ldnull);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Ldc_I4_0);
            yield return Instruction.Create(OpCodes.Ceq);
            yield return Instruction.Create(OpCodes.Stloc_0);
            yield return Instruction.Create(OpCodes.Ldloc_0);
            yield return Instruction.Create(OpCodes.Brtrue_S, blockEnd);

            foreach (var instruction in GetSetCommandInstructions(commandData))
            {
                yield return instruction;
            }

            // BlockEnd is the end of the if (Command == null) {} block (i.e. think the closing brace)
            yield return blockEnd;          
        }

        internal IEnumerable<Instruction> GetSetCommandInstructions(CommandData command)
        {
            var commandConstructors = Assets.CommandImplementationConstructors;
            if (Assets.DelegateCommandImplementationWasInjected)
            {
                var delegateCommandType = Assets.ModuleDefinition.GetType(DelegateCommandClassInjectionProcessor.GeneratedCommandClassName);
                commandConstructors = commandConstructors.Concat(delegateCommandType.GetConstructors()).ToList();
            }
            if (command.OnExecuteMethods.Count > 0)
            {
                MethodReference commandConstructor;
                var onExecuteMethod = command.OnExecuteMethods[0];
                var canExecuteMethod = command.CanExecuteMethods.FirstOrDefault();
                if (canExecuteMethod == null)
                {
                    commandConstructor = commandConstructors.FirstOrDefault(mf=>mf.Parameters.Count == 1);
                    commandConstructor = GetConstructorResolved(commandConstructor, onExecuteMethod);
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldftn, onExecuteMethod);
                    yield return Instruction.Create(OpCodes.Newobj, GetActionConstructorForExecuteMethod(onExecuteMethod, commandConstructor));
                    yield return Instruction.Create(OpCodes.Newobj, commandConstructor);
                    yield return Instruction.Create(OpCodes.Call, command.CommandProperty.SetMethod);
                    yield return Instruction.Create(OpCodes.Nop);
                    yield return Instruction.Create(OpCodes.Nop);
                }
                else
                {
                    commandConstructor = commandConstructors.OrderByDescending(mf => mf.Parameters.Count).First();
                    commandConstructor = GetConstructorResolved(commandConstructor, onExecuteMethod);
                    yield return Instruction.Create(OpCodes.Nop);
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldftn, command.OnExecuteMethods.Single());
                    yield return Instruction.Create(OpCodes.Newobj, GetActionConstructorForExecuteMethod(onExecuteMethod, commandConstructor));
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldftn, command.CanExecuteMethods.Single());
                    yield return Instruction.Create(OpCodes.Newobj, GetPredicateConstructorForCanExecuteMethod(canExecuteMethod, commandConstructor));
                    yield return Instruction.Create(OpCodes.Newobj, commandConstructor);
                    yield return Instruction.Create(OpCodes.Call, command.CommandProperty.SetMethod);
                    yield return Instruction.Create(OpCodes.Nop);
                    yield return Instruction.Create(OpCodes.Nop);
                }
            }
        }

        private MethodReference GetConstructorResolved(MethodReference commandConstructor, MethodDefinition method)
        {
            var parameterType = method.HasParameters
                ? method.Parameters[0].ParameterType
                : Assets.TypeReferences.Object;

            var commandType = commandConstructor.DeclaringType;
            bool isGeneric = commandType.HasGenericParameters;
            if (commandType.HasGenericParameters)
            {
                var commandTypeResolved = commandConstructor.DeclaringType.MakeGenericInstanceType(parameterType);
                commandType = commandTypeResolved;
                var resolvedConstructor =
                    commandType.GetElementType()
                        .Resolve()
                        .GetConstructors()
                        .First(c => c.Parameters.Count == commandConstructor.Parameters.Count);
                commandConstructor = resolvedConstructor;
            }

            if (isGeneric)
            {
                if (method.HasParameters)
                {
                    commandConstructor =
                        commandConstructor.MakeHostInstanceGeneric(method.Parameters[0].ParameterType);
                }
                else
                {
                    commandConstructor = commandConstructor.MakeHostInstanceGeneric(Assets.TypeReferences.Object);
                }
            }
            return commandConstructor;
        }

        private MethodReference GetActionConstructorForExecuteMethod(MethodReference method, MethodReference targetConstructor)
        {
            if (method.Parameters.Count > 1)
            {
                throw new WeavingException(
                    string.Format("Cannot generate command initialization for method {0}, because the method has too many parameters."
                    ,method));
            }

            if (method.HasParameters)
            {
                // Action<TCommandParameter> where TCommandParameter = method.Parameters[0].ParameterType
                return Assets.ActionOfTConstructorReference.MakeHostInstanceGeneric(method.Parameters[0].ParameterType);
            }

            // Action()
            return Assets.ActionConstructorReference;
        }

        private MethodReference GetPredicateConstructorForCanExecuteMethod(MethodReference method, MethodReference targetConstructor)
        {
            if (method.Parameters.Count > 1)
            {
                throw new WeavingException(
                    $"Cannot generate command initialization for 'CanExecute' method {method}, because the method has too many parameters.");
            }

            if (targetConstructor.Parameters.Count != 2)
            {
                throw new WeavingException(
                    $"Cannot generate command initialization for 'CanExecute' method {method}, because the method has the wrong signature.");
            }

            var targetParameter = targetConstructor.Parameters[1];
            if (method.HasParameters)
            {
                var methodParameter = method.Parameters[0];             
                return Assets.PredicateOfTConstructorReference.MakeHostInstanceGeneric(methodParameter.ParameterType);
            }

            if (targetParameter.ParameterType.Name != "Func`1")
            {
                throw new WeavingException(
                    $"Cannot generate command initialization for 'CanExecute' method {method}, because the method has the wrong signature.");
            }
           
            // Action()
            return Assets.FuncOfBoolConstructorReference;
        }
    }
}