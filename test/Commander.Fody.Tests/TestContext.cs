using System.IO;
using System;
using System.Reflection;
public class TestContext
{
    private static Lazy<TestContext> _currentContextAccessor = new Lazy<TestContext>();

    public TestContext()
    {
        TestDirectory = GetTestDirectory();
    }

    public static TestContext CurrentContext => _currentContextAccessor.Value;
    public string TestDirectory { get; }

    private string GetTestDirectory()
    {
        //var location = typeof(TestContext).GetTypeInfo().Assembly.Location;
        //return Path.GetDirectoryName(location);
        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        return Path.GetDirectoryName(codeBasePath);
    }
}