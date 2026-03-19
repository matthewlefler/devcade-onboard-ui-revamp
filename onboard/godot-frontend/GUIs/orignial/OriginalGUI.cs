using Godot;

namespace onboard.devcade.GUI.originalGUI;
public partial class OriginalGUI : Control
{
    /// <summary>
    /// the current state of the GUI
    /// this is used to determine what actions to take based on what is / should be on the screen
    /// </summary>
    private GuiState state = GuiState.ViewGames;

    // the different states of the GUI
    public enum GuiState
    {
        ViewGames, // the game list is shown
        Description, // the description of a game is shown
        Tags, // the tag list is shown
        Theme, // the theme picker is shown
        GameLaunched // a game is being run
    }

    /// <summary>
    /// The camera,
    /// which is moved around to show the 
    /// tags, games, theme picker, and similar
    /// </summary>
    [Export]
    public SlerpCamera2d camera;

    /// <summary>
    /// the node that has the game buttons as children
    /// </summary>
    [Export]
    public GamesContainer gameContainer;

    /// <summary>
    /// the node that has the tag buttons as children
    /// </summary>
    [Export]
    public TagContainer tagContainer;

    /// <summary>
    /// the node that is shown/hidden to show/hide the description
    /// </summary>
    [Export]
    public CanvasItem description;

    /// <summary>
    /// the label that holds the description text
    /// </summary>
    [Export]
    public Label descriptionLabel;

    /// <summary>
    /// the label that holds the title text of the game
    /// </summary>
    [Export]
    public Label titleLabel;

    public override void _Ready()
    {
        GuiManagerGlobal.instance.gameTitlesUpdated += () =>
        {
            gameContainer.updateGames(GuiManagerGlobal.gameTitles, showDescription);
        };

        GuiManagerGlobal.instance.tagListUpdated += () =>
        {
            tagContainer.updateTags(GuiManagerGlobal.tagList, setCurrentTag);
        };

        // hide the description if it is not already hidden
        description.Hide();

        state = GuiState.ViewGames;

        // poll the game list
        GuiManagerGlobal.instance.reloadGameList();
    }

    // unhandled to ignore input that the gui manager consumes (aka when a game is launched)
    public override void _UnhandledInput(InputEvent @event)
    {
        if (state != GuiState.Description)
        {
            // stick right
            if (@event.IsActionPressed("Player1_StickRight") || @event.IsActionPressed("Player2_StickRight"))
            {
                if (state == GuiState.ViewGames)
                {
                    showTagList();
                    AcceptEvent();
                    return;
                }
            }

            // stick left
            if (@event.IsActionPressed("Player1_StickLeft") || @event.IsActionPressed("Player2_StickLeft"))
            {
                if (state == GuiState.Tags && tagContainer.currentX == 0)
                {
                    showGameList();
                    AcceptEvent();
                    return;
                }
            }
        }

        // back button (blue button)
        if (@event.IsActionPressed("Player1_A2") || @event.IsActionPressed("Player2_A2"))
        {
            if (state == GuiState.Description)
            {
                description.Hide();
                gameContainer.selectLastPressedButton();
                state = GuiState.ViewGames;
            }
        }

        // enter button (red button)
        if (@event.IsActionPressed("Player1_A1") || @event.IsActionPressed("Player2_A1"))
        {
            if (state == GuiState.Description)
            {
                lauchCurrentGame();
            }
        }

        if (state == GuiState.ViewGames)
        {
            if (@event.IsActionPressed("Player1_StickUp") || @event.IsActionPressed("Player2_StickUp"))
            {
                gameContainer.previousGame();
            }
            if (@event.IsActionPressed("Player1_StickDown") || @event.IsActionPressed("Player2_StickDown"))
            {
                gameContainer.nextGame();
            }
        }
        if (state == GuiState.Tags)
        {
            // stick up
            if (@event.IsActionPressed("Player1_StickUp") || @event.IsActionPressed("Player2_StickUp"))
            {
                tagContainer.selectUp();
            }
            // stick down
            if (@event.IsActionPressed("Player1_StickDown") || @event.IsActionPressed("Player2_StickDown"))
            {
                tagContainer.selectDown();
            }
            // stick left
            if (@event.IsActionPressed("Player1_StickLeft") || @event.IsActionPressed("Player2_StickLeft"))
            {
                tagContainer.selectLeft();
            }
            // stick right
            if (@event.IsActionPressed("Player1_StickRight") || @event.IsActionPressed("Player2_StickRight"))
            {
                tagContainer.selectRight();
            }
        }
    }

    /// <summary>
    /// lauches the game that is referenced by the button in the aspect ratio container
    /// in the lastButtonContainerPressed variable, 
    /// this is used to lauch a game from a description page
    /// </summary>
    private void lauchCurrentGame()
    {
        DevcadeGame gameToLaunch = gameContainer.buttonsGames[gameContainer.lastButtonPressed.childButton];
        launchGame(gameToLaunch);
    }

    /// <summary>
    /// launches the given game
    /// calls the launchGame function of the model
    /// </summary>
    /// <param name="game"></param>
    private void launchGame(DevcadeGame game)
    {
        state = GuiState.GameLaunched;
        // this launches the selected game, and continues when the game closes
        GuiManagerGlobal.instance.launchGame(game).ContinueWith(_ => state = GuiState.Description);
        gameContainer.selectLastPressedButton();
    }

    /// <summary>
    /// show the description for an arbitrary game
    /// </summary>
    /// <param name="game"> the game's description to show </param>
    private void showDescription(DevcadeGame game)
    {
        state = GuiState.Description;

        titleLabel.Text = game.name;
        descriptionLabel.Text = game.description;

        description.Show();
    }

    /// <summary>
    /// show the tag list 
    /// </summary>
    public void showTagList()
    {
        state = GuiState.Tags;
        tagContainer.grabFocus();
        camera.setRelativeTargetIndex(1);
    }

    /// <summary>
    /// shows the game list
    /// </summary>
    private void showGameList()
    {
        state = GuiState.ViewGames;
        gameContainer.resetLastPressedButton();
        gameContainer.grabFocus();
        camera.setRelativeTargetIndex(0);
    }

    /// <summary>
    /// set the current tag to the tag given
    /// </summary>
    /// <param name="tag"> the new tag </param>
    public void setCurrentTag(Tag tag)
    {
        GuiManagerGlobal.instance.setTag(tag);
        showGameList();
    }
}
