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
        }

        public BindingList<ClipboardEntry> Clips { get; } = new BindingList<ClipboardEntry>();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _clipboard = new Clipboard(this);
            _clipboard.ClipboardChanged += ClipboardChanged;

            var source = (HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow);
            _host = new HotKeyHost(source);
            _hotkey = new HotKey(Key.V, ModifierKeys.Windows | ModifierKeys.Shift);
            _hotkey.HotKeyPressed += PastePressed;
            _host.AddHotKey(_hotkey);
        }

        private void ClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            Clips.Add(e.Data);
        }

        private void PastePressed(object sender, HotKeyEventArgs hotKeyEventArgs)
        {
            var popup = new Popup(Clips);
            popup.Show();
            popup.Activate();
        }
    }
}