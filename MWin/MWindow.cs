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
    [DefaultProperty("ExtendedSettings")]
    public partial class MWindow : Form {

        #region members

        private Image IconImage;
        private GraphicsPath WindowGP;
        private bool HasBeenResized;
        private Size LastSize;
        private Rectangle SrcRect, DestRect, _WindowRect, _StatusTextRect, _TitleTextRect;

        //private MWindowSettings Settings;

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
            AppPenBorder = new Pen(this.Settings.BorderColor);
            AppPenSeparator = new Pen(this.Settings.SeparatorColor);
            AppPenSizeGrip = new Pen(this.Settings.BorderColor);

            DestRect = new Rectangle(1 + this.Settings.BorderThickness, 1 + this.Settings.BorderThickness, 26 - this.Settings.BorderThickness, 26 - this.Settings.BorderThickness);

            List<Control> cs = new List<Control>();
            foreach (Control c in this.Controls) {
                if (c is RoundButton) {
                    ((RoundButton)c).ExtendedSettings.BorderThickness = this.Settings.BorderThickness;
                    ((RoundButton)c).BorderPen = this.BorderPen;
                    if (c.Name != "rbExit") {
                        cs.Add(c);
                        }
                    }
                }
            if (Settings.MenuSeparator) {
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

            AppPenSeparator.Width = Settings.SeparatorThickness;

            if (Settings.ExitBox) {

                RoundButton rbExit = new RoundButton();
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
            if (Settings.MaximizeBox) {
                RoundButton rbMaximize = new RoundButton();
                rbMaximize.Height = 20;
                rbMaximize.Width = 30;
                rbMaximize.Name = "rbMaximize";
                rbMaximize.Text = "□";
                rbMaximize.ForeColor = Color.LightSlateGray;
                rbMaximize.Font = new Font(rbMaximize.Font.FontFamily, 14f);
                rbMaximize.ExtendedSettings.MouseDownBackColor = Color.CornflowerBlue;
                rbMaximize.ExtendedSettings.MouseDownBackColorAlt = Color.Black;
                rbMaximize.ExtendedSettings.VistaStyleGradientBackground = true;
                if (!Settings.MinimizeBox) {
                    rbMaximize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.BottomLeft;
                    } else {
                    rbMaximize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.None;
                    }
                rbMaximize.Click += rbMaximize_Click;
                this.Controls.Add(rbMaximize);
                }
            if (Settings.MinimizeBox) {
                RoundButton rbMinimize = new RoundButton();
                rbMinimize.Height = 20;
                rbMinimize.Width = 30;
                rbMinimize.Name = "rbMinimize";
                rbMinimize.ExtendedSettings.VistaStyleGradientBackground = true;
                rbMinimize.ExtendedSettings.MouseDownBackColor = Color.CornflowerBlue;
                rbMinimize.ExtendedSettings.MouseDownBackColorAlt = Color.Black;
                rbMinimize.Text = "__";
                rbMinimize.ForeColor = Color.LightSlateGray;
                //rbMinimize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.BottomLeft;
                if (Settings.ExitBox || MaximizeBox) {
                    rbMinimize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.BottomLeft;
                    } else {
                    rbMinimize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.Bottom;
                    }
                rbMinimize.Click += rbMinimize_Click;
                this.Controls.Add(rbMinimize);
                }

            }
        #endregion constructor

        #region wincontrols
        private void MoveButtons() {

            if (Settings.ExitBox) {
                if (!Settings.MaximizeBox && !Settings.MinimizeBox) {
                    ((RoundButton)this.Controls["rbExit"]).ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.Bottom;
                    }
                this.Controls["rbExit"].Top = Settings.BorderThickness-1;// (Settings.BorderThickness <= 1 ? 0 : 1);
                this.Controls["rbExit"].Left = this.Width - (this.Settings.CornerRadius + this.Settings.BorderThickness) - 30;
                } else {
                this.Controls["rbExit"].Hide();
                }

            if (Settings.MaximizeBox) {
                this.Controls["rbMaximize"].Top = Settings.BorderThickness-1;// (Settings.BorderThickness <= 1 ? 0 : 1);
                this.Controls["rbMaximize"].Left = this.Width - (this.Settings.CornerRadius + this.Settings.BorderThickness) - 59;
                } else {
                this.Controls["rbMaximize"].Hide();
                }

            if (Settings.MinimizeBox) {
                this.Controls["rbMinimize"].Top = Settings.BorderThickness-1;// (Settings.BorderThickness <= 1 ? 0 : 1);
                int leftOffset = Settings.MaximizeBox ? 88 : 59; // move the offset right if there's no maximize button
                this.Controls["rbMinimize"].Left = this.Width - (this.Settings.CornerRadius + this.Settings.BorderThickness) - leftOffset;
                } else {
                this.Controls["rbMinimize"].Hide();
                }

            foreach (Control c in this.Controls) {
                if (c is RoundButton) {
                    c.Invalidate();
                    }
                }
            }

        protected void ForceIcoIcon() {
            //BUG this needs to be fixed.. though it semi-works for now
            if (this.Settings.Icon != null) {
                IconImage = this.Settings.Icon;
                Bitmap Cbitmap;
                using (Cbitmap = new Bitmap(IconImage)) {
                    //Cbitmap.MakeTransparent(Color.Transparent);
                    System.IntPtr icH = Cbitmap.GetHicon();
                    this.Icon = (Icon)Icon.FromHandle(icH).Clone();
                    DestroyIcon(icH);
                    }
                } else {
                IconImage = this.Icon.ToBitmap();
                }
            }

        public void SetIcon(Icon i) {
            IconImage = (Image)i.ToBitmap().Clone();
            SrcRect = new Rectangle(0, 0, this.Icon.Width, this.Icon.Height);
            ForceIcoIcon();
            }

        public void SetIcon(Image i) {
            IconImage = (Image)i.Clone();
            SrcRect = new Rectangle(0, 0, IconImage.Width, IconImage.Height);
            ForceIcoIcon();
            }
        #endregion wincontrols

        #region properties
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Extended")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public MWindowSettings ExtendedSettings {
            get {
                return Settings;
                }
            set {
                Settings = value;
                }
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
                Point loc = this.PointToClient(Cursor.Position);

                if (loc.Y <= 26 + Settings.BorderThickness) {
                    m.Result = (IntPtr)2;
                    }

                bool blnRight = (loc.X > this.Width - (16 + Settings.BorderThickness));
                bool blnBottom = (loc.Y > this.Height - (16 + Settings.BorderThickness));
                if (Settings.Resizable)
                    if (blnRight && blnBottom) {
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                        return;
                        } else if (blnRight) {
                        m.Result = (IntPtr)HTRIGHT;
                        return;
                        } else if (blnBottom) {
                        m.Result = (IntPtr)HTBOTTOM;
                        return;
                        } else if (Settings.ShowSizeGrip) {
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
            if (this.Settings.MenuSeparator && !rbMinimize.IsDisposed) {
                this.Controls.Remove(rbMinimize);
                rbMinimize.Dispose();
                }
            if (this.Settings.MenuSeparator && !rbMaximize.IsDisposed) {
                this.Controls.Remove(rbMaximize);
                rbMaximize.Dispose();
                }
            }

        public void DrawWindow(Graphics g, Rectangle r, int d, Pen p, int topWidth = 0) {
            if (firstDraw) {
                AppBrushBackground = new SolidBrush(this.BackColor);
                firstDraw = false;
                if (this.Settings.Icon != null) {
                    IconImage = this.Settings.Icon;
                    }
                pBlankWindow(g);
                }
            int t = this.Settings.BorderThickness;

            if (WindowGP == null || HasBeenResized) {
                using (GraphicsPath gp = new GraphicsPath()) {
                    r = ClientRectangle;
                    int width = r.Width - (Settings.BorderThickness/2);
                    int height = r.Height - (Settings.BorderThickness/2);
                    int radius = d;
                    int gradientOffset = r.Y;
                    int rHalfBorder = (int)Math.Ceiling((float)(Settings.BorderThickness / 2));
                    int wxr = (int)Math.Ceiling((float)(width - radius * 2));

                    gp.AddArc(rHalfBorder, rHalfBorder, (radius * 2) + rHalfBorder, (radius * 2) + rHalfBorder, 180, 90); // TL
                    gp.AddArc(wxr - (Settings.BorderThickness+rHalfBorder)-1, rHalfBorder, (radius * 2) + (t + 2), (radius * 2) + (t + 2), 270, 90); // TR
                    gp.AddArc(wxr - (Settings.BorderThickness+rHalfBorder)-1, (height - (radius * 2)) - (Settings.BorderThickness + rHalfBorder+1), (radius * 2) + (t + 2), (radius * 2) + (t + 2), 0, 90); // BR
                    gp.AddArc(rHalfBorder, (height - (radius * 2)) - (Settings.BorderThickness+rHalfBorder+1), (radius * 2) + t + 2, (radius * 2) + t + 2, 90, 90); // BL
                    gp.CloseFigure();

                    WindowGP = (GraphicsPath)gp.Clone();
                    gp.Dispose();
                    }

                pUpdateAlignment();
                _WindowRect = new Rectangle(0, 0, Width, Height);
                _StatusTextRect = new Rectangle(0, (Height - (24 + Settings.BorderThickness)), Width, 30);
                _TitleTextRect = new Rectangle(0, 0, Width, 30);

                }

            HasBeenResized = false;
            pBlankWindow(g);
            g.FillPath(AppBrushBackground, WindowGP);
            g.DrawPath(p, WindowGP);

            if (IconImage != null && ShowIcon) g.DrawImage(
                IconImage, 
                DestRect.X + (Settings.BorderThickness / 2), 
                DestRect.Y + (Settings.BorderThickness / 2), 
                DestRect.Width + (Settings.BorderThickness / 2), 
                DestRect.Height + (Settings.BorderThickness / 2)
                );

            if (Settings.ShowSizeGrip) {
                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.BorderThickness + 2), this.Height - 12 - t, this.Width - 12 - t, this.Height - (this.Settings.BorderThickness + 2));
                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.BorderThickness + 2), this.Height - 16 - t, this.Width - 16 - t, this.Height - (this.Settings.BorderThickness + 2));
                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.BorderThickness + 2), this.Height - 20 - t, this.Width - 20 - t, this.Height - (this.Settings.BorderThickness + 2));
                }

            if (Settings.MenuSeparator) g.DrawLine(AppPenSeparator, 1, 52 + t, this.Width - 1, 52 + t);
            if (Settings.StatusBarSeparator) g.DrawLine(AppPenSeparator, 1, this.Height - 24 - t, this.Width - 1, this.Height - 24 - t);

            // title separator
            g.DrawLine(AppPenBorder, 1, 26 + t + Settings.SeparatorThickness, this.Width - 1, 26 + t + Settings.SeparatorThickness);


            }

        //TODO moving things around and optimizing a bit here so we aren't creating a boatload of objects on each paint event, wasting cycles
        protected override void OnPaint(PaintEventArgs e) {
            if (HasBeenResized) MoveButtons();
            if (this.BackColor != OldBackColor) {
                AppBrushBackground = new SolidBrush(this.BackColor);
                OldBackColor = this.BackColor;
                }
            if (AppPenBorder.Color != this.Settings.BorderColor) AppPenBorder.Color = this.Settings.BorderColor;
            if (AppPenBorder.Width != this.Settings.BorderThickness) AppPenBorder.Width = this.Settings.BorderThickness;

            Graphics g = e.Graphics; // I HAD a using(graphics g = e.graphics){} block here BUT C# doesn't like it.. no idea why. Maybe not disposable?
            int vOffset = (24 - (int)g.MeasureString(this.Text, Settings.TitleFont).Height) / 2;

            DrawWindow(g, this.ClientRectangle, this.Settings.CornerRadius, AppPenBorder, (int)g.MeasureString(" " + this.Text + " ", Settings.TitleFont).Width);

            g.DrawString(this.Text, Settings.TitleFont, AppBrushTitle, _TitleTextRect, TitleTextAlignment);
            g.DrawString(Settings.StatusBarText, Settings.StatusBarFont, AppBrushTitle, _StatusTextRect, StatusTextAlignment);
            }

        protected internal void pUpdateAlignment() {
            if (Settings.StatusBarAlignment == Alignment.Left) StatusTextAlignment.Alignment = StringAlignment.Near;
            else if (Settings.StatusBarAlignment == Alignment.Right) StatusTextAlignment.Alignment = StringAlignment.Far;
            else if (Settings.StatusBarAlignment == Alignment.Center) StatusTextAlignment.Alignment = StringAlignment.Center;

            if (Settings.TitleAlignment == Alignment.Left) TitleTextAlignment.Alignment = StringAlignment.Near;
            else if (Settings.TitleAlignment == Alignment.Right) TitleTextAlignment.Alignment = StringAlignment.Far;
            else if (Settings.TitleAlignment == Alignment.Center) TitleTextAlignment.Alignment = StringAlignment.Center;
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
            if (!this.ExtendedSettings.MaximizeBox) return;
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
