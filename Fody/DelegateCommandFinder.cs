using System.Linq;
using Mono.Cecil;

namespace Commander.Fody
{
    public static class DelegateCommandFinder
    {
        public static bool TryFindDelegateCommandType(this ModuleWeavingContext context, out TypeReference delegateCommand)
        {
            var results =
                from type in context.AllTypes
                where type.Name == "DelegateCommand" || type.Name == "RelayCommand"
                select type;

            delegateCommand = results.FirstOrDefault();
            if (delegateCommand != null)
            {
                context.Logger(string.Format("Found DelegateCommand in {0}.", context.ModuleDefinition.FullyQualifiedName));
            }
            return delegateCommand != null;
        }
    }
}