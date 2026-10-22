using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MashWin {
    public partial class MWindow {
        private MToolStripRenderer toolStripRenderer;
        private readonly HashSet<Control> toolStripControls = new HashSet<Control>();

        private void StartToolStripStyling() {
            toolStripRenderer = new MToolStripRenderer(settings.ToolStrips);
            WatchToolStripControls(this);
            }

        private void StopToolStripStyling() {
            // Removed controls are unwatched separately. Detach the remaining
            // handlers before child disposal, then release the settings subscription.
            UnwatchToolStripControls(this);
            toolStripRenderer?.Dispose();
            toolStripRenderer = null;
            }

        private void WatchToolStripControls(Control control) {
            if (!toolStripControls.Add(control)) {
                return;
                }
            control.ControlAdded += ToolStripControlAdded;
            control.ControlRemoved += ToolStripControlRemoved;
            control.ContextMenuStripChanged += ToolStripContextMenuChanged;
            control.Disposed += ToolStripControlDisposed;
            ApplyToolStripRenderer(control);
            foreach (Control child in control.Controls) {
                WatchToolStripControls(child);
                }
            }

        private void UnwatchToolStripControls(Control control) {
            if (!toolStripControls.Remove(control)) {
                return;
                }
            control.ControlAdded -= ToolStripControlAdded;
            control.ControlRemoved -= ToolStripControlRemoved;
            control.ContextMenuStripChanged -= ToolStripContextMenuChanged;
            control.Disposed -= ToolStripControlDisposed;
            foreach (Control child in control.Controls) {
                UnwatchToolStripControls(child);
                }
            }

        private void ApplyToolStripRenderer(Control control) {
            if (control is ToolStrip strip) {
                strip.Renderer = toolStripRenderer;
                }
            if (control.ContextMenuStrip != null) {
                control.ContextMenuStrip.Renderer = toolStripRenderer;
                }
            // WinForms automatically inherits the owner strip's renderer for
            // generated submenus and overflow menus, including items added later.
            }

        private void ToolStripControlAdded(object sender, ControlEventArgs arguments) {
            WatchToolStripControls(arguments.Control);
            }

        private void ToolStripControlRemoved(object sender, ControlEventArgs arguments) {
            UnwatchToolStripControls(arguments.Control);
            }

        private void ToolStripContextMenuChanged(object sender, EventArgs arguments) {
            ApplyToolStripRenderer((Control)sender);
            }

        private void ToolStripControlDisposed(object sender, EventArgs arguments) {
            UnwatchToolStripControls((Control)sender);
            }
        }
    }
