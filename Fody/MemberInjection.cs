using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Commander.Fody
{
    public static class MemberInjection
    {
        public static void AddCanExecuteChangedEvent(this Assets assets, TypeDefinition typeDefinition)
        {
            var addMethod = assets.CreateCanExecuteChangedAddMethod();
            typeDefinition.Methods.Add(addMethod);

            var removeMethod = assets.CreateCanExecuteChangedRemoveMethod();
            typeDefinition.Methods.Add(removeMethod);

            var eventDefinition = new EventDefinition("CanExecuteChanged", EventAttributes.None, assets.TypeReferences.EventHandler)
            {
                AddMethod = addMethod,
                RemoveMethod = removeMethod
            };
            typeDefinition.Events.Add(eventDefinition);

        }     

        public static MethodDefinition CreateCanExecuteChangedAddMethod(this Assets assets)
        {
            var method = new MethodDefinition("add_CanExecuteChanged",
                MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot |
                MethodAttributes.Virtual | MethodAttributes.Public,
                assets.TypeReferences.Void)
            {
                Body = { InitLocals = true }
            };

            var eventHandlerParameter = new ParameterDefinition("value", ParameterAttributes.None, assets.TypeReferences.EventHandler);
            method.Parameters.Add(eventHandlerParameter);
            var il = method.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Call, assets.CommandManagerAddRequerySuggestedMethodReference));
            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ret));
            return method;
        }

        public static MethodDefinition CreateCanExecuteChangedRemoveMethod(this Assets assets)
        {
            var method = new MethodDefinition("remove_CanExecuteChanged",
                MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot |
                MethodAttributes.Virtual | MethodAttributes.Public,
                assets.TypeReferences.Void)
            {
                Body = { InitLocals = true }
            };

            var eventHandlerParameter = new ParameterDefinition("value", ParameterAttributes.None, assets.TypeReferences.EventHandler);
            method.Parameters.Add(eventHandlerParameter);
            var il = method.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Call, assets.CommandManagerRemoveRequerySuggestedMethodReference));
            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ret));
            return method;
        }        
    }
}
