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
        var type = ((object) testCommand).GetType();
        type.ShouldHave().Properties(t=>t.Name).EqualTo(new {Name="DelegateCommand"});
    }
}