using System;
using System.Linq;
using FluentAssertions;
using Xunit;

public class AssemblyWithDelegateCommandDotNet4Tests : BaseTaskTests, IClassFixture<WeaverFixture>
{

    public AssemblyWithDelegateCommandDotNet4Tests(WeaverFixture weaverFixture)
        : base(weaverFixture, @"TestAssemblies\AssemblyWithDelegateCommand\AssemblyWithDelegateCommandDotNet4.csproj")
    {
    }    

    [Fact]
    public void TestCommand_Should_Be_Injected()
    {
        var instance = Assembly.GetInstance("CommandClass");
        var testCommand = instance.TestCommand;
        object testCommandObject = testCommand;
        testCommandObject.Should().NotBeNull("TestCommand should be initialized.");
        var type = (testCommandObject).GetType();
        type.Name.Should().Be("DelegateCommand");
    }

    [Fact]
    public void SubmitCommand_Should_Be_Injected()
    {
        var instance = Assembly.GetInstance("CommandClass");
        var testCommand = instance.TestCommand;
        object testCommandObject = testCommand;
        testCommandObject.Should().NotBeNull("SubmitCommand should be initialized.");
        var type = (testCommandObject).GetType();
        type.Name.Should().Be("DelegateCommand");
    }
}