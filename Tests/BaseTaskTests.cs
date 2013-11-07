using System;
using System.Reflection;
using Commander.Fody;
using NUnit.Framework;

public abstract class BaseTaskTests
{
    private readonly string _projectPath;
    private readonly Action<ModuleWeaver> _configureAction;
    public Assembly Assembly;

    protected BaseTaskTests(string projectPath, Action<ModuleWeaver> configureAction = null)
    {

#if (!DEBUG)

            projectPath = projectPath.Replace("Debug", "Release");
#endif
        _projectPath = projectPath;
        _configureAction = configureAction;
    }

    [TestFixtureSetUp]
    public void Setup()
    {
        var weaverHelper = new WeaverHelper(_projectPath, _configureAction);
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