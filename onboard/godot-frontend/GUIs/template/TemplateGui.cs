using Godot;

using System.Collections.Generic;

namespace onboard.devcade.GUI.template;

/// <summary>
/// this is somewhat simple GUI that implements all the basic functions 
/// to show how to create a simple UI script
/// all logic for creating buttons and adding functions to the buttons is done here
/// </summary>
public partial class TemplateGui : Node
{
    /// <summary>
    /// runs once after the node is loaded into the scene tree,
    /// used in this case to set the monochrome missing texture, 
    /// and to get the screen width/height
    /// </summary>
    public override void _Ready()
    {
        GuiManagerGlobal.instance.gameTitlesUpdated += () =>
        {
            List<DevcadeGame> games = GuiManagerGlobal.gameTitles;
            // called on the list of games List<DevcadeGame> being updated
        };

        GuiManagerGlobal.instance.tagListUpdated += () =>
        {
            List<Tag> tags = GuiManagerGlobal.tagList;
            // called on the list of tags List<Tag> being updated
        };
    }

    public override void _Input(InputEvent @event)
    {
        // add any input that happends once per a given keypress here

        // if both black buttons are pressed
        if(@event.IsActionPressed("Player1_Menu") && @event.IsActionPressed("Player2_Menu"))
        {
            GuiManagerGlobal.instance.reloadGameList();
        }
    }

    public override void _Process(double delta)
    {

    }

    /// <summary>
    /// launches the given game
    /// calls the launchGame function of the model
    /// </summary>
    /// <param name="game"></param>
    private void launchGame(DevcadeGame game)
    {
        // discard result, in this instance we don't care when the game is closed or the result,
        // only that it is killed at some point
        _ = GuiManagerGlobal.instance.launchGame(game);
    }

    /// <summary>
    /// sets the current tag variable in the model to the given tag
    /// </summary>
    /// <param name="tag"> the new tag </param>
    private void setCurrentTag(Tag tag)
    {
        GuiManagerGlobal.instance.setTag(tag);
    }

    /// <summary>
    /// kills the currently running game 
    /// IMPORTANT: does not wait for the game to be killed for the function to return
    /// </summary>
    private void killCurrentlyRunningGame()
    {
        // discard the result,
        // Because this call is not awaited, execution of the current method continues before the call is completed
        _ = GuiManagerGlobal.instance.killGame();
    }
}
