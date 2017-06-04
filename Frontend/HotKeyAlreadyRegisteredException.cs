using System;
using System.Runtime.Serialization;

namespace Frontend
{
    [Serializable]
    public class HotKeyAlreadyRegisteredException : Exception
    {
        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey) : base(message)
        {
            HotKey = hotKey;
        }

        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey, Exception inner) : base(message, inner)
        {
            HotKey = hotKey;
        }

        protected HotKeyAlreadyRegisteredException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }

        public HotKey HotKey { get; }
    }
}