## Contributing tips
Make a new branch
If you use Visual Studio or VSCode use tabs as spacing
because opening the scripts in the Godot script editor will auto format the files to use tabs anyway

## docs
There are three main scripts for the onboard system
* Client.cs
* GuiManager.cs
* GuiInterface.cs

# Client.cs
found in onboard/godot-frontend/devcade/Client.cs
It is a static class, which is one of the ways to implement a singlton, see: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members 

This script interfaces with the backend through a Unix domain socket, local to the machine the frontend is running on:
socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);

# GuiManager.cs
found in onboard/godot-frontend/GuiManager.cs

This script is meant to handle switching between the seperate GUIs and handle communication between the Client.cs script and the GUIs
It should handle shared logic such as the loading animation

Some of the functions that it provides are: 
* kill the current running game
* set the current tag
* launching a given game

# GuiInterface.cs
found in onboard/godot-frontend/devcade/GuiInterface.cs

this is an interface that defines the needed functions for a GUI to interface with the GuiManager
mainly tasks related to the game list and tags