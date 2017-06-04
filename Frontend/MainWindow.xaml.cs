using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Clip;
using Clipboard = Clip.Clipboard;

namespace Frontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Clipboard _clipboard;
        private HotKeyHost _host;
        private HotKey _hotkey;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += OnLoaded;
        }

        public BindingList<ClipboardEntry> Clips { get; } = new BindingList<ClipboardEntry>();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Initialize the clipboard now that we have a window soruce to use
            _clipboard = new Clipboard(this);
            _clipboard.ClipboardChanged += ClipboardChanged;
        }

        private void ClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            Clips.Add(e.Data);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var source = (HwndSource)HwndSource.FromVisual(App.Current.MainWindow);
            _host = new HotKeyHost(source);
            _hotkey = new HotKey(Key.V, ModifierKeys.Windows | ModifierKeys.Shift);
            _hotkey.HotKeyPressed += PastePressed;
            _host.AddHotKey(_hotkey);
        }

        private void PastePressed(object sender, HotKeyEventArgs hotKeyEventArgs)
        {
            BringIntoView();
        }
    }
}