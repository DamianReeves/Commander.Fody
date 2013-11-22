using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Commander;

[assembly:Commander.CommandImplementation(typeof(Commander.Commands.DelegateCommand))]

namespace AssemblyToProcessSilverlight
{
    public class FooViewModel
    {
        public FooViewModel()
        {             
        }
        public ICommand ExistingCommand { get; set; }

        [OnCommand("ExistingCommand")]
        public void ExecuteExistingCommand()
        {
            
        }
    }
}
