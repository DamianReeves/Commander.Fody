using System.Linq;
using Mono.Cecil;

public static class CommandPropertyInjector
{
    public static PropertyDefinition AddProperty(TypeReference propertyType, TypeDefinition targetType, string commandName)
    {
        var propertyDefinition = targetType.Properties.FirstOrDefault(x => x.Name == commandName);
        if (propertyDefinition != null)
        {
            propertyDefinition.ValidateIsOfType(propertyType);
            return propertyDefinition;
        }

        propertyDefinition = new PropertyDefinition(commandName, PropertyAttributes.HasDefault, propertyType);
        targetType.Properties.Add(propertyDefinition);
        return propertyDefinition;
    }
}