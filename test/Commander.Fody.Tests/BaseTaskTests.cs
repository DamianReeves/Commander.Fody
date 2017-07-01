using System;
using System.Reflection;
using Commander.Fody;
using Xunit;

public abstract class BaseTaskTests
{
    public Assembly Assembly;

    protected BaseTaskTests(WeaverFixture weaverFixture, string projectPath, Action<ModuleWeaver> configureAction = null)
    {
#if (!DEBUG)

            projectPath = projectPath.Replace("Debug", "Release");
#endif
        weaverFixture.SetProjectPath(projectPath);
        weaverFixture.ConfigureWeaver(configureAction);
        Assembly = weaverFixture.GetWeaverHelper().Assembly;
    }


#if(DEBUG)
    [Fact]
    public void PeVerify()
    {
        Verifier.Verify(Assembly.CodeBase.Remove(0, 8));
    }
#endif

}