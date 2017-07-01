using System;

namespace Commander
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = true)]
    public class CommandInterfaceAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public CommandInterfaceAttribute(Type type) => CommandInterfaceType = type ?? throw new ArgumentNullException(nameof(type));

        /// <summary>
        /// 
        /// </summary>
        public Type CommandInterfaceType { get; }
    }
}   