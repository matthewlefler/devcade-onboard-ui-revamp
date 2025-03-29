using Godot;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using onboard.devcade;
using onboard.util;

using GodotFrontend;

public partial class Main : Control
{
    [Export]
    public PackedScene initialGuiScene;

    // logger for currently unknown purposes??
    //
    // deos not run, exits with an error msg:
    // log4net:ERROR Exception while reading ConfigurationSettings. Check your .config file is well formed XML. log4net:ERROR Exception while reading ConfigurationSettings. Check your .config file is well formed XML.
    //
    // private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);

    // list of games
    public List<DevcadeGame> gameTitles;

    public bool loading { get; private set; } = true;

    Node guiScene;
    private bool guiSceneReady = false;

    private DevcadeGame defaultGame = new DevcadeGame {
        name = "Error",
        description = "There was a problem loading games from the API. Please check the logs for more information.",
        id = "error",
        author = "None",
    };

    public override void _Ready()
    {
        var guiScene = initialGuiScene.Instantiate();
        this.guiScene = guiScene;
        AddChild(guiScene);
        guiSceneReady = true;
    }


    public Main()
    {
        // init this, (the model) 

        // init initial GUI scene
            // gui should set up its:
            // buttons, text labels, etc. 
            // !set init focus!

        // load the .env file (contains the enviorment variables)
        Env.load("../.env");
        
        var sWidth = Env.get("VIEW_WIDTH");
        var sHeight = Env.get("VIEW_HEIGHT");

        if (sWidth.is_none()) {
            GD.Print("VIEW_WIDTH not set. Using default 1080"); // logger.Warn("VIEW_WIDTH not set. Using default 1080");
        }

        if (sHeight.is_none()) {
            GD.Print("VIEW_HEIGHT not set. Using default 2560"); // logger.Warn("VIEW_HEIGHT not set. Using default 2560");
        }

        int width = sWidth.map_or(1080, int.Parse);
        int height = sHeight.map_or(2560, int.Parse);

        GD.Print("display width: " + width);
        GD.Print("display height: " + height);

        // start client (backend networked communicator)
        Client.init();

        // grab the list of games 

        // the game list is set to this if an error conditions is encountered 
        var errorList = new List<DevcadeGame> { defaultGame }; 

        Task gameTask = Client.getGameList()
            .ContinueWith(t => {
                if (!t.IsCompletedSuccessfully) {
                    GD.Print(t.Exception); // logger.Error($"Failed to fetch game list: {t.Exception}");
                    gameTitles = errorList;
                    return;
                }

                var res = t.Result.into_result<List<DevcadeGame>>();
                if (!res.is_ok()) {
                    GD.Print(res.err().unwrap()); // logger.Error($"Failed to fetch game list: {res.err().unwrap()}");
                    gameTitles = errorList;
                    return;
                }

                GD.Print("Got game list, setting titles"); // logger.Info("Got game list, setting titles");
                gameTitles = res.unwrap();
            })
            .ContinueWith(_ => {
                GD.Print("setting cards"); // logger.Info("Setting cards");
                updateGUI();
                this.loading = false;
            })
            .WaitAsync(TimeSpan.FromSeconds(10))
            .ContinueWith(t => {
                if (t.IsCompletedSuccessfully) return;
                // Take timed out, so we need to set the state back to input and game titles to the error list
                this.loading = false;
                gameTitles = errorList;
                updateGUI();
            });
    }

    private void updateGUI()
    {
        if(!guiSceneReady)
        {
            return;
        }

        if(guiScene is TestGui) 
        {
            TestGui gui = guiScene as TestGui;
            gui.make_buttons(gameTitles);
        }        
    }

    private void launchGame() 
    {
        // TODO:
    }

    private void killGame()
    {
        // TODO:
    }
}
