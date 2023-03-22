using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using onboard.util;

namespace onboard.devcade; 

public static class DevcadeAPI {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);
    private static readonly string route;
    private const int maximumConcurrentDownloads = 3;
    private const int maximumAcquireAttempts = 10;
    private static readonly object[] downloadLocks = new object[maximumConcurrentDownloads];
    private static readonly List<int> availableLocks = new();
    private static readonly object lockLock = new();
    
    static DevcadeAPI() {
        Option<string> routeOption = Env.get("DEVCADE_API_DOMAIN");
        if (routeOption.is_some()) {
            route = $"https://{routeOption.unwrap()}/api/games";
            logger.Info($"API Initialized. Route is {route}");
        } else {
            logger.Fatal("DEVCADE_API_DOMAIN environment variable not set");
            Environment.Exit(1);
        }
        for (int i = 0; i < maximumConcurrentDownloads; i++) {
            downloadLocks[i] = new object();
            availableLocks.Add(i);
        }
    }

    #region API
    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.String")]
    public static Result<byte[], Exception> getGameBinary(string id) {
        string uri = $"{route}/download/{id}";
        logger.Debug($"Downloading game binary from {uri}");
        return binaryRoute(uri)
            .inspect_err(e => logger.Warn($"Failed to download game binary from {uri}: {e}"));
    }

    public static Task<Result<byte[], Exception>> getGameBinaryAsync(string uri) {
        return Task.Run(() => getGameBinary(uri));
    }

    public static Result<byte[], Exception> getGameBanner(string id) {
        string uri = $"{route}/download/banner/{id}";
        logger.Debug($"Downloading game banner from {uri}");
        return binaryRoute(uri)
            .inspect_err(e => logger.Warn($"Failed to download game banner from {uri}: {e}"));
    }
    
    public static Task<Result<byte[], Exception>> getGameBannerAsync(string uri) {
        return Task.Run(() => getGameBanner(uri));
    }
    
    public static Result<byte[], Exception> getGameIcon(string id) {
        string uri = $"{route}/download/icon/{id}";
        logger.Debug($"Downloading game icon from {uri}");
        return binaryRoute(uri)
            .inspect_err(e => logger.Warn($"Failed to download game icon from {uri}: {e}"));
    }
    
    public static Task<Result<byte[], Exception>> getGameIconAsync(string uri) {
        return Task.Run(() => getGameIcon(uri));
    }

    public static Result<string, Exception> getGameList() {
        string uri = $"{route}/gamelist";
        logger.Debug($"Downloading game list from {uri}");
        return stringRoute(uri)
            .inspect_err(e => logger.Warn($"Failed to download game list from {uri}: {e}"));
    }
    
    public static Task<Result<string, Exception>> getGameListAsync() {
        return Task.Run(getGameList);
    }

    public static Result<string, Exception> getGameIDList() {
        string uri = $"{route}/gamelist/ids";
        logger.Debug($"Downloading game ID list from {uri}");
        return stringRoute(uri)
            .inspect_err(e => logger.Warn($"Failed to download game ID list from {uri}: {e}"));
    }
    #endregion

    #region Route Types
    private static Result<string, Exception> stringRoute(string uri) {
        int lockIndex = acquire();
        int retries = 0;
        
        while(lockIndex == -1 && retries < maximumAcquireAttempts) {
            retries++;
            // logger.Warn($"Failed to acquire download lock. Retrying... ({retries})");
            Task.Delay(1000).Wait();
            lockIndex = acquire();
        }
        
        if (lockIndex == -1) {
            return Result<string, Exception>.Err(new Exception($"No locks available after {retries} attempts"));
        }
        lock (downloadLocks[lockIndex]) {
            var res = Network.getResponseAsync(uri);
            res.Wait();
            var ret = res.Result.is_ok() ? 
                Result<string, Exception>.Ok(res.Result.unwrap().Content.ReadAsStringAsync().Result) : 
                Result<string, Exception>.Err(res.Result.unwrap_err());
            release(lockIndex);
            return ret;
        }
    }

    private static Result<byte[], Exception> binaryRoute(string uri) {
        int lockIndex = acquire();
        int retries = 0;
        
        while (lockIndex == -1 && retries < maximumAcquireAttempts) {
            retries++;
            // logger.Warn($"Failed to acquire download lock. Retrying... ({retries})");
            Task.Delay(1000).Wait();
            lockIndex = acquire();
        }
        
        if (lockIndex == -1) {
            return Result<byte[], Exception>.Err(new Exception($"No locks available after {retries} attempts"));
        }
        
        lock (downloadLocks[lockIndex]) {
            var res = Network.getResponseAsync(uri);
            res.Wait();
            var ret = res.Result.is_ok() ? 
                Result<byte[], Exception>.Ok(res.Result.unwrap().Content.ReadAsByteArrayAsync().Result) : 
                Result<byte[], Exception>.Err(res.Result.unwrap_err());
            release(lockIndex);
            return ret;
        }
    }
    #endregion

    #region lock
    private static int acquire() {
        lock (lockLock) {
            if (availableLocks.Count == 0) {
                return -1;
            }
            int lockIndex = availableLocks[0];
            availableLocks.RemoveAt(0);
            return lockIndex;
        }
    }
    
    private static void release(int lockIndex) {
        lock (lockLock) {
            availableLocks.Add(lockIndex);
        }
    }
    #endregion
}