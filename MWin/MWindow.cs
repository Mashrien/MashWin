using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Rectangle SrcRect, DestRect;

        //private MWindowSettings Settings;

        private Pen AppPenBorder = Pens.DarkCyan;
        public Pen BorderPen {
            get {
                return AppPenBorder;
                }
            }
        private Pen AppPenSizeGrip = Pens.DarkCyan;
        private Brush AppBrushTitle = new SolidBrush(Color.FromArgb(190, Color.DarkCyan));
        private Brush AppBrushBackground;

        public bool UserDrawn = false;

        private bool firstDraw = true;
        private Color OldBackColor;

        public object OwnerObj;

        // import for icon
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        #endregion members

        public MWindow() {
            //Settings = new MWindowSettings();
            AppBrushBackground = new SolidBrush(this.BackColor);
            AppPenBorder = new Pen(this.Settings.BorderColor);
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
            if (Settings.IsDialog) {
                foreach (Control c in cs) {
                    this.Controls.Remove(c);
                    }
                }
            cs.Clear();

            LastSize = new Size(0, 0);
            if (this.IconImage != null) {
                SrcRect = new Rectangle(0, 0, this.Icon.Width, this.Icon.Height);
                }
            else {
                SrcRect = new Rectangle(0, 0, 1, 1);
                //                DestRect = new Rectangle(0, 0, 1, 1);
                }
            InitializeComponent();

            base.Size = this.Size;
            base.ClientSize = this.ClientSize;

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
                rbMinimize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.BottomLeft;
                rbMinimize.Click += rbMinimize_Click;
                this.Controls.Add(rbMinimize);
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
                rbMaximize.ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.None;
                rbMaximize.Click += rbMaximize_Click;
                this.Controls.Add(rbMaximize);
                }

            }

        private void MoveButtons() {
            //rbExit.Location = new Point(this.Width - (this.Settings.CornerRadius + this.Settings.BorderThickness) - 8, 1);
            if (Settings.ExitBox) {
                if (!Settings.MaximizeBox && !Settings.MinimizeBox) {
                    ((RoundButton)this.Controls["rbExit"]).ExtendedSettings.RoundCorners = ExtendedButtonSettings.RoundedCorners.Bottom;
                    }
                this.Controls["rbExit"].Top = 1;
                //((RoundButton)this.Controls["rbExit"]).BorderPen.Width = this.Settings.BorderThickness;
                this.Controls["rbExit"].Left = this.Width - (this.Settings.CornerRadius + this.Settings.BorderThickness) - 30;
                } else {
                this.Controls["rbExit"].Hide();
                }

            if (Settings.MaximizeBox) {
                this.Controls["rbMaximize"].Top = 1;
                //((RoundButton)this.Controls["rbMaximize"]).BorderPen.Width = this.Settings.BorderThickness;
                this.Controls["rbMaximize"].Left = this.Width - (this.Settings.CornerRadius + this.Settings.BorderThickness) - 59;
                } else {
                this.Controls["rbMaximize"].Hide();
                }

            if (Settings.MinimizeBox) {
                this.Controls["rbMinimize"].Top = 1;
                //((RoundButton)this.Controls["rbMinimize"]).BorderPen.Width = this.Settings.BorderThickness;
                this.Controls["rbMinimize"].Left = this.Width - (this.Settings.CornerRadius + this.Settings.BorderThickness) - 88;
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
                }
            else {
                IconImage = this.Icon.ToBitmap();
                }
            }

        public void SetIcon(Icon i) {
            IconImage = (Image)i.ToBitmap().Clone();
            SrcRect = new Rectangle(0, 0, this.Icon.Width, this.Icon.Height);
            ForceIcoIcon();
            //            DestRect = new Rectangle(3 + this.Settings.BorderThickness, 3 + this.Settings.BorderThickness, 24 - this.Settings.BorderThickness, 24 - this.Settings.BorderThickness);
            }
        public void SetIcon(Image i) {
            IconImage = (Image)i.Clone();
            SrcRect = new Rectangle(0, 0, IconImage.Width, IconImage.Height);
            ForceIcoIcon();
            //            DestRect = new Rectangle(3 + this.Settings.BorderThickness, 3 + this.Settings.BorderThickness, 24 - this.Settings.BorderThickness, 24 - this.Settings.BorderThickness);
            }

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

        #region overrides
        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x0084) {
                Point loc = this.PointToClient(Cursor.Position);

                if (loc.Y < 24)
                    m.Result = (IntPtr)2;

                bool blnRight = (loc.X > this.Width - (16));
                bool blnBottom = (loc.Y > this.Height - (16));
                if (!Settings.IsDialog || Settings.ShowSizeGrip)
                    if (blnRight && blnBottom) {
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                        return;
                        }
                    else if (blnRight) {
                        m.Result = (IntPtr)HTRIGHT;
                        return;
                        }
                    else if (blnBottom) {
                        m.Result = (IntPtr)HTBOTTOM;
                        return;
                        }

                if (loc.X < 30 && loc.Y < 30) {
                    m.Result = (IntPtr)0x3;
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
                cp.Style |= WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
                return cp;
                }
            }
        #endregion overrides

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
            if (this.Settings.IsDialog && !rbMinimize.IsDisposed) {
                this.Controls.Remove(rbMinimize);
                rbMinimize.Dispose();
                }
            if (this.Settings.IsDialog && !rbMaximize.IsDisposed) {
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
                }
            int t = this.Settings.BorderThickness;

            if (WindowGP == null || HasBeenResized) {
                //if (HasBeenResized) {
                //    WindowGP = null;
                //    GC.Collect(2);
                //    }
                using (GraphicsPath gp = new GraphicsPath()) {
                    r = ClientRectangle;
                    int width = r.Width - (this.Settings.BorderThickness * 2);
                    int height = r.Height - (this.Settings.BorderThickness * 2);
                    int radius = d;
                    int gradientOffset = r.Y;

                    gp.AddArc(1, 1, (radius * 2) + 1, (radius * 2) + 1, 180, 90); // TL
                    gp.AddArc((width - radius * 2) - 1, 1, (radius * 2) + (t + 2), (radius * 2) + (t + 2), 270, 90); // TR
                    gp.AddArc((width - radius * 2) - 1, (height - (radius * 2)) - 1, (radius * 2) + (t + 2), (radius * 2) + (t + 2), 0, 91); // BR
                    gp.AddArc(1, (height - (radius * 2)) - 1, (radius * 2) + t + 2, (radius * 2) + t + 2, 90, 90); // BL
                    gp.CloseFigure();

                    WindowGP = (GraphicsPath)gp.Clone();
                    gp.Dispose();
                    }
                }

            g.FillPath(AppBrushBackground, WindowGP);
            g.DrawPath(p, WindowGP);

            if (!Settings.IsDialog || Settings.ShowSizeGrip) {
                //TL
                //g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.CornerRadius + 1), this.Height - 6 - (this.Settings.CornerRadius + this.Settings.BorderThickness), this.Width - 6 - t, this.Height - (this.Settings.CornerRadius + 1));
                //MID
                //g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.CornerRadius + 1), this.Height - 10 - (this.Settings.CornerRadius + this.Settings.BorderThickness), this.Width - 10 - t, this.Height - (this.Settings.CornerRadius + 1));
                //BR                       int X - ORIGIN                                 int Y - ORIGIN                                 int X - END          int Y - END
                //g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.CornerRadius + 1), this.Height - 14 - (this.Settings.CornerRadius + this.Settings.BorderThickness), this.Width - 14 - t, this.Height - (this.Settings.CornerRadius + 1));

                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.BorderThickness + 2), this.Height - 12 - t, this.Width - 12 - t, this.Height - (this.Settings.BorderThickness + 2));
                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.BorderThickness + 2), this.Height - 16 - t, this.Width - 16 - t, this.Height - (this.Settings.BorderThickness + 2));
                g.DrawLine(AppPenSizeGrip, this.Width - (this.Settings.BorderThickness + 2), this.Height - 20 - t, this.Width - 20 - t, this.Height - (this.Settings.BorderThickness + 2));

                if (!Settings.IsDialog) {
                    g.DrawLine(AppPenBorder, 1, this.Height - 24 - t, this.Width - 1, this.Height - 24 - t);
                    g.DrawLine(AppPenBorder, 1, 48 + t, this.Width - 1, 48 + t);
                    }
                }

            g.DrawLine(AppPenBorder, 1, 26 + t, this.Width - 1, 26 + t);



            }

        protected override void OnPaint(PaintEventArgs e) {
            if (HasBeenResized) {
                MoveButtons();
                }
            if (this.BackColor != OldBackColor) {
                AppBrushBackground = new SolidBrush(this.BackColor);
                OldBackColor = this.BackColor;
                }
            if (AppPenBorder.Color != this.Settings.BorderColor) {
                AppPenBorder.Color = this.Settings.BorderColor;
                }
            if (AppPenBorder.Width != this.Settings.BorderThickness) {
                AppPenBorder.Width = this.Settings.BorderThickness;
                }
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.FillRectangle(Brushes.Fuchsia, new Rectangle(0, 0, Width, Height));
            e.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            DrawWindow(e.Graphics, this.ClientRectangle, this.Settings.CornerRadius, AppPenBorder, (int)e.Graphics.MeasureString(" " + this.Text + " ", Settings.TitleFont).Width);
            int vOffset = (24 - (int)e.Graphics.MeasureString(this.Text, Settings.TitleFont).Height) / 2;
            if (this.IconImage != null && this.ShowIcon) {
                e.Graphics.DrawString(this.Text, Settings.TitleFont, AppBrushTitle, 28 + this.Settings.BorderThickness, vOffset + this.Settings.BorderThickness);
                e.Graphics.DrawImage(IconImage, DestRect.X, DestRect.Y, DestRect.Width, DestRect.Height);
                }
            else {
                e.Graphics.DrawString(this.Text, Settings.TitleFont, AppBrushTitle, 10 + this.Settings.BorderThickness, vOffset + this.Settings.BorderThickness);
                }
            e.Graphics.DrawString(Settings.StatusBarText, Settings.TitleFont, AppBrushTitle, 5 + this.Settings.BorderThickness, (Height - 24) + vOffset - this.Settings.BorderThickness);
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
