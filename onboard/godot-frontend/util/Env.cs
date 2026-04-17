using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace onboard.util; 

public static class Env {
    private static readonly Logger LOG = Log.get(nameof(Env));

    private static readonly Dictionary<string, string> env = new();

    // Logging level for the backend
    // Allowed log levels: trace, debug, info, warn, error 
    public static string RUST_LOG() { return get("RUST_LOG").unwrap_or("error"); }
    public static Option<string> DEVCADE_API_DOMAIN() { return get("DEVCADE_API_DOMAIN"); }
    public static Option<string> DEVCADE_DEV_API_DOMAIN() { return get("DEVCADE_DEV_API_DOMAIN"); }
    
    // Frontend 
    // Logging level for the frontend 
    // Allowed log levels: trace, verbose, debug, info, warn, error, fatal 
    public static string FRONTEND_LOG() { return get("FRONTEND_LOG").unwrap_or("error"); }
    // Amount of time in seconds until the screen saver is shown
    public static double SCREENSAVER_TIMEOUT_SEC() { return get("SCREENSAVER_TIMEOUT_SEC").map_or(5.0, double.Parse); }
    // Amount of time in seconds that the supervisor buttons need to be heldW
    public static double SUPERVISOR_BUTTON_TIMEOUT_SEC() { return get("SUPERVISOR_BUTTON_TIMEOUT_SEC").map_or(5.0, double.Parse); }

    // Demo mode will not display games with certain tags (e.g. "CSH Only") 
    // Allowed values: true, false 
    public static bool DEMO_MODE() { return get("DEMO_MODE").map_or(true, bool.Parse); }

    // Shared 
    // Games data and shared sockets will be placed here, defaults to ~/devcade
    public static string DEVCADE_PATH() { return get("DEVCADE_PATH").unwrap_or(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/devcade" ); }
    // Where to place the log files
    public static string LOG_LOCATION() { return get("LOG_LOCATION").unwrap_or(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/devcade/logs" ); }

    // toggle for low performance features
    public static bool LOW_PERFORMANCE_MODE() { return get("LOW_PERFORMANCE_MODE").map_or(false, bool.Parse); }

    
    static Env() {
        if(File.Exists("../.env"))
        {
            load("../.env");
        }
        else
        {
            LOG.Error("File: ../.env does not exist");
        }

        foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables()) {
            env.Add((string)entry.Key, (string)entry.Value);
        }
    }

    public static Option<string> get(string key) {
        return env.ContainsKey(key) ? Option<string>.Some(env[key]) : Option<string>.None();
    }
    
    public static void set(string key, string value) {
        env[key] = value;
    }
    
    public static void unset(string key) {
        env.Remove(key);
    }
    
    public static void clear() {
        env.Clear();
    }

    public static void load(string path) {
        if (!File.Exists(path)) {
            LOG.Error($"File: {path} does not exist");
            return;
        }
        string[] lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i];
            // remove all comments
            int index = line.Find("#");
            if(index != -1)
            {
                line = line.Remove(index);
            }

            string[] parts = line.Split('=');
            if (parts.Length == 2) {
                LOG.Verbose($"found env value: {parts[0]} = {parts[1]}");

                env[parts[0]] = parts[1];
            }
        }
    }
}