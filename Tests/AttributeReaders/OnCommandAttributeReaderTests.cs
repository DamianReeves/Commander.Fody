using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Commander;
using Commander.Fody;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class OnCommandAttributeReaderTests
{
    [Test,Ignore]
    public void Simple()
    {
        var weaver = new ModuleWeaver
        {
            ModuleDefinition = DefinitionFinder.FindType<CommandClass>().Module
        };
        var logs = new List<string>();
        weaver.LogInfo = s =>
        {
            logs.Add(s);
            Console.WriteLine(s);
        };

        var type =  DefinitionFinder.FindType<CommandClass>();
        var context = new ModuleWeavingContext(weaver.ModuleDefinition, weaver.LogInfo);
        weaver.Prepare(type, context);

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
