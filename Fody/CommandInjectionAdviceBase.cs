using System.Collections.Generic;
using Mono.Cecil;

namespace Commander.Fody
{
    public abstract class CommandInjectionAdviceBase
    {
        public TypeReference ImplementingType { get; set; }
        public TypeDefinition ImplementingTypeDefinition { get; set; }

        public abstract bool ShouldInjectImplementation(CommandJoinPoint joinPoint);
        public abstract bool InjectImplementation(CommandJoinPoint joinPoint);
    }

    public abstract class CommandAdviceProvider
    {
        
    }

    public class CommandJoinPoint
    {
        public TypeReference TargetType { get; set; }
        public CommandAdviceInstance AdviceInstance { get; set; }
    }

    public class CommandAdviceInstance
    {
        public string CommandName { get; set; }

        public TypeReference CanExecuteParameterType { get; set; }
        public TypeReference ExecuteParameterType { get; set; }
        public IList<MethodReference> CanExecuteMethods { get; set; }
        public IList<MethodReference> ExecuteMethods { get; set; }
    }
}