using System;
using System.Linq;
using System.Text;
using Mono.Cecil;

public class ModuleWeaver
{
    public ModuleWeaver()
    {
        LogInfo = s => { };
    }

    // Will log an informational message to MSBuild
    public Action<string> LogInfo { get; set; }
    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }    

    public void Execute()
    {
        var context = new WeavingContext(ModuleDefinition);
        ProcessTypes(context);
    }

    public void ProcessTypes(WeavingContext context)
    {
        foreach (var type in context.AllTypes)
        {
            ProcessType(type, context);
        }
    }

    public void ProcessType(TypeDefinition typeDefinition, WeavingContext context)
    {
        var onCommandMethods = typeDefinition.FindOnCommandMethods().ToList();
        if (!onCommandMethods.Any())
        {
            return;
        }
        foreach (var method in onCommandMethods)
        {
            ProcessOnCommandMethod(method, context);
        }
    }

    public void ProcessOnCommandMethod(MethodDefinition method, WeavingContext context)
    {
        var onCommandAttributes = 
            method.CustomAttributes
                .Where(attrib => attrib.Constructor.DeclaringType.Name == "OnCommandAttribute")
                .Where(attrib => attrib.HasConstructorArguments && attrib.ConstructorArguments.First().Type.Name == "String");
        var commandNames = onCommandAttributes.Select(attrib => attrib.ConstructorArguments.First().Value).OfType<string>();
        var type = method.DeclaringType;
        foreach (var commandName in commandNames)
        {
            LogInfo(string.Format("Found OnCommand method {0} for command {1} on type {2}"
                                  , method
                                  , commandName
                                  , type.Name));
            try
            {
                CommandPropertyInjector.AddProperty(context.CommonTypes.ICommand, type, commandName);
            }
            catch (Exception ex)
            {
                LogInfo(string.Format("Error while adding property {0} to {1}: {2}", commandName, type, ex));
            }
        }               
    }
}