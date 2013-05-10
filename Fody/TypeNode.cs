using System.Collections.Generic;
using Mono.Cecil;

public class TypeNode
{
    public TypeNode()
    {        
        //Nodes = new List<TypeNode>();
        ReferencedCommands = new List<CommandData>();
        InjectedCommands = new List<CommandData>();
    }

    public TypeDefinition TypeDefinition;
    public List<CommandData> ReferencedCommands;
    public List<CommandData> InjectedCommands;
}

public class CommandData
{
    public readonly string CommandName;
    public bool InjectionRequired;    
    public PropertyDefinition CommandProperty;
    public List<MethodDefinition> OnExecuteMethods;
    public MethodDefinition CanExecuteMethod; 
    public TypeReference DelegateCommandTypeReference;
    public MethodReference DelegateCommandConstructorReference;

    public CommandData(string commandName)
    {
        OnExecuteMethods = new List<MethodDefinition>();
    }
}