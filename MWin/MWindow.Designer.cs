using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static MashWin.Win32;
using MashWin;

namespace MashWin {
    partial class MWindow : Form {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public MWindowSettings Settings = new MWindowSettings();

        private RoundButton rbExit;
        private RoundButton rbMaximize;
        private RoundButton rbMinimize;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
                }
            base.Dispose(disposing);
            }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // MashWin
            // 
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(64, 40);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = this.Size;
            this.Name = "MashWin";
            this.Opacity = 0.97D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MashWin";
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.Resize += new System.EventHandler(this.MWin_Resize);
            this.ResumeLayout(false);

            }

        #endregion
        }
    }