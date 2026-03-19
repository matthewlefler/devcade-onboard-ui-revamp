using Godot;
using System;
using System.Collections.Generic;

namespace onboard.devcade.GUI.originalGUI;

public partial class GamesContainer : Control
{
    /// <summary>
    /// the last game button that has been pressed
    /// </summary>
    internal GameButton lastButtonPressed = null;

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

    /// <summary>
    /// the percent of effect that the offset index has on the size of the game cards
    /// aka this changes the amount that the closer a card gets to the center, the larger it gets
    /// </summary>
    [Export]
    public float cardScaleAmount = 0.3f;

    [Export]
    public float percentYPositionValue = 66.0f;

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

    public int index {get; private set;} = 0;
    public int numberOfGames {get; private set;} = 1;

    public override void _Ready()
    {
        this.Position = new Vector2(0.0f, GetViewportRect().Size.Y * percentYPositionValue / 100.0f);
    }

    public override void _Process(double delta)
    {
        // update each GameButton so that the animations play out
        foreach (GameButton gameButton in gameButtons)
        {
            gameButton.process(delta);
        }
    }

    public void nextGame()
    {
        ++index;
        if(index > numberOfGames - 1)
        {
            index = numberOfGames - 1;
        }
        setFocusedGame(index);
    }

    public void previousGame()
    {
        index = --index;
        if(index < 0)
        {
            index = 0;
        }
        setFocusedGame(index);
    }

    public void updateGames(List<DevcadeGame> games, Action<DevcadeGame> showDescription)
    {
        foreach (Node child in this.GetChildren())
        {
            this.CallDeferred(Node.MethodName.AddChild, child);
        }

        this.numberOfGames = games.Count;

        gameButtons = new List<GameButton>();

        for (int i = 0; i < games.Count; i++)
        {
            DevcadeGame game = games[i];

            BaseButton button;

            if (game.banner != null)
            {

                TextureButton textureButton = new TextureButton
                {
                    IgnoreTextureSize = true,
                    StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,

                    Name = game.name,

                    TextureDisabled = game.banner,
                    TextureNormal = game.banner,
                    TextureHover = game.banner,
                    TexturePressed = game.banner,
                    TextureFocused = game.banner,

                    // dont use inbuilt navigation 
                    FocusMode = FocusModeEnum.Click,

                    CustomMinimumSize = gameButtonsSize * gameButtonsScale,
                    Scale = new Vector2(gameButtonsScale, gameButtonsScale),
                };

                button = textureButton;
            }
            else
            {
                Button textButton = new Button
                {
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                    SizeFlagsVertical = SizeFlags.ExpandFill,

                    // textButton.Theme = tagButtonTheme;

                    Name = game.name,
                    Text = game.name,

                    // dont use inbuilt navigation 
                    FocusMode = FocusModeEnum.Click,

                    // size of game buttons
                    CustomMinimumSize = gameButtonsSize * gameButtonsScale,
                    Scale = new Vector2(gameButtonsScale, gameButtonsScale),
                };

                button = textButton;
            }

            // pivot, inital rotation, and z-index (what should be draw on top of what)
            button.PivotOffset = new Vector2(0, gameButtonsSize.Y / 2 * gameButtonsScale);
            button.Rotation = i * cardSpacing;
            button.ZIndex = games.Count - i;

            GameButton gameButton = new GameButton(i * cardSpacing, button, i);
            // skip error games
            if (game.name != "Error")
            {
                // lambda function is required to "bind" the game parameter 
                // to the function showDescription called when the button is pressed
                button.Pressed += () =>
                {
                    lastButtonPressed = gameButton;
                    button.ReleaseFocus();
                    showDescription(game);
                };
            }

            // add the new button to the game container and the list of game buttons
            this.CallDeferred("add_child", button);
            gameButtons.Add(gameButton);

            if (i > 5)
            {
                button.CallDeferred(CanvasItem.MethodName.Hide);
            }

            buttonsGames.Add(button, game);
        }

        lastButtonPressed = gameButtons[0];
    }

    /// <summary>
    /// each button has this set for their on focus gained action
    /// </summary>
    /// <param name="index"> the index of the button </param>
    private void setFocusedGame(int index)
    {
        var currentGames = this.GetChildren();

        // loops over all the game buttons in the saved list
        // skiping the ones that are not part of the current tag (aka not in the tree)
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

            if(i - index == 0)
            {
                gameButton.childButton.GrabFocus();
            }

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
        int i = 0;
        gameButtons.ForEach(buttonWrapper =>
        {
            DevcadeGame game = buttonsGames[buttonWrapper.childButton];

            if (game.tags.Contains(tag))
            {
                this.CallDeferred("add_child", buttonWrapper.childButton);
                buttonWrapper.index = i;
                ++i;
            }
        });
    }

    /// <summary>
    /// Sets the focus to the last pressed button
    /// </summary>
    public void grabFocus()
    {
        setFocusedGame(lastButtonPressed.index);
    }

    /// <summary>
    /// Sets the last pressed button to a button with an arbitrary index in the gameButtons list
    /// </summary>
    /// <param name="index">must be within or equal to the length of gameButtons and 0</param>
    public void setLastPressedButton(int index)
    {
        lastButtonPressed = gameButtons[index];
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
    public void selectLastPressedButton()
    {
        setFocusedGame(lastButtonPressed.index);
    }

}
