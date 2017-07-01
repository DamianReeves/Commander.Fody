using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Commander.Fody
{
    public static class EventInjection
    {
        public static MethodDefinition CreateAddEventMethod(this TypeDefinition targetType, string eventName,
            TypeReference eventType, Assets assets, Action<ILProcessor> methodBodyWriter = null)
        {
            var method = new MethodDefinition("add_"+eventName,
                MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot |
                MethodAttributes.Virtual | MethodAttributes.Public,
                assets.TypeReferences.Void)
            {
                Body = { InitLocals = true }
            };

            var eventHandlerParameter = new ParameterDefinition("value", ParameterAttributes.None, assets.TypeReferences.EventHandler);
            method.Parameters.Add(eventHandlerParameter);
            var il = method.Body.GetILProcessor();
            if (methodBodyWriter == null)
            {
                var varDef0 = new VariableDefinition(eventType);
                method.Body.Variables.Add(varDef0);
                var varDef1 = new VariableDefinition(eventType);
                method.Body.Variables.Add(varDef1);
                var varDef2 = new VariableDefinition(eventType);
                method.Body.Variables.Add(varDef2);
                var varDef3 = new VariableDefinition(assets.TypeReferences.Boolean);
                method.Body.Variables.Add(varDef3);
                var field = targetType.Fields.FirstOrDefault(fld => fld.Name == eventName);
                if (field == null)
                {
                    assets.Log.LogInfo($"Adding field {eventName} to {targetType.FullName} in CreateAddEventMethod.");
                    field = targetType.AddField(eventType, eventName);
                }
                Instruction loopStart;
                il.Append(il.Create(OpCodes.Nop));
                il.Append(il.Create(OpCodes.Ldarg_0));
                il.Append(il.Create(OpCodes.Ldfld, field));
                il.Append(il.Create(OpCodes.Stloc_0));
                il.Append(loopStart = il.Create(OpCodes.Ldloc_0));
                il.Append(il.Create(OpCodes.Stloc_1));
                il.Append(il.Create(OpCodes.Ldloc_1));
                il.Append(il.Create(OpCodes.Ldarg_1));
                il.Append(il.Create(OpCodes.Call, assets.DelegateCombineMethodReference));
                il.Append(il.Create(OpCodes.Castclass, eventType));
                il.Append(il.Create(OpCodes.Stloc_2));
                il.Append(il.Create(OpCodes.Ldarg_0));
                il.Append(il.Create(OpCodes.Ldflda, field));
                il.Append(il.Create(OpCodes.Ldloc_2));
                il.Append(il.Create(OpCodes.Ldloc_1));
                il.Append(il.Create(OpCodes.Call, assets.InterlockedCompareExchangeOfEventHandler));
                il.Append(il.Create(OpCodes.Stloc_0));
                il.Append(il.Create(OpCodes.Ldloc_0));
                il.Append(il.Create(OpCodes.Ldloc_1));
                il.Append(il.Create(OpCodes.Ceq));
                il.Append(il.Create(OpCodes.Ldc_I4_0));
                il.Append(il.Create(OpCodes.Ceq));
                il.Append(il.Create(OpCodes.Stloc_3));
                il.Append(il.Create(OpCodes.Ldloc_3));
                il.Append(il.Create(OpCodes.Brtrue_S, loopStart));
                il.Append(il.Create(OpCodes.Ret));
            }
            else
            {
                methodBodyWriter(il);
            }
            return method;
        }

        public static MethodDefinition CreateRemoveEventMethod(this TypeDefinition targetType, string eventName,
            TypeReference eventType, Assets assets, Action<ILProcessor> methodBodyWriter = null)
        {
            var method = new MethodDefinition("remove_" + eventName,
                MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot |
                MethodAttributes.Virtual | MethodAttributes.Public,
                assets.TypeReferences.Void)
            {
                Body = { InitLocals = true }
            };

            var eventHandlerParameter = new ParameterDefinition("value", ParameterAttributes.None, assets.TypeReferences.EventHandler);
            method.Parameters.Add(eventHandlerParameter);
            var il = method.Body.GetILProcessor();
            if (methodBodyWriter == null)
            {
                var varDef0 = new VariableDefinition(eventType);
                method.Body.Variables.Add(varDef0);
                var varDef1 = new VariableDefinition(eventType);
                method.Body.Variables.Add(varDef1);
                var varDef2 = new VariableDefinition(eventType);
                method.Body.Variables.Add(varDef2);
                var varDef3 = new VariableDefinition(assets.TypeReferences.Boolean);
                method.Body.Variables.Add(varDef3);
                var field = targetType.Fields.FirstOrDefault(fld => fld.Name == eventName);
                if (field == null)
                {
                    assets.Log.LogInfo($"Adding field {eventName} to {targetType.FullName} in CreateRemoveEventMethod.");
                    field = targetType.AddField(eventType, eventName);
                }
                il.Append(il.Create(OpCodes.Nop));
                il.Append(il.Create(OpCodes.Ldarg_0));
                il.Append(il.Create(OpCodes.Ldfld, field));
                il.Append(il.Create(OpCodes.Stloc_0));
                var loopStart = il.Create(OpCodes.Nop);
                il.Append(loopStart);
                il.Append(il.Create(OpCodes.Ldloc_0));
                il.Append(il.Create(OpCodes.Stloc_1));
                il.Append(il.Create(OpCodes.Ldloc_1));
                il.Append(il.Create(OpCodes.Ldarg_1));
                il.Append(il.Create(OpCodes.Call, assets.DelegateRemoveMethodReference));
                il.Append(il.Create(OpCodes.Castclass, eventType));
                il.Append(il.Create(OpCodes.Stloc_2));
                il.Append(il.Create(OpCodes.Ldarg_0));
                il.Append(il.Create(OpCodes.Ldflda, field));
                il.Append(il.Create(OpCodes.Ldloc_2));
                il.Append(il.Create(OpCodes.Ldloc_1));
                il.Append(il.Create(OpCodes.Call, assets.InterlockedCompareExchangeOfEventHandler));
                il.Append(il.Create(OpCodes.Stloc_0));
                il.Append(il.Create(OpCodes.Ldloc_0));
                il.Append(il.Create(OpCodes.Ldloc_1));
                il.Append(il.Create(OpCodes.Ceq));
                il.Append(il.Create(OpCodes.Ldc_I4_0));
                il.Append(il.Create(OpCodes.Ceq));
                il.Append(il.Create(OpCodes.Stloc_3));
                il.Append(il.Create(OpCodes.Ldloc_3));
                il.Append(il.Create(OpCodes.Brtrue_S, loopStart));
                il.Append(il.Create(OpCodes.Ret));
            }
            else
            {
                methodBodyWriter(il);
            }
            return method;
        }
    }
}