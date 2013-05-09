using System;
using System.Linq;
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
    public void TestCommand_Should_Be_Injected()
    {
        object instance = Assembly.GetInstance("CommandClass");
        var type = instance.GetType();
        var testCommandProperty = type.Properties().Single(prop => prop.Name == "TestCommand");
        testCommandProperty.PropertyType.FullName.Should().Be("System.Windows.Input.ICommand");
        //Action action = () => { var command = instance.TestCommand; };
        //action.ShouldNotThrow();
    }
}