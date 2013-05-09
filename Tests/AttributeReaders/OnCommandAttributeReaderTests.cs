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
    [Test,Ignore]
    public void Simple()
    {
        var reader = new ModuleWeaver
        {
            ModuleDefinition = DefinitionFinder.FindType<CommandClass>().Module
        };
        var logs = new List<string>();
        reader.LogInfo = s =>
        {
            logs.Add(s);
            Console.WriteLine(s);
        };

        var type =  DefinitionFinder.FindType<CommandClass>();
        var context = new ModuleWeavingContext(reader.ModuleDefinition, reader.LogInfo);
        reader.GatherCommandPointcuts(type, context);

        var onCommandLog = logs.Where(s => s.StartsWith("Found OnCommand method")).ToList();
        onCommandLog.Should().HaveCount(1);
    }

    public class CommandClass
    {        
        [OnCommand("TestCommand")]
        public void OnTestCommand()
        {
        }
    }
}
