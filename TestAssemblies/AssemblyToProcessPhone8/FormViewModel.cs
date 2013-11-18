using System;
using System.Collections;
using System.Collections.Generic;
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

namespace AssemblyToProcessPhone8
{
    public class FormViewModel
    {
        public FormViewModel()
        {
            ExecuteParameters = new List<Tuple<string,object>>();
            CanExecuteParameters = new List<Tuple<string,object>>();
        }

        public event EventHandler Submitted;        

        public IList<Tuple<string,object>> ExecuteParameters { get; set; }
        public IList<Tuple<string,object>> CanExecuteParameters { get; set; }

        [OnCommandCanExecute("SubmitCommand")]
        public bool CanSubmit(object parameter)
        {
            NotifyOfCanExecuted("SubmitCommand", parameter);
            return parameter != null;
        }

        [OnCommand("SubmitCommand")]
        public void Submit(object parameter)
        {
            NotifyOfExecuted("SubmitCommand", parameter);
            OnSubmitted();
        }

        protected void NotifyOfExecuted(string commandName, object parameter)
        {
            var list = ExecuteParameters;
            if (list != null)
            {
                list.Add(Tuple.Create(commandName, parameter));
            }
        }

        protected void NotifyOfCanExecuted(string commandName, object parameter)
        {
            var list = CanExecuteParameters;
            if (list != null)
            {
                list.Add(Tuple.Create(commandName, parameter));
            }
        }

        protected virtual void OnSubmitted()
        {
            EventHandler handler = Submitted;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
