using System;
using System.Windows;
using System.Windows.Interop;
using IDataObject = System.Windows.IDataObject;

namespace Clip
{
    public class Clipboard
    {
        private static readonly IntPtr WndProcSuccess = IntPtr.Zero;

        public Clipboard(Window windowSource)
        {
            var source = PresentationSource.FromVisual(windowSource) as HwndSource;
            if (source == null)
            {
                throw new ArgumentException(
                    "Window source MUST be initialized first, such as in the Window's OnSourceInitialized handler."
                    , nameof(windowSource));
            }

            source.AddHook(WndProc);

            // get window handle for interop
            var windowHandle = new WindowInteropHelper(windowSource).Handle;

            // register for clipboard events
            NativeMethods.AddClipboardFormatListener(windowHandle);
        }

        public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;

        private void OnClipboardChanged()
        {
            var obj = System.Windows.Clipboard.GetDataObject();
            var entry = new ClipboardEntry(obj);
            var args = new ClipboardChangedEventArgs(entry);
            ClipboardChanged?.Invoke(this, args);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                OnClipboardChanged();
                handled = true;
            }

            return WndProcSuccess;
        }
    }

    public class ClipboardChangedEventArgs : EventArgs
    {
        public readonly ClipboardEntry Data;

        public ClipboardChangedEventArgs(ClipboardEntry dataObject)
        {
            Data = dataObject;
        }
    }

    public class ClipboardEntry
    {
        private readonly IDataObject _dataObject;
        private string _text;

        public ClipboardEntry(IDataObject dataObject)
        {
            _dataObject = dataObject;
        }

        public string DisplayText => Text?.Trim()?.RemoveNewlines();

        public string Text => _text ?? (_text = GetData<string>(DataFormats.UnicodeText));

        public T GetData<T>(string format)
        {
            if (!_dataObject.GetDataPresent(format))
                return default(T);

            return (T)_dataObject.GetData(format);
        }
    }
}