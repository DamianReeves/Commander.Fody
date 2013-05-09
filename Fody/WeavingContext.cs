using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public class WeavingContext
{
    private readonly WeaverCommonTypes _commonTypes;
    private readonly List<TypeDefinition> _allTypes;

    public WeavingContext(ModuleDefinition moduleDefinition)
    {
        _allTypes = moduleDefinition.GetTypes().Where(x => x.IsClass).ToList();
        _commonTypes = new WeaverCommonTypes(moduleDefinition);
    }

    public List<TypeDefinition> AllTypes
    {
        get { return _allTypes; }
    }

    public WeaverCommonTypes CommonTypes
    {
        get { return _commonTypes; }
    }  
  
}