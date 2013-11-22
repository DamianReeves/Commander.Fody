using System;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public class AssemblyLevelCommandImplementationFinder : ICommandImplementationFinder
    {
        private readonly AssemblyDefinition _assemblyDefinition;

        public AssemblyLevelCommandImplementationFinder([NotNull] AssemblyDefinition assemblyDefinition)
        {
            if (assemblyDefinition == null) throw new ArgumentNullException("assemblyDefinition");
            _assemblyDefinition = assemblyDefinition;
        }

        public AssemblyDefinition AssemblyDefinition
        {
            get { return _assemblyDefinition; }
        }

        public bool TryFindCommandImplementation(out CommandInjectionAdviceBase injectionAdvice)
        {
            injectionAdvice = null;
            var implAttr = AssemblyDefinition.CustomAttributes.SingleOrDefault(x => x.AttributeType.DeclaringType.Name == "CommandImplementationAttribute");
            return false;
        }
    }
}