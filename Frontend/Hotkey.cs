using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Windows.Interop;

namespace Frontend
{
    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }

    public class Hotkey : IDisposable
    {
        public const int WmHotKey = 0x0312;
        private static Dictionary<int, Hotkey> _dictHotKeyToCalBackProc;

        private bool _disposed;

        public Hotkey(Key k, KeyModifier keyModifiers, Action<Hotkey> action, bool register = true)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;
            if (register)
            {
                Register();
            }
        }

        public Action<Hotkey> Action { get; }

        public int Id { get; set; }

        public Key Key { get; }

        public KeyModifier KeyModifiers { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Register()
        {
            var virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            var result = RegisterHotKey(IntPtr.Zero, Id, (UInt32)KeyModifiers, (UInt32)virtualKeyCode);

            if (_dictHotKeyToCalBackProc == null)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, Hotkey>();
                ComponentDispatcher.ThreadFilterMessage += ComponentDispatcherThreadFilterMessage;
            }

            _dictHotKeyToCalBackProc.Add(Id, this);

            Debug.Print(result + ", " + Id + ", " + virtualKeyCode);
            return result;
        }

        public void Unregister()
        {
            if (_dictHotKeyToCalBackProc.TryGetValue(Id, out Hotkey _))
            {
                UnregisterHotKey(IntPtr.Zero, Id);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Unregister();
            }

            _disposed = true;
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (handled || msg.message != WmHotKey)
                return;

            if (!_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out Hotkey hotKey))
                return;

            hotKey.Action?.Invoke(hotKey);
            handled = true;
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    /// <summary>
    /// Represents an hotKey
    /// </summary>
    [Serializable]
    public class HotKey : INotifyPropertyChanged, ISerializable, IEquatable<HotKey>
    {
        private bool enabled;

        private Key key;

        private ModifierKeys modifiers;

        /// <summary>
        /// Creates an HotKey object. This instance has to be registered in an HotKeyHost.
        /// </summary>
        public HotKey() { }

        /// <summary>
        /// Creates an HotKey object. This instance has to be registered in an HotKeyHost.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="modifiers">The modifier. Multiple modifiers can be combined with or.</param>
        public HotKey(Key key, ModifierKeys modifiers) : this(key, modifiers, true) { }

        /// <summary>
        /// Creates an HotKey object. This instance has to be registered in an HotKeyHost.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="modifiers">The modifier. Multiple modifiers can be combined with or.</param>
        /// <param name="enabled">Specifies whether the HotKey will be enabled when registered to an HotKeyHost</param>
        public HotKey(Key key, ModifierKeys modifiers, bool enabled)
        {
            Key = key;
            Modifiers = modifiers;
            Enabled = enabled;
        }

        protected HotKey(SerializationInfo info, StreamingContext context)
        {
            Key = (Key)info.GetValue("Key", typeof(Key));
            Modifiers = (ModifierKeys)info.GetValue("Modifiers", typeof(ModifierKeys));
            Enabled = info.GetBoolean("Enabled");
        }

        /// <summary>
        /// Will be raised if the hotkey is pressed (works only if registed in HotKeyHost)
        /// </summary>
        public event EventHandler<HotKeyEventArgs> HotKeyPressed;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (value != enabled)
                {
                    enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }

        /// <summary>
        /// The Key. Must not be null when registering to an HotKeyHost.
        /// </summary>
        public Key Key
        {
            get
            {
                return key;
            }
            set
            {
                if (key != value)
                {
                    key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        /// <summary>
        /// The modifier. Multiple modifiers can be combined with or.
        /// </summary>
        public ModifierKeys Modifiers
        {
            get
            {
                return modifiers;
            }
            set
            {
                if (modifiers != value)
                {
                    modifiers = value;
                    OnPropertyChanged("Modifiers");
                }
            }
        }

        public override bool Equals(object obj)
        {
            HotKey hotKey = obj as HotKey;
            if (hotKey != null)
                return Equals(hotKey);
            else
                return false;
        }

        public bool Equals(HotKey other)
        {
            return (Key == other.Key && Modifiers == other.Modifiers);
        }

        public override int GetHashCode()
        {
            return (int)Modifiers + 10 * (int)Key;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", Key, typeof(Key));
            info.AddValue("Modifiers", Modifiers, typeof(ModifierKeys));
            info.AddValue("Enabled", Enabled);
        }

        public override string ToString()
        {
            return string.Format("{0} + {1} ({2}Enabled)", Key, Modifiers, Enabled ? "" : "Not ");
        }

        internal void RaiseOnHotKeyPressed()
        {
            OnHotKeyPress();
        }

        protected virtual void OnHotKeyPress()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this, new HotKeyEventArgs(this));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}