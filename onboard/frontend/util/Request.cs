#nullable enable
namespace onboard.util; 

/**
 * A request to the Devcade API.
 */
public class Request {
    private static uint _id;
    
    public enum RequestType {
        GetGameList,
        GetGameListFromFs,
        GetGame,
        DownloadGame,
        DownloadIcon,
        DownloadBanner,
        
        SetProduction,
        
        LaunchGame,
    }
    
    public readonly uint id;
    private readonly RequestType type;
    private readonly bool? prod;
    private readonly string? game_id;

    private Request(RequestType type, string? game_id = null, bool? prod = null) {
        this.id = _id++;
        this.type = type;
        this.game_id = game_id;
        this.prod = prod;
    }

    public string serialize() {
        return this.type switch {
            RequestType.GetGameList or RequestType.GetGameListFromFs => $"{{\"{this.type}\":{this.id}}}",
            RequestType.SetProduction => $"{{\"{this.type}\":[{this.id}, {(this.prod ?? true).ToString().ToLower()}]}}",
            _ => $"{{\"{this.type}\":[{this.id},\"{this.game_id}\"]}}"
        };
    }
    
    public static Request GetGameList() {
        return new Request(RequestType.GetGameList);
    }
    
    public static Request GetGameListFromFs() {
        return new Request(RequestType.GetGameListFromFs);
    }
    
    public static Request GetGame(string game_id) {
        return new Request(RequestType.GetGame, game_id);
    }
    
    public static Request DownloadGame(string game_id) {
        return new Request(RequestType.DownloadGame, game_id);
    }
    
    public static Request DownloadIcon(string game_id) {
        return new Request(RequestType.DownloadIcon, game_id);
    }
    
    public static Request DownloadBanner(string game_id) {
        return new Request(RequestType.DownloadBanner, game_id);
    }
    
    public static Request LaunchGame(string game_id) {
        return new Request(RequestType.LaunchGame, game_id);
    }
    
    public static Request SetProduction(bool prod) {
        return new Request(RequestType.SetProduction, null, prod);
    }
}