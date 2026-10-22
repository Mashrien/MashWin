using System;
using System.ComponentModel;
using System.Globalization;

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

    // Real groups back both Settings.Frame.BorderColor and the property-grid entries.
    [TypeConverter(typeof(MashWindowSettingsConverter))]
    public class MWindowSettings : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public MWindowSettings() {
            Frame.PropertyChanged += GroupChanged;
            CaptionButtons.PropertyChanged += GroupChanged;
            Title.PropertyChanged += GroupChanged;
            MenuSeparator.PropertyChanged += GroupChanged;
            StatusBar.PropertyChanged += GroupChanged;
            ToolStrips.PropertyChanged += GroupChanged;
            }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FrameSettings Frame { get; } = new FrameSettings();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public CaptionButtonSettings CaptionButtons { get; } = new CaptionButtonSettings();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TitleSettings Title { get; } = new TitleSettings();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public MenuSeparatorSettings MenuSeparator { get; } = new MenuSeparatorSettings();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public StatusBarSettings StatusBar { get; } = new StatusBarSettings();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ToolStripSettings ToolStrips { get; } = new ToolStripSettings();

        // Preserve the group in notifications so the form can recognize Title.Icon
        // separately from ordinary frame, caption, and text updates.
        private void GroupChanged(object sender, PropertyChangedEventArgs arguments) {
            string groupName;
            if (ReferenceEquals(sender, Frame)) {
                groupName = nameof(Frame);
                }
            else if (ReferenceEquals(sender, CaptionButtons)) {
                groupName = nameof(CaptionButtons);
                }
            else if (ReferenceEquals(sender, Title)) {
                groupName = nameof(Title);
                }
            else if (ReferenceEquals(sender, MenuSeparator)) {
                groupName = nameof(MenuSeparator);
                }
            else if (ReferenceEquals(sender, StatusBar)) {
                groupName = nameof(StatusBar);
                }
            else if (ReferenceEquals(sender, ToolStrips)) {
                groupName = nameof(ToolStrips);
                }
            else {
                return;
                }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(groupName + "." + arguments.PropertyName));
            }
        }
    }
