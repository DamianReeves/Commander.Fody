using System;
using Mono.Cecil;

namespace Commander.Fody
{
    public abstract class TypeProcessorBase
    {
        private readonly TypeDefinition _type;
        private readonly ModuleWeaver _moduleWeaver;
        private readonly Assets _assets;

        protected TypeProcessorBase(TypeDefinition type, ModuleWeaver moduleWeaver)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (moduleWeaver == null)
            {
                throw new ArgumentNullException("moduleWeaver");
            }
            _type = type;
            _moduleWeaver = moduleWeaver;
            _assets = _moduleWeaver.Assets;
        }

        public TypeDefinition Type
        {
            get { return _type; }
        }

        public ModuleWeaver ModuleWeaver
        {
            get { return _moduleWeaver; }
        }

        public Assets Assets
        {
            get { return _assets; }
        }

        public abstract void Execute();
    }
}