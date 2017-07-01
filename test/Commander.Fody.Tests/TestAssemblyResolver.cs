using System.IO;
using Mono.Cecil;

public class TestAssemblyResolver : DefaultAssemblyResolver
{
    public TestAssemblyResolver(string targetFramework = "net462")
    {
        var fullPath = Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory, 
            $@"..\..\..\..\TestAssemblyBin\{targetFramework}"));
        AddSearchDirectory(fullPath);
    }
}
