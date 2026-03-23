using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using onboard.devcade;
using onboard.util;

namespace onboard; 

/// <summary>
/// A global interface for better interactions with the Client.cs script
/// 
/// 
/// </summary>
/// Notes: 
/// the various call_ and state_ variables are used to call their related signals
/// in the proccess loop aka the main thread.
public partial class GuiManagerGlobal : Node
{
    /// <summary>
    /// The one instance of this class
    /// </summary>
    public static GuiManagerGlobal instance;

    /// <summary>
    /// If the cabneit is in demo mode
    /// i.e. show only the curated game list
    /// </summary>
    public static bool isDemoMode { get; private set; } = false;

    /// <summary>
    /// True if the loading animation is being shown
    /// </summary>
    public static bool showingLoadingAnimation { get; private set; } = false;
    /// <summary>
    /// Emit to set the state of the loading animation
    /// </summary>
    [Signal]
    public delegate void setLoadingAnimationEventHandler(bool show);
    private int call_setLoadingAnimation = 0;
    private bool state_setLoadingAnimation = false;

    ////////////
#region Tags ///
    ////////////

    /// <summary>
    /// show only games that are curated (the best of the best)
    /// </summary>
    public readonly static Tag curatedTag = new Tag("Curated", "Curated by the Devcade Team");
    /// <summary>
    /// Show all games
    /// </summary>
    public readonly static Tag allTag = new Tag("All Games", "View all available games");

    /// <summary>
    /// The current tag
    /// </summary>
    public static Tag currentTag;
    /// <summary>
    /// Emitted when the current tag changes value
    /// </summary>
    [Signal]
    public delegate void currentTagUpdatedEventHandler();
    private int call_currentTagUpdated = 0;

    /// <summary>
    /// a list of all the tags
    /// </summary>
    public static List<Tag> tagList = new List<Tag>() { allTag };
    /// <summary>
    /// Emitted when the tag list changes
    /// </summary>
    [Signal]
    public delegate void tagListUpdatedEventHandler();
    private int call_tagListUpdated = 0;

    /// <summary>
    /// a list of all the tags
    /// </summary>
    public static Dictionary<string, List<DevcadeGame>> tagLists = new Dictionary<string, List<DevcadeGame>> { { allTag.name, new List<DevcadeGame>() } };
    /// <summary>
    /// Emitted when the tag list changes
    /// </summary>
    [Signal]
    public delegate void tagListsUpdatedEventHandler();
    private int call_tagListsUpdated = 0;

#endregion
    /////////////
#region Games ///
    /////////////
    
    /// <summary>
    /// True if the game list is being refreshed from the backend
    /// </summary>
    public static bool reloadingGameList = false;
    /// <summary>
    /// Emitted when the reloading game list value is changed
    /// </summary>
    [Signal]
    public delegate void reloadingGameListUpdatedEventHandler(bool reloadGameList);
    private int call_reloadingGameListUpdated = 0;

    /// <summary>
    /// The default game to use for errors
    /// </summary>
    private static readonly DevcadeGame defaultGame = new DevcadeGame {
        name = "Error",
        description = "There was a problem loading games from the API. Please check the logs for more information.",
        id = "error",
        author = "None",
        banner = null,
    };

    /// <summary>
    /// the game list is set to this if an error conditions is encountered 
    /// </summary>
    private static readonly List<DevcadeGame> errorList = new List<DevcadeGame> { defaultGame }; 

    /// <summary>
    /// A list of all the games
    /// </summary>
    public static List<DevcadeGame> gameTitles;

    /// <summary>
    /// Emitted when the game titles changes
    /// </summary>
    [Signal]
    public delegate void gameTitlesUpdatedEventHandler();
    private int call_gameTitlesUpdated = 0;

    /// <summary>
    /// Emitted when a game is launched or closed
    /// True when launched, False when closed
    /// </summary>
    [Signal]
    public delegate void onGameLaunchedEventHandler(bool launched);
    private int call_onGameLaunched = 0;
    private bool state_onGameLaunched = false;

#endregion 
    ///////////////
#region Methods ///
    ///////////////
    
    public GuiManagerGlobal()
    {
        instance = this;

        isDemoMode = Env.DEMO_MODE();
    }

    public static void init()
    {
        // force initialization of the class
    }

    public void setTag(Tag newTag)
    {
        currentTag = newTag;
        
        Interlocked.Increment(ref call_currentTagUpdated);
    }

    /// <summary>
    /// fetches the game list from the backend
    /// does take time to do so
    /// </summary>
    /// <returns></returns>
    public Task reloadGameList()
    {
        reloadingGameList = true;
        Interlocked.Increment(ref call_reloadingGameListUpdated);

        state_setLoadingAnimation = true;
        Interlocked.Increment(ref call_setLoadingAnimation);

        tagLists = new Dictionary<string, List<DevcadeGame>> { { allTag.name, new List<DevcadeGame>() } };
        tagList = new List<Tag>() { allTag };

        gameTitles = errorList;

        Task gameTask = Client.getGameList()
            .ContinueWith(t => {
                if (!t.IsCompletedSuccessfully) {
                    GD.PushError($"Failed to fetch game list: {t.Exception}");
                    gameTitles = errorList;
                    return;
                }

                var res = t.Result.into_result<List<DevcadeGame>>();
                if (!res.is_ok()) {
                    GD.PushError($"Failed to fetch game list: {res.err().unwrap()}");
                    gameTitles = errorList;
                    return;
                }

                GD.Print("Got game list, setting titles");

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
                GD.Print("Setting cards");

                loadBanners();

                reloadingGameList = false;

                call_reloadingGameListUpdated++;
                Interlocked.Increment(ref call_gameTitlesUpdated);

                state_setLoadingAnimation = false;
                Interlocked.Increment(ref call_setLoadingAnimation);
            });
        return gameTask;
    }

    /// <summary>
    /// loads the banners from the downloaded files from the database
    /// and saves the images to the DevcadeGame gamse
    /// </summary>
    public void loadBanners()
    {
        foreach(DevcadeGame game in gameTitles)
        {            
            // Start downloading the textures
            if (game.id != "error") {
                // don't download the banner for the default game
                Client.downloadBanner(game.id);
            } // check if /tmp/ has the banner
            
            string bannerPath = $"{Env.DEVCADE_PATH()}/{game.id}/banner.png";

            if (File.Exists(bannerPath)) {
                try {
                    // godot image class 
                    Image image = Image.LoadFromFile(bannerPath);
                    ImageTexture texture = ImageTexture.CreateFromImage(image); // inherits from godot texture2D class 

                    game.banner = texture;
                }
                catch (Exception e) {
                    GD.PushWarning($"Unable to set card: {e.Message}");
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

            Interlocked.Increment(ref call_tagListsUpdated);
            Interlocked.Increment(ref call_tagListUpdated);
        }
    }


    /// <summary>
    /// launch the given game
    /// </summary>
    /// <param name="game"> the game to launch </param>
    public async Task launchGame(DevcadeGame game) 
    {  
        Interlocked.Increment(ref call_setLoadingAnimation);
        state_setLoadingAnimation = true;

        Interlocked.Increment(ref call_onGameLaunched);
        state_onGameLaunched = true;
        
        GD.Print("launching game: " + game.name);

        await Client.launchGame(
            game.id).ContinueWith(res => {
                if (res.IsCompletedSuccessfully) {
                    // runs after the game completes running
                    Interlocked.Increment(ref call_onGameLaunched);
                    state_onGameLaunched = false;

                    Interlocked.Increment(ref call_setLoadingAnimation);
                    state_setLoadingAnimation = false;
                }
                else {
                    GD.PushError("Failed to launch game: " + res.Exception);
                }
                // ProcessMode = ProcessModeEnum.Always; 
        });
    }

    /// <summary>
    /// kill the currently running game.
    /// will run async, but can await the function call
    /// </summary>
    public async Task killGame()
    {
        await Client.killGame();

        Interlocked.Increment(ref call_onGameLaunched);
        state_onGameLaunched = false;
    }

    #endregion

    public override void _Process(double delta)
    {
        // call all signals in the main thread 
        // don't want to deal with all the call_deffered calls and issues with that 
        if(call_currentTagUpdated > 0)
        {
            EmitSignal(SignalName.currentTagUpdated);
            Interlocked.Decrement(ref call_currentTagUpdated);
        }

        if(call_gameTitlesUpdated > 0)
        {
            EmitSignal(SignalName.gameTitlesUpdated);
            Interlocked.Decrement(ref call_gameTitlesUpdated);
        }

        if(call_onGameLaunched > 0)
        {
            EmitSignal(SignalName.onGameLaunched, state_onGameLaunched);
            Interlocked.Decrement(ref call_onGameLaunched);
        }

        if(call_reloadingGameListUpdated > 0)
        {
            EmitSignal(SignalName.reloadingGameListUpdated, reloadingGameList);
            Interlocked.Decrement(ref call_reloadingGameListUpdated);
        }

        if(call_setLoadingAnimation > 0)
        {
            EmitSignal(SignalName.setLoadingAnimation, state_setLoadingAnimation);
            Interlocked.Decrement(ref call_setLoadingAnimation);
        }

        if(call_tagListsUpdated > 0)
        {
            EmitSignal(SignalName.tagListsUpdated);
            Interlocked.Decrement(ref call_tagListsUpdated);
        }

        if(call_tagListUpdated > 0)
        {
            EmitSignal(SignalName.tagListUpdated);
            Interlocked.Decrement(ref call_tagListUpdated);
        }
    }
}