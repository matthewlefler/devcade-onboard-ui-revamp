# Contributing tips
Make a new branch
If you use Visual Studio or VSCode use spaces instead of tabs. If opening the scripts in the Godot script editor auto formats the files to use tabs, there is an option to change it. 

If you are creating a new GUI refer to the readme at `onboard/godot-frontend/GUIs/CreatingAGuiREADME.md`

# Docs
There are the main scripts for the onboard system
* Client.cs
* Env.cs
* GuiManager.cs
* GuiManagerGlobal.cs

## Client.cs
Found in `onboard/godot-frontend/devcade/Client.cs`
It is a static class, which is one of the ways to implement a singlton, see: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members 

This script interfaces with the backend through a Unix domain socket, local to the machine the frontend is running on:
```C#
socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
```

## Env.cs
Found in `onboard/godot-frontend/util/Env.cs`

This script handles the enviormental values that are used by various parts of the onboard. See `.env.template` for the values it contains. It is also a static class.

It provides its values in key, value pairs that 

## GuiManager.cs
Found in `onboard/godot-frontend/GuiManager.cs`

This script is meant to handle the creation of the seperate GUIs and the states of the loading animation, and screensaver, etc.

It is a Control node in the scene tree and the root of its scene tree in the current case
the GUIs are added as a child node to the node the script is attached to, 
starting with the initial GUI scene as set in the editor for the variable: initialGuiScene

## GuiManagerGlobal.cs
Found in `onboard/godot-frontend/GuiManagerGlobal.cs`

This script is meant to handle the communication between the Client.cs script and the GUIs.
It handles some of the shared logic such as setting the state of the loading animation.

Some of the functions that it provides are: 
* Kill the current running game
* Set the current tag
* Launching a given game
* Loading/Reloading the game list from the backend
* Interface for showing/hiding the loading animation

## SupervisorButton.cs
Found in `onboard/godot-frontend/util/SupervisorButton.cs`

This is a simple class to encapsulate the detecting of the "supervisor button" that will 
kill the currently running game if held for some amount of time defined in `GuiManager.cs`.
!IMPORTANT! due to how gamepad and keyboard events are propogated, only gamepad events are able
to be recieved when the onboard is not the focused window