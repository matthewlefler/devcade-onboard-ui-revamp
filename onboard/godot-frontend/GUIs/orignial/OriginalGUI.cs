using Godot;
using GodotFrontend;
using onboard.devcade;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace onboard.devcade.GUI.originalGUI;
public partial class OriginalGUI : Control, GuiInterface
{
    /// <summary>
    /// the current state of the GUI
    /// this is used to determine what actions to take based on what is on the screen
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
    /// the node that has the game buttons as children
    /// </summary>
    [Export]
    public Control gameContainer;

    /// <summary>
    /// the control node that is moved to show/hide the tag list
    /// </summary>
    [Export]
    public SlerpControl tagsMenu;

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
    /// the spacing between the game buttons in radians
    /// </summary>
    [Export]
    public float cardSpacing = 0.26f;

    /// <summary>
    /// a scalar value for the size of the game buttons
    /// </summary>
    [Export]
    public float gameButtonsScale = 1.0f;

    /// <summary>
    /// the size of the game buttons
    /// mulitplied this by the gameButtonsScale
    /// </summary>
    private Vector2 gameButtonsSize = new Vector2(470.0f, 273.0f);

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
    /// list of game buttons
    /// a wrapper class that conatins a BaseButton to animate the rotation of them
    /// </summary>
    private List<GameButton> gameButtons = new List<GameButton>();

    /// <summary>
    /// used to update the game buttons within the Process loop
    /// </summary>
    private bool updateGames = false;

    private BaseButton lastButtonPressed = null;

    /// <summary>
    /// a dictionary of buttons to games, 
    /// used for accessing a games based on the button, 
    /// which is useful in this case for when the current tag changes
    /// and it is needed to hide all the buttons/games that do not have that tag
    /// </summary>
    private Dictionary<BaseButton, DevcadeGame> buttonsGames = new Dictionary<BaseButton, DevcadeGame>();

    /// <summary>
    /// b/c signals can have many actions connected to them, but for certine actions, in this case the setFocusedGame action,
    /// we only want one, we need to keep track of the actions and remove them before adding them with different parameters again
    /// </summary>
    private Dictionary<BaseButton, Action> buttonFocusActions = new Dictionary<BaseButton, Action>();

    public override void _Ready()
    {
        // hide the description if it is not already
        description.Hide();
        // and hopefully make it appear on top of everything else
        description.ZIndex = 1000;

        // offset the tag menu by the screen width at the start
        tagsMenu.Position = new Vector2(DisplayServer.ScreenGetSize().X, 0.0f);
    }

    public override void _Input(InputEvent @event)
    {
        // back button (blue button)
        if (@event.IsAction("Player1_A2") || @event.IsAction("Player2_A2"))
        {
            if (state == GuiState.Description)
            {
                description.Hide();
                lastButtonPressed.GrabFocus();
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
            foreach (Node child in gameContainer.GetChildren())
            {
                gameContainer.RemoveChild(child);
            }

            gameButtons = new List<GameButton>();

            for (int i = 0; i < games.Count; i++)
            {
                DevcadeGame game = games[i];

                BaseButton button;

                if (game.banner != null)
                {

                    TextureButton textureButton = new TextureButton();
                    textureButton.IgnoreTextureSize = true;
                    textureButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;

                    textureButton.Name = game.name;

                    textureButton.TextureDisabled = game.banner;
                    textureButton.TextureNormal = game.banner;
                    textureButton.TextureHover = game.banner;
                    textureButton.TexturePressed = game.banner;
                    textureButton.TextureFocused = game.banner;

                    button = textureButton;
                }
                else
                {
                    Button textButton = new Button();
                    textButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                    textButton.SizeFlagsVertical = SizeFlags.ExpandFill;

                    // textButton.Theme = tagButtonTheme;


                    textButton.Name = game.name;

                    textButton.Text = game.name;

                    button = textButton;
                }

                // size of game buttons
                button.CustomMinimumSize = gameButtonsSize * gameButtonsScale;

                // pivot, inital rotation, and z-index (what should be draw on-top of what)
                button.PivotOffset = new Vector2(0, gameButtonsSize.Y / 2 * gameButtonsScale);
                button.Rotation = i * cardSpacing;
                button.ZIndex = games.Count - i;

                if (game.name != "Error")
                {
                    // lambda function is required to "bind" the game parameter 
                    // to the function showDescription called when the button is pressed
                    button.Pressed += () =>
                    {
                        lastButtonPressed = button;
                        button.ReleaseFocus();
                        showDescription(game);
                    };
                }

                // add the new button to the game container and the list of game buttons
                gameContainer.AddChild(button);
                gameButtons.Add(new GameButton(i * cardSpacing, button));

                if (i > 5)
                {
                    button.Hide();
                }

                buttonsGames.Add(button, game);
            }

            setUpButtons();

            updateGames = false;
        }

        if (updateTags)
        {
            // passes the new tag list and the function that the tags 
            // should execute to the tag container
            tagContainer.updateTags(tagList, setCurrentTag);

            updateTags = false;
        }

        // update each GameButton so that the animations play out
        foreach (GameButton gameButton in gameButtons)
        {
            gameButton.process(delta);
        }
    }

    /// <summary>
    /// sets the neighbor values and the focus actions
    /// </summary>
    private void setUpButtons()
    {
        var currentGames = gameContainer.GetChildren();

        for (int i = 0; i < currentGames.Count; i++)
        {
            BaseButton button = currentGames[i] as BaseButton;

            // so uhh, simply put lambda functions capture by reference by default
            // so a copy of the int i is needed 
            // see the answers in: https://stackoverflow.com/questions/451779/how-to-tell-a-lambda-function-to-capture-a-copy-instead-of-a-reference-in-c
            int iCopy = i;
            Action focusAction = () =>
            {
                setFocusedGame(iCopy);
            };

            if (buttonFocusActions.ContainsKey(button))
            {
                // remove the previous action
                button.FocusEntered -= buttonFocusActions[button];

                // add the new one
                button.FocusEntered += focusAction;

                // and save it in the dictionary
                buttonFocusActions[button] = focusAction;
            }
            else
            {
                // if this is the first instance of this button
                // add it to the focus actions dictionary and set the action
                buttonFocusActions.Add(button, focusAction);
                button.FocusEntered += focusAction;
            }

            // then set the default focus neighbor node paths
            // so that going left/right/up/down doesn't go to random unintended places
            button.FocusNeighborTop = button.GetPath();
            button.FocusNeighborLeft = button.GetPath();
            button.FocusNeighborRight = button.GetPath();
            button.FocusNext = button.GetPath();
            button.FocusPrevious = button.GetPath();
            button.FocusNeighborBottom = button.GetPath();

            // skipping the first button,
            // override the rest to set the top and bottom neighbors
            if (i != 0)
            {
                BaseButton aboveButton = currentGames[i - 1] as BaseButton;
                button.FocusNeighborTop = aboveButton.GetPath();
                aboveButton.FocusNeighborBottom = button.GetPath();
            }

            // make the first button focused by default,
            // makes using the arrow keys and joysticks to navigate easy
            if (i == 0)
            {
                button.CallDeferred("grab_focus");
                lastButtonPressed = button;
            }
        }
    }


    /// <summary>
    /// lauches the game that is referenced by the button in the aspect ratio container
    /// in th the lastButtonContainerPressed variable, 
    /// this is used to lauch a game from a description page
    /// </summary>
    private void lauchCurrentGame()
    {
        DevcadeGame gameToLaunch = buttonsGames[lastButtonPressed];
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
        model.launchGame(game).ContinueWith(_ => state = GuiState.Description);
    }

    /// <summary>
    /// each button has this set for their on focus gained action
    /// </summary>
    /// <param name="index"> the index of the button </param>
    private void setFocusedGame(int index)
    {
        var currentGames = gameContainer.GetChildren();

        // loops over all the game buttons in the saved list
        // skiping the ones that are not part of the current tag (aka no in the tree)
        // and setting the z-index and rotation of the buttons based on the integer i
        // which represents the "index" of the button in the game container (0 being the first/top one)
        int i = 0;
        foreach (GameButton gameButton in gameButtons)
        {
            if (!gameButton.isInsideTree)
            {
                continue;
            }

            int offset = Math.Abs(i - index);

            gameButton.childButton.ZIndex = currentGames.Count - offset;
            gameButton.targetRotation = (i - index) * cardSpacing;

            ++i;
        }
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
        lastButtonPressed = gameButtons[0].childButton;
        gameContainer.Position = new Vector2(-DisplayServer.ScreenGetSize().X, 0.0f);

        tagsMenu.targetPosition = new Vector2(0.0f, 0.0f);
    }

    /// <summary>
    /// hides the tag list
    /// </summary>
    private void hideTagList()
    {
        state = GuiState.ViewGames;

        lastButtonPressed.GrabFocus();

        gameContainer.Position = new Vector2(0.0f, 0.0f);

        tagsMenu.targetPosition = new Vector2(DisplayServer.ScreenGetSize().X, 0.0f);
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

    /// <summary>
    /// used by the GuiManager to set the tag
    /// </summary>
    /// <param name="tag"></param>
    public void setTag(Tag tag)
    {
        // remove all the buttons 
        foreach (Node child in gameContainer.GetChildren())
        {
            gameContainer.RemoveChild(child);
        }

        // add back the ones that have the tag
        gameButtons.ForEach(buttonWrapper =>
        {
            DevcadeGame game = buttonsGames[buttonWrapper.childButton];

            if (game.tags.Contains(tag))
            {
                gameContainer.AddChild(buttonWrapper.childButton);
            }
        });

        setUpButtons();
    }

    public void setTagList(List<Tag> tags)
    {
        this.tagList = tags;
        updateTags = true;
    }
}
