using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class AssemblyWithDelegateCommandDotNet4Tests : BaseTaskTests
{

    public AssemblyWithDelegateCommandDotNet4Tests()
        : base(@"TestAssemblies\AssemblyWithDelegateCommand\AssemblyWithDelegateCommandDotNet4.csproj", Console.WriteLine)
    {
    }    

    [Test]
    public void TestCommand_Should_Be_Injected()
    {
        var instance = Assembly.GetInstance("CommandClass");
        var testCommand = instance.TestCommand;
        object testCommandObject = testCommand;
        testCommandObject.Should().NotBeNull("TestCommand should be initialized.");
        var type = (testCommandObject).GetType();
        type.Name.Should().Be("DelegateCommand");
    }

    [Test]
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