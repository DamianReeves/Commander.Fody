using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Commander.Fody
{
    public static class CecilExtensions
    {
        public static bool IsCustomAttribute(this CustomAttribute attribute, string attributeName, bool useFullName = false)
        {
            return useFullName
                ? attribute.Constructor.DeclaringType.FullName == attributeName
                : attribute.Constructor.DeclaringType.Name == attributeName;
        }

        public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName, bool useFullName = false)
        {
            return attributes.Any(x => x.IsCustomAttribute(attributeName, useFullName));
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

        public static bool Matches(this TypeReference self, TypeReference other, 
            Func<TypeReference, TypeReference, bool> predicate = null)
        {
            predicate = predicate ?? NameMatches;
            return predicate(self, other);
        }

        public static bool NameMatches(this TypeReference self, TypeReference other)
        {
            return self.Name == other.Name;
        }

        public static bool FullNameMatches(this TypeReference self, TypeReference other)
        {
            return self.FullName == other.FullName;
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

        public static bool IsBoolean(this TypeReference type)
        {
            return (type.FullName == "System.Boolean" || type.Name == "bool" );
        }

        public static bool Implements(this TypeDefinition typeDefinition, TypeReference interfaceTypeReference)
        {
            while (typeDefinition != null && typeDefinition.BaseType != null)
            {
                if (typeDefinition.Interfaces != null && typeDefinition.Interfaces.Contains(interfaceTypeReference))
                    return true;

                typeDefinition = typeDefinition.BaseType.Resolve();
            }

            return false;
        }

        public static bool DerivesFrom(this TypeReference typeReference, TypeReference expectedBaseTypeReference)
        {
            while (typeReference != null)
            {
                if (typeReference == expectedBaseTypeReference)
                    return true;

                typeReference = typeReference.Resolve().BaseType;
            }

            return false;
        }
    }
}