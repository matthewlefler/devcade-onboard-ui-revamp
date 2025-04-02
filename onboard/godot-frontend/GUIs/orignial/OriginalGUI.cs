using Godot;
using GodotFrontend;
using onboard.devcade;
using System;
using System.Collections.Generic;

public partial class OriginalGUI : Control, GuiInterface
{
    private GuiState state = GuiState.ViewGames;
    
    public enum GuiState
    {
        ViewGames, // the game list is shown
        Description, // the description of a game is shown
        Tags, // the tag list is shown
        Theme, // the theme picker is shown
        GameLaunched // a game is being run
    }

    [Export]
    public Control gameContainer;

    [Export]
    public GridContainer tagContainer;

    [Export]
    public CanvasItem description;

    [Export]
    public Label descriptionLabel;

    [Export]
    public Label titleLabel;

    [Export]
    public float cardSpacing = 0.26f;

    [Export]
    public float moveTime = 0.5f;

    [Export]
    public Vector2 gameButtonsSize = new Vector2(100, 100);

    GuiManager model = null;
    List<DevcadeGame> games = new List<DevcadeGame>();
    List<Tag> tagList = new List<Tag>();

    List<BaseButton> gameButtons = new List<BaseButton>();

    bool updateGames = false;

    BaseButton lastButtonPressed = null;

    /// <summary>
    /// a dictionary of buttons to games, 
    /// used for accessing a games based on the button, 
    /// which is useful in this case for when the current tag changes
    /// and it is needed to hide all the buttons/games that do not have that tag
    /// </summary>
    private Dictionary<BaseButton, DevcadeGame> buttonsGames = new Dictionary<BaseButton, DevcadeGame>();

    public override void _Ready()
    {
        description.Hide();
        description.ZIndex = 1000;
    }

    public override void _Input(InputEvent @event)
    {
        if(@event.IsAction("Player1_A2") || @event.IsAction("Player2_A2"))
        {
            if(description.IsVisibleInTree())
            {
                description.Hide();
                lastButtonPressed.GrabFocus();
                state = GuiState.ViewGames;
            }
        }

        if(@event.IsAction("Player1_A1") || @event.IsAction("Player2_A1"))
        {
            if(description.IsVisibleInTree())
            {
                lauchCurrentGame();
            }
        }

        if(@event.IsAction("Player1_StickRight") || @event.IsAction("Player2_StickRight"))
        {
            
        }

        if(@event.IsAction("Player1_StickLeft") || @event.IsAction("Player2_StickLeft"))
        {
            if(!description.IsVisibleInTree())
            {

            }
        }
    }

    public override void _Process(double delta)
    {
        if(updateGames)
        {
            foreach(Node child in gameContainer.GetChildren())
            {
                gameContainer.RemoveChild(child);
            }

            gameButtons = new List<BaseButton>();

            for(int i = 0; i < games.Count; i++)
            {
                DevcadeGame game = games[i];

                BaseButton button;

                if(game.banner != null)
                {
                    TextureButton textureButton = new TextureButton();
                    textureButton.IgnoreTextureSize = true;
                    textureButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;

                    Texture2D monochromeBanner = makeMonochrome(game.banner);

                    textureButton.TextureDisabled = game.banner;
                    textureButton.TextureNormal   = monochromeBanner;
                    textureButton.TextureHover    = game.banner;
                    textureButton.TexturePressed  = game.banner;
                    textureButton.TextureFocused  = game.banner;

                    button = textureButton;
                }
                else
                {
                    Button textButton = new Button();
                    textButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                    textButton.SizeFlagsVertical = SizeFlags.ExpandFill;

                    textButton.Text = game.name;

                    button = textButton;
                }

                // size of game buttons
                button.CustomMinimumSize = gameButtonsSize;

                // pivot, inital rotation, and z-index (what should be draw on-top of what)
                button.PivotOffset = new Vector2(0, gameButtonsSize.Y / 2);
                button.Rotation = i * cardSpacing;
                button.ZIndex = games.Count - i;

                if(game.name != "Error")
                {
                    // lambda function is required to "bind" the game parameter 
                    // to the function showDescription called when the button is pressed
                    button.Pressed += () => { 
                        lastButtonPressed = button; 
                        showDescription(game);
                    };
                } 
                
                // so uhh, simply put lambda functions capture by reference by default
                // so a copy of the int i is needed 
                // see: https://stackoverflow.com/questions/451779/how-to-tell-a-lambda-function-to-capture-a-copy-instead-of-a-reference-in-c
                int iCopy = i;
                button.FocusEntered += () => {
                    setFocusedGame(iCopy);
                };

                // make the first button focused by default,
                // makes using the arrow keys and joysticks to navigate easy
                if(i == 0)
                {
                    button.CallDeferred("grab_focus");
                    lastButtonPressed = button;
                }

                // add the new button to the game container and the list of game buttons
                gameContainer.AddChild(button);
                gameButtons.Add(button);

                // then set the focus neighbor node paths
                button.FocusNeighborTop = button.GetPath();
                button.FocusNeighborLeft = button.GetPath();
                button.FocusNeighborRight = button.GetPath();
                button.FocusNext = button.GetPath();
                button.FocusPrevious = button.GetPath();
                button.FocusNeighborBottom = button.GetPath();
                if(i != 0)
                {
                    button.FocusNeighborTop = gameButtons[i - 1].GetPath();
                    gameButtons[i - 1].FocusNeighborBottom = button.GetPath();
                }

                buttonsGames.Add(button, game);
            }

            updateGames = false;
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
        model.launchGame(game);
    }

    private void setFocusedGame(int index)
    {
        for (int i = 0; i < gameButtons.Count; i++)
        {
            gameButtons[i].ZIndex = gameButtons.Count - Math.Abs(i - index);
            gameButtons[i].Rotation = (i - index) * cardSpacing;
        }
    }

    private void showDescription(DevcadeGame game)
    {
        state = GuiState.Description;
        
        titleLabel.Text = game.name;
        descriptionLabel.Text = game.description;
        
        description.Show();
    }

    public void showTagList(BaseButton button)
    {
        state = GuiState.Tags;
        lastButtonPressed = button;
        gameContainer.Hide();
    }

    private void hideTagList()
    {
        state = GuiState.ViewGames;

        gameContainer.Show();
        lastButtonPressed.GrabFocus();
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
    public void updateCurrentTag(Tag tag)
    {
        model.setTag(tag);
    }

    /// <summary>
    /// used by the GuiManager to set tag
    /// </summary>
    /// <param name="tag"></param>
    public void setTag(Tag tag)
    {
        gameButtons.ForEach(button => {
            DevcadeGame game = buttonsGames[button];
            if(!game.tags.Contains(tag)) {
                button.Hide();
            }
        });
    }

    public void setTagList(List<Tag> tags)
    {
        this.tagList = tags;   
    }  
    
    
    /// <summary>
    /// returns a new instance of the texture with all pixels set to the monochrome space
    /// does not modify the transparency values, or the original texture
    /// </summary>
    /// <param name="tex"></param>
    /// <returns> a new instance of the texture with all pixels in the monochrome space </returns>
    private Texture2D makeMonochrome(Texture2D tex)
    {
        // to modify a texture, it is reqiured that 
        // we get an image object first.
        // this is somewhat expensive as it gets the texture from the gpu,
        // so use it sparingly 
        Image image = tex.GetImage();

        int height = image.GetHeight();
        int width = image.GetWidth();

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixelColor = image.GetPixel(x, y);

                float avg = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0f;

                image.SetPixel(x, y, new Color(avg, avg, avg, pixelColor.A));
            }
        }

        // translate the image back into a texture object
        return ImageTexture.CreateFromImage(image);
    } 
}
