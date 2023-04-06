#nullable enable
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Newtonsoft.Json;

namespace onboard.util;

public class Response {
    public enum ResponseType {
        Pong,
        
        Ok,
        Err,
        GameList,
        Game,

        Unknown,
    }

    private readonly Dictionary<string, object> data;

    public ResponseType type {
        get {
            if (data.ContainsKey("Pong")) return ResponseType.Pong;
            if (data.ContainsKey("Ok")) return ResponseType.Ok;
            if (data.ContainsKey("Err")) return ResponseType.Err;
            if (data.ContainsKey("GameList")) return ResponseType.GameList;
            if (data.ContainsKey("Game")) return ResponseType.Game;
            return ResponseType.Unknown;
        }
    }

    public uint id {
        get {
            switch (type) 
            {
                case ResponseType.Pong:
                    return (uint)(long)data["Pong"];
                case ResponseType.Ok:
                    return (uint)(long)data["Ok"];
                case ResponseType.Err:
                    object[]? a = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["Err"]));
                    return (uint)(long)a[0];
                case ResponseType.GameList:
                    object[]? b =
                        JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["GameList"]));
                    return (uint)(long)b[0];
                case ResponseType.Game:
                    object[]? c = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["Game"]));
                    return (uint)(long)c[0];
                default:
                    return uint.MaxValue;
            }
        }
    }

    /// <summary>
    /// If the response is an error, this will contain the error message
    /// </summary>
    private string? err {
        get {
            if (type != ResponseType.Err) return null;
            object[]? a = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["Err"]));
            return a[1] as string;
        }
    }

    /// <summary>
    /// If the response is a game list, this will contain the list of games
    /// </summary>
    private List<devcade.DevcadeGame> game_list {
        get {
            if (type != ResponseType.GameList) return new List<devcade.DevcadeGame>();
            object[]? a = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["GameList"]));
            return JsonConvert.DeserializeObject<List<devcade.DevcadeGame>>(JsonConvert.SerializeObject(a[1]));
        }
    }

    /// <summary>
    /// If the response is a game, this will contain the game
    /// </summary>
    private devcade.DevcadeGame? game {
        get {
            if (type != ResponseType.Game) return null;
            object[]? a = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(data["Game"]));
            return JsonConvert.DeserializeObject<devcade.DevcadeGame>(JsonConvert.SerializeObject(a[1]));
        }
    }

    /// <summary>
    /// Converts the response to a Result type to force consumers to handle errors rather than access nullable fields
    /// directly
    /// </summary>
    /// <typeparam name="T">The expected return type of the response</typeparam>
    /// <returns>A Result containing either the expected return type or an error string</returns>
    public Result<T, string> into_result<T>() {
        return type switch {
            ResponseType.Err => Result<T, string>.Err(err ?? "Unknown error"),
            ResponseType.GameList when typeof(T) == typeof(List<devcade.DevcadeGame>) => Result<T, string>.Ok(
                (T)(object)game_list),
            ResponseType.Game when typeof(T) == typeof(devcade.DevcadeGame) => Result<T, string>.Ok((T)(object)game),
            ResponseType.Ok when typeof(T) == typeof(uint) => Result<T, string>.Ok((T)(object)id),
            ResponseType.Pong when typeof(T) == typeof(uint) => Result<T, string>.Ok((T)(object)id),
            ResponseType.Unknown when typeof(T) == typeof(string) => Result<T, string>.Ok(
                (T)(object)JsonConvert.SerializeObject(data)),
            _ => Result<T, string>.Err("Invalid response type")
        };
    }

    /// <summary>
    /// Converts the response to an Option type to indicate that the response may not contain a value. This works identically
    /// to `to_result` except that it implicitly discards the error message. Used when the error message is not important
    /// </summary>
    /// <typeparam name="T">The expected return type of the response</typeparam>
    /// <returns>An option containing either the expected return type or None</returns>
    public Option<T> into_option<T>() {
        return into_result<T>().ok();
    }
    
    /// <summary>
    /// Converts the response to a result and unwraps it. This is equivalent to `to_result().unwrap()`
    /// </summary>
    /// <typeparam name="T">The expected return type of the response</typeparam>
    /// <returns>An instance of type T</returns>
    /// <exception cref="">Throws an exception if the result is an Err</exception>
    public T into<T>() {
        return into_result<T>().unwrap();
    }

    private Response(Dictionary<string, object> data) {
        this.data = data;
    }

    public static Response deserialize(string json) {
        return new Response(JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ??
                            new Dictionary<string, object>());
    }
}
