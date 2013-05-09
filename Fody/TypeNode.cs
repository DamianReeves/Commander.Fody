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
    public PropertyDefinition CommandProperty;
    public TypeReference DelegateCommandTypeReference;
    public MethodReference DelegateCommandConstructorReference;
}