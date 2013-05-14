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
        [NotNull] private readonly ModuleDefinition _moduleDefinition;

        protected ModuleProcessorBase([NotNull] ModuleWeaver moduleWeaver)
        {
            if (moduleWeaver == null)
            {
                throw new ArgumentNullException("moduleWeaver");
            }

            _moduleWeaver = moduleWeaver;
            _assets = _moduleWeaver.Assets;
            _moduleDefinition = _moduleWeaver.ModuleDefinition;
        }

        public Assets Assets
        {
            get { return _assets; }
        }

        public ModuleWeaver ModuleWeaver { get { return _moduleWeaver; } }

        public ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
        }

        public abstract void Execute();        
    }
}