using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Commander.Fody
{
    internal static class AttributeFinder
    {
        public static IEnumerable<MethodDefinition> FindOnCommandMethods(this TypeDefinition type)
        {
            return type.Methods.Where(method => method.CustomAttributes.ContainsAttribute("OnCommandAttribute"));
        }
    }
}