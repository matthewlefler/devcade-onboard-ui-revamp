#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace onboard.util; 

public class Response {
    // Deserialized response. Equivalent to the following Rust enum:
    /*
     * pub enum Response {
     *   Ok(u32),
     *   Err(u32, String),
     *   GameList(u32, Vec<DevcadeGame>),
     *   Game(u32, DevcadeGame),
     * }
     */
    
    // Trying to get C# to deserialize something that serde serialized is actual hell
    // This is probably the best I can do in this fucked up language

    public enum ResponseType {
        Ok,
        Err,
        GameList,
        Game,
        
        Unknown,
    }
    
    private readonly Dictionary<string, object> data;

    public ResponseType type {
        get {
            if (data.ContainsKey("Ok")) return ResponseType.Ok;
            if (data.ContainsKey("Err")) return ResponseType.Err;
            if (data.ContainsKey("GameList")) return ResponseType.GameList;
            if (data.ContainsKey("Game")) return ResponseType.Game;
            return ResponseType.Unknown;
        }
    }

    public uint? id {
        get {
            switch (type) {
                case ResponseType.Ok:
                    return (uint)(long) data["Ok"];
                case ResponseType.Err:
                    object[]? a = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["Err"]));
                    return (uint)(long) a[0];
                case ResponseType.GameList:
                    object[]? b = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["GameList"]));
                    return (uint)(long) b[0];
                case ResponseType.Game:
                    object[]? c = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["Game"]));
                    return (uint)(long) c[0];
                default:
                    return null;
            }
        }
    }

    public string? err {
        get {
            if (type != ResponseType.Err) return null;
            object[]? a = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["Err"]));
            return a[1] as string;
        }
    }

    public List<devcade.DevcadeGame> game_list {
        get {
            if (type != ResponseType.GameList) return new List<devcade.DevcadeGame>();
            object[]? a = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["GameList"]));
            return JsonConvert.DeserializeObject<List<devcade.DevcadeGame>>(JsonConvert.SerializeObject(a[1]));
        }
    }

    public devcade.DevcadeGame? game {
        get {
            if (type != ResponseType.Game) return null;
            object[]? a = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["Game"]));
            return JsonConvert.DeserializeObject<devcade.DevcadeGame>(JsonConvert.SerializeObject(a[1]));
        }
    }

    private Response(Dictionary<string, object> data) {
        this.data = data;
    }

    public static Response deserialize(string json) {
        return new Response(JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>());
    }
}
