using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commander.Fody;
using Xunit;

namespace Tests
{
 
    public class Phone8WeavingTaskTests : BaseTaskTests, IClassFixture<WeaverFixture>
    {
        public Phone8WeavingTaskTests(WeaverFixture weaverFixture)
            : base(weaverFixture, @"TestAssemblies\AssemblyToProcessPhone8\AssemblyToProcessPhone8.csproj")
        {
        }

        [Fact]
        public void Simple()
        {
            //C:\Users\Damian\Documents\GitHub\DamianReeves\Commander.Fody\TestAssemblies\AssemblyToProcessPhone8\FormViewModel.cs
            
        }
    }
}
