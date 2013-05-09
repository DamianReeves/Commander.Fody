using System;
using System.Reflection;
using NUnit.Framework;

public abstract class BaseTaskTests
{
    private readonly string _projectPath;
    private readonly Action<string> _logger;
    public Assembly Assembly;

    protected BaseTaskTests(string projectPath, Action<string> logger = null)
    {

#if (!DEBUG)

            projectPath = projectPath.Replace("Debug", "Release");
#endif
        _projectPath = projectPath;
        _logger = logger;
    }

    [TestFixtureSetUp]
    public void Setup()
    {
        var weaverHelper = new WeaverHelper(_projectPath, _logger);
        Assembly = weaverHelper.Assembly;
    }



#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(Assembly.CodeBase.Remove(0, 8));
    }
#endif

}