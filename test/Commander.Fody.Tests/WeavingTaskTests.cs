using Xunit;

public class WeavingTaskTests : BaseTaskTests, IClassFixture<WeaverFixture>
{
    public WeavingTaskTests(WeaverFixture weaverFixture)
        : base(weaverFixture, "AssemblyToProcess")
    {
    }
}