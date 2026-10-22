using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static MashWin.Win32;

namespace MashWin {
    [DefaultProperty("Frame")]
    public partial class MWindow : Form {

        #region members

        private Image IconImage;
        private Icon ownedWindowIcon;
        private GraphicsPath WindowGP;
        private bool HasBeenResized;
        private Size LastSize;
        private Rectangle SrcRect, DestRect, _WindowRect, _StatusTextRect, _TitleTextRect;

        private MWindowSettings settings = new MWindowSettings();

        private Pen AppPenBorder = Pens.DarkCyan;
        private Pen AppPenSeparator = Pens.DarkCyan;
        public Pen BorderPen {
            get {
                return AppPenBorder;
                }
            }
        private Pen AppPenSizeGrip = Pens.DarkCyan;
        private Brush AppBrushTitle = new SolidBrush(Color.FromArgb(190, Color.DarkCyan));
        private Brush AppBrushBackground;
        private StringFormat StatusTextAlignment = new StringFormat() { LineAlignment = StringAlignment.Center };
        private StringFormat TitleTextAlignment = new StringFormat() { LineAlignment = StringAlignment.Center };

        public bool UserDrawn = false;

        private bool firstDraw = true;
        private Color OldBackColor;

        public object OwnerObj;

        // import for icon
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        #endregion members

        #region constructor
        public MWindow() {
            //Settings = new MWindowSettings();
            AppBrushBackground = new SolidBrush(this.BackColor);
            AppPenBorder = new Pen(this.Settings.Frame.BorderColor);
            AppPenSeparator = new Pen(this.Settings.MenuSeparator.Color);
            AppPenSizeGrip = new Pen(this.Settings.Frame.BorderColor);

            DestRect = new Rectangle(1 + this.Settings.Frame.BorderThickness, 1 + this.Settings.Frame.BorderThickness, 26 - this.Settings.Frame.BorderThickness, 26 - this.Settings.Frame.BorderThickness);

            List<Control> cs = new List<Control>();
            foreach (Control c in this.Controls) {
                if (c is RoundButton) {
                    ((RoundButton)c).ExtendedSettings.BorderThickness = this.Settings.Frame.BorderThickness;
                    ((RoundButton)c).BorderPen = this.BorderPen;
                    if (c.Name != "rbExit") {
                        cs.Add(c);
                        }
                    }
                }
            if (Settings.MenuSeparator.Enabled) {
                foreach (Control c in cs) {
                    this.Controls.Remove(c);
                    }
                }
            cs.Clear();

            LastSize = new Size(0, 0);
            if (this.IconImage != null) {
                SrcRect = new Rectangle(0, 0, this.Icon.Width, this.Icon.Height);
                } else {
                SrcRect = new Rectangle(0, 0, 1, 1);
                //                DestRect = new Rectangle(0, 0, 1, 1);
                }
            InitializeComponent();

            base.Size = this.Size;
            base.ClientSize = this.ClientSize;

            AppPenSeparator.Width = Settings.MenuSeparator.Thickness;

            if (Settings.CaptionButtons.ExitBox) {

                // .NET port: assign caption controls to the fields used during resize/disposal.
                rbExit = new RoundButton();
                rbExit.Height = 20;
                rbExit.Width = 30;
                rbExit.Name = "rbExit";
                rbExit.Text = "X";
                rbExit.UseCompatibleTextRendering = true;
                if (MinimizeBox || MaximizeBox) {
                    rbExit.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.BottomRight;
                    } else {
                    rbExit.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.Bottom;
                    }
                rbExit.ExtendedSettings.MouseOverBackColor = Color.PaleVioletRed;
                rbExit.ExtendedSettings.MouseDownBackColor = Color.Red;
                rbExit.ExtendedSettings.MouseDownBackColorAlt = Color.Black;
                rbExit.ForeColor = Color.LightSlateGray;
                rbExit.ExtendedSettings.VistaStyleGradientBackground = true;
                rbExit.Click += rbExit_Click;
                this.Controls.Add(rbExit);
                }
            if (Settings.CaptionButtons.MaximizeBox) {
                rbMaximize = new RoundButton();
                rbMaximize.Height = 20;
                rbMaximize.Width = 30;
                rbMaximize.Name = "rbMaximize";
                rbMaximize.Text = "□";
                rbMaximize.ForeColor = Color.LightSlateGray;
                rbMaximize.Font = new Font(rbMaximize.Font.FontFamily, 14f);
                rbMaximize.ExtendedSettings.MouseDownBackColor = Color.CornflowerBlue;
                rbMaximize.ExtendedSettings.MouseDownBackColorAlt = Color.Black;
                rbMaximize.ExtendedSettings.VistaStyleGradientBackground = true;
                if (!Settings.CaptionButtons.MinimizeBox) {
                    rbMaximize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.BottomLeft;
                    } else {
                    rbMaximize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.None;
                    }
                rbMaximize.Click += rbMaximize_Click;
                this.Controls.Add(rbMaximize);
                }
            if (Settings.CaptionButtons.MinimizeBox) {
                rbMinimize = new RoundButton();
                rbMinimize.Height = 20;
                rbMinimize.Width = 30;
                rbMinimize.Name = "rbMinimize";
                rbMinimize.ExtendedSettings.VistaStyleGradientBackground = true;
                rbMinimize.ExtendedSettings.MouseDownBackColor = Color.CornflowerBlue;
                rbMinimize.ExtendedSettings.MouseDownBackColorAlt = Color.Black;
                rbMinimize.Text = "__";
                rbMinimize.ForeColor = Color.LightSlateGray;
                //rbMinimize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.BottomLeft;
                if (Settings.CaptionButtons.ExitBox || MaximizeBox) {
                    rbMinimize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.BottomLeft;
                    } else {
                    rbMinimize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.Bottom;
                    }
                rbMinimize.Click += rbMinimize_Click;
                this.Controls.Add(rbMinimize);
                }

            settings.PropertyChanged += SettingsChanged;
            ApplySettings();
            }
        #endregion constructor

        #region wincontrols
        private void MoveButtons() {

            // Keep caption visibility reversible when Settings.CaptionButtons is edited.
            if (rbExit != null) {
                rbExit.Visible = Settings.CaptionButtons.ExitBox;
                rbExit.Top = Settings.Frame.BorderThickness - 1;
                rbExit.Left = Width - (Settings.Frame.CornerRadius + Settings.Frame.BorderThickness) - 30;
                rbExit.ExtendedSettings.RoundCorners = Settings.CaptionButtons.MaximizeBox || Settings.CaptionButtons.MinimizeBox
                    ? ExtendedButtonSettings.RoundedCorners.BottomRight
                    : ExtendedButtonSettings.RoundedCorners.Bottom;
                }
            if (rbMaximize != null) {
                rbMaximize.Visible = Settings.CaptionButtons.MaximizeBox;
                rbMaximize.Top = Settings.Frame.BorderThickness - 1;
                rbMaximize.Left = Width - (Settings.Frame.CornerRadius + Settings.Frame.BorderThickness) - 59;
                }
            if (rbMinimize != null) {
                rbMinimize.Visible = Settings.CaptionButtons.MinimizeBox;
                rbMinimize.Top = Settings.Frame.BorderThickness - 1;
                int leftOffset = Settings.CaptionButtons.MaximizeBox ? 88 : 59;
                rbMinimize.Left = Width - (Settings.Frame.CornerRadius + Settings.Frame.BorderThickness) - leftOffset;
                }

            foreach (Control c in this.Controls) {
                if (c is RoundButton) {
                    c.Invalidate();
                    }
                }
            }

        protected void ForceIcoIcon() {
            // Keep an owned bitmap for painting; never dispose a caller's image.
            if (Settings.Title.Icon != null) {
                SetIcon(Settings.Title.Icon);
                }
            else if (Icon != null) {
                SetIcon(Icon);
                }
            }

        public void SetIcon(Icon icon) {
            if (icon == null) {
                throw new ArgumentNullException(nameof(icon));
                }
            using (Bitmap bitmap = icon.ToBitmap()) {
                SetIcon(bitmap);
                }
            }

        public void SetIcon(Image image) {
            if (image == null) {
                throw new ArgumentNullException(nameof(image));
                }
            Image previousImage = IconImage;
            IconImage = (Image)image.Clone();
            previousImage?.Dispose();
            SrcRect = new Rectangle(0, 0, IconImage.Width, IconImage.Height);
            using (Bitmap bitmap = new Bitmap(IconImage)) {
                IntPtr iconHandle = bitmap.GetHicon();
                try {
                    // Clone before releasing the temporary native HICON.
                    Icon replacement = (Icon)Icon.FromHandle(iconHandle).Clone();
                    Icon = replacement;
                    ownedWindowIcon?.Dispose();
                    ownedWindowIcon = replacement;
                    }
                finally {
                    DestroyIcon(iconHandle);
                    }
                }
            Invalidate();
            }
        #endregion wincontrols

        #region properties
        // Separate expandable entries share a named category in the property grid.
        [Category("MWin Customization")]
        [DisplayName("Frame")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FrameSettings Frame {
            get {
                return settings.Frame;
                }
            }

        [Category("MWin Customization")]
        [DisplayName("Caption Buttons")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public CaptionButtonSettings CaptionButtons {
            get {
                return settings.CaptionButtons;
                }
            }

        [Category("MWin Customization")]
        [DisplayName("Title")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TitleSettings Title {
            get {
                return settings.Title;
                }
            }

        [Category("MWin Customization")]
        [DisplayName("Menu Separator")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public MenuSeparatorSettings MenuSeparator {
            get {
                return settings.MenuSeparator;
                }
            }

        [Category("MWin Customization")]
        [DisplayName("Status Bar")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public StatusBarSettings StatusBar {
            get {
                return settings.StatusBar;
                }
            }

        // The public container shares the same groups as the designer entries.
        // Only those entries serialize, avoiding duplicate assignments.
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MWindowSettings Settings {
            get {
                return settings;
                }
            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                    }
                if (ReferenceEquals(settings, value)) {
                    return;
                    }
                settings.PropertyChanged -= SettingsChanged;
                settings = value;
                settings.PropertyChanged += SettingsChanged;
                UpdateSettingsIcon();
                ApplySettings();
                }
            }

        // Keep the old container name as a hidden alias.
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MWindowSettings ExtendedSettings {
            get {
                return Settings;
                }
            set {
                Settings = value;
                }
            }

        private void SettingsChanged(object sender, PropertyChangedEventArgs arguments) {
            if (arguments.PropertyName == "Title.Icon") {
                UpdateSettingsIcon();
                }
            ApplySettings();
            }

        private void UpdateSettingsIcon() {
            if (settings.Title.Icon != null) {
                SetIcon(settings.Title.Icon);
                }
            else {
                IconImage?.Dispose();
                IconImage = null;
                Icon = null;
                ownedWindowIcon?.Dispose();
                ownedWindowIcon = null;
                }
            }

        private void ApplySettings() {
            if (IsDisposed || Disposing) {
                return;
                }

            // Shape and alignment used to refresh only on resize. Nested property
            // changes must also discard those cached values and update drawing pens.
            HasBeenResized = true;
            AppPenBorder.Color = settings.Frame.BorderColor;
            AppPenBorder.Width = settings.Frame.BorderThickness;
            AppPenSeparator.Color = settings.MenuSeparator.Color;
            AppPenSeparator.Width = settings.MenuSeparator.Thickness;
            AppPenSizeGrip.Color = settings.Frame.BorderColor;
            pUpdateAlignment();
            DestRect = new Rectangle(
                1 + settings.Frame.BorderThickness,
                1 + settings.Frame.BorderThickness,
                Math.Max(1, 26 - settings.Frame.BorderThickness),
                Math.Max(1, 26 - settings.Frame.BorderThickness)
                );
            MoveButtons();
            Invalidate(true);
            }
        #endregion properties

        #region events
        public event CloseClicked bClose;
        public delegate bool CloseClicked(object c, EventArgs e);
        public event MaximizeClicked bMaximize;
        public delegate bool MaximizeClicked(object c, EventArgs e);
        public event MinimizeClicked bMinimize;
        public delegate bool MinimizeClicked(object c, EventArgs e);
        #endregion events

        #region wndproc
        protected override void WndProc(ref Message m) {
            // this is SPECIFICALLY to abort a maximize call by double-clicking the titlebar
            if (m.Msg == WM_LBUTTONDBLCLK && !MaximizeBox) {
                m.Result = (IntPtr)0;
                return;
                }

            if (m.Msg == WM_NCHITTEST) {
                // Keep ordinary client areas interactive while preserving native caption/resize handling.
                m.Result = (IntPtr)1; // HTCLIENT
                Point loc = this.PointToClient(Cursor.Position);

                if (loc.Y <= 26 + Settings.Frame.BorderThickness) {
                    m.Result = (IntPtr)2;
                    }

                bool blnRight = (loc.X > this.Width - (16 + Settings.Frame.BorderThickness));
                bool blnBottom = (loc.Y > this.Height - (16 + Settings.Frame.BorderThickness));
                if (Settings.Frame.Resizable)
                    if (blnRight && blnBottom) {
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                        return;
                        } else if (blnRight) {
                        m.Result = (IntPtr)HTRIGHT;
                        return;
                        } else if (blnBottom) {
                        m.Result = (IntPtr)HTBOTTOM;
                        return;
                        } else if (Settings.Frame.ShowSizeGrip) {
                        if (blnRight && blnBottom) {
                            m.Result = (IntPtr)HTBOTTOMRIGHT;
                            return;
                            }
                        }

                if (loc.X < 30 && loc.Y < 30) {
                    m.Result = (IntPtr)3;
                    return;
                    }

                return;
                }
            if (m.Msg == 0x0312) {
                Keys keys = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                }

            base.WndProc(ref m);
            }

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                if (MaximizeBox) cp.Style |= WS_MAXIMIZEBOX;
                if (MinimizeBox) cp.Style |= WS_MINIMIZEBOX;
                return cp;
                }
            }
        #endregion wndproc

        #region pinvoke
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        [DllImport("gdi32.dll")]
        public static extern bool RoundRect(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidth, int nHeight);

        public static GraphicsPath RoundedRect(GraphicsPath path, Rectangle bounds, int radius) {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            bounds.Offset(1, 1);
            size = new Size(size.Width + 1, size.Height + 1);
            Rectangle arc = new Rectangle(bounds.Location, size);

            if (radius == 0) {
                path.AddRectangle(bounds);
                return path;
                }

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
            }

        #endregion pinvoke

        #region drawing

        private void MWindow_Shown(object sender, EventArgs e) {
            if (this.Settings.MenuSeparator.Enabled && !rbMinimize.IsDisposed) {
                this.Controls.Remove(rbMinimize);
                rbMinimize.Dispose();
                }
            if (this.Settings.MenuSeparator.Enabled && !rbMaximize.IsDisposed) {
                this.Controls.Remove(rbMaximize);
                rbMaximize.Dispose();
                }
            }

        public void DrawWindow(Graphics g, Rectangle r, int d, Pen p, int topWidth = 0) {
            if (firstDraw) {
                AppBrushBackground?.Dispose();
                AppBrushBackground = new SolidBrush(this.BackColor);
                firstDraw = false;
                if (this.Settings.Title.Icon != null) {
                    SetIcon(this.Settings.Title.Icon);
                    }
                pBlankWindow(g);
                }
            int t = this.Settings.Frame.BorderThickness;

            if (WindowGP == null || HasBeenResized) {
                using (GraphicsPath gp = new GraphicsPath()) {
                    r = ClientRectangle;
                    int width = r.Width - (Settings.Frame.BorderThickness/2);
                    int height = r.Height - (Settings.Frame.BorderThickness/2);
                    int radius = d;
                    int gradientOffset = r.Y;
                    int rHalfBorder = (int)Math.Ceiling((float)(Settings.Frame.BorderThickness / 2));
                    int wxr = (int)Math.Ceiling((float)(width - radius * 2));

                    // A square frame is valid too; AddArc rejects zero-sized arcs.
                    if (radius <= 0) {
                        gp.AddRectangle(new Rectangle(rHalfBorder, rHalfBorder,
                            Math.Max(1, width - Settings.Frame.BorderThickness),
                            Math.Max(1, height - Settings.Frame.BorderThickness)));
                        }
                    else {
                        gp.AddArc(rHalfBorder, rHalfBorder, (radius * 2) + rHalfBorder, (radius * 2) + rHalfBorder, 180, 90); // TL
                        gp.AddArc(wxr - (Settings.Frame.BorderThickness+rHalfBorder)-1, rHalfBorder, (radius * 2) + (t + 2), (radius * 2) + (t + 2), 270, 90); // TR
                        gp.AddArc(wxr - (Settings.Frame.BorderThickness+rHalfBorder)-1, (height - (radius * 2)) - (Settings.Frame.BorderThickness + rHalfBorder+1), (radius * 2) + (t + 2), (radius * 2) + (t + 2), 0, 90); // BR
                        gp.AddArc(rHalfBorder, (height - (radius * 2)) - (Settings.Frame.BorderThickness+rHalfBorder+1), (radius * 2) + t + 2, (radius * 2) + t + 2, 90, 90); // BL
                        }
                    gp.CloseFigure();

                    // Release the previous shape before caching a resized outline.
                    WindowGP?.Dispose();
                    WindowGP = (GraphicsPath)gp.Clone();
                    gp.Dispose();
                    }

                pUpdateAlignment();
                _WindowRect = new Rectangle(0, 0, Width, Height);
                _StatusTextRect = new Rectangle(0, (Height - (24 + Settings.Frame.BorderThickness)), Width, 30);
                _TitleTextRect = new Rectangle(0, 0, Width, 30);

                }

            HasBeenResized = false;
            pBlankWindow(g);
            g.FillPath(AppBrushBackground, WindowGP);
            g.DrawPath(p, WindowGP);

            if (IconImage != null && ShowIcon) g.DrawImage(
                IconImage,
                DestRect.X + (Settings.Frame.BorderThickness / 2),
                DestRect.Y + (Settings.Frame.BorderThickness / 2),
                DestRect.Width + (Settings.Frame.BorderThickness / 2),
                DestRect.Height + (Settings.Frame.BorderThickness / 2)
                );

            if (Settings.Frame.ShowSizeGrip) {
                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.Frame.BorderThickness + 2), this.Height - 12 - t, this.Width - 12 - t, this.Height - (this.Settings.Frame.BorderThickness + 2));
                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.Frame.BorderThickness + 2), this.Height - 16 - t, this.Width - 16 - t, this.Height - (this.Settings.Frame.BorderThickness + 2));
                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.Frame.BorderThickness + 2), this.Height - 20 - t, this.Width - 20 - t, this.Height - (this.Settings.Frame.BorderThickness + 2));
                }

            if (Settings.MenuSeparator.Enabled) g.DrawLine(AppPenSeparator, 1, 52 + t, this.Width - 1, 52 + t);
            if (Settings.StatusBar.ShowSeparator) g.DrawLine(AppPenSeparator, 1, this.Height - 24 - t, this.Width - 1, this.Height - 24 - t);

            // title separator
            g.DrawLine(AppPenBorder, 1, 26 + t + Settings.MenuSeparator.Thickness, this.Width - 1, 26 + t + Settings.MenuSeparator.Thickness);


            }

        //TODO moving things around and optimizing a bit here so we aren't creating a boatload of objects on each paint event, wasting cycles
        protected override void OnPaint(PaintEventArgs e) {
            if (HasBeenResized) MoveButtons();
            if (this.BackColor != OldBackColor) {
                // A color change replaces an owned GDI brush.
                AppBrushBackground?.Dispose();
                AppBrushBackground = new SolidBrush(this.BackColor);
                OldBackColor = this.BackColor;
                }
            if (AppPenBorder.Color != this.Settings.Frame.BorderColor) AppPenBorder.Color = this.Settings.Frame.BorderColor;
            if (AppPenBorder.Width != this.Settings.Frame.BorderThickness) AppPenBorder.Width = this.Settings.Frame.BorderThickness;

            Graphics g = e.Graphics; // I HAD a using(graphics g = e.graphics){} block here BUT C# doesn't like it.. no idea why. Maybe not disposable?
            int vOffset = (24 - (int)g.MeasureString(this.Text, Settings.Title.Font).Height) / 2;

            DrawWindow(g, this.ClientRectangle, this.Settings.Frame.CornerRadius, AppPenBorder, (int)g.MeasureString(" " + this.Text + " ", Settings.Title.Font).Width);

            // Respect the public title visibility setting when painting custom chrome.
            if (Settings.Title.ShowTitle) {
                g.DrawString(this.Text, Settings.Title.Font, AppBrushTitle, _TitleTextRect, TitleTextAlignment);
                }
            g.DrawString(Settings.StatusBar.Text, Settings.StatusBar.Font, AppBrushTitle, _StatusTextRect, StatusTextAlignment);
            }

        protected internal void pUpdateAlignment() {
            if (Settings.StatusBar.Alignment == Alignment.Left) StatusTextAlignment.Alignment = StringAlignment.Near;
            else if (Settings.StatusBar.Alignment == Alignment.Right) StatusTextAlignment.Alignment = StringAlignment.Far;
            else if (Settings.StatusBar.Alignment == Alignment.Center) StatusTextAlignment.Alignment = StringAlignment.Center;

            if (Settings.Title.Alignment == Alignment.Left) TitleTextAlignment.Alignment = StringAlignment.Near;
            else if (Settings.Title.Alignment == Alignment.Right) TitleTextAlignment.Alignment = StringAlignment.Far;
            else if (Settings.Title.Alignment == Alignment.Center) TitleTextAlignment.Alignment = StringAlignment.Center;
            }

        protected internal void pBlankWindow(Graphics g) {
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.FillRectangle(Brushes.Fuchsia, _WindowRect);
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            }

        #endregion drawing

        #region buttons
        private void rbExit_Click(object sender, EventArgs e) {
            if (bClose != null) {
                bClose(sender, e);
                }
            this.Close();
            }

        private void rbMinimize_Click(object sender, EventArgs e) {
            if (bMinimize != null) {
                if (bMinimize(sender, e) == false)
                    return;
                }
            this.WindowState = FormWindowState.Minimized;
            }

        private void rbMaximize_Click(object sender, EventArgs e) {
            if (!this.Settings.CaptionButtons.MaximizeBox) return;
            if (bMaximize != null) {
                if (bMaximize(sender, e) == false)
                    return;
                }

            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
            }
        #endregion buttons

        private void MWin_Resize(object sender, EventArgs e) {
            if (LastSize != this.Size) {
                HasBeenResized = true;
                LastSize = this.Size;

                }
            }


        }
    }
