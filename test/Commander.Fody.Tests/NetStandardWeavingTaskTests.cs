using Xunit;

public class NetStandardWeavingTaskTests : BaseTaskTests, IClassFixture<WeaverFixture>
{
    public NetStandardWeavingTaskTests(WeaverFixture weaverFixture)
        : base(weaverFixture, "AssemblyToProcess","netstandard1.4")
    {
    }
}