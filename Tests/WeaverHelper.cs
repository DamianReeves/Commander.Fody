using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Commander.Fody;
using Mono.Cecil;

public class WeaverHelper
{
    public Assembly Assembly { get; set; }

    public WeaverHelper(string projectPath, Action<ModuleWeaver> configure = null )
    {
        projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\", projectPath));

        var assemblyPath = GetAssemblyPath(projectPath);

        var newAssembly = assemblyPath.Replace(".dll", "2.dll").Replace(".exe","2.exe");
        var pdbFileName = Path.ChangeExtension(assemblyPath, "pdb");
        var newPdbFileName = Path.ChangeExtension(newAssembly, "pdb");
        File.Copy(assemblyPath, newAssembly, true);
        File.Copy(pdbFileName, newPdbFileName, true);

        var assemblyResolver = new TestAssemblyResolver(assemblyPath, projectPath);
        var moduleDefinition = ModuleDefinition.ReadModule(newAssembly, new ReaderParameters {AssemblyResolver = assemblyResolver});
        var weavingTask = new ModuleWeaver
        {
            ModuleDefinition = moduleDefinition,
            AssemblyResolver = assemblyResolver,
            LogInfo = Console.WriteLine,
            LogError = Console.Error.WriteLine
        };

        if (configure != null)
        {
            configure(weavingTask);
        }        

        weavingTask.Execute();
        var writerParameters = new WriterParameters
        {
            WriteSymbols = true
        };
        moduleDefinition.Write(newAssembly,writerParameters);

        Assembly = Assembly.LoadFile(newAssembly);
    }

    public static ModuleDefinition GetModuleDefinition(string projectPath)
    {
        projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\", projectPath));

        var assemblyPath = GetAssemblyPath(projectPath);

        var newAssembly = assemblyPath.Replace(".dll", "2.dll").Replace(".exe", "2.exe");
        var pdbFileName = Path.ChangeExtension(assemblyPath, "pdb");
        var newPdbFileName = Path.ChangeExtension(newAssembly, "pdb");
        File.Copy(assemblyPath, newAssembly, true);
        File.Copy(pdbFileName, newPdbFileName, true);

        var assemblyResolver = new TestAssemblyResolver(assemblyPath, projectPath);
        var moduleDefinition = ModuleDefinition.ReadModule(newAssembly, new ReaderParameters { AssemblyResolver = assemblyResolver });
        return moduleDefinition;
    }

    private static string GetAssemblyPath(string projectPath)
    {
        return Path.Combine(
            Path.GetDirectoryName(projectPath)??string.Empty
            , GetOutputPathValue(projectPath)
            , GetAssemblyName(projectPath) + GetOutputExtension(projectPath)
        );
    }

    private static string GetAssemblyName(string projectPath)
    {
        var xDocument = XDocument.Load(projectPath);
        xDocument.StripNamespace();

        return xDocument.Descendants("AssemblyName")
            .Select(x => x.Value)
            .First();
    }

    private static string GetOutputPathValue(string projectPath)
    {
        var xDocument = XDocument.Load(projectPath);
        xDocument.StripNamespace();

        var outputPathValue = (from propertyGroup in xDocument.Descendants("PropertyGroup")
                               let condition = ((string)propertyGroup.Attribute("Condition"))
                               where (condition != null) &&
                                     (condition.Trim() == "'$(Configuration)|$(Platform)' == 'Debug|AnyCPU'")
                               from outputPath in propertyGroup.Descendants("OutputPath")
                               select outputPath.Value).First();
#if (!DEBUG)
            outputPathValue = outputPathValue.Replace("Debug", "Release");
#endif
        return outputPathValue;
    }

    private static string GetOutputExtension(string projectPath)
    {
        var xDocument = XDocument.Load(projectPath);
        xDocument.StripNamespace();
        var outputTypeValue = (
            from propertyGroup in xDocument.Descendants("PropertyGroup")
            from outputType in propertyGroup.Descendants("OutputType")
            select outputType.Value).First();
        switch (outputTypeValue)
        {
            case "WinExe":
                return ".exe";
            default:
                return ".dll";
        }
    }

}