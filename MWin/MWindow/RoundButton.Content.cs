using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MashWin {
    public partial class RoundButton {
        // ButtonBase already resolves ImageList/ImageIndex/ImageKey through Image.
        // Custom painting must draw that image explicitly and leave its ownership
        // with the caller (or ImageList), just as an ordinary WinForms Button does.
        private void DrawContent(Graphics graphics) {
            Image image = Image;
            Rectangle content = new Rectangle(
                Padding.Left + 2,
                Padding.Top + 2,
                Math.Max(0, ClientSize.Width - Padding.Horizontal - 4),
                Math.Max(0, ClientSize.Height - Padding.Vertical - 4)
                );
            if (content.Width == 0 || content.Height == 0) {
                return;
                }

            Rectangle imageArea;
            Rectangle textArea;
            GetContentAreas(graphics, image, content, out imageArea, out textArea);
            GraphicsState previousState = graphics.Save();
            try {
                // Oversized images and pressed offsets stay inside the padded area.
                graphics.SetClip(content, CombineMode.Intersect);
                int pressedOffset = Enabled && isMouseDown ? 1 : 0;
                if (image != null) {
                    Rectangle imageBounds = AlignRectangle(image.Size, imageArea, RtlTranslateContent(ImageAlign));
                    imageBounds.Offset(pressedOffset, pressedOffset);
                    if (Enabled) {
                        graphics.DrawImageUnscaled(image, imageBounds.Location);
                        }
                    else {
                        ControlPaint.DrawImageDisabled(graphics, image, imageBounds.X, imageBounds.Y, BackColor);
                        }
                    }

                if (Text.Length > 0 && textArea.Width > 0 && textArea.Height > 0) {
                    textArea.Offset(pressedOffset, pressedOffset);
                    if (UseCompatibleTextRendering) {
                        textArea.Offset(0, 2);
                        }
                    Brush textBrush = TextColor;
                    if (!Enabled) {
                        textBrush = SystemBrushes.GrayText;
                        }
                    else if (isMouseDown && Settings.InvertForeColorOnMouseDown) {
                        textBrush = TextInvertedColor;
                        }
                    using (StringFormat format = new StringFormat()) {
                        format.Alignment = HorizontalAlignment(TextAlign);
                        format.LineAlignment = VerticalAlignment(TextAlign);
                        format.HotkeyPrefix = UseMnemonic ? HotkeyPrefix.Show : HotkeyPrefix.None;
                        format.Trimming = AutoEllipsis ? StringTrimming.EllipsisCharacter : StringTrimming.None;
                        if (RightToLeft == RightToLeft.Yes) {
                            format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                            }
                        graphics.DrawString(Text, Font, textBrush, textArea, format);
                        }
                    }
                }
            finally {
                graphics.Restore(previousState);
                }
            }

        private void GetContentAreas(Graphics graphics, Image image, Rectangle content,
            out Rectangle imageArea, out Rectangle textArea) {
            imageArea = content;
            textArea = content;
            if (image == null || Text.Length == 0 || TextImageRelation == TextImageRelation.Overlay) {
                return;
                }

            Size textSize = Size.Ceiling(graphics.MeasureString(Text, Font));
            const int gap = 4;
            TextImageRelation relation = TextImageRelation;
            // "Before" and "after" follow reading direction, rather than always left/right.
            if (RightToLeft == RightToLeft.Yes) {
                if (relation == TextImageRelation.ImageBeforeText) {
                    relation = TextImageRelation.TextBeforeImage;
                    }
                else if (relation == TextImageRelation.TextBeforeImage) {
                    relation = TextImageRelation.ImageBeforeText;
                    }
                }

            if (relation == TextImageRelation.ImageAboveText || relation == TextImageRelation.TextAboveImage) {
                int imageHeight = Math.Min(image.Height, content.Height);
                int textHeight = Math.Min(textSize.Height, Math.Max(0, content.Height - imageHeight - gap));
                Rectangle group = AlignRectangle(
                    new Size(content.Width, imageHeight + gap + textHeight), content, RtlTranslateContent(TextAlign)
                    );
                bool imageFirst = relation == TextImageRelation.ImageAboveText;
                imageArea = new Rectangle(content.Left, imageFirst ? group.Top : group.Top + textHeight + gap,
                    content.Width, imageHeight);
                textArea = new Rectangle(content.Left, imageFirst ? group.Top + imageHeight + gap : group.Top,
                    content.Width, textHeight);
                }
            else {
                int imageWidth = Math.Min(image.Width, content.Width);
                int textWidth = Math.Min(textSize.Width, Math.Max(0, content.Width - imageWidth - gap));
                Rectangle group = AlignRectangle(
                    new Size(imageWidth + gap + textWidth, content.Height), content, RtlTranslateContent(TextAlign)
                    );
                bool imageFirst = relation == TextImageRelation.ImageBeforeText;
                imageArea = new Rectangle(imageFirst ? group.Left : group.Left + textWidth + gap, content.Top,
                    imageWidth, content.Height);
                textArea = new Rectangle(imageFirst ? group.Left + imageWidth + gap : group.Left, content.Top,
                    textWidth, content.Height);
                }
            }

        private static Rectangle AlignRectangle(Size size, Rectangle area, ContentAlignment alignment) {
            int left = area.Left;
            int top = area.Top;
            StringAlignment horizontal = HorizontalAlignment(alignment);
            StringAlignment vertical = VerticalAlignment(alignment);
            if (horizontal == StringAlignment.Center) {
                left += (area.Width - size.Width) / 2;
                }
            else if (horizontal == StringAlignment.Far) {
                left = area.Right - size.Width;
                }
            if (vertical == StringAlignment.Center) {
                top += (area.Height - size.Height) / 2;
                }
            else if (vertical == StringAlignment.Far) {
                top = area.Bottom - size.Height;
                }
            return new Rectangle(left, top, size.Width, size.Height);
            }

        private static StringAlignment HorizontalAlignment(ContentAlignment alignment) {
            switch (alignment) {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    return StringAlignment.Near;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Center;
                }
            }

        private static StringAlignment VerticalAlignment(ContentAlignment alignment) {
            switch (alignment) {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    return StringAlignment.Near;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Center;
                }
            }
        }
    }
