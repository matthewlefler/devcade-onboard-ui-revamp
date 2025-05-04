## initialization 

If you are creating a new GUI create a new folder in the GUIs folder with a name you like, 
and either copy the template over or create a new class that inherits from the GuiInterface

## required functionality

the GUI you create must:
* have a way to select and set a tag from the list of all the tags
* have a way to select and set another GUI from the list of all the GUIs
* have a way to select and lauch a game from the list of the games
* be able to view the description and author of a game
* help text, explaining what the buttons do

while not strictly necessary these are things that are nice to have:
* animations, such as transisions between game 1 and 2 being selected


# tips
the Control class and all inheriting nodes (such as the button class) have a top, bottom, left, and right neighbor property
while the engine auto populates these fields to the best of its abilities, manualy seting these is better 
and might be required in some situations such as in the original GUI

