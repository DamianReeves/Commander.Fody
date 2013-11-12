using System.Linq;
using Mono.Cecil;

namespace Commander.Fody
{
    public class ReferenceCleanerProcessor : IModuleProcessor
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly IFodyLogger _logger;

        public ReferenceCleanerProcessor(ModuleDefinition moduleDefinition, IFodyLogger logger)
        {
            _moduleDefinition = moduleDefinition;
            _logger = logger;
        }

        public IFodyLogger Logger
        {
            get { return _logger; }
        }

        public ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
        }

        public void Execute()
        {
            CleanReferences();
        }

        public void CleanReferences()
        {
            var referenceToRemove = ModuleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == "Commander");
            if (referenceToRemove == null)
            {
                Logger.LogInfo("\tNo reference to 'Commander' found. References not modified.");
                return;
            }

            ModuleDefinition.AssemblyReferences.Remove(referenceToRemove);
            Logger.LogInfo("\tRemoving reference to 'Commander'.");
        }
    }
}