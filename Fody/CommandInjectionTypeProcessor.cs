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
        private readonly List<CommandData> _commands;
        public CommandInjectionTypeProcessor(TypeDefinition type, ModuleWeaver moduleWeaver) : base(type, moduleWeaver)
        {
            _commands = Assets.Commands.Values.Where(cmd => cmd.DeclaringType.FullName == type.FullName).ToList();
        }        

        public List<CommandData> Commands
        {
            get { return _commands; }
        }

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
                    PropertyDefinition propertyDefinition;
                    if (Type.TryAddCommandProperty(commandTypeReference, commandData.CommandName, out propertyDefinition))
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
            if (Assets.CommandImplementationConstructors.Count == 0)
            {
                if (ModuleWeaver.Settings.FallbackToNestedCommands)
                {
                    Assets.Log.Info("Opting for nested command injection for type: {0} since there were no eligible command implementations.", Type.FullName);
                    //Assets.Log.Info("Command initialization for type: {0} skipped since there were no eligible command implementations.", Type.FullName);
                    InjectCommandInitializationWithNestedCommand(initializeMethod);
                }                
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
            var commandConstructor = Assets.CommandImplementationConstructors.FirstOrDefault();
            if (commandConstructor == null)
            {
                Assets.Log.Info("Skipped command initialization for command {0}, because there is no eligible command implementation to bind to.", commandData);
                return false;
            }

            if (!initializeMethod.Body.Variables.Any(vDef => vDef.VariableType.IsBoolean() && vDef.Name == "isNull"))
            {
                var vDef = new VariableDefinition("isNull", Type.Module.TypeSystem.Boolean);
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

            var canExecuteMethod = commandData.CanExecuteMethods.SingleOrDefault();
            if (commandData.OnExecuteMethods.Count > 0)
            {
                var onExecuteMethod = commandData.OnExecuteMethods[0];
                MethodReference commandConstructor;
                if (canExecuteMethod == null)
                {
                    commandConstructor = Assets.CommandImplementationConstructors.FirstOrDefault();
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldftn, onExecuteMethod);
                    yield return Instruction.Create(OpCodes.Newobj, Assets.ActionConstructorReference);
                    yield return Instruction.Create(OpCodes.Newobj, commandConstructor);
                    yield return Instruction.Create(OpCodes.Call, commandData.CommandProperty.SetMethod);
                    yield return Instruction.Create(OpCodes.Nop);
                    yield return Instruction.Create(OpCodes.Nop);
                }
                else
                {
                    commandConstructor = Assets.CommandImplementationConstructors.OrderByDescending(mf => mf.Parameters.Count).First();
                    yield return Instruction.Create(OpCodes.Nop);
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldftn, commandData.OnExecuteMethods.Single());
                    yield return Instruction.Create(OpCodes.Newobj, Assets.ActionConstructorReference);
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                    yield return Instruction.Create(OpCodes.Ldftn, commandData.CanExecuteMethods.Single());
                    yield return Instruction.Create(OpCodes.Newobj, Assets.FuncOfBoolConstructorReference);
                    yield return Instruction.Create(OpCodes.Newobj, commandConstructor);
                    yield return Instruction.Create(OpCodes.Call, commandData.CommandProperty.SetMethod);
                    yield return Instruction.Create(OpCodes.Nop);
                    yield return Instruction.Create(OpCodes.Nop);
                }
            }
            //else
            //{
            //    foreach (var onExecuteMethod in commandData.OnExecuteMethods)
            //    {
            //        if (canExecuteMethod == null)
            //        {

            //        }
            //    }
            //}            

            // BlockEnd is the end of the if (Command == null) {} block (i.e. think the closing brace)
            yield return blockEnd;          
        }
    }
}