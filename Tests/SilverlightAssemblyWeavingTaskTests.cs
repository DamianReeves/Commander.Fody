using NUnit.Framework;

namespace Tests
{
    public class SilverlightAssemblyWeavingTaskTests : BaseTaskTests
    {
        public SilverlightAssemblyWeavingTaskTests()
            : base(@"TestAssemblies\AssemblyToProcessSilverlight\AssemblyToProcessSilverlight.csproj")
        {
        }

        [Test]
        public void Simple()
        {            
        }
    }
}