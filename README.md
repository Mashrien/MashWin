# MashWin

 Subclass of a Form for custom theme'ing and design, has a great many customization options.
 
 In the designer, under properties, is a new category of settings called "MWin Customization"- These allow for near infinite customization of the forms
 
 WndProc is also intercepted and OS calls are passed/handled so that dragging/resizing are handled via the OS and .NET, rather than the slow/clunky manual resizing methods.. Meaning the forms are fast and responsive, with no noticable performance impact over a native window
 
 Below is just a small example of what's possible with MWindow.  There's also an included class called "RoundButton" that can be imported via 'using MashWin;', giving access to all the forms and controls.
 
![mwindow](https://user-images.githubusercontent.com/29497715/147797182-34c0aa64-060a-4c2f-8036-4fd3b6538e6f.png)



I *HIGHLY* recommend ObjectListView as an addition to this tool for truly unique and creative forms, without having to delve into XML and WPF (I hate both, lol)

Form settings:

![form_settings](https://user-images.githubusercontent.com/29497715/147797180-f8a41de3-75f1-4058-8b5e-39ab869f110c.PNG)


RoundButton settings:

![button_settings](https://user-images.githubusercontent.com/29497715/147797176-71f8a54e-3a40-48e2-817c-d487b648eb9c.PNG)


## .NET 10 Windows port — 0.0.3.0

Open `MWindow.sln` in Visual Studio with the .NET desktop development workload,
or run `dotnet build MWindow.sln -c Release` on Windows. The project now targets
`net10.0-windows`. It does not require the .NET Framework 4.8 targeting pack.
Build outputs default to `artifacts/` outside the source files; an embedding
project can supply `MNXBuildRoot` to share its build output directory.

Derive your form from `MashWin.MWindow`, build once, and open your derived form
in the WinForms designer. The current settings groups are described below.
The default property metadata matches the actual property values.

Changes in this port are commented in the source:

- Caption buttons use their original instance fields and can be shown again
  after disabling them through settings.
- Resizing and background changes release replaced drawing resources.
- Form and button disposal releases their owned drawing resources.
- Icon conversion clones caller data and releases temporary native icon handles.
- Rounded button painting supports an ordinary Form parent, avoids recursive
  Paint callbacks, and allocates regions only when requested.
- Square frames accept a zero corner radius, title visibility is respected,
  and changing button text colors refreshes cached brushes.
- Unhandled client hit tests return HTCLIENT so normal control interaction works.

The original .NET Framework version remains available in Git history.


## Grouped live designer settings — 0.0.4.0

The **MWin Customization** category contains separate expandable entries:
Frame, Caption Buttons, Title, Menu Separator, Status Bar, and Tool Strips
(added in 0.0.5.0). Each is an actual group of settings, shared by the form and
the public settings container:

```csharp
Settings.Frame.BorderColor = Color.Red;
Settings.Frame.CornerRadius = 10;
Settings.CaptionButtons.MaximizeBox = false;
Settings.Title.Alignment = MashWin.Alignment.Center;
Settings.MenuSeparator.Color = Color.DarkGray;
Settings.StatusBar.Text = "Ready";
```

Flat paths such as `ExtendedSettings.BorderColor` have moved to their groups.
The hidden `ExtendedSettings` container alias remains available, but its members
are grouped too. New designer code uses the shorter `Frame.BorderColor` form;
it changes the same value as `Settings.Frame.BorderColor`.

Nested settings publish `INotifyPropertyChanged` notifications, including their
group name (for example `Frame.BorderColor`). The form updates its drawing pens,
outline, icon, text alignment, and caption buttons without requiring a resize.
Replacing the settings detaches the old subscription; disposing the form detaches
the current subscription too.

To run the Windows regression checks (no test framework packages required):

```sh
dotnet run --project tests/SettingsChecks/SettingsChecks.csproj -c Release
```

The checks use the same nested property descriptors as the property grid and
verify rendered output, caption visibility, labels, and settings subscriptions.


`RoundButton` now paints `Image` (including `ImageList` selections) together with
text. It honors `ImageAlign`, `TextAlign`, `TextImageRelation`, padding, disabled
appearance, and right-to-left ordering, and clips oversized content to the padded
area. Caller-supplied images remain caller-owned. Image and text painting lives
in the commented `RoundButton.Content.cs` partial file.

```csharp
button.Image = myIcon;
button.Text = "Extract";
button.TextImageRelation = TextImageRelation.ImageAboveText;
```


## ToolStrip styling — 0.0.5.0

Standard toolbars, menu bars, status strips, and context menus inside `MWindow`
share an `MToolStripRenderer`. This includes controls added through panels later,
context menus assigned after construction, generated submenus, and overflow menus.
The normal WinForms controls continue to provide image/text layout, clicks,
keyboard navigation, check marks, and menu lifetime handling.

The designer exposes **Tool Strips** under **MWin Customization**. Its values
are the same objects available through the nested public API:

```csharp
Settings.ToolStrips.HoverColor = Color.LightBlue;
Settings.ToolStrips.PressedColor = Color.CornflowerBlue;
Settings.ToolStrips.CornerRadius = 5;
Settings.ToolStrips.UseGradients = true;
```

Colors, borders, gradients, and rounded highlights update without resizing.
High-contrast mode uses the standard Windows renderer. Framework drawing still
handles item images, including disabled images; the renderer does not own them.

`MToolStripRenderer` can also be assigned directly to a ToolStrip on an ordinary
Form. In that case, the caller owns the renderer and should dispose it with the
form. Its `Settings` property accepts a shared `ToolStripSettings` object.
Tracked strips use weak references, and disposal detaches settings notifications.

## Caption alignment repair — 0.0.6.0

Caption buttons explicitly center their symbols now that RoundButton honors
TextAlign. Minimize remains low and horizontally centered. The maximize square
has a small DPI-scaled optical correction that preserves the full clipping area.
The Windows bitmap checks cover centering and the square's unclipped shape.
