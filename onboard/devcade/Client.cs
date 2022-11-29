using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;

namespace onboard.devcade; 
using util;

public static class Client {

    public static event EventHandler<DevcadeGame> onBannerFinished;
    public static event EventHandler<DevcadeGame> onIconFinished;
    public static event EventHandler<DevcadeGame> onGameFinished;

    static Client() {
        logger.Info("Initializing Devcade Client");
        onGameFinished += (_, game) => {
            logger.Debug("On game finished invoked");
            int ret = Cmd.chmod($"/tmp/devcade/{game.name}/publish/{game.name}", "u+x"); // make executable
            if (ret != 0) {
                logger.Error($"Failed to make {game.name} executable");
                return;
            }
            logger.Debug($"Made file {game.name} executable");
        };
        onGameFinished += (_, game) => {
            Container.createDockerfileFromGame(game);
        };
        init();
    }

    /// <summary>
    /// Calling this method implicitly calls the static constructor.
    /// It must be called before any other method in this class
    /// </summary>
    public static void start() {
        
    }

    /**
     * FS
     *   tmp/
     *   |- {game.name}/
     *       |- banner.png
     *       |- icon.png
     *       |- publish
     *          |- {game.name} (executable)
    */
    
    private static List<DevcadeGame> games = new();

    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);

    #region Game Search
    public static Option<DevcadeGame> getGameByName(string name) {
        return games.Select(game => game.name).Contains(name) ? 
            Option<DevcadeGame>.Some(games.Find(game => game.name == name)) : 
            Option<DevcadeGame>.None();
    }

    public static Option<DevcadeGame> getGameByID(string id) {
            return games.Select(game => game.id).Contains(id) ? 
            Option<DevcadeGame>.Some(games.Find(game => game.id == id)) : 
            Option<DevcadeGame>.None();
    }
    
    public static IEnumerable<DevcadeGame> getGames() {
        return games;
    }
    
    public static IEnumerable<DevcadeGame> getGamesByPredicate(Func<DevcadeGame, bool> predicate) {
        return games.Where(predicate);
    }
    #endregion

    #region Init
    private static void init() {
        // Get Game List doesn't need to be async, as it's only called once on startup
        // and the games list is small enough that it doesn't matter
        if (!Directory.Exists("/tmp/devcade")) {
            Directory.CreateDirectory("/tmp/devcade");
        }
        Result<string, Exception> response = DevcadeAPI.getGameList();
        if (response.is_ok()) {
            // logger.Debug(response.unwrap());
            List<DevcadeGame> gameList = JsonConvert.DeserializeObject<List<DevcadeGame>>(response.unwrap());
            games = (gameList ?? new List<DevcadeGame>()).GroupBy(game => game.name)
                .Select(group => {
                    group.ToList().Sort((a, b) => a.uploadDate.CompareTo(b.uploadDate));
                    return group.Last();
                }).ToList();
            logger.Info($"Finished initializing Devcade Client. Found {games.Count} games");
        } else {
            logger.Error($"Failed to get game list from Devcade API (Retrying in 10 seconds): {response.unwrap_err()}");
            Task.Delay(10000)
                .ContinueWith(_ => refreshGames())
                .ContinueWith(t => { 
                    if (!t.Result) initFromFS();
            });
        }
    }

    /// <summary>
    /// Initializes the game list from the filesystem, if the API is down
    /// This should never be used under normal circumstances.
    /// </summary>
    private static void initFromFS() {
        // Construct a list of games from folders in the tmp directory
        // This is used to get the list of games that have been downloaded
        // and allows a fallback when the API is down or internet is unavailable
        logger.Info("Checking for cached games in /tmp/devcade/");
        string[] gameFolders = Directory.GetDirectories("/tmp/devcade/");
        foreach (string gameFolder in gameFolders) {
            string name = gameFolder.Split('/').Last();
            if (!File.Exists($"{gameFolder}/{name}.json")) continue;
            if (JsonConvert.DeserializeObject(File.ReadAllText($"{gameFolder}/{name}.json"), typeof(DevcadeGame)) is DevcadeGame game) {
                games.Add(game);
            }
        }
        logger.Info($"Finished initializing Devcade Client. Found {games.Count} games");
    }
    #endregion

    #region Downloads
    
    /*
     * I feel like a lot of the logic of these methods could be abstracted into a single method
     * but I'm not sure how to do that without making it more confusing
     */

    /// <summary>
    /// Downloads a game from the Devcade API and saves it to the filesystem
    /// Once the game is downloaded, invokes the onGameFinished event
    /// </summary>
    /// <param name="game"></param>
    public static void DownloadGame(DevcadeGame game) {
        // Create the directory if it does not exist
        initGameDirectory(game);
        
        // if the game is already downloaded, don't download it again
        if (Directory.Exists($"/tmp/devcade/{game.name}/publish")) {
            logger.Info($"Game {game.name} is already downloaded");
            onGameFinished?.Invoke(null, game);
            return;
        }
        
        // if the game is not in the games list, don't download it
        if (!games.Select(g => g.id).Contains(game.id)) {
            return;
        }
        
        // Queue the download
        QueueGame(game).ContinueWith(t => {
            if (t.Result.is_some()) {
                logger.Error($"Failed to download game {game.name}: {t.Result.unwrap()}");
                return;
            }
            logger.Info($"Downloaded game {game.name}");
            onGameFinished?.Invoke(null, game);
        });
    }
    
    /// <summary>
    /// Internal method to queue a game download
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private static Task<Option<Exception>> QueueGame(DevcadeGame game) {
        Task<Result<byte[], Exception>> gameTask = DevcadeAPI.getGameBinaryAsync(game.id);

        return gameTask.ContinueWith(t => {
            if (!t.IsCompletedSuccessfully || t.Result.is_err()) {
                return Option<Exception>.Some(t.Result.is_err() ? t.Result.unwrap_err() : t.Exception);
            }

            byte[] gameBinary = t.Result.unwrap();
            // var arc = Zip.unzip(gameBinary)?; In more civilized languages.
            var arc = Zip.unzip(gameBinary);
            if (arc.is_err()) {
                return Option<Exception>.Some(arc.unwrap_err());
            }
            var files = arc.unwrap();
            Directory.CreateDirectory($"/tmp/devcade/{game.name}/publish");
            foreach (Zip.Entry entry in files) {
                if (entry.Path.EndsWith("/") /* File is a directory */) {
                    Directory.CreateDirectory($"/tmp/devcade/{game.name}/{entry.Path}");
                    continue;
                }
                File.WriteAllBytes($"/tmp/devcade/{game.name}/{entry.Path}", entry.Data);
            }
            return Option<Exception>.None();
        });
    }
    
    /// <summary>
    /// Downloads a game's banner from the Devcade API and saves it to the filesystem
    /// Once the banner is downloaded, invokes the onBannerFinished event
    /// </summary>
    /// <param name="game"></param>
    public static void DownloadBanner(DevcadeGame game) {
        // Create the directory if it does not exist
        initGameDirectory(game);
        
        // if the game banner is already downloaded, don't download it again
        if (Directory.EnumerateFiles($"/tmp/devcade/{game.name}").Any(a => a.Split('/').Last().StartsWith("banner"))) {
            logger.Info($"Found cached banner for game {game.name}");
            onBannerFinished?.Invoke(null, game);
            return;
        }
        
        // if the game is not in the games list, don't download its banner
        if (!games.Select(g => g.id).Contains(game.id)) {
            return;
        }
        
        // Queue the download
        QueueBanner(game).ContinueWith(t => {
            if (t.Result.is_some()) {
                logger.Error($"Failed to download banner for game {game.name}: {t.Result.unwrap()}");
                return;
            }
            logger.Info($"Downloaded banner for game {game.name}");
            onBannerFinished?.Invoke(null, game);
        });
    }

    /// <summary>
    /// Internal method to queue a banner download
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private static Task<Option<Exception>> QueueBanner(DevcadeGame game) {
        Task<Result<byte[], Exception>> bannerTask = DevcadeAPI.getGameBannerAsync(game.id);
        
        return bannerTask.ContinueWith(t => {
            if (!t.IsCompletedSuccessfully || t.Result.is_err()) {
                return Option<Exception>.Some(t.Result.is_err() ? t.Result.unwrap_err() : t.Exception);
            }

            byte[] bannerBinary = t.Result.unwrap();
            try {
                File.WriteAllBytes($"/tmp/devcade/{game.name}/banner.png", bannerBinary);
            }
            catch (Exception e) {
                return Option<Exception>.Some(e);
            }
            return Option<Exception>.None();
        });
    }
    
    /// <summary>
    /// Downloads a game's icon from the Devcade API and saves it to the filesystem
    /// Once the icon is downloaded, invokes the onIconFinished event
    /// </summary>
    /// <param name="game"></param>
    public static void DownloadIcon(DevcadeGame game) {
        // Create the directory if it does not exist
        initGameDirectory(game);
        
        // if the game icon is already downloaded, don't download it again
        if (Directory.EnumerateFiles($"/tmp/devcade/{game.name}").Any(a => a.StartsWith("icon."))) {
            logger.Info("Found cached icon for game {game.name}");
            onIconFinished?.Invoke(null, game);
            return;
        }
        
        // if the game is not in the games list, don't download its icon
        if (!games.Select(g => g.id).Contains(game.id)) {
            return;
        }
        
        // Queue the download
        QueueIcon(game).ContinueWith(t => {
            if (t.Result.is_some()) {
                logger.Error("Failed to download icon for game " + game.name, t.Result.unwrap());
                return;
            }
            logger.Info("Downloaded icon for game " + game.name);
            onIconFinished?.Invoke(null, game);
        });
    }
    
    /// <summary>
    /// Internal method to queue an icon download
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private static Task<Option<Exception>> QueueIcon(DevcadeGame game) {
        var iconTask = DevcadeAPI.getGameIconAsync(game.id);
        
        return iconTask.ContinueWith(t => {
            if (!t.IsCompletedSuccessfully || t.Result.is_err()) {
                return Option<Exception>.Some(t.Result.is_err() ? t.Result.unwrap_err() : t.Exception);
            }

            byte[] iconBinary = t.Result.unwrap();
            try {
                File.WriteAllBytes($"/tmp/devcade/{game.name}/icon.png", iconBinary);
            }
            catch (Exception e) {
                return Option<Exception>.Some(e);
            }
            return Option<Exception>.None();
        });
    }
    #endregion

    /// <summary>
    /// Creates the game directory if it does not exist
    /// </summary>
    /// <param name="game"></param>
    private static void initGameDirectory(DevcadeGame game) {
        if (!Directory.Exists($"/tmp/devcade/{game.name}")) {
            // Create the directory
            Directory.CreateDirectory($"/tmp/devcade/{game.name}");
        }
        // Serialize the game to a file
        File.WriteAllBytes($"/tmp/devcade/{game.name}/{game.name}.json", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(game)));
    }
    
    private static bool refreshGames() {
        var response = DevcadeAPI.getGameList();
        if (response.is_ok()) {
            var gameList = JsonConvert.DeserializeObject<List<DevcadeGame>>(response.unwrap());
            var newGames = (gameList ?? new List<DevcadeGame>()).Where(g => !games.Select(gg => gg.id).Contains(g.id)).ToList();
            if (newGames.Count == 0) return true;
            logger.Info($"Found {newGames.Count} new games");
            newGames.ForEach(initGameDirectory);
            // Replace the games in the list with the same name with the new games
            games = games.Where(g => !newGames.Select(gg => gg.id).Contains(g.id)).Concat(newGames).ToList();
            return true;
        }
        logger.Warn($"Failed to refresh games: {response.unwrap_err()}");
        return false;
    }
}
