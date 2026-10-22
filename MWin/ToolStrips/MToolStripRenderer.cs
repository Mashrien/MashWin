using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MashWin {
    // Style drawing belongs here; WinForms still owns layout, hit testing,
    // keyboard navigation, image ownership, and menu opening/closing behavior.
    public sealed class MToolStripRenderer : ToolStripSystemRenderer, IDisposable {
        private ToolStripSettings settings;
        private readonly List<WeakReference<ToolStrip>> strips = new List<WeakReference<ToolStrip>>();
        private bool disposed;

        public MToolStripRenderer() : this(new ToolStripSettings()) {
            }

        public MToolStripRenderer(ToolStripSettings settings) {
            Settings = settings;
            }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ToolStripSettings Settings {
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
                if (disposed) {
                    throw new ObjectDisposedException(nameof(MToolStripRenderer));
                    }
                if (settings != null) {
                    settings.PropertyChanged -= SettingsChanged;
                    }
                settings = value;
                settings.PropertyChanged += SettingsChanged;
                InvalidateStrips();
                }
            }

        protected override void Initialize(ToolStrip toolStrip) {
            base.Initialize(toolStrip);
            RememberStrip(toolStrip);
            }

        private void RememberStrip(ToolStrip strip) {
            for (int index = strips.Count - 1; index >= 0; index--) {
                ToolStrip existing;
                if (!strips[index].TryGetTarget(out existing) || existing.IsDisposed) {
                    strips.RemoveAt(index);
                    }
                else if (ReferenceEquals(existing, strip)) {
                    return;
                    }
                }
            // A retained renderer must not keep closed controls and forms alive.
            strips.Add(new WeakReference<ToolStrip>(strip));
            }

        private void SettingsChanged(object sender, PropertyChangedEventArgs arguments) {
            InvalidateStrips();
            }

        private void InvalidateStrips() {
            for (int index = strips.Count - 1; index >= 0; index--) {
                ToolStrip strip;
                if (!strips[index].TryGetTarget(out strip) || strip.IsDisposed || !ReferenceEquals(strip.Renderer, this)) {
                    strips.RemoveAt(index);
                    }
                else {
                    strip.Invalidate(true);
                    }
                }
            }

        public void Dispose() {
            if (disposed) {
                return;
                }
            disposed = true;
            settings.PropertyChanged -= SettingsChanged;
            strips.Clear();
            }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs arguments) {
            // Inherited dropdown renderers do not always receive Initialize.
            // Remember painted popups too so live settings repaint open menus.
            RememberStrip(arguments.ToolStrip);
            if (SystemInformation.HighContrast || arguments.ToolStrip.BackgroundImage != null) {
                base.OnRenderToolStripBackground(arguments);
                return;
                }
            Color color = arguments.ToolStrip is ToolStripDropDown ? settings.MenuBackgroundColor : settings.BackgroundColor;
            using (SolidBrush brush = new SolidBrush(color)) {
                arguments.Graphics.FillRectangle(brush, arguments.AffectedBounds);
                }
            }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs arguments) {
            if (SystemInformation.HighContrast) {
                base.OnRenderButtonBackground(arguments);
                return;
                }
            DrawItemSurface(arguments, true);
            }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs arguments) {
            if (SystemInformation.HighContrast) {
                base.OnRenderMenuItemBackground(arguments);
                return;
                }
            DrawItemSurface(arguments, false);
            }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs arguments) {
            if (SystemInformation.HighContrast) {
                base.OnRenderDropDownButtonBackground(arguments);
                return;
                }
            DrawItemSurface(arguments, true);
            }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs arguments) {
            if (SystemInformation.HighContrast) {
                base.OnRenderSplitButtonBackground(arguments);
                return;
                }
            DrawItemSurface(arguments, true);
            ToolStripSplitButton button = (ToolStripSplitButton)arguments.Item;
            Rectangle divider = button.SplitterBounds;
            using (Pen pen = new Pen(settings.BorderColor)) {
                arguments.Graphics.DrawLine(pen, divider.Left, 3, divider.Left, button.Height - 4);
                }
            // Split buttons delegate their arrow painting to the renderer too.
            DrawArrow(new ToolStripArrowRenderEventArgs(arguments.Graphics, button,
                button.DropDownButtonBounds, ItemTextColor(button), ArrowDirection.Down));
            }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs arguments) {
            if (SystemInformation.HighContrast) {
                base.OnRenderOverflowButtonBackground(arguments);
                return;
                }
            DrawItemSurface(arguments, true);
            Rectangle bounds = new Rectangle(Point.Empty, arguments.Item.Size);
            bool vertical = arguments.ToolStrip.Orientation == Orientation.Vertical;
            DrawArrow(new ToolStripArrowRenderEventArgs(arguments.Graphics, arguments.Item, bounds,
                ItemTextColor(arguments.Item), vertical ? ArrowDirection.Right : ArrowDirection.Down));
            }

        private void DrawItemSurface(ToolStripItemRenderEventArgs arguments, bool drawNormalBorder) {
            ToolStripItem item = arguments.Item;
            bool isChecked = item is ToolStripButton button && button.Checked;
            if (item is ToolStripMenuItem menuItem && menuItem.Checked) {
                isChecked = true;
                }
            // A disabled toggle still needs to communicate its checked state.
            bool active = isChecked || (item.Enabled && (item.Pressed || item.Selected));
            if (!active && !drawNormalBorder) {
                return;
                }
            Color color = arguments.ToolStrip is ToolStripDropDown ? settings.MenuBackgroundColor : settings.BackgroundColor;
            if (active) {
                if (item.Enabled && item.Pressed) {
                    color = settings.PressedColor;
                    }
                else if (item.Enabled && item.Selected) {
                    color = settings.HoverColor;
                    }
                else {
                    color = settings.CheckedColor;
                    }
                }
            float scale = arguments.ToolStrip.DeviceDpi / 96f;
            float thickness = settings.BorderThickness * scale;
            float inset = Math.Max(1f, thickness / 2f);
            RectangleF bounds = new RectangleF(inset, inset,
                item.Width - inset * 2f, item.Height - inset * 2f);
            if (bounds.Width <= 0 || bounds.Height <= 0) {
                return;
                }
            float radius = Math.Min(settings.CornerRadius * scale, Math.Min(bounds.Width, bounds.Height) / 2f);
            GraphicsState state = arguments.Graphics.Save();
            try {
                arguments.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundedOutline(bounds, radius)) {
                    if (active && settings.UseGradients) {
                        Color lighter = Color.FromArgb(color.A,
                            Math.Min(255, color.R + 32), Math.Min(255, color.G + 32), Math.Min(255, color.B + 32));
                        using (LinearGradientBrush brush = new LinearGradientBrush(bounds, lighter, color, LinearGradientMode.Vertical)) {
                            arguments.Graphics.FillPath(brush, path);
                            }
                        }
                    else {
                        using (SolidBrush brush = new SolidBrush(color)) {
                            arguments.Graphics.FillPath(brush, path);
                            }
                        }
                    if (thickness > 0) {
                        using (Pen pen = new Pen(settings.BorderColor, thickness)) {
                            arguments.Graphics.DrawPath(pen, path);
                            }
                        }
                    }
                }
            finally {
                arguments.Graphics.Restore(state);
                }
            }

        private static GraphicsPath RoundedOutline(RectangleF bounds, float radius) {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) {
                path.AddRectangle(bounds);
                }
            else {
                float diameter = radius * 2f;
                path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
                path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
                path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                }
            return path;
            }

        private Color ItemTextColor(ToolStripItem item) {
            if (!item.Enabled) {
                return settings.DisabledTextColor;
                }
            if (item.Pressed) {
                return settings.PressedTextColor;
                }
            return settings.TextColor;
            }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs arguments) {
            if (!SystemInformation.HighContrast) {
                arguments.TextColor = ItemTextColor(arguments.Item);
                }
            base.OnRenderItemText(arguments);
            }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs arguments) {
            if (!SystemInformation.HighContrast && arguments.Item != null) {
                arguments.ArrowColor = ItemTextColor(arguments.Item);
                }
            base.OnRenderArrow(arguments);
            }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs arguments) {
            if (SystemInformation.HighContrast) {
                base.OnRenderImageMargin(arguments);
                return;
                }
            using (SolidBrush brush = new SolidBrush(settings.MenuBackgroundColor)) {
                arguments.Graphics.FillRectangle(brush, arguments.AffectedBounds);
                }
            }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs arguments) {
            if (SystemInformation.HighContrast) {
                base.OnRenderToolStripBorder(arguments);
                return;
                }
            ToolStrip strip = arguments.ToolStrip;
            if (settings.BorderThickness == 0 || strip.Width < 2 || strip.Height < 2) {
                return;
                }
            float thickness = settings.BorderThickness * (strip.DeviceDpi / 96f);
            // Draw inward so the full configured border stays inside the strip's
            // clipping region; a wide pen centered on the edge would lose half.
            using (SolidBrush brush = new SolidBrush(settings.BorderColor)) {
                if (strip is ToolStripDropDown) {
                    arguments.Graphics.FillRectangle(brush, 0, 0, strip.Width, thickness);
                    arguments.Graphics.FillRectangle(brush, 0, 0, thickness, strip.Height);
                    }
                if (strip is ToolStripDropDown || strip.Orientation == Orientation.Vertical) {
                    arguments.Graphics.FillRectangle(brush, strip.Width - thickness, 0, thickness, strip.Height);
                    }
                if (strip is ToolStripDropDown || strip.Orientation == Orientation.Horizontal) {
                    arguments.Graphics.FillRectangle(brush, 0, strip.Height - thickness, strip.Width, thickness);
                    }
                }
            }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs arguments) {
            if (SystemInformation.HighContrast) {
                base.OnRenderSeparator(arguments);
                return;
                }
            using (Pen pen = new Pen(settings.BorderColor)) {
                if (arguments.Vertical) {
                    int middle = arguments.Item.Width / 2;
                    arguments.Graphics.DrawLine(pen, middle, 3, middle, Math.Max(3, arguments.Item.Height - 4));
                    }
                else {
                    int middle = arguments.Item.Height / 2;
                    arguments.Graphics.DrawLine(pen, 4, middle, Math.Max(4, arguments.Item.Width - 5), middle);
                    }
                }
            }
        }
    }
