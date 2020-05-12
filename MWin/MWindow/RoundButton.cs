using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Globalization;
using static MashWin.EDrawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.IO;

namespace MashWin {

    public class ExtendedSettingsConverter : ExpandableObjectConverter {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) {
                return "";
                }

            return base.ConvertTo(
                context,
                culture,
                value,
                destinationType);
            }
        }

    [TypeConverter(typeof(ExtendedSettingsConverter))]
    public class ExtendedButtonSettings {

        public enum RoundedCorners : int {
            None = 0,
            TopLeft = 1 << 0,
            BottomLeft = 1 << 1,
            TopRight = 1 << 2,
            BottomRight = 1 << 4,
            Top = 5,
            Bottom = 18,
            Left = 3,
            Right = 20,
            All = 23
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(bool))]
        public bool VistaStyleGradientBackground {
            get; set;
            }
        = false;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(bool))]
        public bool InvertForeColorOnMouseDown {
            get; set;
            }
        = false;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color BackgroundColor {
            get; set;
            }
        = Color.WhiteSmoke;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color BackgroundColorAlt {
            get; set;
            }
        = Color.WhiteSmoke;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color MouseOverBackColor {
            get; set;
            }
        = Color.LightBlue;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color MouseOverBackColorAlt {
            get; set;
            }
        = Color.LightBlue;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color MouseDownBackColor {
            get; set;
            }
        = Color.Blue;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color MouseDownBackColorAlt {
            get; set;
            }
        = Color.Blue;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color BorderColor {
            get; set;
            }
        = Color.Gray;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(int))]
        public int BorderThickness {
            get; set;
            }
        = 1;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(int))]
        public int CornerRadius {
            get; set;
            }
        = 5;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(ExtendedButtonSettings.RoundedCorners))]
        public ExtendedButtonSettings.RoundedCorners RoundCorners {
            get; set;
            }
        = ExtendedButtonSettings.RoundedCorners.All;

        }

    //----------------------------------------------------------------------------------------------------------
    [DefaultPropertyAttribute("ExtendedSettings")]
    public class RoundButton : Button {
        ExtendedButtonSettings Settings = new ExtendedButtonSettings();

        //private Region ControlRegion;
        private bool ForceMouseLeave;
        private Brush TextInvertedColor;
        private Brush TextColor;

        //private Color BackColor = Color.Black;

        public Pen BorderPen = new Pen(Color.Goldenrod, 1);

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Extended")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ExtendedButtonSettings ExtendedSettings {
            get {
                return Settings;
                }
            set {
                Settings = value;
                }
            }

        [Browsable(false)]
        public bool isMouseOver = false;
        [Browsable(false)]
        public bool isMouseDown = false;
        [Browsable(false)]
        public bool isMoveOffDown = false;

        public RoundButton() {
            this.MouseLeave += RoundButton_MouseLeave;
            this.MouseEnter += RoundButton_MouseEnter;
            this.MouseMove += RoundButton_MouseMove;
            this.TextAlign = ContentAlignment.BottomRight;
            this.Paint += RoundButton_Paint;
            }

        private void RoundButton_Paint(object sender, PaintEventArgs e) {
            OnPaint(e);
            }

        private void RoundButton_MouseMove(object sender, MouseEventArgs e) {
            this.Invalidate();
            }

        private void RoundButton_MouseEnter(object sender, EventArgs e) {
            this.Invalidate();
            }

        private void RoundButton_MouseLeave(object sender, EventArgs e) {
            ForceMouseLeave = true;
            this.Invalidate();
            }

        protected override void OnPaint(PaintEventArgs e) {
            this.Settings.BorderColor = ((MWindow)this.Parent).ExtendedSettings.BorderColor;
            this.Settings.BorderThickness = ((MWindow)this.Parent).ExtendedSettings.BorderThickness;
            BorderPen.Color = this.Settings.BorderColor;

            if (BackColor != ((MWindow)Parent).BackColor) {
                BackColor = ((MWindow)Parent).BackColor;
                }
            Point MousePosition = PointToClient(Control.MousePosition);

            if (TextInvertedColor == null) {
                TextInvertedColor = new SolidBrush(InvertColor(this.Settings.MouseDownBackColor));
                TextColor = new SolidBrush(this.ForeColor);
                }

            if (ClientRectangle.Contains(MousePosition)) {
                isMouseOver = true;

                if (isMoveOffDown && !isMouseDown)
                    isMouseDown = true;
                else
                    isMouseDown = false;

                if ((Control.MouseButtons & MouseButtons.Left) != 0) {
                    isMouseDown = true;
                    }

                }
            else {
                isMouseOver = false;
                isMouseDown = false;
                }

            if (ForceMouseLeave) {
                isMouseOver = false;
                ForceMouseLeave = false;

                }

            Rectangle r = this.ClientRectangle;

            e.Graphics.Clear(Color.FromArgb(255, BackColor));

            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            //e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            SizeF stringSize = e.Graphics.MeasureString(this.Text, this.Font);
            PointF stringPosition = new PointF();
            if (stringSize.Width > this.Width) {
                stringPosition.X = 0;
                }
            else {
                stringPosition.X = (this.Width - stringSize.Width) / 2;
                }
            stringPosition.Y = (this.Height - stringSize.Height) / 2;

            if (isMouseOver) {
                if (Settings.VistaStyleGradientBackground) {
                    using (GraphicsPath gp = new GraphicsPath()) {
                        gp.AddEllipse(this.ClientRectangle.X - this.Settings.CornerRadius, this.ClientRectangle.Y - this.Settings.CornerRadius, this.ClientRectangle.Width + (this.Settings.CornerRadius * 2), this.ClientRectangle.Height + (this.Settings.CornerRadius * 2));
                        using (PathGradientBrush pgb = new PathGradientBrush(gp)) {
                            pgb.CenterPoint = new Point(this.Width / 2, this.Height);//PointToClient(Cursor.Position);
                            pgb.CenterColor = Settings.MouseOverBackColor;
                            pgb.SurroundColors = new Color[] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0) };
                            e.Graphics.DrawRoundedRectangle(Color.Black, r, Settings.CornerRadius, this.Settings.RoundCorners, BorderPen, pgb);
                            pgb.Dispose();
                            }
                        gp.Dispose();
                        }
                    }
                else {
                    e.Graphics.DrawRoundedRectangle(Settings.MouseOverBackColor, r, Settings.CornerRadius, this.Settings.RoundCorners, BorderPen);
                    }
                }

            if (isMouseDown) {
                if (Settings.VistaStyleGradientBackground) {
                    using (GraphicsPath gp = new GraphicsPath()) {
                        gp.AddEllipse(this.ClientRectangle.X - 10, this.ClientRectangle.Y - 10, this.ClientRectangle.Width + 20, this.ClientRectangle.Height + 20);
                        using (PathGradientBrush pgb = new PathGradientBrush(gp)) {
                            pgb.CenterPoint = new Point(this.Width / 2, this.Height);
                            pgb.CenterColor = Settings.MouseDownBackColor;
                            pgb.SurroundColors = new Color[] { Settings.MouseDownBackColorAlt };
                            e.Graphics.DrawRoundedRectangle(Color.Black, r, Settings.CornerRadius, this.Settings.RoundCorners, BorderPen, pgb);
                            pgb.Dispose();
                            }
                        gp.Dispose();
                        }
                    }
                else {
                    e.Graphics.DrawRoundedRectangle(Settings.MouseDownBackColor, r, Settings.CornerRadius, this.Settings.RoundCorners, BorderPen);
                    }
                stringPosition.X += 1;
                stringPosition.Y += 1;
                }

            e.Graphics.DrawRoundedRectangle(Settings.BorderColor, r, Settings.CornerRadius, this.Settings.RoundCorners, BorderPen, null, true);

            if (this.UseCompatibleTextRendering)
                stringPosition.Y += 2;

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit; //System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            if (isMouseDown && Settings.InvertForeColorOnMouseDown) {
                e.Graphics.DrawString(this.Text, this.Font, TextInvertedColor, stringPosition);
                }
            else {
                e.Graphics.DrawString(this.Text, this.Font, TextColor, stringPosition);
                }

            e.Graphics.Flush();
            return;
            }
        }

    internal static class EDrawing {

        public static Color InvertColor(Color ColorToInvert) {
            return Color.FromArgb((byte)~ColorToInvert.R, (byte)~ColorToInvert.G, (byte)~ColorToInvert.B);
            }

        public static Region CreateRegion(this Graphics g, Rectangle rec, int radius, ExtendedButtonSettings.RoundedCorners corners) {

            int x = rec.X;
            int y = rec.Y;
            int width = rec.Width - 1;
            int height = rec.Height - 1;
            int diameter = radius * 2;
            Rectangle r = rec;
            Region controlRegion;

            using (GraphicsPath gp = new GraphicsPath()) {

                if ((corners & ExtendedButtonSettings.RoundedCorners.TopLeft) == ExtendedButtonSettings.RoundedCorners.TopLeft)
                    gp.AddArc(r.X, r.Y, (radius * 2) - 1, (radius * 2) - 1, 180, 90); // TL
                else
                    gp.AddLine(x, 12, x, y); // TL

                if ((corners & ExtendedButtonSettings.RoundedCorners.TopRight) == ExtendedButtonSettings.RoundedCorners.TopRight)
                    gp.AddArc((r.X + width) - (radius * 2), r.Y, (radius * 2), (radius * 2), 270, 90); // TR
                else
                    gp.AddLine(r.Width - 1, y, rec.Width - 1, y + 12); // TR

                if ((corners & ExtendedButtonSettings.RoundedCorners.BottomRight) == ExtendedButtonSettings.RoundedCorners.BottomRight)
                    gp.AddArc((r.X + width) - (radius * 2), (r.Y + height) - (radius * 2), (radius * 2), (radius * 2), 0, 90); // BR
                else
                    gp.AddLine(rec.Width - 1, 12, rec.Width - 1, rec.Height - 1); // BR

                if ((corners & ExtendedButtonSettings.RoundedCorners.BottomLeft) == ExtendedButtonSettings.RoundedCorners.BottomLeft)
                    gp.AddArc(r.X, (r.Y + height) - (radius * 2), (radius * 2), (radius * 2), 90, 90); // BL
                else
                    gp.AddLine(x + diameter, rec.Height - 1, x, rec.Height - 1); // BL

                gp.CloseAllFigures();

                controlRegion = new Region(gp);

                }
            return controlRegion;
            }

        public static Region DrawRoundedRectangle(this Graphics g, Color color, Rectangle rec, int radius, ExtendedButtonSettings.RoundedCorners corners, Pen pen, Brush br = null, bool outline = false, bool maskRegion = false) {
            using (var b = new SolidBrush(color)) {

                int x = rec.X;
                int y = rec.Y;
                int width = rec.Width - 1;
                int height = rec.Height - 1;
                int diameter = radius * 2;
                Rectangle r = rec;

                Region retRegion;

                using (GraphicsPath gp = new GraphicsPath()) {

                    if ((corners & ExtendedButtonSettings.RoundedCorners.TopLeft) == ExtendedButtonSettings.RoundedCorners.TopLeft)
                        gp.AddArc(r.X, r.Y, (radius * 2) - 1, (radius * 2) - 1, 180, 90); // TL
                    else
                        gp.AddLine(x, 12, x, y); // TL

                    if ((corners & ExtendedButtonSettings.RoundedCorners.TopRight) == ExtendedButtonSettings.RoundedCorners.TopRight)
                        gp.AddArc((r.X + width) - (radius * 2), r.Y, (radius * 2), (radius * 2), 270, 90); // TR
                    else
                        gp.AddLine(r.Width - 1, y, rec.Width - 1, y + 12); // TR

                    if ((corners & ExtendedButtonSettings.RoundedCorners.BottomRight) == ExtendedButtonSettings.RoundedCorners.BottomRight)
                        gp.AddArc((r.X + width) - (radius * 2), (r.Y + height) - (radius * 2), (radius * 2), (radius * 2), 0, 90); // BR
                    else
                        gp.AddLine(rec.Width - 1, 12, rec.Width - 1, rec.Height - 1); // BR

                    if ((corners & ExtendedButtonSettings.RoundedCorners.BottomLeft) == ExtendedButtonSettings.RoundedCorners.BottomLeft)
                        gp.AddArc(r.X, (r.Y + height) - (radius * 2), (radius * 2), (radius * 2), 90, 90); // BL
                    else
                        gp.AddLine(x + diameter, rec.Height - 1, x, rec.Height - 1); // BL

                    gp.CloseAllFigures();

                    if (outline) {
                        g.DrawPath(pen, gp);
                        }
                    else {
                        if (br != null) {
                            g.FillPath(br, gp);
                            }
                        else {
                            g.FillPath(b, gp);
                            }
                        }
                    retRegion = new Region(gp);
                    }

                return retRegion;

                }
            }
        }
    }