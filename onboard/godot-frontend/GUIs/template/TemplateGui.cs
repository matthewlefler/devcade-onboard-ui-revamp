using Godot;
using System;

using onboard.devcade;
using System.Collections.Generic;

namespace GodotFrontend;

/// <summary>
/// this is somewhat simple GUI that implements all the basic functions 
/// to show how to create a simple UI script
/// all logic for creating buttons and adding functions to the buttons is done here
/// </summary>
public partial class TemplateGui : Control, GuiInterface
{
    GuiManager model;
    public List<DevcadeGame> gameTitles;

    /// <summary>
    /// the container that holds the buttons that represent the games
    /// which are analogous to the menucards in the old frontend 
    /// </summary>
    [Export]
    public GridContainer gameContainer;    

    /// <summary>
    /// holds the tag buttons 
    /// </summary>
    [Export]
    public GridContainer tagContainer; 

    /// <summary>
    /// the overall parent panel 
    /// that holds all the ui nodes related to the description
    /// used to hide/show the description
    /// </summary>
    [Export]
    public Panel descriptionPanel;
    
    /// <summary>
    /// the label that holds the actual description text (DevcadeGame.description)
    /// </summary>
    [Export]
    public Label desriptionLabel; 

    /// <summary>
    /// a label to show the title of the game when the description is shown
    /// </summary>
    [Export]
    public Label titleLabel;

    /// <summary>
    /// the button when showing the description that is connected to lauching the game
    /// </summary>
    [Export]
    public BaseButton lauchGameButton;

    private List<Tag> tagList = new List<Tag>();

    private bool tagListOutOfDate = false;

    /// <summary>
    /// The last pressed button's aspectratiocontainer
    /// Used for saving focus when the a description of a game is shown.
    /// </summary>
    private AspectRatioContainer lastButtonContainerPressed = null;

    /// <summary>
    /// a dictionary of the buttons' containers to games, 
    /// used for accessing a games based on the button, 
    /// which is useful in this case for when the current tag changes
    /// and it is needed to hide all the buttons/games that do not have that tag
    /// </summary>
    private Dictionary<AspectRatioContainer, DevcadeGame> gameContainers = new Dictionary<AspectRatioContainer, DevcadeGame>();

    /// <summary>
    /// this varible is used so that updating the ui elements that represent the game's 
    /// is done after the scene is instatiated, and the rest of the required nodes exist
    /// </summary>
    private bool gameListOutOfDate = false;

    int screenHeight = 0;
    int screenWidth = 0;


    /// <summary>
    /// runs once after the node is loaded into the scene tree,
    /// used in this case to set the monochrome missing texture, 
    /// and to get the screen width/height
    /// </summary>
    public override void _Ready()
    {
        descriptionPanel.Hide();
        lauchGameButton.Pressed += lauchCurrentGame;

        Vector2I screenDims = DisplayServer.ScreenGetSize();
        screenHeight = screenDims.Y;
        screenWidth = screenDims.X;
    }

    public override void _Input(InputEvent @event)
    {
        // add any input that happends once per a given keypress here

        // if the back (blue) button is pressed or the Menu (black) button is pressed
        if(@event.IsActionPressed("Player1_A2") || @event.IsActionPressed("Player1_Menu") || @event.IsActionPressed("Player2_A2") || @event.IsActionPressed("Player2_Menu"))
        {
            // and the description panel is visible, hide it
            if(descriptionPanel.IsVisibleInTree())
            {
                descriptionPanel.Hide();
                
                // make the description's game button focused again
                // this is a bit cursed and could be done in a better way,
                // but as there is only one child of each aspect ratio container
                // and it has to be a button of some kind
                // this should not fail
                (lastButtonContainerPressed.GetChild(0) as BaseButton).GrabFocus();
            }
        }
    }

    // private double timeout = 5;
    // private double timer = 0;
    public override void _Process(double delta)
    {
        // the update loop
        // delta refers to delta time in seconds
        // some examples of uses are:
        //      timers based on how long a button is held
        //      global animations
        //      in this case, update the game list if it is out of date / old

        if(gameListOutOfDate == true)
        {
            // clear the dict, as the old data is no longer current
            gameContainers = new Dictionary<AspectRatioContainer, DevcadeGame>();

            // for each game create a new texture button with the texture set to the banner if it exists
            // put it in an aspectRatioContainer so the aspect ratio is saved on scaling,
            // sets the size flags so that each aspectRatioContainer takes up the same space
            // add the button as a child to the aspectRatioContainer and that as a child to the gameContainer
            for(int i = 0; i < gameTitles.Count; i++)
            {
                DevcadeGame game = gameTitles[i];

                BaseButton button;

                if(game.banner != null)
                {
                    // this is slow, save textures some where, so they don't have to be re-calculated every time?
                    TextureButton textureButton = new TextureButton();
                    textureButton.IgnoreTextureSize = true;
                    textureButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;

                    // make a monochrome variant of the banner that is slightly darker too 
                    Texture2D monochromeBanner = makeMonochrome(game.banner);
                    monochromeBanner = changeBrightness(monochromeBanner, +0.4f);

                    // make a color variant that is darker too
                    Texture2D darkerBanner = changeBrightness(game.banner, -0.2f);

                    textureButton.TextureDisabled = darkerBanner;
                    textureButton.TextureNormal   = monochromeBanner;
                    textureButton.TextureHover    = game.banner;
                    textureButton.TexturePressed  = darkerBanner;
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

                
                button.CustomMinimumSize = new Vector2(10, 10);

                var aspectRatioContainer = new AspectRatioContainer();
                aspectRatioContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
                aspectRatioContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;

                if(game.name != "Error")
                {
                    // lambda function is required to "bind" the parameter 
                    // to the function called when the button is pressed
                    button.Pressed += () => { 
                        lastButtonContainerPressed = aspectRatioContainer; 
                        showDescription(game);
                    };
                } 

                aspectRatioContainer.AddChild(button);
                
                gameContainer.AddChild(aspectRatioContainer);

                // make the first button focused by default,
                // makes using the arrow keys and joysticks to navigate easy
                if(i == 0)
                {
                    button.CallDeferred("grab_focus");
                    lastButtonContainerPressed = aspectRatioContainer;
                }

                // add the new button and its corresponding game to the dictionary
                gameContainers.Add(aspectRatioContainer, game);
            }

            gameListOutOfDate = false;
        }

        if(tagListOutOfDate == true)
        {
            foreach(Node node in tagContainer.GetChildren())
            {
                tagContainer.RemoveChild(node);
            }

            for (int i = 0; i < tagList.Count; i++)
            {
                Tag tag = tagList[i];
                Button button = new Button();

                button.Pressed += () => setCurrentTag(tag);

                button.Text = tag.name;

                tagContainer.AddChild(button);
            }
            tagListOutOfDate = false;
        }
    }

    /// <summary>
    /// this function is called by the GuiManager.cs script, and sets/resets the games
    /// as well as saving a reference to the GuiManager.cs script so that other functions,
    /// such as the launch game function, can be run
    /// 
    /// the boolean gameListOutOfDate is used because this function is called from an async context,
    /// and Godot requires Object.CallDeferred("{function name}"); in such contexts which looked weird
    /// </summary>
    /// <param name="gameTitles"> the list of devcade games </param>
    /// <param name="model"> the manager object </param>
    public void setGameList(List<DevcadeGame> gameTitles, GuiManager model)
    {
        this.gameTitles = gameTitles;
        this.model = model;

        gameListOutOfDate = true;
    }

    /// <summary>
    /// shows the description for a game
    /// the description includes 
    /// the title, description, and a button to launch the game
    /// </summary>
    /// <param name="game"></param>
    private void showDescription(DevcadeGame game)
    {
        titleLabel.Text = game.name;
        desriptionLabel.Text = game.description;
        
        // set the action to run when the launch button is pressed
        // also make this button grab focus
        lauchGameButton.GrabFocus();

        descriptionPanel.Show();
    }
    
    /// <summary>
    /// lauches the game that is referenced by the button in the aspect ratio container
    /// in th the lastButtonContainerPressed variable, 
    /// this is used to lauch a game from a description page
    /// </summary>
    private void lauchCurrentGame()
    {
        DevcadeGame gameToLaunch = gameContainers[lastButtonContainerPressed];
        launchGame(gameToLaunch);
    }

    /// <summary>
    /// launches the given game
    /// calls the launchGame function of the model
    /// </summary>
    /// <param name="game"></param>
    private void launchGame(DevcadeGame game)
    {
        model.launchGame(game);
    }

    /// <summary>
    /// sets the current tag variable in the model to the given tag
    /// </summary>
    /// <param name="tag"> the new tag </param>
    private void setCurrentTag(Tag tag)
    {
        model.setTag(tag);
    }

    /// <summary>
    /// kills the currently running game 
    /// IMPORTANT: does not wait for the game to be killed for the function to return
    /// </summary>
    private void killCurrentlyRunningGame()
    {
        // discard the result,
        // supresses the warning that:
        // Because this call is not awaited, execution of the current method continues before the call is completed
        _ = model.killGame();
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

    /// <summary>
    /// returns a new instance of the texture with all pixels + the brightness value
    /// does not modify the transparency values, or the original texture
    /// </summary>
    /// <param name="tex"></param>
    /// <returns> a new instance of the texture with all pixels + the brightness value </returns>
    private Texture2D changeBrightness(Texture2D tex, float brightness)
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

                float r = MathF.Min(MathF.Max(pixelColor.R - brightness, 0.0f), 1.0f);
                float g = MathF.Min(MathF.Max(pixelColor.G - brightness, 0.0f), 1.0f);
                float b = MathF.Min(MathF.Max(pixelColor.B - brightness, 0.0f), 1.0f);

                image.SetPixel(x, y, new Color(r, g, b, pixelColor.A));
            }
        }

        // translate the image back into a texture object
        return ImageTexture.CreateFromImage(image);
    }

    public void setTagList(List<Tag> tags)
    {
        this.tagList = tags;
        tagListOutOfDate = true;
    }

    public void setTag(Tag tag)
    {
        // because the games do not have the allTag within their tag list, 
        // a special case is required to handle this instance
        if(tag == GuiManager.allTag)
        {
            foreach(AspectRatioContainer container in gameContainer.GetChildren())
            {
                container.Show();
            }
            return;
        }

        foreach(AspectRatioContainer container in gameContainer.GetChildren())
        {
            if(gameContainers[container].tags.Contains(tag))
            {
                container.Show();
            }
            else 
            {
                container.Hide();
            }
        }
    }
}
