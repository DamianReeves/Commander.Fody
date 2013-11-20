using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Commander.Commands
{
    /// <summary>
    /// Manages a list of weak references to delegates.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
    public class WeakEvent<TDelegate> where TDelegate : class
    {
        private readonly List<WeakReference> _delegates = new List<WeakReference>();

        /// <summary>
        /// Adds the specified event handler to the invocation list.
        /// </summary>
        /// <param name="handler">The handler to add.</param>
        [DebuggerStepThrough]
        public void Add(TDelegate handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            lock (_delegates)
                _delegates.Add(new WeakReference(handler));
        }

        /// <summary>
        /// Removes the specified event handler from the invocation list.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        [DebuggerStepThrough]
        public void Remove(TDelegate handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            lock (_delegates)
                for (int i = _delegates.Count - 1; i >= 0; i--)
                {
                    var current = _delegates[i].Target;
                    if (current == null)
                        _delegates.RemoveAt(i);
                    else if (current.Equals(handler))
                    {
                        _delegates.RemoveAt(i);
                        break;
                    }
                }
        }

        /// <summary>
        /// Invokes each handler in the invocation list with the specified invoker.
        /// </summary>
        /// <param name="invoker">An action that, when called, invokes the given handler with the proper arguments.</param>
        [DebuggerStepThrough]
        public void Invoke(Action<TDelegate> invoker)
        {
            if (invoker == null) throw new ArgumentNullException("invoker");

            lock (_delegates)
                for (int i = _delegates.Count - 1; i >= 0; i--)
                {
                    var current = _delegates[i].Target;
                    if (current == null)
                        _delegates.RemoveAt(i);
                    else
                        invoker((TDelegate)current);
                }
        }
    }
}