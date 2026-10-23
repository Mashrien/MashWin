using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MashWin {
    // Every group owns its values and reports edits to its containing settings.
    [TypeConverter(typeof(MashWindowSettingsConverter))]
    public abstract class CustomizationGroup : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return;
                }
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    public sealed class FrameSettings : CustomizationGroup {
        private bool resizable = true;
        private bool showChrome = true;

        [DefaultValue(true)]
        [NotifyParentProperty(true)]
        public bool ShowChrome {
            get { return showChrome; }
            set { SetValue(ref showChrome, value); }
            }

        private Color borderColor = Color.Gray;
        private int borderThickness = 1;
        private int cornerRadius = 5;
        private bool showSizeGrip = false;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(true)]
        public bool Resizable {
            get {
                return resizable;
                }
            set {
                SetValue(ref resizable, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor {
            get {
                return borderColor;
                }
            set {
                SetValue(ref borderColor, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(1)]
        public int BorderThickness {
            get {
                return borderThickness;
                }
            set {
                SetValue(ref borderThickness, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(5)]
        public int CornerRadius {
            get {
                return cornerRadius;
                }
            set {
                SetValue(ref cornerRadius, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(false)]
        public bool ShowSizeGrip {
            get {
                return showSizeGrip;
                }
            set {
                SetValue(ref showSizeGrip, value);
                }
            }
        }

    public sealed class CaptionButtonSettings : CustomizationGroup {
        private bool exitBox = true;
        private bool maximizeBox = true;
        private bool minimizeBox = true;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(true)]
        public bool ExitBox {
            get {
                return exitBox;
                }
            set {
                SetValue(ref exitBox, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(true)]
        public bool MaximizeBox {
            get {
                return maximizeBox;
                }
            set {
                SetValue(ref maximizeBox, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(true)]
        public bool MinimizeBox {
            get {
                return minimizeBox;
                }
            set {
                SetValue(ref minimizeBox, value);
                }
            }
        }

    public sealed class TitleSettings : CustomizationGroup {
        private Image icon;
        private Font font = new Font("Segoe UI", 9f);
        private bool showTitle = true;
        private Alignment alignment = Alignment.Left;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(null)]
        public Image Icon {
            get {
                return icon;
                }
            set {
                SetValue(ref icon, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]

        public Font Font {
            get {
                return font;
                }
            set {
                SetValue(ref font, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(true)]
        public bool ShowTitle {
            get {
                return showTitle;
                }
            set {
                SetValue(ref showTitle, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(MashWin.Alignment.Left)]
        public Alignment Alignment {
            get {
                return alignment;
                }
            set {
                SetValue(ref alignment, value);
                }
            }
        }

    public sealed class MenuSeparatorSettings : CustomizationGroup {
        private bool enabled = true;
        private Color color = Color.Gray;
        private int thickness = 1;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(true)]
        public bool Enabled {
            get {
                return enabled;
                }
            set {
                SetValue(ref enabled, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color), "Gray")]
        public Color Color {
            get {
                return color;
                }
            set {
                SetValue(ref color, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(1)]
        public int Thickness {
            get {
                return thickness;
                }
            set {
                SetValue(ref thickness, value);
                }
            }
        }

    public sealed class StatusBarSettings : CustomizationGroup {
        private Font font = new Font("Segoe UI", 9f);
        private string text = "";
        private Alignment alignment = Alignment.Left;
        private bool showSeparator = false;
        private bool visible = true;
        private int height = 24;
        private int horizontalPadding = 4;

        [DefaultValue(true)]
        [NotifyParentProperty(true)]
        public bool Visible {
            get { return visible; }
            set { SetValue(ref visible, value); }
            }

        [DefaultValue(24)]
        [NotifyParentProperty(true)]
        public int Height {
            get { return height; }
            set { SetValue(ref height, Math.Max(18, Math.Min(256, value))); }
            }

        [DefaultValue(4)]
        [NotifyParentProperty(true)]
        public int HorizontalPadding {
            get { return horizontalPadding; }
            set { SetValue(ref horizontalPadding, Math.Max(0, Math.Min(64, value))); }
            }


        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]

        public Font Font {
            get {
                return font;
                }
            set {
                SetValue(ref font, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("")]
        public string Text {
            get {
                return text;
                }
            set {
                SetValue(ref text, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(MashWin.Alignment.Left)]
        public Alignment Alignment {
            get {
                return alignment;
                }
            set {
                SetValue(ref alignment, value);
                }
            }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(false)]
        public bool ShowSeparator {
            get {
                return showSeparator;
                }
            set {
                SetValue(ref showSeparator, value);
                }
            }
        }
    }
