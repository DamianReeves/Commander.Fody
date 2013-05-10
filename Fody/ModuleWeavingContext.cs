using System;
using System.Collections.Generic;
using System.Linq;
using Commander.Fody;
using Mono.Cecil;

public class ModuleWeavingContext
{
    private readonly ModuleDefinition _moduleDefinition;
    private readonly WeaverCommonTypes _commonTypes;
    private readonly List<TypeDefinition> _allTypes;
    private readonly List<TypeNode> _weavableTypes;
    private readonly Action<string> _logger;

    public ModuleWeavingContext(ModuleDefinition moduleDefinition, Action<string> logger)
    {
        _moduleDefinition = moduleDefinition;
        _logger = logger;
        _allTypes = moduleDefinition.GetTypes().Where(x => x.IsClass).ToList();
        _weavableTypes = new List<TypeNode>();
        _commonTypes = new WeaverCommonTypes(this);           
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

    public ModuleDefinition ModuleDefinition
    {
        get { return _moduleDefinition; }
    }

    public Action<string> Logger
    {
        get { return _logger; }
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
