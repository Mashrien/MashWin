using System;
using System.ComponentModel;
using System.Drawing;

namespace MashWin {
    public sealed class ToolStripSettings : CustomizationGroup {
        private Color backgroundColor = Color.WhiteSmoke;
        private Color menuBackgroundColor = Color.WhiteSmoke;
        private Color textColor = SystemColors.ControlText;
        private Color disabledTextColor = SystemColors.GrayText;
        private Color hoverColor = Color.LightBlue;
        private Color pressedColor = Color.CornflowerBlue;
        private Color pressedTextColor = Color.White;
        private Color checkedColor = Color.LightSteelBlue;
        private Color borderColor = Color.Gray;
        private int borderThickness = 1;
        private int cornerRadius = 5;
        private bool useGradients = true;

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "WhiteSmoke")]
        public Color BackgroundColor {
            get {
                return backgroundColor;
                }
            set {
                SetValue(ref backgroundColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "WhiteSmoke")]
        public Color MenuBackgroundColor {
            get {
                return menuBackgroundColor;
                }
            set {
                SetValue(ref menuBackgroundColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "ControlText")]
        public Color TextColor {
            get {
                return textColor;
                }
            set {
                SetValue(ref textColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "GrayText")]
        public Color DisabledTextColor {
            get {
                return disabledTextColor;
                }
            set {
                SetValue(ref disabledTextColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "LightBlue")]
        public Color HoverColor {
            get {
                return hoverColor;
                }
            set {
                SetValue(ref hoverColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "CornflowerBlue")]
        public Color PressedColor {
            get {
                return pressedColor;
                }
            set {
                SetValue(ref pressedColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "White")]
        public Color PressedTextColor {
            get {
                return pressedTextColor;
                }
            set {
                SetValue(ref pressedTextColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "LightSteelBlue")]
        public Color CheckedColor {
            get {
                return checkedColor;
                }
            set {
                SetValue(ref checkedColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor {
            get {
                return borderColor;
                }
            set {
                SetValue(ref borderColor, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(1)]
        public int BorderThickness {
            get {
                return borderThickness;
                }
            set {
                if (value < 0 || value > 8) {
                    throw new ArgumentOutOfRangeException(nameof(value), "BorderThickness must be between 0 and 8.");
                    }
                SetValue(ref borderThickness, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(5)]
        public int CornerRadius {
            get {
                return cornerRadius;
                }
            set {
                if (value < 0 || value > 64) {
                    throw new ArgumentOutOfRangeException(nameof(value), "CornerRadius must be between 0 and 64.");
                    }
                SetValue(ref cornerRadius, value);
                }
            }

        [NotifyParentProperty(true)]
        [DefaultValue(true)]
        public bool UseGradients {
            get {
                return useGradients;
                }
            set {
                SetValue(ref useGradients, value);
                }
            }
        }
    }
