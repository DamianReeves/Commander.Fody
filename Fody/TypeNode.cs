using System.Collections.Generic;
using Mono.Cecil;

public class TypeNode
{
    public TypeNode()
    {        
        Nodes = new List<TypeNode>();
    }

    public TypeDefinition TypeDefinition;
    public List<TypeNode> Nodes;
}