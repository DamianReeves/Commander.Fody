using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public abstract class ModuleProcessorBase : IModuleProcessor
    {
        protected ModuleProcessorBase([NotNull] ModuleWeaver moduleWeaver)
        {
            ModuleWeaver = moduleWeaver ?? throw new ArgumentNullException(nameof(moduleWeaver));
            Assets = ModuleWeaver.Assets;
            ModuleDefinition = ModuleWeaver.ModuleDefinition;
            Settings = ModuleWeaver.Settings;
        }

        [NotNull]
        public Assets Assets { get; }

        [NotNull]
        public ModuleWeaverSettings Settings { get; }

        [NotNull]
        public ModuleWeaver ModuleWeaver { get; }

        [NotNull]
        public ModuleDefinition ModuleDefinition { get; }

        public IFodyLogger Logger => ModuleWeaver;

        public abstract void Execute();        
    }
}