using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using log4net;

using onboard.util;

using Godot;
using onboard.util.supervisor_button;

namespace onboard.devcade.GUI;

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

    /// <summary>
    /// the root node of the screen saver
    /// the control node to hide when not showing the screen saver
    /// </summary>
    [Export]
    public Control screenSaver;

    /// <summary>
    /// the node to animate when showing the screen saver animation
    /// </summary>
    [Export]
    public AnimatedSprite2D screenSaverAnimation;

    /// <summary>
    /// true if the screensaver animation is being shown
    /// </summary>
    public bool showingScreenSaverAnimation { get; private set; } = false;

    /// <summary>
    /// logger related to this class
    /// </summary>
    private static ILog logger;

    /// <summary>
    /// A list of all the games
    /// </summary>
    public List<DevcadeGame> gameTitles;

    /// <summary>
    /// if the cabneit is in demo mode
    /// i.e. show only the curated game list
    /// </summary>
    public bool isDemoMode = false;

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
    public readonly static Tag curatedTag = new Tag("Curated", "Curated by the Devcade Team");
    public readonly static Tag allTag = new Tag("All Games", "View all available games");
    public Tag currentTag = allTag;

    /// <summary>
    /// True if the game list is being reloaded from the backend
    /// </summary>
    public bool reloadingGameList { get; private set; } = false;

    /// <summary>
    /// True if one of the games is being run, false otherwise
    /// </summary>
    public bool gameLauched { get; private set; } = false;

    /// <summary>
    /// The current GUI scene being shown
    /// </summary>
    GuiInterface guiScene;

    /// <summary>
    /// The root node of the GUI scene
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
        // get the logger
        logger = LogManager.GetLogger("onboard.GUI");
        logger.Info($"Date: {Time.GetDateStringFromSystem()} \n");

        supervisorButtonTimeoutSeconds = Env.get("SUPERVISOR_BUTTON_TIMEOUT_SEC").map_or(5.0, double.Parse); // default 5 seconds
        supervisorButtonTimerSeconds = supervisorButtonTimeoutSeconds;

        screenSaverTimeoutSeconds = Env.get("SCREENSAVER_TIMEOUT_SEC").map_or(120.0, double.Parse); // default 2 minutes
        screenSaverTimerSeconds = screenSaverTimeoutSeconds;
        
        isDemoMode = Env.get("DEMO_MODE").map_or(false, bool.Parse);

        // hide the loading screen by default
        hideLoadingAnimation();
        // hide the screen saver by default
        hideScreenSaver();

        // spawn initial gui scene
        this.guiSceneRootNode = initialGuiScene.Instantiate() as CanvasItem;

        if(guiSceneRootNode == null)
        {
            logger.Fatal("Assert Failed: gui scene root node is not a node that derives from the CanvasItem node");
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
            logger.Fatal("Assert Failed: gui scene root node script does not implement the GuiInterface interface");
            throw new ApplicationException("Assert Failed: the gui scene's root node script does not implement the GuiInterface interface");
        } 
        
        // add the new scene instance as a child of this node
        AddChild(guiSceneRootNode);

        // and reload the game list
        reloadGameList();
    }

    public override void _Input(InputEvent @event)
    {   
        // if(@event is InputEventAction) 
        // {
        //     InputEventAction tmp = (InputEventAction) @event;
        //     GD.Print(@event.Device, tmp.Action);
        //     base._Input(@event);
        // }
    }

    double supervisorButtonTimeoutSeconds;
    double supervisorButtonTimerSeconds;

    double screenSaverTimeoutSeconds;
    double screenSaverTimerSeconds;
    public override void _Process(double delta)
    {
        // frontend reset button, reloads all the games from the backend
        if (Input.IsActionPressed("Player1_Menu") && Input.IsActionPressed("Player2_Menu"))
        {
            reloadGameList();
        }

        // switch between dev and normal mode
        if (Input.IsActionPressed("Player1_B4") && Input.IsActionPressed("Player2_B4"))
        {
            Client.setProduction(!Client.isProduction).ContinueWith(_ => { setTag(allTag); reloadGameList(); });
        }

        //
        // supervisor button (aka force kill)
        //
        if (SupervisorButton.isSupervisorButtonPressed())
        {
            supervisorButtonTimerSeconds -= delta;
            if (supervisorButtonTimerSeconds <= 0.0)
            {
                // if the timer has timed out
                // kill the currently running game
                _ = killGame();
            }
        }
        else
        {
            supervisorButtonTimerSeconds = supervisorButtonTimeoutSeconds;
        }

        //
        // screen saver
        //
        if (!Input.IsAnythingPressed())
        {
            screenSaverTimerSeconds -= delta;
            if (screenSaverTimerSeconds <= 0.0 && showingScreenSaverAnimation == false)
            {
                // if the timer has timed out
                // kill the currently running game 
                // and show the screensaver
                if(gameLauched)
                {
                    _ = killGame();
                }
                showingScreenSaverAnimation = true;
                showScreenSaver();
            }
        }
        else
        {
            if (showingScreenSaverAnimation)
            {
                showingScreenSaverAnimation = false;
                hideScreenSaver();
            }
            screenSaverTimerSeconds = screenSaverTimeoutSeconds;
        }
        
        
    }

    /// <summary>
    /// the constructor of a class will run before the _Ready() function is called
    /// </summary>
    public GuiManager()
    {

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
                    gameTitles = errorList;
                    return;
                }

                var res = t.Result.into_result<List<DevcadeGame>>();
                if (!res.is_ok()) {
                    logger.Error($"Failed to fetch game list: {res.err().unwrap()}");
                    gameTitles = errorList;
                    return;
                }

                logger.Info("Got game list, setting titles");

                gameTitles = res.unwrap();

                // each game does not have the "all tag"
                // adding it removes the requirement for an extra condition in each gui's code
                gameTitles.ForEach(game =>
                {
                    game.tags.Add(allTag);
                });
                
                // remove all games that do not have the curated tag if
                // demo mode is enabled
                if(isDemoMode)
                {
                    for (int i = 0; i < gameTitles.Count; i++)
                    {
                        DevcadeGame game = gameTitles[i];
                        // if it does not have the curatedTag 
                        if(!game.tags.Contains(curatedTag))
                        {
                            // remove it
                            gameTitles.Remove(game);
                            i--; // fix the index, so we don't skip any games
                        }
                    }
                }
                
            })
            .ContinueWith(_ => {
                logger.Info("Setting cards");

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
    /// shows the base screen saver
    /// thereby hiding the current GUI
    /// </summary>
    public void showScreenSaver()
    {
        logger.Info("Showing Screen Saver");

        screenSaver.CallDeferred("show");
        screenSaverAnimation.CallDeferred("play");
    }

    /// <summary>
    /// hides the base screen saver 
    /// thereby showing the current GUI
    /// </summary>
    public void hideScreenSaver()
    {
        logger.Info("Hiding Screen Saver");

        screenSaver.CallDeferred("hide");
        screenSaverAnimation.CallDeferred("stop");
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
    public async Task launchGame(DevcadeGame game) 
    {
        long previousProcessMode = (long) this.ProcessMode;
        // pause when a game is launched so the onboard does not receive input when not focused 
        // SceneTree tree = GetTree();
        // tree.Paused = true;
        ProcessMode = ProcessModeEnum.Disabled;
        
        this.gameLauched = true;
        logger.Info("launching game: " + game.name);

        showLoadingAnimation();

        await Client.launchGame(
            game.id).ContinueWith(res => {
                if (res.IsCompletedSuccessfully) {
                    // runs after the game completes running
                    hideLoadingAnimation();
                }
                else {
                    logger.Error("Failed to launch game: " + res.Exception);
                }
                // ProcessMode = ProcessModeEnum.Always; 
                this.SetDeferred("process_mode", previousProcessMode);

                this.gameLauched = false;
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
