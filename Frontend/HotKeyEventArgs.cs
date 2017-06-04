using System;

namespace Frontend
{
    public class HotKeyEventArgs : EventArgs
    {
        public HotKeyEventArgs(HotKey hotKey)
        {
            HotKey = hotKey;
        }

        public HotKey HotKey { get; }
    }
}