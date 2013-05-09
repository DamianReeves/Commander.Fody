using System;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class Net4WeavingTaskTests : BaseTaskTests
{

    public Net4WeavingTaskTests()
        : base(@"AssemblyToProcess\AssemblyToProcessDotNet4.csproj")
    {

    }    

    [Test]
    public void Simple()
    {
        var instance = Assembly.GetInstance("CommandClass");
        Action action = () => { var command = instance.TestCommand; };
        action.ShouldNotThrow();
    }
}