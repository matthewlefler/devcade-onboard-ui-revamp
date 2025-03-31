using Godot;
using System;

using onboard.devcade;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GodotFrontend;

/// <summary>
/// this is somewhat of a template class to show how to use the GuiInterface
/// and create a simple UI
/// </summary>
public partial class TemplateGui : Control, GuiInterface
{
    GuiManager model;
    public List<DevcadeGame> gameTitles;

    [Export]
    public Control gameContainer;    

    [Export]
    public Control tabContainer;  
    
    [Export]
    public Texture2D missingTexture;
    public Texture2D missingTextureMonochrome;

    private List<Tag> tagList = new List<Tag>();

    private bool tagListOutOfDate = false;

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
        missingTextureMonochrome = makeMonochrome(missingTexture);

        Vector2I screenDims = DisplayServer.ScreenGetSize();
        screenHeight = screenDims.Y;
        screenWidth = screenDims.X;
    }

    public override void _Input(InputEvent @event)
    {
        // add any input that happends once per a given keypress here
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
        //      in this case, update the game list if it is old


        // TODO:
        // add supervisor button
        // looks like it'll require a library as 
        // the godot engine properly handles inputs and
        // does not read inputs when not in foucus
        // aka when a game is running
        // see: https://thegodotbarn.com/contributions/question/178/how-to-make-games-recognize-background-input

        // if(Input.IsActionPressed("ui_accept"))
        // {
        //     GD.Print("enter");
        //     timer -= delta;
        //     if(timer <= 0.0) 
        //     {
        //         // if the timer has timed out
        //         // kill the currently running game
        //         killCurrentlyRunningGame();
        //     }
        // }
        // else
        // {
        //     timer = timeout;
        // }

        if(gameListOutOfDate == true)
        {
            foreach(Node node in gameContainer.GetChildren())
            {
                gameContainer.RemoveChild(node);
            }
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
                    TextureButton textureButton = new TextureButton();
                    textureButton.IgnoreTextureSize = true;
                    textureButton.StretchMode = TextureButton.StretchModeEnum.Scale;

                    Texture2D monochromeBanner = makeMonochrome(game.banner);
                    monochromeBanner = changeBrightness(monochromeBanner, -0.1f);

                    Texture2D darkerBanner = changeBrightness(game.banner, -0.2f);

                    textureButton.TextureDisabled = missingTexture;
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

                if(game.name != "Error")
                {
                    // lambda function is required to "bind" the parameter 
                    // to the function called when the button is pressed
                    button.Pressed += () => launchGame(game);
                }
                
                
                button.CustomMinimumSize = new Vector2(10, 10);

                var aspectRatioContainer = new AspectRatioContainer();
                aspectRatioContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
                aspectRatioContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;

                aspectRatioContainer.AddChild(button);
                
                gameContainer.AddChild(aspectRatioContainer);

                if(i == 0)
                {
                    button.CallDeferred("grab_focus");
                }
            }

            gameListOutOfDate = false;
        }

        if(tagListOutOfDate == true)
        {
            foreach(Node node in tabContainer.GetChildren())
            {
                tabContainer.RemoveChild(node);
            }

            for (int i = 0; i < tagList.Count; i++)
            {
                Tag tag = tagList[i];
                Button button = new Button();

                button.Pressed += () => setTag(tag);

                button.Text = tag.name;

                tabContainer.AddChild(button);
            }
            tagListOutOfDate = false;
        }
    }

    /// <summary>
    /// this function is called by the Main.cs script, and sets/resets the games
    /// as well as saving a reference to the Main.cs script so that other functions,
    /// such as the launch game function, can be run
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
    private void setTag(Tag tag)
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
}
