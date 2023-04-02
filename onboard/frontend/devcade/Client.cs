#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using System.Threading.Tasks;

namespace onboard.devcade; 
using util;

public static class Client {

    public static event EventHandler<DevcadeGame> onBannerFinished = (_, _) => {
        logger.Trace("onBannerFinished Invoked");
    };
    public static event EventHandler<DevcadeGame> onIconFinished = (_, _) => {
        logger.Trace("onIconFinished Invoked");
    };
    public static event EventHandler<DevcadeGame> onGameFinished = (_, _) => {
        logger.Trace("onGameFinished Invoked");
    };

    public static bool isProduction { get; private set; } = true;

    public static void init() {
        // This method is called to implicitly call the static constructor
    }

    static Client() {
        logger.Info("Initializing Devcade Client");
        workingDir = Env.get("DEVCADE_PATH").match(
            v => v,
            () => {
                logger.Warn("DEVCADE_PATH not set, using default");
                return "/tmp/devcade";
            });
        logger.Info("DEVCADE_PATH: " + workingDir);

        clientThread = new Thread(start) {
            IsBackground = true
        };

        clientThread.Start();
    }

    /// <summary>
    /// Calling this method implicitly calls the static constructor.
    /// It must be called before any other method in this class
    /// </summary>
    [DoesNotReturn]
    private static void start() {
        logger.Info("Starting Devcade Client");
        
        // Open the read/write pipe to the backend
        
        reader = new StreamReader(new FileStream($"{workingDir}/write_onboard.pipe", FileMode.Open, FileAccess.ReadWrite));
        logger.Info($"Opened read pipe: {workingDir}/write_onboard.pipe");
        
        writer = new StreamWriter(new FileStream($"{workingDir}/read_onboard.pipe", FileMode.Open, FileAccess.Write));
        logger.Info($"Opened write pipe: {workingDir}/read_onboard.pipe");

        // Start the main loop
        while (true) {
            string message = read();

            if (message == "") {
                // sleep for a bit
                Thread.Sleep(100);
                continue;
            }
            
            logger.Trace("Received message: " + message);
            
            // Parse the message
            Response res = Response.deserialize(message);
            
            // Check if the request id is valid
            if (res.id == null) {
                logger.Error("Received response with null request id");
                continue;
            }

            if (!tasks.ContainsKey(res.id.Value)) {
                logger.Warn("Received response for unknown request id: " + res.id);
                continue;
            }
            
            // Get the task associated with the request id
            var task = tasks[res.id.Value];
            
            // Remove the task from the dictionary
            tasks.Remove(res.id.Value);
            
            // Set the result of the task
            task.SetResult(res);

            // Log the response
            switch (res.type) {
                case Response.ResponseType.Err:
                    logger.Error($"Received error response for request {res.id}: {res.err}");
                    break;
                case Response.ResponseType.Ok:
                    logger.Debug($"Received ok response for request {res.id}");
                    break;
                case Response.ResponseType.Game:
                    logger.Debug($"Received game response for request {res.id}");
                    break;
                case Response.ResponseType.GameList:
                    logger.Debug($"Received game list response for request {res.id} (contained {res.game_list.Count} games)");
                    break;
                default:
                    logger.Warn($"Received unknown response type for request {res.id}: {res.type}");
                    break;
            }
        }
    }

    /**
     * FS
     *   tmp/
     *   |- {game.id}/
     *       |- banner.png
     *       |- icon.png
     *       |- publish
     *          |- {game.name} (executable)
    */
    
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);
    
    private static readonly Thread clientThread;

    private static readonly string workingDir;
    
    // Unix FIFO pipe reader and writer for communication with backend
    private static StreamReader? reader;
    private static StreamWriter? writer;
    
    // Dictionary of started tasks by request id
    private static readonly Dictionary<uint, TaskCompletionSource<Response>> tasks = new();
    
    
    #region Backend Communication
    
    private static string read() {
        return reader?.ReadLine() ?? "";
    }

    private static void write(string message) {
        writer?.WriteLine(message);
        writer?.Flush();
    }
    
    #endregion
    
    #region Request Methods

    public static Task<Response> getGameList() {
        logger.Debug("Getting game list");
        return sendRequest(Request.GetGameList());
    }
    
    public static Task<Response> getGame(string id) {
        logger.Debug($"Getting game with id {id}");
        return sendRequest(Request.GetGame(id));
    }
    
    public static Task downloadBanner(string id) {
        logger.Debug($"Downloading banner for game with id {id}");
        return sendRequest(Request.DownloadBanner(id))
            .ContinueWith(response => {
                onBannerFinished.Invoke(null, getGame(id).Result.game ?? throw new Exception("Game not found"));
            });
    }
    
    public static Task downloadIcon(string id) {
        logger.Debug($"Downloading icon for game with id {id}");
        return sendRequest(Request.DownloadIcon(id))
            .ContinueWith(_ => {
                onIconFinished.Invoke(null, getGame(id).Result.game ?? throw new Exception("Game not found"));
            });
    }
    
    public static Task downloadGame(string id) {
        logger.Debug($"Downloading game with id {id}");
        return sendRequest(Request.DownloadGame(id))
            .ContinueWith(_ => {
                onGameFinished.Invoke(null, getGame(id).Result.game ?? throw new Exception("Game not found"));
            });
    }

    public static Task<Response> launchGame(string id) {
        logger.Debug($"Launching game with id {id}");
        return sendRequest(Request.LaunchGame(id));
    }

    public static Task setProduction(bool prod) {
        string prodStr = prod ? "Production" : "Development";
        logger.Info($"Setting API to {prodStr}");
        return sendRequest(Request.SetProduction(prod)).ContinueWith(res => {
            if (res.Result.type == Response.ResponseType.Ok) {
                isProduction = prod;
            }
        });
    }

    #endregion
    
    private static Task<Response> sendRequest(Request req) {
        write(req.serialize());
        
        TaskCompletionSource<Response> tcs = new();
        
        tasks.Add(req.id, tcs);
        
        return tcs.Task;
    }
}
