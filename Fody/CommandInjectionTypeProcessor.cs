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

            var initializeMethod = AddCommandInitializerMethod();
            if (Assets.CommandImplementationConstructors.Count == 0)
            {
                Assets.Log.Info("Opting for nested command injection for type: {0} since there were no eligible command implementations.", Type.FullName);
                //Assets.Log.Info("Command initialization for type: {0} skipped since there were no eligible command implementations.", Type.FullName);
                InjectCommandInitializationWithNestedCommand(initializeMethod);
            }
            else
            {
                InjectCommandInitializationWithDelegateCommand(initializeMethod);     
            }             
            AddInitializationToConstructors(initializeMethod);
        }        

        public MethodDefinition AddCommandInitializerMethod()
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

            Type.Methods.Add(initializeMethod);

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
            var returnInst = GetOrCreateLastReturnInstruction(initializeMethod);

            Instruction blockEnd = Instruction.Create(OpCodes.Nop);

            // Null check
            // if (Command == null) { ... }
            instructions.Prepend(
                Instruction.Create(OpCodes.Nop),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, commandData.CommandProperty.GetMethod),
                Instruction.Create(OpCodes.Ldnull),
                Instruction.Create(OpCodes.Ceq),
                Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Ceq),
                Instruction.Create(OpCodes.Stloc_0),
                Instruction.Create(OpCodes.Ldloc_0),
                Instruction.Create(OpCodes.Brtrue_S, blockEnd),                
                blockEnd
                );

            // TODO: Just building up support at this time need to be able to handle more combinations and edge cases
            var onExecuteMethod = commandData.OnExecuteMethods.Single();
            var canExecuteMethod = commandData.CanExecuteMethods.FirstOrDefault();

            if (canExecuteMethod == null)
            {                
                instructions.BeforeInstruction(inst => inst == blockEnd,
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldftn, onExecuteMethod),
                    Instruction.Create(OpCodes.Newobj, Assets.ActionConstructorReference),
                    Instruction.Create(OpCodes.Newobj, commandConstructor),
                    Instruction.Create(OpCodes.Call, commandData.CommandProperty.SetMethod),
                    Instruction.Create(OpCodes.Nop),
                    Instruction.Create(OpCodes.Nop)
                    );
            }
            else
            {
                commandConstructor = Assets.CommandImplementationConstructors.OrderByDescending(mf => mf.Parameters.Count).First();
                instructions.BeforeInstruction(inst => inst == blockEnd,
                    Instruction.Create(OpCodes.Nop),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldftn, commandData.OnExecuteMethods.Single()),                    
                    Instruction.Create(OpCodes.Newobj, Assets.ActionConstructorReference),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldftn, commandData.CanExecuteMethods.Single()),
                    Instruction.Create(OpCodes.Newobj, Assets.FuncOfBoolConstructorReference),
                    Instruction.Create(OpCodes.Newobj, commandConstructor),
                    Instruction.Create(OpCodes.Call, commandData.CommandProperty.SetMethod),
                    Instruction.Create(OpCodes.Nop),
                    Instruction.Create(OpCodes.Nop)
                    );
            }            
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
    }
}