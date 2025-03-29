#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using onboard.devcade;

namespace onboard.util;

public class Response {
    public enum ResponseType {
        Pong,
        
        Ok,
        Err,
        GameList,
        Game,
        
        TagList,
        Tag,
        
        User,

        Unknown,
    }

    private ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);
    
    public ResponseType type { get; private set; }
    private object? data { get; set; }
    public uint request_id { get; private set; }

    private Response(ResponseType type, object? data) {
        this.type = type;
        this.data = data;
    }

    public static Response deserialize(string json) {
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        Debug.Assert(dict != null, nameof(dict) + " != null");
        var type = (ResponseType)Enum.Parse(typeof(ResponseType), (string)dict["type"]);
        uint id = (uint)(long)dict["request_id"];
        Response res;
        switch (type) {
            case ResponseType.Ok or ResponseType.Pong:
                res = new Response(type, null);
                break;
            default:
                object data = JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(dict["data"])) ?? throw new NullReferenceException();
                res = new Response(type, data);
                break;
        }
        res.request_id = id;
        return res;
    }

    public Result<T, string> into_result<T>() {
        string data = JsonConvert.SerializeObject(this.data);
        if (type == ResponseType.Err) {
            return Result<T, string>.Err(data);
        }
        // logger.Trace($"Serialized internal data to {data}");
        T deserializeT;
        try {
            deserializeT = JsonConvert.DeserializeObject<T>(data) ?? throw new NullReferenceException();
        } catch (Exception e) {
            logger.Error($"Failed to deserialize {data} to {typeof(T)}", e);
            return Result<T, string>.Err("Failed to deserialize response");
        }
        if (deserializeT == null) {
            return Result<T, string>.Err("Failed to deserialize response");
        }
        Type expected = type switch {
            ResponseType.Ok => typeof(uint),
            ResponseType.GameList => typeof(List<DevcadeGame>),
            ResponseType.Game => typeof(DevcadeGame),
            ResponseType.TagList => typeof(List<Tag>),
            ResponseType.Tag => typeof(Tag),
            ResponseType.User => typeof(User),
            _ => throw new ArgumentOutOfRangeException()
        };
        if (typeof(T) != expected) {
            logger.Error($"Invalid response type and data combination: {type}\nTypeof(T) was {typeof(T)} but expected {expected}");
        }
        return type switch {
            ResponseType.Ok => Result<T, string>.Ok(deserializeT),
            ResponseType.GameList => Result<T, string>.Ok(deserializeT),
            ResponseType.Game => Result<T, string>.Ok(deserializeT),
            ResponseType.TagList => Result<T, string>.Ok(deserializeT),
            ResponseType.Tag => Result<T, string>.Ok(deserializeT),
            ResponseType.User => Result<T, string>.Ok(deserializeT),
        };
    }

    public Option<T> into_option<T>() {
        return into_result<T>().ok();
    }
    
    public T unwrap<T>() {
        return into_result<T>().unwrap();
    }

    public static Response fromError(uint id, string err) {
        object data = new object[] { id, err };
        return new Response(ResponseType.Err, data);
    }
}
