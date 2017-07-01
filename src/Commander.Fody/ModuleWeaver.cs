using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Commander.Fody.LightInject;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Commander.Fody
{
    public interface IModuleWeaver
    {
        ModuleDefinition ModuleDefinition { get; set; }
        string AssemblyFilePath { get; set; }
    }
    public class ModuleWeaver: IModuleWeaver, IFodyLogger
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
        public ModuleWeaverSettings Settings { get; set; }

        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarning { get; set; }
        public Action<string, SequencePoint> LogWarningPoint { get; set; }
        public Action<string> LogError { get; set; }
        public Action<string, SequencePoint> LogErrorPoint { get; set; }

        public ModuleDefinition ModuleDefinition { get; set; }
        public string AssemblyFilePath { get; set; }
        public IAssemblyResolver AssemblyResolver { get; set; }
        public Assets Assets { get; private set; }        

        public void Execute()
        {
            LogWarning($"AssemblyFilePath is {AssemblyFilePath}");
            var container = CreateContainer();
            ConfigureContainer(container);
            Setup();
            var processors = GetProcessors(container);
            ExecuteProcessors(processors);
        }

        private static void ExecuteProcessors(IEnumerable<IModuleProcessor> processors)
        {
            foreach (var processor in processors)
            {
                processor.Execute();
            }
        }

        private IEnumerable<IModuleProcessor> GetProcessors(IServiceFactory factory)
        {
            var processors = new IModuleProcessor[]
            {
                factory.GetInstance<CommandAttributeScanner>(),
                factory.GetInstance<DelegateCommandClassInjectionProcessor>(),
                factory.GetInstance<ModuleTypesProcessor>(),
                factory.GetInstance<AttributeCleanerProcessor>(),
                factory.GetInstance<ReferenceCleanerProcessor>()
            };
            return processors;
        }

        private void Setup()
        {
            Settings = new ModuleWeaverSettings(Config);
            Assets = new Assets(this);
        }

        private IServiceContainer CreateContainer()
        {
            return new ServiceContainer();
        }

        private void ConfigureContainer(IServiceRegistry registry)
        {
            registry.RegisterInstance<IFodyLogger>(this);
            registry.RegisterInstance<ModuleWeaver>(this);
            registry.RegisterInstance<ModuleDefinition>(this.ModuleDefinition);
            registry.RegisterAssembly(GetType().Assembly
                , (serviceType, implementingType) => typeof(IModuleProcessor).IsAssignableFrom(implementingType));
        }
    }
}