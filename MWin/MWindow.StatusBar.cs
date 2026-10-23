using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MashWin {
    public partial class MWindow {
        // Protected fields make the inherited container editable in the designer.
        protected Panel statusBarControls;
        private StatusStrip statusBarStrip;
        private ToolStripStatusLabel statusText;
        private bool layingOutChrome;

        [Category("MWin Customization")]
        [Description("Owns status controls inside the frame, clear of the resize grip.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Panel StatusBarControls {
            get { return statusBarControls; }
            }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StatusStrip StatusBarStrip {
            get {
                EnsureStatusBar();
                return statusBarStrip;
                }
            }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolStripStatusLabel StatusText {
            get {
                EnsureStatusBar();
                return statusText;
                }
            }

        private int TitleDividerY {
            get { return 26 + Math.Max(1, settings.Frame.BorderThickness); }
            }

        private int TitleContentTop {
            get { return TitleDividerY + (Math.Max(1, settings.Frame.BorderThickness) + 1) / 2 + 1; }
            }

        private int BottomChromeHeight {
            get {
                int height = settings.StatusBar.Visible ? settings.StatusBar.Height : 0;
                // A resize corner needs a clear hit-test surface even when its
                // diagonal hint is hidden or the status bar is disabled.
                return settings.Frame.Resizable ? Math.Max(height, 22) : height;
                }
            }

        public override Rectangle DisplayRectangle {
            get {
                Rectangle available = base.DisplayRectangle;
                if (settings == null || statusBarControls == null || !settings.Frame.ShowChrome) {
                    return available;
                    }
                int border = Math.Max(1, settings.Frame.BorderThickness);
                available.X += border;
                available.Y += TitleContentTop;
                available.Width = Math.Max(0, available.Width - border * 2);
                available.Height = Math.Max(0, available.Height - TitleContentTop - border - BottomChromeHeight);
                return available;
                }
            }

        private void InitializeStatusBar() {
            statusBarControls = new Panel();
            statusBarControls.Name = "MWinStatusBarControls";
            statusBarControls.TabStop = false;
            Controls.Add(statusBarControls);
            LayoutChrome();
            }

        private void EnsureStatusBar() {
            if (statusBarStrip != null) {
                return;
                }
            StatusStrip strip = new StatusStrip();
            strip.Name = "MWinStatusBarStrip";
            ToolStripStatusLabel text = new ToolStripStatusLabel();
            text.Name = "MWinStatusText";
            text.Spring = true;
            text.Text = settings.StatusBar.Text ?? "";
            strip.Items.Add(text);
            SetStatusBar(strip, text);
            }

        /// <summary>Transfers ownership of a designer-created status strip to this frame.</summary>
        public void SetStatusBar(StatusStrip strip, ToolStripStatusLabel text) {
            if (strip == null || text == null || text.Owner != strip) {
                throw new ArgumentException("The status text must belong to the supplied status strip.");
                }
            if (statusText != null) {
                statusText.TextChanged -= StatusTextChanged;
                }
            if (statusBarStrip != null && statusBarStrip != strip) {
                // The form owns its status content, including replaced default strips.
                statusBarStrip.Dispose();
                }
            statusBarStrip = strip;
            statusText = text;
            statusText.TextChanged += StatusTextChanged;
            strip.Dock = DockStyle.Fill;
            strip.AutoSize = false;
            strip.SizingGrip = false;
            if (strip.Parent != statusBarControls) {
                statusBarControls.Controls.Add(strip);
                }
            settings.StatusBar.Text = text.Text;
            ApplyStatusBarSettings();
            }

        private void StopStatusBar() {
            if (statusText != null) {
                statusText.TextChanged -= StatusTextChanged;
                }
            }

        private void StatusTextChanged(object sender, EventArgs arguments) {
            settings.StatusBar.Text = statusText.Text;
            }

        private void ApplyStatusBarSettings() {
            if (statusBarControls == null || statusBarControls.IsDisposed) {
                return;
                }
            // Defer default content so inherited panels remain editable in VS.
            // A derived form can create and serialize its own strip normally.
            if (statusBarStrip == null && !string.IsNullOrEmpty(settings.StatusBar.Text)) {
                EnsureStatusBar();
                }
            if (statusBarStrip != null) {
                statusText.Text = settings.StatusBar.Text ?? "";
                statusBarStrip.Font = settings.StatusBar.Font;
                statusBarStrip.Padding = new Padding(settings.StatusBar.HorizontalPadding, 0,
                    settings.StatusBar.HorizontalPadding, 0);
                statusText.TextAlign = settings.StatusBar.Alignment == Alignment.Right ? ContentAlignment.MiddleRight
                    : settings.StatusBar.Alignment == Alignment.Center ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
                }
            LayoutChrome();
            }

        protected override void OnLayout(LayoutEventArgs arguments) {
            LayoutChrome();
            base.OnLayout(arguments);
            }

        private void LayoutChrome() {
            if (statusBarControls == null || layingOutChrome || IsDisposed || Disposing) {
                return;
                }
            layingOutChrome = true;
            try {
                bool chrome = settings.Frame.ShowChrome;
                statusBarControls.Visible = chrome && settings.StatusBar.Visible;
                int border = Math.Max(1, settings.Frame.BorderThickness);
                int leftInset = Math.Max(border, settings.Frame.CornerRadius);
                int rightInset = Math.Max(leftInset, settings.Frame.Resizable ? 22 + border : border);
                statusBarControls.SetBounds(leftInset, ClientSize.Height - border - settings.StatusBar.Height,
                    Math.Max(0, ClientSize.Width - leftInset - rightInset), settings.StatusBar.Height);
                if (rbExit != null) {
                    MoveButtons();
                    }
                }
            finally {
                layingOutChrome = false;
                }
            }
        }
    }
