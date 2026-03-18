using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using onboard.devcade;
using onboard.util;
using static Godot.Node;

namespace onboard; 

public static class GuiManagerGlobal
{
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
    public static event Action<bool> setLoadingAnimation;

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
    public static event Action<Tag> tagUpdated;

    /// <summary>
    /// a list of all the tags
    /// </summary>
    public static List<Tag> tagList = new List<Tag>() { allTag };
    /// <summary>
    /// Emitted when the tag list changes
    /// </summary>
    public static event Action<List<Tag>> tagListUpdated;

    /// <summary>
    /// a list of all the tags
    /// </summary>
    public static Dictionary<string, List<DevcadeGame>> tagLists = new Dictionary<string, List<DevcadeGame>> { { allTag.name, new List<DevcadeGame>() } };
    /// <summary>
    /// Emitted when the tag list changes
    /// </summary>
    public static event Action<Dictionary<string, List<DevcadeGame>>> tagListsUpdated;

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
    public static event Action<bool> reloadingGameListUpdated;

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
    public static event Action<List<DevcadeGame>> gameTitlesUpdated;

    /// <summary>
    /// Emitted when a game is launched or closed
    /// True when launched, False when closed
    /// </summary>
    public static event Action<bool> gameLaunched;

#endregion 
    ///////////////
#region Methods ///
    ///////////////
    
    static GuiManagerGlobal()
    {
        isDemoMode = Env.DEMO_MODE();
    }

    public static void setTag(Tag newTag)
    {
        currentTag = newTag;
        tagUpdated.Invoke(newTag);
    }

    /// <summary>
    /// fetches the game list from the backend
    /// does take time to do so
    /// </summary>
    /// <returns></returns>
    public static Task reloadGameList()
    {
        
        reloadingGameList = true;
        reloadingGameListUpdated.Invoke(reloadingGameList);

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
                reloadingGameListUpdated.Invoke(reloadingGameList);
                gameTitlesUpdated.Invoke(gameTitles);
            });
        return gameTask;
    }

    /// <summary>
    /// loads the banners from the downloaded files from the database
    /// and saves the images to the DevcadeGame gamse
    /// </summary>
    public static void loadBanners()
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
            tagListsUpdated.Invoke(tagLists);
            tagListUpdated.Invoke(tagList);
        }
    }


    /// <summary>
    /// launch the given game
    /// </summary>
    /// <param name="game"> the game to launch </param>
    public static async Task launchGame(DevcadeGame game) 
    {  
        gameLaunched.Invoke(true);
        
        GD.Print("launching game: " + game.name);

        setLoadingAnimation.Invoke(true);

        await Client.launchGame(
            game.id).ContinueWith(res => {
                if (res.IsCompletedSuccessfully) {
                    gameLaunched.Invoke(false);
                    // runs after the game completes running
                    setLoadingAnimation.Invoke(false);
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
    public static async Task killGame()
    {
        await Client.killGame();
        gameLaunched.Invoke(false);
    }

    #endregion
}