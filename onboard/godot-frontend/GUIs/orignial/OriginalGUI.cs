using Godot;
using GodotFrontend;
using System.Collections.Generic;

namespace onboard.devcade.GUI.originalGUI;
public partial class OriginalGUI : Control, GuiInterface
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
    /// the control node that is moved to show/hide the tag list
    /// </summary>
    [Export]
    public TagContainer tagsMenu;

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

    /// <summary>
    /// the object that interfaces with the client to get the game list, tag list, etc
    /// the model, assuming a mvc like pattern
    /// </summary>
    private GuiManager model = null;

    /// <summary>
    /// the list of games
    /// </summary>
    private List<DevcadeGame> games = new List<DevcadeGame>();

    /// <summary>
    /// the list of tags
    /// </summary>
    private List<Tag> tagList = new List<Tag>();

    /// <summary>
    /// used to update the tags within the Process loop
    /// </summary>
    private bool updateTags = false;

    /// <summary>
    /// used to update the game buttons within the Process loop
    /// </summary>
    private bool updateGames = false;

    public override void _Ready()
    {
        // hide the description if it is not already
        description.Hide();
        // and hopefully make it appear on top of everything else
        description.ZIndex = 1000;
    }

    public override void _Input(InputEvent @event)
    {
        // back button (blue button)
        if (@event.IsAction("Player1_A2") || @event.IsAction("Player2_A2"))
        {
            if (state == GuiState.Description)
            {
                description.Hide();
                gameContainer.lastButtonPressed.GrabFocus();
                state = GuiState.ViewGames;
            }
        }

        // enter button (red button)
        if (@event.IsAction("Player1_A1") || @event.IsAction("Player2_A1"))
        {
            if (state == GuiState.Description)
            {
                lauchCurrentGame();
            }
        }

        // stick right
        if (@event.IsAction("Player1_StickRight") || @event.IsAction("Player2_StickRight"))
        {
            if (state == GuiState.ViewGames)
            {
                showTagList();
            }
        }

        // stick left
        if (@event.IsAction("Player1_StickLeft") || @event.IsAction("Player2_StickLeft"))
        {
            if (state == GuiState.Tags)
            {
                hideTagList();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (updateGames)
        {
            gameContainer.updateGames(games, showDescription);

            updateGames = false;
        }

        if (updateTags)
        {
            // passes the new tag list and the function that the tags 
            // should execute to the tag container
            tagContainer.updateTags(tagList, setCurrentTag);

            updateTags = false;
        }
    }


    /// <summary>
    /// lauches the game that is referenced by the button in the aspect ratio container
    /// in the lastButtonContainerPressed variable, 
    /// this is used to lauch a game from a description page
    /// </summary>
    private void lauchCurrentGame()
    {
        DevcadeGame gameToLaunch = gameContainer.buttonsGames[gameContainer.lastButtonPressed];
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
        model.launchGame(game).ContinueWith(_ => state = GuiState.Description);
    }

    /// <summary>
    /// show the description for an arbitrary game
    /// </summary>
    /// <param name="game"></param>
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
        gameContainer.resetLastPressedButton();
        camera.moveRight();
    }

    /// <summary>
    /// hides the tag list
    /// </summary>
    private void hideTagList()
    {
        state = GuiState.ViewGames;

        gameContainer.grabFocus();

        camera.moveLeft();
    }

    public void setGameList(List<DevcadeGame> gameTitles, GuiManager model)
    {
        this.model = model;
        games = gameTitles;

        updateGames = true;
    }

    /// <summary>
    /// set the current tag to the tag given
    /// </summary>
    /// <param name="tag"> the new tag </param>
    public void setCurrentTag(Tag tag)
    {
        model.setTag(tag);
    }

    public void setTagList(List<Tag> tags)
    {
        this.tagList = tags;
        updateTags = true;
    }

    /// <summary>
    /// used by the GuiManager to set the tag
    /// </summary>
    /// <param name="tag"> the new tag </param>
    public void setTag(Tag tag)
    {
        gameContainer.setTag(tag);
    }
}
