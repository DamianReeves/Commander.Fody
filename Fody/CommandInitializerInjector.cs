using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public static class CommandInitializerInjector
{
    private const string InitializerMethodName = "<FodyCommander>InitializeCommands";

    public static bool TryAddCommandInitializerMethod(this TypeDefinition targetType, out MethodDefinition initializeMethod)
    {
        initializeMethod = targetType.Methods.FirstOrDefault(x => x.Name == InitializerMethodName);
        if (initializeMethod != null)
        {
            return false;
        }

        initializeMethod = new MethodDefinition(InitializerMethodName, MethodAttributes.Private | MethodAttributes.SpecialName, targetType.Module.TypeSystem.Void)
        {
            HasThis = true,
            Body = {InitLocals = true}
        };

        initializeMethod.Body.Instructions.Append(
            Instruction.Create(OpCodes.Ret)
        );

        targetType.Methods.Add(initializeMethod);

        return true;
    }

    public static bool TryAddCommandPropertyInitialization(this TypeDefinition targetType,
        MethodDefinition initializeMethod, CommandData commandData)
    {
        if (!initializeMethod.Body.Variables.Any(vDef => vDef.VariableType.IsBoolean() && vDef.Name == "isNull"))
        {
            var vDef = new VariableDefinition("isNull", targetType.Module.TypeSystem.Boolean);
            initializeMethod.Body.Variables.Add(vDef);
        }
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

        Instruction blockEnd = Instruction.Create(OpCodes.Nop);

        // Null check
        // if (Command == null) { ... }
        initializeMethod.Body.Instructions.Prepend(
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

        return true;
    }
}
