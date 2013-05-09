using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class OnCommandAttribute: Attribute
{
    public OnCommandAttribute(string commandName)
    {        
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class OnCommandCanExecuteAttribute : Attribute
{
    public OnCommandCanExecuteAttribute(string commandName)
    {
    }
}