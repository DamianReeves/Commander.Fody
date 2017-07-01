using System;
using System.Reflection;
using Commander.Fody;
using Xunit;

public abstract class BaseTaskTests
{
    public Assembly Assembly;
    private WeaverHelper weaverHelper;

    protected BaseTaskTests(WeaverFixture weaverFixture, string assemblyName, string targetFrameork = null)
    {
        targetFrameork = targetFrameork ?? "net462";
        weaverFixture.SetAssemblyName(assemblyName);
        weaverFixture.SetTargetFramework(targetFrameork);
        weaverHelper = weaverFixture.GetWeaverHelper();
        Assembly = weaverFixture.GetWeaverHelper().Assembly;
    }


    [Fact]
    public void PeVerify()
    {
        Verifier.Verify(weaverHelper.BeforeAssemblyPath, weaverHelper.AfterAssemblyPath);
    }

}