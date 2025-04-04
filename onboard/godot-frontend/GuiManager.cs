using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using log4net;

using onboard.devcade;
using onboard.util;

using Godot;
using System.Threading;

namespace GodotFrontend;

public partial class GuiManager : Control
{
    /// <summary>
    /// the initial GUI scene to show when devcade starts up
    /// </summary>
    [Export]
    public PackedScene initialGuiScene;

    /// <summary>
    /// the root node of the loading screen
    /// the control node to hide when not showing the loading animation
    /// </summary>
    [Export]
    public Control loadingScreen;

    /// <summary>
    /// the node to animate when showing the loading animation
    /// </summary>
    [Export]
    public AnimatedSprite2D loadingAnimation;

    // logger for currently unknown purposes??
    //
    // deos not run, writes an error msg, but does not block output:
    // log4net:ERROR Exception while reading ConfigurationSettings. Check your .config file is well formed XML. log4net:ERROR Exception while reading ConfigurationSettings. Check your .config file is well formed XML.
    //
    private static ILog logger = LogManager.GetLogger("onboard.ui.Devcade");

    /// <summary>
    /// A list of all the games
    /// </summary>
    public List<DevcadeGame> gameTitles;

    //////////
    // TAGS //
    //////////

    /// <summary>
    /// a list of all the tags
    /// </summary>
    public List<Tag> tagList = new List<Tag>() { allTag };
    
    /// <summary>
    /// A dictionary of tags to lists of games that have that tag
    /// </summary>
    private Dictionary<string, List<DevcadeGame>> tagLists = new Dictionary<string, List<DevcadeGame>>();
    public readonly static Tag allTag = new Tag("All Games", "View all available games");
    public Tag currentTag = allTag;

    /// <summary>
    /// true if the game list is being reloaded from the backend
    /// </summary>
    public bool reloadingGameList { get; private set; } = false;

    /// <summary>
    /// the current GUI scene being shown
    /// </summary>
    GuiInterface guiScene;

    /// <summary>
    /// the root node of the GUI scene
    /// </summary>
    CanvasItem guiSceneRootNode;

    private static readonly DevcadeGame defaultGame = new DevcadeGame {
        name = "Error",
        description = "There was a problem loading games from the API. Please check the logs for more information.",
        id = "error",
        author = "None",
    };

    // the game list is set to this if an error conditions is encountered 
    private static readonly List<DevcadeGame> errorList = new List<DevcadeGame> { defaultGame }; 

    /// <summary>
    /// A godot specific function that is ran once after this node is initialized 
    /// </summary>
    public override void _Ready()
    {
        // hide the loading screen by default
        hideLoadingAnimation();

        // spawn initial gui scene
        this.guiSceneRootNode = initialGuiScene.Instantiate() as CanvasItem;

        if(guiSceneRootNode == null)
        {
            GD.PrintErr("Assert Failed: gui scene root node is not a node that derives from the CanvasItem node");
            throw new ApplicationException("Assert Failed: gui scene root node is not a node that derives from the CanvasItem node");
        }
        
        // make sure it implements the GuiInterface interface
        GuiInterface gui = guiSceneRootNode as GuiInterface;
        if(gui != null) 
        {
            this.guiScene = gui;
        }
        else
        {
            GD.PrintErr("Assert Failed: gui scene root node script does not implement the GuiInterface interface");
            throw new ApplicationException("Assert Failed: the gui scene's root node script does not implement the GuiInterface interface");
        } 
        
        // add the new scene instance as a child of this node
        AddChild(guiSceneRootNode);

        // and reload the game list
        reloadGameList();
    }

    public override void _Process(double delta)
    {
        // frontend reset button, reloads all the games from the backend
        if(Input.IsActionPressed("Player1_Menu") && Input.IsActionPressed("Player2_Menu"))
        {
            reloadGameList();
        }

        // switch between dev and normal mode
        if(Input.IsActionPressed("Player1_B4") && Input.IsActionPressed("Player2_B4"))
        {
            Client.setProduction(!Client.isProduction).ContinueWith(_ => { setTag(allTag); reloadGameList(); });
        }

        // TODO:
        // add supervisor button (pt. 2 lol)
        // looks like it'll require a library as 
        // the godot engine properly handles inputs and
        // does not read inputs when not in foucus
        // aka when a game is running
        // see: https://thegodotbarn.com/contributions/question/178/how-to-make-games-recognize-background-input

        // if(Input.IsActionPressed("Player1_Menu") && Input.IsActionPressed("Player2_Menu"))
        // {
        //     GD.Print("timing out: " + timer)
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
    }

    /// <summary>
    /// the constructor of a class will run before the _Ready() function is called
    /// </summary>
    public GuiManager()
    {
        // load the .env file (contains the enviorment variables)
        Env.load("../.env");

        // start client (backend networked communicator)
        Client.init();
    }

    private Task reloadGameList()
    {
        showLoadingAnimation();        

        this.reloadingGameList = true;

        tagLists = new Dictionary<string, List<DevcadeGame>> { { allTag.name, new List<DevcadeGame>() } };
        tagList = new List<Tag>() { allTag };

        gameTitles = errorList;

        Task gameTask = Client.getGameList()
            .ContinueWith(t => {
                if (!t.IsCompletedSuccessfully) {
                    logger.Error($"Failed to fetch game list: {t.Exception}");
                    GD.Print($"Failed to fetch game list: {t.Exception}"); 
                    gameTitles = errorList;
                    return;
                }

                var res = t.Result.into_result<List<DevcadeGame>>();
                if (!res.is_ok()) {
                    logger.Error($"Failed to fetch game list: {res.err().unwrap()}");
                    GD.Print($"Failed to fetch game list: {res.err().unwrap()}"); 
                    gameTitles = errorList;
                    return;
                }

                logger.Info("Got game list, setting titles");
                GD.Print("Got game list, setting titles");

                gameTitles = res.unwrap();

                // each game does not have the "all tag"
                // adding it removes the requirement for an extra condition in each gui's code
                gameTitles.ForEach(game => {
                    game.tags.Add(allTag);
                });
            })
            .ContinueWith(_ => {
                logger.Info("Setting cards");
                GD.Print("setting cards");

                loadBanners();

                // initialize the GUI
                initGUI();

                hideLoadingAnimation();        

                this.reloadingGameList = false;
            });
        return gameTask;
    }

    private void loadBanners()
    {
        foreach(DevcadeGame game in gameTitles)
        {            
            // Start downloading the textures
            if (game.id != "error") {
                // don't download the banner for the default game
                Client.downloadBanner(game.id);
            } // check if /tmp/ has the banner
            
            string bannerPath = $"{Env.get("DEVCADE_PATH").unwrap_or_else(() => Env.get("HOME").unwrap() + "/.devcade")}/{game.id}/banner.png";

            if (File.Exists(bannerPath)) {
                try {
                    // godot image class 
                    Image image = Image.LoadFromFile(bannerPath);
                    ImageTexture texture = ImageTexture.CreateFromImage(image); // inherits from godot texture2D class 

                    game.banner = texture;
                }
                catch (Exception e) {
                    logger.Warn($"Unable to set card: {e.Message}");
                    GD.Print($"Unable to set card: {e.Message}");
                }
            }

            // for each tag that this game has, add it to the corresponding list
            // this allows for easy filtering by tag
            foreach(Tag tag in game.tags) {
                // if the tag does not exist in the dictionary, 
                // init it as an empty list
                if(!tagLists.ContainsKey(tag.name))
                {
                    tagLists.Add(tag.name, new List<DevcadeGame>());
                }
                tagLists[tag.name].Add(game);

                // if the overall tag list does not contain the tag
                // add it to the list
                if(!tagList.Contains(tag))
                {
                    tagList.Add(tag);
                }
            }
        }
    }

    /// <summary>
    /// Shows the loading animation,
    /// this hides the GUI, 
    /// shows the node that is the root of the animation tree,
    /// and starts the animation.
    /// </summary>
    public void showLoadingAnimation()
    {
        loadingScreen.CallDeferred("show");
        loadingAnimation.CallDeferred("play", "default", 1.0f, false);
    }

    /// <summary>
    /// Hides the loading animation,
    /// this shows the GUI, 
    /// hides the node that is the root of the animation tree,
    /// and stops the animation.
    /// </summary>
    public void hideLoadingAnimation()
    {
        loadingScreen.CallDeferred("hide");
        loadingAnimation.CallDeferred("stop");
    }

    /// <summary>
    /// updates the current tag in use
    /// and updates the game list to be only the games that have the current tag
    /// </summary>
    /// <param name="newTag"></param>
    public void setTag(Tag newTag)
    {
        currentTag = newTag;
        guiScene.setTag(currentTag);
    }

    /// <summary>
    /// initilizes a gui object with the taglist and game list
    /// </summary>
    /// <exception cref="ApplicationException"> throws if the current GUI does not implement the GuiInterface </exception>
    private void initGUI()
    {
        guiScene.setGameList(tagLists[currentTag.name], this);
        guiScene.setTagList(tagList);
    }

    /// <summary>
    /// lauch the given game
    /// </summary>
    /// <param name="game"> the game to launch </param>
    public void launchGame(DevcadeGame game) 
    {
        this.reloadingGameList = true;
        GD.Print("launching game: " + game.name);

        showLoadingAnimation();

        Client.launchGame(
            game.id).ContinueWith(res => {
                if (res.IsCompletedSuccessfully) {
                    // runs after the game completes running
                    hideLoadingAnimation();
                }
                else {
                    logger.Error("Failed to launch game: " + res.Exception);
                    GD.Print("Failed to launch game: " + res.Exception);
                    this.reloadingGameList = false;
                }
        });
    }

    /// <summary>
    /// kill the currently running game.
    /// will run async, but can await the function call
    /// </summary>
    public async Task killGame()
    {
        await Client.killGame();
    }
}
