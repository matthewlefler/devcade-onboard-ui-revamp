using Godot;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;

namespace onboard.devcade.GUI.originalGUI;

public partial class GamesContainer : Control
{
    /// <summary>
    /// the last game button that has been pressed
    /// </summary>
    internal BaseButton lastButtonPressed = null;

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
    /// the spacing between the game buttons in radians
    /// </summary>
    [Export]
    public float cardSpacing = 0.4f;

    [Export]
    public float cardScaleAmount = 0.8f;

    /// <summary>
    /// list of game buttons
    /// a wrapper class that conatins a BaseButton to animate the rotation of them
    /// </summary>
    private List<GameButton> gameButtons = new List<GameButton>();

    /// <summary>
    /// a dictionary of buttons to games, 
    /// used for accessing a games based on the button, 
    /// which is useful in this case for when the current tag changes
    /// and it is needed to hide all the buttons/games that do not have that tag
    /// </summary>
    internal Dictionary<BaseButton, DevcadeGame> buttonsGames = new Dictionary<BaseButton, DevcadeGame>();

    /// <summary>
    /// b/c signals can have many actions connected to them, but for certine actions, in this case the setFocusedGame action,
    /// we only want one, we need to keep track of the actions and remove them before adding them with different parameters again
    /// </summary>
    private Dictionary<BaseButton, Action> buttonFocusActions = new Dictionary<BaseButton, Action>();

    public override void _Ready()
    {
        this.Position = new Vector2(0.0f, GetViewportRect().Size.Y / 2.0f);
    }

    public override void _Process(double delta)
    {
        // update each GameButton so that the animations play out
        foreach (GameButton gameButton in gameButtons)
        {
            gameButton.process(delta);
        }
    }

    public void updateGames(List<DevcadeGame> games, Action<DevcadeGame> showDescription)
    {
        foreach (Node child in this.GetChildren())
        {
            this.RemoveChild(child);
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
            this.AddChild(button);
            gameButtons.Add(new GameButton(i * cardSpacing, button));

            if (i > 5)
            {
                button.Hide();
            }

            buttonsGames.Add(button, game);
        }

        setUpButtons();
    }

    /// <summary>
    /// sets the neighbor values and the focus actions
    /// </summary>
    private void setUpButtons()
    {
        var currentGames = this.GetChildren();

        for (int i = 0; i < currentGames.Count; i++)
        {
            BaseButton button = currentGames[i] as BaseButton;

            // simply put lambda functions capture by reference by default
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
    /// each button has this set for their on focus gained action
    /// </summary>
    /// <param name="index"> the index of the button </param>
    private void setFocusedGame(int index)
    {
        var currentGames = this.GetChildren();

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

            float scaleFactor = -cardScaleAmount * offset / 10.0f;
            gameButton.childButton.Scale = new Vector2(gameButtonsScale + scaleFactor, gameButtonsScale + scaleFactor);

            ++i;
        }
    }

    /// <summary>
    /// used by the GuiManager to set the tag
    /// </summary>
    /// <param name="tag"> the new tag </param>
    public void setTag(Tag tag)
    {
        // remove all the buttons 
        foreach (Node child in this.GetChildren())
        {
            this.RemoveChild(child);
        }

        // add back the ones that have the tag
        gameButtons.ForEach(buttonWrapper =>
        {
            DevcadeGame game = buttonsGames[buttonWrapper.childButton];

            if (game.tags.Contains(tag))
            {
                this.AddChild(buttonWrapper.childButton);
            }
        });

        setUpButtons();
    }

    /// <summary>
    /// Sets the focus to the last pressed button
    /// </summary>
    public void grabFocus()
    {
        lastButtonPressed.GrabFocus();
    }

    /// <summary>
    /// Sets the last pressed button to the first button
    /// </summary>
    public void resetLastPressedButton()
    {
        setLastPressedButton(0);
    }

    /// <summary>
    /// Sets the last pressed button to a button with an arbitrary index in the gameButtons list
    /// </summary>
    /// <param name="index">must be within or equal to the length of gameButtons and 0</param>
    public void setLastPressedButton(int index)
    {
        lastButtonPressed = gameButtons[index].childButton;
    }

}
