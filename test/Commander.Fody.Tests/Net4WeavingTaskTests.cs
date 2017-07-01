using System;
using System.Linq;
using FluentAssertions;
using Xunit;

public class Net4WeavingTaskTests : BaseTaskTests, IClassFixture<WeaverFixture>
{

    public Net4WeavingTaskTests(WeaverFixture weaverFixture)
        : base(weaverFixture, @"TestAssemblies\AssemblyToProcess\AssemblyToProcess.csproj", Console.WriteLine)
    {
    }    

    [Fact]
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