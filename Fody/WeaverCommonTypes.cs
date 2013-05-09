using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Mono.Cecil;

public class WeaverCommonTypes
{
    public readonly TypeReference ICommand;
    public TypeReference DelegateCommand { get; set; }
    public ModuleDefinition ModuleDefinition { get; private set; }

    public WeaverCommonTypes(ModuleWeavingContext moduleContext)
    {
        ModuleDefinition = moduleContext.ModuleDefinition;
        var assemblyResolver = ModuleDefinition.AssemblyResolver;
        var msCoreLibDefinition = assemblyResolver.Resolve("mscorlib");
        var msCoreTypes = msCoreLibDefinition.MainModule.Types;

        var objectDefinition = msCoreTypes.FirstOrDefault(x => x.Name == "Object");
        if (objectDefinition == null)
        {
            ExecuteWinRT();
            return;
        }

        var presentationCoreDefinition = GetPresentationCoreDefinition();
        var presentationCoreTypes = presentationCoreDefinition.MainModule.Types;
        var systemDefinition = assemblyResolver.Resolve("System");
        var systemTypes = systemDefinition.MainModule.Types;
        var iCommandDefinition = presentationCoreTypes.FirstOrDefault(x => x.Name == "ICommand");                
        if (iCommandDefinition == null)
        {
            iCommandDefinition = systemTypes.FirstOrDefault(x => x.Name == "ICommand");
        }
        ICommand = ModuleDefinition.Import(iCommandDefinition);
        if (ICommand == null)
        {
            var message = "Could not find type System.Windows.Input.ICommand.";
            throw new WeavingException(message);
        }

        TypeReference delegateCommandRef;
        if (moduleContext.TryFindDelegateCommandType(out delegateCommandRef))
        {
            DelegateCommand = delegateCommandRef;
        }
    }

    private void ExecuteWinRT()
    {        
    }

    AssemblyDefinition GetSystemCoreDefinition()
    {
        try
        {
            return ModuleDefinition.AssemblyResolver.Resolve("System.Core");
        }
        catch (Exception exception)
        {
            var message = string.Format(@"Could not resolve System.Core. Please ensure you are using .net 3.5 or higher.{0}Inner message:{1}.", Environment.NewLine, exception.Message);
            throw new WeavingException(message);
        }
    }

    AssemblyDefinition GetPresentationCoreDefinition()
    {
        try
        {
            return ModuleDefinition.AssemblyResolver.Resolve("PresentationCore");
        }
        catch (Exception exception)
        {
            var message = string.Format(@"Could not resolve PresentationCore. Please ensure you are using .net 3.5 or higher.{0}Inner message:{1}.", Environment.NewLine, exception.Message);
            message += " AssemblyResolver is: " + ModuleDefinition.AssemblyResolver.GetType().FullName;
            throw new WeavingException(message);
        }
    }
}