using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

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

        public static MethodReference MakeHostInstanceGeneric(
                                  this MethodReference self,
                                  params TypeReference[] args)
        {
            var reference = new MethodReference(
                self.Name,
                self.ReturnType,
                self.DeclaringType.MakeGenericInstanceType(args))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
            }

            return reference;
        }

        public static bool IsBoolean(this TypeReference type)
        {
            return (type.FullName == "System.Boolean" || type.Name == "bool" );
        }

        public static bool Implements(this TypeDefinition typeDefinition, TypeReference interfaceTypeReference, bool nameCheck = true)
        {
            while (typeDefinition != null && typeDefinition.BaseType != null)
            {
                if (typeDefinition.Interfaces != null && typeDefinition.Interfaces.Any(iface=>iface.FullName == interfaceTypeReference.FullName))
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

        public static void InsertBefore(this ILProcessor processor, Instruction target, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                processor.InsertBefore(target, instruction);
        }

        public static void InsertAfter(this ILProcessor processor, Instruction target, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                processor.InsertAfter(target, instruction);
                target = instruction;
            }
        }

        public static FieldReference GetGeneric(this FieldDefinition definition)
        {
            if (definition.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(definition.DeclaringType);
                foreach (var parameter in definition.DeclaringType.GenericParameters)
                {
                    declaringType.GenericArguments.Add(parameter);
                }
                return new FieldReference(definition.Name, definition.FieldType, declaringType);
            }

            return definition;
        }

        public static MethodReference GetGeneric(this MethodReference reference)
        {
            if (reference.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(reference.DeclaringType);
                foreach (var parameter in reference.DeclaringType.GenericParameters)
                {
                    declaringType.GenericArguments.Add(parameter);
                }
                var methodReference = new MethodReference(reference.Name, reference.MethodReturnType.ReturnType, declaringType);
                foreach (var parameterDefinition in reference.Parameters)
                {
                    methodReference.Parameters.Add(parameterDefinition);
                }
                methodReference.HasThis = reference.HasThis;
                return methodReference;
            }

            return reference;
        }
    }    
}