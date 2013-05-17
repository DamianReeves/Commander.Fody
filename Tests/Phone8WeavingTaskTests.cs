using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commander.Fody;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Phone8WeavingTaskTests : BaseTaskTests
    {
        public Phone8WeavingTaskTests()
            : base(@"TestAssemblies\AssemblyToProcessPhone8\AssemblyToProcessPhone8.csproj")
        {
        }

        [Test]
        public void Simple()
        {
            //C:\Users\Damian\Documents\GitHub\DamianReeves\Commander.Fody\TestAssemblies\AssemblyToProcessPhone8\FormViewModel.cs
            
        }
    }
}
