using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Commander;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class OnCommandAttributeReaderTests
{
    [Test]
    public void Simple()
    {
        var reader = new ModuleWeaver();
        var logs = new List<string>();
        reader.LogInfo = s =>
        {
            logs.Add(s);
            Console.WriteLine(s);
        };
        var node = new TypeNode
                       {
                           TypeDefinition = DefinitionFinder.FindType<CommandClass>()
                       };
        reader.ProcessType(node.TypeDefinition);

        logs.Should().HaveCount(1);
    }

    public class CommandClass
    {
        [OnCommand("TestCommand")]
        public void OnTestCommand()
        {
        }
    }
}
