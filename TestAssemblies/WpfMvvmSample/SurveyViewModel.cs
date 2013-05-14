using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfMvvmSample
{
    public class SurveyViewModel : NotifyingObjectBase
    {
        private string _answer;
        private string _message;

        public string Answer
        {
            get { return _answer; }
            set { SetProperty(ref _answer, value, "Answer"); }
        }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value, "Message"); }
        }

        [OnCommandCanExecute("SubmitCommand")]
        public bool CanSubmit(string answer)
        {
            return !String.IsNullOrWhiteSpace(answer) && answer.Length > 3;
        }

        [OnCommand("SubmitCommand")]
        public void Submit(string answer)
        {
            var sb = new StringBuilder("Submitted answer is: ");
            sb.AppendLine().AppendLine(answer);
            Message = sb.ToString();
            Answer = string.Empty;
        }
    }
}
