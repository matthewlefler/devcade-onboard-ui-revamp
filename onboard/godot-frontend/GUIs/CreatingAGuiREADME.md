## What Happens before a GUI is instantated 
* First the enviormental values are loaded from the `.env` file
* Then the `AutoLoad.cs` script is started by the Godot autoload system
* Then the `Client.cs` script is started for communication with the backend from `AutoLoad.cs`
* Then the `GuiManagerGlobal.cs` script is started by the Godot autoload system
* Then the Scene Tree is created, loading the root node that has `GuiManager.cs` attached
* Which then finally loads the selected Gui scene

## Creating a GUI
* First create a new folder under `onboard/godot-frontend/GUIs` with the name of the new GUI
* Then create a new scene in that folder and start creating
* Use the functions and variables of `GuiManagerGlobal.cs` to get the list of games, tags, etc.

## Required Functionality

### The GUI you create must:
* Have a way to select and set a tag from the list of all the tags
* Have a way to select and lauch a game from the list of the games
* Be able to view the description and author of a game
* Help text, explaining what the buttons do
* Accessiblility is of the upmost importance

### While not strictly necessary these are things that are nice to have:
* Animations, such as transisions between game 1 and 2 being selected


