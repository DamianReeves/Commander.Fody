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
    public class TypeProcessor : TypeProcessorBase
    {
        // TODO: Eventually change this to be configurable
        public const string OnCommandAttributeName = "OnCommandAttribute";
        public const string OnCommandCanExecuteAttributeName = "OnCommandCanExecuteAttribute";
        private const string InitializerMethodName = "<Commander_Fody>InitializeCommands";

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
            if (Commands.Count > 0)
            {
                InjectCommandInitialization();
            }            
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
            return method.ReturnType == Assets.TypeReferences.Void
                && (!method.HasParameters
                    || (method.Parameters.Count == 1 
                    && !method.Parameters[0].IsOut
                    && method.Parameters[0].ParameterType.Matches(Assets.TypeReferences.Object)));
        }

        public bool IsValidCanExecuteMethod(MethodDefinition method)
        {
            return method.ReturnType == Assets.TypeReferences.Boolean
                && (!method.HasParameters
                    || (method.Parameters.Count == 1
                    && !method.Parameters[0].IsOut
                    && method.Parameters[0].ParameterType.Matches(Assets.TypeReferences.Object)));
        }

        internal void ScanForOnCommandAttribute()
        {
            var methods = FindOnCommandMethods(Type);
            foreach (var method in methods)
            {
                if (!IsValidOnExecuteMethod(method))
                {
                    Assets.Log.Warning("Method: {0} is not a valid OnExecute method for ICommand binding..", method);
                    Assets.Log.Warning("Method: {0} parameter info:", method);
                    for (int index = 0; index < method.Parameters.Count; index++)
                    {
                        var parameter = method.Parameters[index];
                        Assets.Log.Info("Parameter[{0}]: {1}", index, parameter);
                    }
                    continue;
                }

                // Find OnCommand methods where name is given
                var attributes =
                    method.CustomAttributes
                    .Where(x => x.IsCustomAttribute(OnCommandAttributeName))
                    .Where(x => x.HasConstructorArguments
                        && x.ConstructorArguments.First().Type.FullNameMatches(Assets.TypeReferences.String));

                foreach (var attribute in attributes)
                {                    
                    var commandName = (string)attribute.ConstructorArguments[0].Value;
                    Assets.Log.Info("Found OnCommand method {0} for command {1} on type {2}"
                    , method
                    , commandName
                    , Type.Name);
                    var command = Commands.GetOrAdd(commandName, name => new CommandData(Type,name));    
                    command.OnExecuteMethods.Add(method);
                }
            }
        }

        internal void ScanForOnCommandCanExecuteAttribute()
        {
            var methods = FindCommandCanExecuteMethods(Type);
            foreach (var method in methods)
            {
                if (!IsValidCanExecuteMethod(method))
                {
                    Assets.Log.Warning("Method: {0} is not a valid CanExecute method for ICommand binding.", method);
                    Assets.Log.Warning("Method: {0} parameter info:", method);
                    for (int index = 0; index < method.Parameters.Count; index++)
                    {
                        var parameter = method.Parameters[index];
                        Assets.Log.Info("Parameter[{0}]: {1}", index, parameter);
                    }
                    continue;
                }

                // Find OnCommandCanExecute methods where name is given
                var attributes =
                    method.CustomAttributes
                    .Where(x => x.IsCustomAttribute(OnCommandCanExecuteAttributeName))
                    .Where(x => x.HasConstructorArguments
                        && x.ConstructorArguments.First().Type.FullNameMatches(Assets.TypeReferences.String));

                foreach (var attribute in attributes)
                {
                    var commandName = (string)attribute.ConstructorArguments[0].Value;
                    Assets.Log.Info("Found OnCommandCanExecute method {0} for command {1} on type {2}"
                    , method
                    , commandName
                    , Type.Name);
                    var command = Commands.GetOrAdd(commandName, name => new CommandData(Type, name));
                    command.CanExecuteMethods.Add(method);
                }
            }
        }  
   
        internal void InjectCommandProperties()
        {
            var commandTypeReference = Assets.TypeReferences.ICommand;
            foreach (var commandData in Commands.Values)
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
            var commandCtor = Assets.CommandImplementationConstructors.FirstOrDefault();
            if (commandCtor == null)
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
                    Instruction.Create(OpCodes.Newobj, commandCtor),
                    Instruction.Create(OpCodes.Call, commandData.CommandProperty.SetMethod),
                    Instruction.Create(OpCodes.Nop),
                    Instruction.Create(OpCodes.Nop)
                    );
            }
            else
            {
                commandCtor = Assets.CommandImplementationConstructors.OrderByDescending(mf => mf.Parameters.Count).First();
                instructions.BeforeInstruction(inst => inst == blockEnd,
                    Instruction.Create(OpCodes.Nop),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldftn, commandData.OnExecuteMethods.Single()),                    
                    Instruction.Create(OpCodes.Newobj, Assets.ActionConstructorReference),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldftn, commandData.CanExecuteMethods.Single()),
                    Instruction.Create(OpCodes.Newobj, Assets.FuncOfBoolConstructorReference),
                    Instruction.Create(OpCodes.Newobj, commandCtor),
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
            foreach (var commandData in Commands.Values)
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
            foreach (var commandData in Commands.Values)
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
            var ctors =
                from ctor in Type.GetConstructors()
                where !ctor.IsStatic
                select ctor;

            foreach (var ctor in ctors)
            {
                var instructions = ctor.Body.Instructions;
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