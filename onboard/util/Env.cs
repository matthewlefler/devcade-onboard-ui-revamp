using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace onboard.util; 

public static class Env {
    private static readonly Dictionary<string, string> env = new Dictionary<string, string>();
    
    static Env() {
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables()) {
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
            return;
        }
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines) {
            string[] parts = line.Split('=');
            if (parts.Length == 2) {
                env[parts[0]] = parts[1];
            }
        }
    }
}