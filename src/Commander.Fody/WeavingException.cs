using System;

namespace Commander.Fody
{
    public class WeavingException : Exception
    {
        public WeavingException(string message)
            : base(message)
        {

        }
    }
}