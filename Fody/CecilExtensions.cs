using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public static class CecilExtensions
{
    public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
    {
        return attributes.Any(x => x.Constructor.DeclaringType.Name == attributeName);
    }

    public static void ValidateIsOfType(this FieldReference targetReference, TypeReference expectedType)
    {
        if (targetReference.FieldType.Name != expectedType.Name)
        {
            throw new WeavingException(string.Format("Field '{0}' could not be re-used because it is not the correct type. Expected '{1}'.", targetReference.Name, expectedType.Name));
        }
    }

    public static void ValidateIsOfType(this PropertyReference targetReference, TypeReference expectedType)
    {
        if (targetReference.PropertyType.Name != expectedType.Name)
        {
            throw new WeavingException(string.Format("Property '{0}' could not be re-used because it is not the correct type. Expected '{1}'.", targetReference.Name, expectedType.Name));
        }
    }

    public static FieldDefinition AddField(this TypeDefinition targetType, TypeReference fieldType, string fieldName)
    {
        var fieldDefinition = targetType.Fields.FirstOrDefault(x => x.Name == fieldName);
        if (fieldDefinition != null)
        {
            fieldDefinition.ValidateIsOfType(fieldType);
            return fieldDefinition;
        }
        fieldDefinition = new FieldDefinition(fieldName, FieldAttributes.Private, fieldType);
        targetType.Fields.Add(fieldDefinition);
        return fieldDefinition;
    }
}