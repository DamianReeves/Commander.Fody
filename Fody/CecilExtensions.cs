using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public static class CecilExtensions
{
    public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
    {
        return attributes.Any(x => x.Constructor.DeclaringType.Name == attributeName);
    }
}