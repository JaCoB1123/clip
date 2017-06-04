using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
    }
}