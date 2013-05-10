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
            var context = new ModuleWeavingContext(ModuleDefinition, LogInfo);
            Prepare(context);
            AddCommandInitialization(context);
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

        public void Prepare(ModuleWeavingContext moduleContext)
        {
            foreach (var type in moduleContext.AllTypes)
            {
                Prepare(type, moduleContext);
            }
        }

        public void Prepare(TypeDefinition typeDefinition, ModuleWeavingContext context)
        {
            var onCommandMethods = typeDefinition.FindOnCommandMethods().ToList();
            if (!onCommandMethods.Any())
            {
                return;
            }
            var typeContext = context.GetTypeWeavingContext(typeDefinition);
            context.WeavableTypes.Add(typeContext.Type);
            foreach (var method in onCommandMethods)
            {
                ProcessOnCommandMethod(method, typeContext);
            }
        }

        public void ProcessOnCommandMethod(MethodDefinition method, TypeWeavingContext context)
        {
            var onCommandAttributes = 
                method.CustomAttributes
                    .Where(attrib => attrib.Constructor.DeclaringType.Name == "OnCommandAttribute")
                    .Where(attrib => attrib.HasConstructorArguments && attrib.ConstructorArguments.First().Type.Name == "String");
            var commandNames = onCommandAttributes.Select(attrib => attrib.ConstructorArguments.First().Value).OfType<string>();
            var type = method.DeclaringType;
            var icommandTypeRef = context.ModuleContext.CommonTypes.ICommand;
            foreach (var commandName in commandNames)
            {
                this.Info("Found OnCommand method {0} for command {1} on type {2}"
                    , method
                    , commandName
                    , type.Name);
                try
                {
                    PropertyDefinition propertyDefinition;
                    if (context.Type.TypeDefinition.TryAddCommandProperty(icommandTypeRef, commandName, out propertyDefinition))
                    {
                        var commandData = new CommandData(commandName) {CommandProperty = propertyDefinition};
                        context.Type.ReferencedCommands.Add(commandData);
                        context.Type.InjectedCommands.Add(commandData);
                    }
                    else
                    {
                        var commandData = new CommandData(commandName) { CommandProperty = propertyDefinition };
                        context.Type.ReferencedCommands.Add(commandData);
                    }
                }
                catch (Exception ex)
                {
                    this.Error("Error while adding property {0} to {1}: {2}", commandName, type, ex);
                }
            }               
        }

        public void AddCommandInitialization(ModuleWeavingContext context)
        {
            MethodDefinition initializeMethod = null;
            foreach (var typeNode in context.WeavableTypes)
            {
                if (typeNode.ReferencedCommands.Any())
                {                
                    typeNode.TypeDefinition.TryAddCommandInitializerMethod(out initializeMethod);
                }
                if (initializeMethod == null)
                {
                    // TODO: First log/notify that there is a problem
                    continue;
                }
                foreach (var command in typeNode.ReferencedCommands)
                {
                    typeNode.TypeDefinition.TryAddCommandPropertyInitialization(initializeMethod, command);
                }
                initializeMethod = null;
            }
        }
    }
}