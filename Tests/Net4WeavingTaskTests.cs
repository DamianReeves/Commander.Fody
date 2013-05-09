using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class Net4WeavingTaskTests : BaseTaskTests
{

    public Net4WeavingTaskTests()
        : base(@"TestAssemblies\AssemblyToProcess\AssemblyToProcessDotNet4.csproj", Console.WriteLine)
    {
    }    

    [Test]
    public void TestCommand_Should_Be_Injected()
    {
        object instance = Assembly.GetInstance("CommandClass");
        var type = instance.GetType();
        var testCommandProperty = type.Properties().Single(prop => prop.Name == "TestCommand");
        testCommandProperty.PropertyType.FullName.Should().Be("System.Windows.Input.ICommand");
        var getter = testCommandProperty.GetGetMethod();
        var setter = testCommandProperty.GetSetMethod();
        getter.As<object>().Should().NotBeNull("property "+testCommandProperty.Name+" should have a getter.");
        setter.As<object>().Should().NotBeNull("property "+testCommandProperty.Name+" should have setter.");
    }
}