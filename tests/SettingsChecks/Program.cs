using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MashWin;

internal static class Program {
    private static int failures;

    [STAThread]
    private static int Main() {
        Application.EnableVisualStyles();
        Check("Property grid label", CheckLabel);
        Check("Every setting notifies its owner", CheckNotifications);
        Check("Nested edits repaint without resizing", CheckRepaint);
        Check("Caption visibility changes immediately", CheckCaption);
        Check("Replacement settings detach the previous owner", CheckReplacement);
        Check("Disposed forms detach settings", CheckDisposal);
        Check("RoundButton image and text layout", CheckButtonImages);
        Console.WriteLine("Failures: " + failures);
        return failures == 0 ? 0 : 1;
        }

    private static void Check(string name, Action test) {
        try {
            test();
            Console.WriteLine("PASS: " + name);
            }
        catch (Exception error) {
            failures++;
            Console.WriteLine("FAIL: " + name + ": " + error.Message);
            }
        }

    private static void Require(bool condition, string message) {
        if (!condition) {
            throw new InvalidOperationException(message);
            }
        }

    private static void CheckLabel() {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(MWindow));
        foreach (string name in new string[] { "Frame", "CaptionButtons", "Title", "MenuSeparator", "StatusBar" }) {
            PropertyDescriptor property = properties[name];
            Require(property != null, "Missing customization group: " + name);
            Require(TypeDescriptor.GetProperties(typeof(MWindowSettings))[name] != null,
                "Settings." + name + " is missing from the public API");
            Require(property.Category == "MWin Customization", "Category still says " + property.Category);
            Require(property.SerializationVisibility == DesignerSerializationVisibility.Content, "Group must remain serializable: " + name);
            }
        Require(properties["CaptionButtons"].DisplayName == "Caption Buttons", "Caption group label is incorrect");
        Require(!properties["ExtendedSettings"].IsBrowsable, "Old flat settings are still displayed");
        using (MWindow window = new MWindow()) {
            foreach (string name in new string[] { "Frame", "CaptionButtons", "Title", "MenuSeparator", "StatusBar" }) {
                object designerGroup = properties[name].GetValue(window);
                object publicGroup = TypeDescriptor.GetProperties(window.Settings)[name].GetValue(window.Settings);
                Require(ReferenceEquals(designerGroup, publicGroup), "Designer and Settings." + name + " use different objects");
                }
            }
        }

    private static void CheckNotifications() {
        MWindowSettings settings = new MWindowSettings();
        INotifyPropertyChanged notifications = settings;
        Require(notifications != null, "Settings provide no change notifications");
        List<string> changes = new List<string>();
        notifications.PropertyChanged += delegate(object sender, PropertyChangedEventArgs arguments) {
            changes.Add(arguments.PropertyName);
            };
        foreach (PropertyDescriptor groupProperty in TypeDescriptor.GetProperties(settings)) {
            object group = groupProperty.GetValue(settings);
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(group)) {
                if (property.IsReadOnly) {
                    continue;
                    }
                object replacement = null;
                if (property.PropertyType == typeof(bool)) {
                    replacement = !(bool)property.GetValue(group);
                    }
                else if (property.PropertyType == typeof(int)) {
                    replacement = (int)property.GetValue(group) + 1;
                    }
                else if (property.PropertyType == typeof(Color)) {
                    replacement = Color.OrangeRed;
                    }
                else if (property.PropertyType == typeof(string)) {
                    replacement = "Updated status";
                    }
                else if (property.PropertyType == typeof(Alignment)) {
                    replacement = Alignment.Right;
                    }
                else if (property.PropertyType == typeof(Font)) {
                    Font original = (Font)property.GetValue(group);
                    replacement = new Font(original.FontFamily, original.Size + 2);
                    }
                else if (property.PropertyType == typeof(Image)) {
                    replacement = SystemIcons.Information.ToBitmap();
                    }
                else {
                    throw new InvalidOperationException("Missing test value for " + property.Name);
                    }
                int before = changes.Count;
                property.SetValue(group, replacement);
                Require(changes.Count == before + 1 && changes[before] == groupProperty.Name + "." + property.Name, property.Name + " did not notify exactly once");
                property.SetValue(group, replacement);
                Require(changes.Count == before + 1, property.Name + " notified for an unchanged value");
                IDisposable disposable = replacement as IDisposable;
                if (disposable != null) {
                    disposable.Dispose();
                    }
                }
            }
        }

    private static MWindow CreateWindow() {
        MWindow window = new MWindow();
        window.Size = new Size(480, 300);
        window.Text = "Live settings test";
        window.Show();
        Application.DoEvents();
        return window;
        }

    private static Bitmap Capture(MWindow window) {
        Bitmap bitmap = new Bitmap(window.Width, window.Height);
        window.DrawToBitmap(bitmap, window.ClientRectangle);
        return bitmap;
        }

    private static void CheckRepaint() {
        using (MWindow window = CreateWindow()) {
            int invalidations = 0;
            window.Invalidated += delegate { invalidations++; };
            Size originalSize = window.Size;
            using (Bitmap before = Capture(window)) {
                object frame = TypeDescriptor.GetProperties(window)["Frame"].GetValue(window);
                PropertyDescriptor radius = TypeDescriptor.GetProperties(frame)["CornerRadius"];
                radius.SetValue(frame, 35);
                Require(invalidations > 0, "Nested property edit did not invalidate the form");
                Application.DoEvents();
                using (Bitmap after = Capture(window)) {
                    bool changed = false;
                    for (int y = 0; y < 35; y++) {
                        for (int x = 0; x < 35; x++) {
                            if (before.GetPixel(x, y) != after.GetPixel(x, y)) {
                                changed = true;
                                }
                            }
                        }
                    Require(changed, "Rounded outline remained cached after changing CornerRadius");
                    }
                }
            window.Settings.MenuSeparator.Color = Color.Red;
            window.Settings.MenuSeparator.Thickness = 3;
            Application.DoEvents();
            using (Bitmap image = Capture(window)) {
                Require(image.GetPixel(100, 53).ToArgb() == Color.Red.ToArgb(), "Separator pen did not adopt the new color and width");
                }
            Require(window.Size == originalSize, "Updating settings unexpectedly resized the form");
            }
        }

    private static void CheckCaption() {
        using (MWindow window = CreateWindow()) {
            Control maximize = window.Controls["rbMaximize"];
            window.Settings.CaptionButtons.MaximizeBox = false;
            Require(!maximize.Visible, "Maximize caption stayed visible");
            window.Settings.CaptionButtons.MaximizeBox = true;
            Require(maximize.Visible, "Maximize caption did not return");
            }
        }

    private static void CheckReplacement() {
        using (MWindow window = CreateWindow()) {
            MWindowSettings original = window.Settings;
            MWindowSettings replacement = new MWindowSettings();
            window.Settings = replacement;
            Application.DoEvents();
            int invalidations = 0;
            window.Invalidated += delegate { invalidations++; };
            original.Frame.CornerRadius = 22;
            Require(invalidations == 0, "Old settings still notify the form");
            replacement.Frame.CornerRadius = 26;
            Require(invalidations > 0, "Replacement settings do not notify the form");
            }
        }

    private static void CheckDisposal() {
        MWindow window = CreateWindow();
        MWindowSettings retained = window.Settings;
        int invalidations = 0;
        window.Invalidated += delegate { invalidations++; };
        window.Dispose();
        int before = invalidations;
        retained.Frame.BorderColor = Color.Purple;
        Require(invalidations == before, "Retained settings still notify a disposed form");
        }
    private static Rectangle ColoredBounds(Bitmap bitmap, Color color) {
        int left = bitmap.Width;
        int top = bitmap.Height;
        int right = -1;
        int bottom = -1;
        for (int y = 0; y < bitmap.Height; y++) {
            for (int x = 0; x < bitmap.Width; x++) {
                if (bitmap.GetPixel(x, y).ToArgb() == color.ToArgb()) {
                    left = Math.Min(left, x);
                    top = Math.Min(top, y);
                    right = Math.Max(right, x);
                    bottom = Math.Max(bottom, y);
                    }
                }
            }
        if (right < left) {
            return Rectangle.Empty;
            }
        return Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

    private static void CheckButtonImages() {
        using (Bitmap icon = new Bitmap(16, 16)) {
            using (Graphics graphics = Graphics.FromImage(icon)) {
                graphics.Clear(Color.Red);
                }
            using (MWindow window = CreateWindow()) {
                window.BackColor = Color.White;
                using (RoundButton button = new RoundButton()) {
                    button.Size = new Size(160, 100);
                    button.Location = new Point(40, 60);
                    button.Text = "Label";
                    button.ForeColor = Color.Blue;
                    button.Image = icon;
                    button.Padding = new Padding(6);
                    window.Controls.Add(button);
                    foreach (TextImageRelation relation in Enum.GetValues(typeof(TextImageRelation))) {
                        button.TextImageRelation = relation;
                        using (Bitmap bitmap = new Bitmap(button.Width, button.Height)) {
                            button.DrawToBitmap(bitmap, button.ClientRectangle);
                            Rectangle image = ColoredBounds(bitmap, Color.Red);
                            Rectangle text = ColoredBounds(bitmap, Color.Blue);
                            Require(!image.IsEmpty, relation + " did not draw the image");
                            Require(!text.IsEmpty, relation + " did not draw the text");
                            Require(image.Left >= 6 && image.Top >= 6, "Image ignored button padding");
                            if (relation == TextImageRelation.ImageAboveText) {
                                Require(image.Bottom <= text.Top, "ImageAboveText overlaps or reverses content");
                                }
                            else if (relation == TextImageRelation.TextAboveImage) {
                                Require(text.Bottom <= image.Top, "TextAboveImage overlaps or reverses content");
                                }
                            else if (relation == TextImageRelation.ImageBeforeText) {
                                Require(image.Right <= text.Left, "ImageBeforeText overlaps or reverses content");
                                }
                            else if (relation == TextImageRelation.TextBeforeImage) {
                                Require(text.Right <= image.Left, "TextBeforeImage overlaps or reverses content");
                                }
                            }
                        }
                    button.TextImageRelation = TextImageRelation.Overlay;
                    button.ImageAlign = ContentAlignment.TopLeft;
                    button.TextAlign = ContentAlignment.BottomRight;
                    using (Bitmap bitmap = new Bitmap(button.Width, button.Height)) {
                        button.DrawToBitmap(bitmap, button.ClientRectangle);
                        Rectangle image = ColoredBounds(bitmap, Color.Red);
                        Rectangle text = ColoredBounds(bitmap, Color.Blue);
                        Require(image.Right < text.Left && image.Bottom < text.Top, "Independent image/text alignment was ignored");
                        }
                    button.Enabled = false;
                    using (Bitmap bitmap = new Bitmap(button.Width, button.Height)) {
                        button.DrawToBitmap(bitmap, button.ClientRectangle);
                        Require(ColoredBounds(bitmap, Color.Red).IsEmpty && ColoredBounds(bitmap, Color.Blue).IsEmpty,
                            "Disabled content retained its enabled colors");
                        }
                    button.Enabled = true;
                    button.ImageAlign = ContentAlignment.MiddleCenter;
                    button.TextAlign = ContentAlignment.MiddleCenter;
                    button.TextImageRelation = TextImageRelation.ImageBeforeText;
                    button.RightToLeft = RightToLeft.Yes;
                    using (Bitmap bitmap = new Bitmap(button.Width, button.Height)) {
                        button.DrawToBitmap(bitmap, button.ClientRectangle);
                        Rectangle image = ColoredBounds(bitmap, Color.Red);
                        Rectangle text = ColoredBounds(bitmap, Color.Blue);
                        Require(image.Left >= text.Right, "Right-to-left ordering was not reversed");
                        }
                    button.RightToLeft = RightToLeft.No;
                    button.Text = "";
                    using (Bitmap bitmap = new Bitmap(button.Width, button.Height)) {
                        button.DrawToBitmap(bitmap, button.ClientRectangle);
                        Require(!ColoredBounds(bitmap, Color.Red).IsEmpty, "Image-only content did not draw");
                        }
                    button.Text = "Label";
                    button.Image = null;
                    using (ImageList images = new ImageList()) {
                        images.Images.Add(icon);
                        button.ImageList = images;
                        button.ImageIndex = 0;
                        using (Bitmap bitmap = new Bitmap(button.Width, button.Height)) {
                            button.DrawToBitmap(bitmap, button.ClientRectangle);
                            Require(!ColoredBounds(bitmap, Color.Red).IsEmpty, "ImageList/ImageIndex did not draw");
                            }
                        button.ImageList = null;
                        }
                    button.Image = icon;
                    }
                }
            Require(icon.GetPixel(0, 0).ToArgb() == Color.Red.ToArgb(), "Button disposed the caller's image");
            }
        }

    }
