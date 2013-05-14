using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public abstract class ModuleProcessorBase
    {
        [NotNull] private readonly ModuleWeaver _moduleWeaver;
        [NotNull] private readonly Assets _assets;

        protected ModuleProcessorBase([NotNull] ModuleWeaver moduleWeaver)
        {
            if (moduleWeaver == null)
            {
                throw new ArgumentNullException("moduleWeaver");
            }

            _moduleWeaver = moduleWeaver;
            _assets = _moduleWeaver.Assets;
        }

        public Assets Assets
        {
            get { return _assets; }
        }

        public ModuleWeaver ModuleWeaver { get { return _moduleWeaver; } }
        public abstract void Execute(IEnumerable<TypeDefinition> types);
    }

    public class ModuleTypesProcessor : ModuleProcessorBase
    {
        public ModuleTypesProcessor([NotNull] ModuleWeaver moduleWeaver) : base(moduleWeaver)
        {
        }

        public override void Execute(IEnumerable<TypeDefinition> types)
        {
            ProcessTypes(types ?? GetTypesToProcess());
        }

        public IEnumerable<TypeDefinition> GetTypesToProcess()
        {
            return Assets.ModuleDefinition.GetTypes().Where(x => x.IsClass);
        }

        public void ProcessTypes(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                try
                {
                    var typeProcessor = new TypeProcessor(type, ModuleWeaver);
                    typeProcessor.Execute();
                }
                catch (Exception ex)
                {
                    Assets.Log.Error(ex);
                }
            }
        }        
    }
}