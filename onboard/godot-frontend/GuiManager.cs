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

using Godot;

namespace GodotFrontend;

public partial class GuiManager : Control
{
    [Export]
    public PackedScene initialGuiScene;

    // logger for currently unknown purposes??
    //
    // deos not run, exits with an error msg:
    // log4net:ERROR Exception while reading ConfigurationSettings. Check your .config file is well formed XML. log4net:ERROR Exception while reading ConfigurationSettings. Check your .config file is well formed XML.
    //
    private static ILog logger = LogManager.GetLogger("onboard.ui.Devcade");

    /// <summary>
    /// A list of all the games
    /// </summary>
    public List<DevcadeGame> gameTitles;

    /// <summary>
    /// a list of all the tags
    /// </summary>
    public List<Tag> tagList = new List<Tag>() { allTag };
    
    /// <summary>
    /// A dictionary of tags to lists of games that have that tag
    /// </summary>
    private Dictionary<string, List<DevcadeGame>> tagLists = new Dictionary<string, List<DevcadeGame>>();
    private static Tag allTag = new Tag("All Games", "View all available games");
    public Tag currentTag = allTag;

    public bool loading { get; private set; } = true;

    Node guiScene;
    private bool guiSceneReady = false;

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
        // spawn initial gui scene
        var guiScene = initialGuiScene.Instantiate();
        this.guiScene = guiScene;
        AddChild(guiScene);
        // and set that the gui scene is ready
        guiSceneReady = true;
    }


    public GuiManager()
    {
        // init this, (the model) 

        // init initial GUI scene
            // gui should set up its:
            // buttons, text labels, etc. 
            // !set init focus!

        // load the .env file (contains the enviorment variables)
        Env.load("../.env");

        // start client (backend networked communicator)
        Client.init();

        reloadGameList();
    }

    private void reloadGameList()
    {
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
            })
            .ContinueWith(_ => {
                logger.Info("Setting cards");
                GD.Print("setting cards");

                loadBanners();
                

                updateGUI();
                this.loading = false;
            });
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
            
            tagLists[allTag.name].Add(game);
        }
    }

    /// <summary>
    /// updates the current tag in use
    /// and updates the game list to be only the games that have the current tag
    /// </summary>
    /// <param name="newTag"></param>
    public void setTag(Tag newTag)
    {
        currentTag = newTag;
        updateGUI();
    }

    /// <summary>
    /// updates the gui with the most recent data
    /// </summary>
    /// <exception cref="ApplicationException"> throws if the current GUI does not implement the GuiInterface </exception>
    private void updateGUI()
    {
        if(!guiSceneReady)
        {
            return;
        }

        GuiInterface gui = guiScene as GuiInterface;

        if(gui != null) 
        {
            gui.setGameList(tagLists[currentTag.name], this);
            gui.setTagList(tagList);
        }       
        else
        {
            GD.PrintErr("Assert Failed: gui scene root node script does not implement the GuiInterface interface");
            throw new ApplicationException("Assert Failed: the gui scene's root node script does not implement the GuiInterface interface");
        } 
    }

    /// <summary>
    /// lauch the given game
    /// </summary>
    /// <param name="game"> the game to launch </param>
    public void launchGame(DevcadeGame game) 
    {
        this.loading = true;
        GD.Print("launching game: " + game.name);

        Client.launchGame(
            game.id).ContinueWith(res => {
                if (res.IsCompletedSuccessfully) {
                    // set some state that a game is running?
                }
                else {
                    logger.Error("Failed to launch game: " + res.Exception);
                    GD.Print("Failed to launch game: " + res.Exception);
                    this.loading = false;
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
