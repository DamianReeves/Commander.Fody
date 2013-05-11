using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Commander.Fody
{
    public class ModuleWeaver: IFodyLogger
    {
        public ModuleWeaver()
        {
            LogInfo = m => { };
            LogWarning = m => { };
            LogWarningPoint = (m, p) => { };
            LogError = m => { };
            LogErrorPoint = (m, p) => { };
        }

        // Will contain the full element XML from FodyWeavers.xml. OPTIONAL
        public XElement Config { get; set; }

        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarning { get; set; }
        public Action<string, SequencePoint> LogWarningPoint { get; set; }
        public Action<string> LogError { get; set; }
        public Action<string, SequencePoint> LogErrorPoint { get; set; }

        public ModuleDefinition ModuleDefinition { get; set; }
        public IAssemblyResolver AssemblyResolver { get; set; }
        public Assets Assets { get; private set; }

        public void Execute()
        {
            Assets = new Assets(ModuleDefinition, this);
            ProcessTypes(GetTypesToProcess());
        }

        public IEnumerable<TypeDefinition> GetTypesToProcess()
        {
            return ModuleDefinition.GetTypes().Where(x => x.IsClass);
        }

        public void ProcessTypes(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                try
                {
                    var typeProcessor = new TypeProcessor(type, this);
                    typeProcessor.Execute();
                }
                catch(Exception ex)
                {
                    Assets.Log.Error(ex);
                }                
            }
        }        
    }
}