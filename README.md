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
