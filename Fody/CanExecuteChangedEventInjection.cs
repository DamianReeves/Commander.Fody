using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace Commander.Fody
{
    public static class CanExecuteChangedEventInjection
    {
        public static void AddCanExecuteChangedEvent(this Assets assets, TypeDefinition typeDefinition)
        {
            var addMethod = typeDefinition.CreateCanExecuteChangedAddMethod(assets);
            typeDefinition.Methods.Add(addMethod);

            var removeMethod = typeDefinition.CreateCanExecuteChangedRemoveMethod(assets);
            typeDefinition.Methods.Add(removeMethod);

            var eventDefinition = new EventDefinition("CanExecuteChanged", EventAttributes.None, assets.TypeReferences.EventHandler)
            {
                AddMethod = addMethod,
                RemoveMethod = removeMethod
            };
            typeDefinition.Events.Add(eventDefinition);

        }     

        private static MethodDefinition CreateCanExecuteChangedAddMethod(this TypeDefinition typeDefinition, Assets assets)
        {
            Action<ILProcessor> methodBodyWriter = null;
            if (assets.CommandManagerAddRequerySuggestedMethodReference != null)
            {
                methodBodyWriter = il =>
                {
                    il.Append(il.Create(OpCodes.Ldarg_1));
                    il.Append(il.Create(OpCodes.Call, assets.CommandManagerAddRequerySuggestedMethodReference));
                    il.Append(il.Create(OpCodes.Nop));
                    il.Append(il.Create(OpCodes.Ret));
                };
            }

            var method = typeDefinition.CreateAddEventMethod(
                "CanExecuteChanged",
                assets.TypeReferences.EventHandler,
                assets,
                methodBodyWriter
            );
            return method;
        }

        private static MethodDefinition CreateCanExecuteChangedRemoveMethod(this TypeDefinition typeDefinition, Assets assets)
        {
            Action<ILProcessor> methodBodyWriter = null;
            if (assets.CommandManagerAddRequerySuggestedMethodReference != null)
            {
                methodBodyWriter = il =>
                {
                    il.Append(il.Create(OpCodes.Ldarg_1));
                    il.Append(il.Create(OpCodes.Call, assets.CommandManagerRemoveRequerySuggestedMethodReference));
                    il.Append(il.Create(OpCodes.Nop));
                    il.Append(il.Create(OpCodes.Ret));
                };
            }

            var method = typeDefinition.CreateRemoveEventMethod(
                "CanExecuteChanged",
                assets.TypeReferences.EventHandler,
                assets,
                methodBodyWriter
            );
            return method;
        }        
    }
}
