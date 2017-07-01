using System;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Commander.Fody
{
    public static class InstructionListExtensions
    {
        public static void Prepend(this Collection<Instruction> collection, params Instruction[] instructions)
        {
            for (var index = 0; index < instructions.Length; index++)
            {
                var instruction = instructions[index];
                collection.Insert(index, instruction);
            }
        }
        public static void BeforeLast(this Collection<Instruction> collection, params Instruction[] instructions)
        {
            var index = collection.Count - 1;
            foreach (var instruction in instructions)
            {
                collection.Insert(index, instruction);
                index++;
            }
        }
        public static void Append(this Collection<Instruction> collection, params Instruction[] instructions)
        {
            for (var index = 0; index < instructions.Length; index++)
            {
                collection.Insert(index, instructions[index]);
            }
        }

        public static Instruction GetLastInstructionWhere(this Collection<Instruction> collection, Func<Instruction, bool> predicate)
        {
            for (int idx = collection.Count - 1; idx >= 0; idx--)
            {
                var instruction = collection[idx];
                if (predicate(instruction))
                {
                    return instruction;
                }            
            }

            return null;
        }

        public static void BeforeInstruction(this Collection<Instruction> collection, Func<Instruction, bool> predicate, params Instruction[] instructions)
        {
            int targetPos = collection.Count - 1;
            for (int idx = collection.Count - 1; idx >= 0; idx--)
            {
                var instruction = collection[idx];
                if (predicate(instruction))
                {
                    targetPos = idx;
                    break;
                }
            }
            
            if (targetPos < 0)
            {
                targetPos = 0;
            } 
            else if (targetPos > collection.Count)
            {
                targetPos = collection.Count;
            }

            foreach (var instruction in instructions)
            {
                collection.Insert(targetPos, instruction);
                targetPos++;
            }
        }
    }
}