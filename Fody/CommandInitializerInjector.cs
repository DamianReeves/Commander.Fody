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
}