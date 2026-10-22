# MashWin

 Subclass of a Form for custom theme'ing and design, has a great many customization options.
 
 In the designer, under properties, is a new category of settings called "ExtendedSettings"- These allow for near infinite customization of the forms
 
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
in the WinForms designer. `ExtendedSettings` remains expandable and serializable.
The default property metadata now matches the actual property values.

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
