using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Tests
{
    public class WpfMvvmSampleTests : BaseTaskTests
    {
        public WpfMvvmSampleTests()
            : base(@"TestAssemblies\WpfMvvmSample\WpfMvvmSample.csproj", Console.WriteLine)
        {
            
        }

        [Test]
        public void DelegateCommand_Class_Should_Be_Injected()
        {
            var delegateCommandType = (
                from type in Assembly.GetTypes()
                where type.IsClass
                    && type.Name == "<Commander_Fody>__DelegateCommand"
                select type).FirstOrDefault();

            Assert.That(delegateCommandType, Is.Not.Null);
        }

        [Test]
        public void Injected_DelegateCommand_Should_Implement_ICommand()
        {
            var delegateCommandType = (
                from type in Assembly.GetTypes()
                where type.IsClass
                    && type.Name == "<Commander_Fody>__DelegateCommand"
                select type).FirstOrDefault();

            var iface = delegateCommandType.GetInterface("ICommand");
            Assert.That(iface, Is.Not.Null);
        }

        [Test]
        public void TestCommand_Should_Be_Injected()
        {
            var instance = Assembly.GetInstance("WpfMvvmSample.SurveyViewModel");
            var submitCommand = instance.SubmitCommand;
            object submitCommandObject = submitCommand;
            submitCommandObject.Should().NotBeNull("SubmitCommand should be initialized.");
            var type = (submitCommandObject).GetType();
            type.Name.Should().Be("DelegateCommand");
        }
    }
}
