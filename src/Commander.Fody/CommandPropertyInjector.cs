using System;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Commander.Fody
{
    public static class CommandPropertyInjector
    {
        public static bool TryAddCommandProperty(this TypeDefinition targetType, TypeReference propertyType, string commandName, out PropertyDefinition propertyDefinition)
        {
            propertyDefinition = targetType.Properties.FirstOrDefault(x => x.Name == commandName);
            if (propertyDefinition != null)
            {
                propertyDefinition.ValidateIsOfType(propertyType);
                return false;
            }

            propertyDefinition = new PropertyDefinition(commandName, PropertyAttributes.HasDefault, propertyType);
            targetType.Properties.Add(propertyDefinition);
            var backingField = AddPropertyBackingField(propertyDefinition);
            AddPropertyGetter(propertyDefinition, backingField: backingField);
            AddPropertySetter(propertyDefinition, backingField: backingField);
            return true;
        }

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
            var backingField = AddPropertyBackingField(propertyDefinition);
            AddPropertyGetter(propertyDefinition, backingField:backingField);
            AddPropertySetter(propertyDefinition, backingField:backingField);
            return propertyDefinition;
        }

        public static FieldDefinition AddPropertyBackingField(PropertyDefinition property)
        {
            var fieldName = string.Format("<{0}>k__BackingField", property.Name);
            return property.DeclaringType.AddField(property.PropertyType, fieldName);
        }

        public static MethodDefinition AddPropertyGetter(
            PropertyDefinition property
            , MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual
            , FieldDefinition backingField = null)
        {
            if (backingField == null)
            {
                // TODO: Try and find existing friendly named backingFields first.
                backingField = AddPropertyBackingField(property);
            }

            var methodName = "get_" + property.Name;
            var getter = new MethodDefinition(methodName, methodAttributes, property.PropertyType)
            {
                IsGetter = true,
                Body = {InitLocals = true},
            };

            getter.Body.Variables.Add(new VariableDefinition(property.PropertyType));

            var returnStart = Instruction.Create(OpCodes.Ldloc_0);
            getter.Body.Instructions.Append(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, backingField),
                Instruction.Create(OpCodes.Stloc_0),
                Instruction.Create(OpCodes.Br_S, returnStart),
                returnStart,
                Instruction.Create(OpCodes.Ret)
                );        
                
            property.GetMethod = getter;
            property.DeclaringType.Methods.Add(getter);
            return getter;
        }

        public static MethodDefinition AddPropertySetter(
            PropertyDefinition property
            , MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual
            , FieldDefinition backingField = null)
        {
            if (backingField == null)
            {
                // TODO: Try and find existing friendly named backingFields first.
                backingField = AddPropertyBackingField(property);
            }

            var methodName = "set_" + property.Name;
            var setter = new MethodDefinition(methodName, methodAttributes, property.Module.TypeSystem.Void)
            {
                IsSetter = true,
                Body = { InitLocals = true },
            };
            setter.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, property.PropertyType));
            setter.Body.Instructions.Append(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Stfld, backingField),
                Instruction.Create(OpCodes.Ret)
                );

            property.SetMethod = setter;
            property.DeclaringType.Methods.Add(setter);
            return setter;
        }
    }
}