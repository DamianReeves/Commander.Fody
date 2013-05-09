using System;
using System.Collections.Generic;
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
        var allTypes = ModuleDefinition.GetTypes().Where(x => x.IsClass).ToList();
        ProcessTypes(allTypes);
    }

    public void ProcessTypes(List<TypeDefinition> allTypes)
    {
        foreach (var type in allTypes)
        {
            ProcessType(type);
        }
    }

    public void ProcessType(TypeDefinition typeDefinition)
    {
        var onCommandMethods = typeDefinition.FindOnCommandMethods().ToList();
        if (!onCommandMethods.Any())
        {
            return;
        }
        foreach (var method in onCommandMethods)
        {
            ProcessOnCommandMethod(method);
        }
    }

    public void ProcessOnCommandMethod(MethodDefinition method)
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
        }               
    }
}