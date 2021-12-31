using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MashWin {
    public class MashWindowSettingsConverter : ExpandableObjectConverter {
        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType) {
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

    public enum Alignment {
        Left,
        Center,
        Right,
        }

    [TypeConverter(typeof(MashWindowSettingsConverter))]
    public class MWindowSettings {

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Image))]
        public Image Icon { get; set; }

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(bool))]
        public bool MaximizeBox { get; set; } = true;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(bool))]
        public bool Resizable { get; set; } = true;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(bool))]
        public bool MinimizeBox { get; set; } = true;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(bool))]
        public bool ExitBox { get; set; } = true;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(bool))]
        public bool MenuSeparator { get; set; } = true;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color BorderColor { get; set; } = Color.Gray;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color))]
        public Color SeparatorColor { get; set; } = Color.Gray;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(int))]
        public int BorderThickness { get; set; } = 1;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(int))]
        public int SeparatorThickness { get; set; } = 1;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(int))]
        public int CornerRadius { get; set;} = 5;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Font))]
        public Font TitleFont { get; set; } = new Font("Segoe UI", 9f);

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Boolean))]
        public bool ShowTitle { get; set; } = true;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Alignment))]
        public Alignment TitleAlignment { get; set; } = Alignment.Left;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Font))]
        public Font StatusBarFont { get; set; } = new Font("Segoe UI", 9f);

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Font))]
        public string StatusBarText { get; set; } = "";

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Alignment))]
        public Alignment StatusBarAlignment { get; set; } = Alignment.Left;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Boolean))]
        public bool StatusBarSeparator { get; set; } = false;

        [Browsable(true)]
        [NotifyParentProperty(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(bool))]
        public bool ShowSizeGrip { get; set; } = false;

        }
    }
