using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public class ModuleWeavingContext
{
    private readonly WeaverCommonTypes _commonTypes;
    private readonly List<TypeDefinition> _allTypes;
    private readonly List<TypeNode> _weavableTypes;

    public ModuleWeavingContext(ModuleDefinition moduleDefinition)
    {
        _allTypes = moduleDefinition.GetTypes().Where(x => x.IsClass).ToList();
        _commonTypes = new WeaverCommonTypes(moduleDefinition);
        _weavableTypes = new List<TypeNode>();
    }

    public List<TypeDefinition> AllTypes
    {
        get { return _allTypes; }
    }

    public WeaverCommonTypes CommonTypes
    {
        get { return _commonTypes; }
    }  
  
    public List<TypeNode> WeavableTypes
    {
        get { return _weavableTypes; }
    }

    public TypeWeavingContext GetTypeWeavingContext(TypeDefinition type)
    {
        return new TypeWeavingContext(type, this);
    }
}

public class TypeWeavingContext
{
    public readonly TypeNode Type;
    public readonly ModuleWeavingContext ModuleContext;
    public TypeWeavingContext(TypeDefinition type, ModuleWeavingContext moduleContext)
    {
        Type = new TypeNode {TypeDefinition = type};
        ModuleContext = moduleContext;
    }
}
