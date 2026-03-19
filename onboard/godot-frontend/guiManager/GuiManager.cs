using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
    public Screensaver screenSaver;

    /// <summary>
    /// true if the screensaver animation is being shown
    /// </summary>
    public bool showingScreenSaverAnimation { get; private set; } = false;

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
    /// The root node of the GUI scene
    /// </summary>
    Node guiSceneRootNode;

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
        GuiManagerGlobal.gameLaunched += (bool isOpened) =>
        {
            if(isOpened)
            {
                // pause when a game is launched so the onboard does not receive input when not focused 
                // ProcessMode = ProcessModeEnum.Disabled;
                guiSceneRootNode.ProcessMode = ProcessModeEnum.Disabled;  
            }
            else
            {
                guiSceneRootNode.SetDeferred("process_mode", (long) ProcessModeEnum.Inherit);
            }

        };

        supervisorButtonTimeoutSeconds = Env.SUPERVISOR_BUTTON_TIMEOUT_SEC(); // default 5 seconds
        supervisorButtonTimerSeconds = supervisorButtonTimeoutSeconds;

        screenSaverTimeoutSeconds = Env.SCREENSAVER_TIMEOUT_SEC(); // default 2 minutes
        screenSaverTimerSeconds = screenSaverTimeoutSeconds;
        
        // hide the loading screen by default
        hideLoadingAnimation();
        // hide the screen saver by default
        hideScreenSaver();

        // spawn initial gui scene
        this.guiSceneRootNode = initialGuiScene.Instantiate();
        
        // add the new scene instance as a child of this node
        AddChild(guiSceneRootNode);

        // and reload the game list
        GuiManagerGlobal.reloadGameList();
    }

    public override void _Input(InputEvent @event)
    {   
        if(@event is InputEventJoypadButton joy)
        {
            GD.Print(joy.Device, joy.ButtonIndex);
        }

        if(@event is InputEventJoypadMotion axis)
        {
            GD.Print(axis.Device, axis.Axis);
        }

        if(showingScreenSaverAnimation)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    double supervisorButtonTimeoutSeconds;
    double supervisorButtonTimerSeconds;

    static readonly double reloadButtonCooldown = 1.0; 
    double reloadButtonCooldownTimer = reloadButtonCooldown; 
    static readonly double switchDevButtonCooldown = 1.0; 
    double switchDevButtonCooldownTimer = switchDevButtonCooldown; 

    double screenSaverTimeoutSeconds;
    double screenSaverTimerSeconds;

    public override void _Process(double delta)
    {
        reloadButtonCooldownTimer -= delta;
        if(reloadButtonCooldownTimer < 0)
        {
            reloadButtonCooldownTimer = 0;
        }

        // frontend reset button, reloads all the games from the backend
        if (Input.IsActionPressed("Player1_Menu") && Input.IsActionPressed("Player2_Menu") && reloadButtonCooldownTimer <= 0)
        {
            GuiManagerGlobal.reloadGameList();
            reloadButtonCooldownTimer = reloadButtonCooldown;
        }

        switchDevButtonCooldownTimer -= delta;
        if(switchDevButtonCooldownTimer < 0)
        {
            switchDevButtonCooldownTimer = 0;
        }

        // switch between dev and normal mode
        if (Input.IsActionPressed("Player1_B4") && Input.IsActionPressed("Player2_B4") && switchDevButtonCooldownTimer <= 0)
        {
            Client.setProduction(!Client.isProduction).ContinueWith(_ => { GuiManagerGlobal.setTag(allTag); GuiManagerGlobal.reloadGameList(); });
            switchDevButtonCooldownTimer = switchDevButtonCooldown;
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
                _ = GuiManagerGlobal.killGame();
                GD.Print("log: Killing current running game");
                
                supervisorButtonTimerSeconds = supervisorButtonTimeoutSeconds;
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
                if(Client.gameLauched)
                {
                    _ = GuiManagerGlobal.killGame();
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
        GD.Print("Showing Screen Saver");

        screenSaver.CallDeferred("show");
        screenSaver.CallDeferred("play");
    }

    /// <summary>
    /// hides the base screen saver 
    /// thereby showing the current GUI
    /// </summary>
    public void hideScreenSaver()
    {
        GD.Print("Hiding Screen Saver");

        screenSaver.CallDeferred("hide");
        screenSaver.CallDeferred("stop");
    }
}
