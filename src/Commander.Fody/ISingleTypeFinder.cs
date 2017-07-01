using System.Linq;
using Mono.Cecil;

namespace Commander.Fody
{
    public interface ISingleTypeFinder
    {
        bool TryFind(ModuleDefinition moduleDefinition, out TypeDefinition foundType);
    }

    public class CommandImplementationFactoryFinder : ISingleTypeFinder
    {
        public bool TryFind(ModuleDefinition moduleDefinition, out TypeDefinition foundType)
        {
            foundType = moduleDefinition.Types.FirstOrDefault(x => x.Name == "CommandImplementationFactory");
            return foundType != null;
        }
    }
}