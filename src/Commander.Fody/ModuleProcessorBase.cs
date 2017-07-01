using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public abstract class ModuleProcessorBase : IModuleProcessor
    {
        [NotNull] private readonly ModuleWeaver _moduleWeaver;
        [NotNull]private readonly ModuleWeaverSettings _settings;
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
            _settings = _moduleWeaver.Settings;
        }

        public Assets Assets
        {
            get { return _assets; }
        }

        public ModuleWeaverSettings Settings
        {
            get { return _settings; }
        }

        public ModuleWeaver ModuleWeaver { get { return _moduleWeaver; } }

        public ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
        }

        public IFodyLogger Logger
        {
            get { return ModuleWeaver; }
        }

        public abstract void Execute();        
    }
}