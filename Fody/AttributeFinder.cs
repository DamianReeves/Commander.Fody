using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Commander.Fody
{
    internal static class AttributeFinder
    {
        public static IEnumerable<MethodDefinition> FindOnCommandMethods(this TypeDefinition type)
        {
            return type.Methods.Where(method => CecilExtensions.ContainsAttribute(method.CustomAttributes, "OnCommandAttribute"));
        }
    }
}