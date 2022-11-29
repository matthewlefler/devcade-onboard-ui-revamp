using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using onboard.devcade;

namespace onboard.ui; 
    
public class Menu : IMenu {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);
    
    public static readonly Menu instance = new();
    private Dictionary<string, MenuCard> cards { get; } = new();
    private int selected;
    
    private double scalingAmount = 0;

    private const float moveTime = 0.15f;
    private float timeRemaining = 0f;

    private float descX;
    private float descOpacity = 0f;
    private const float descFadeTime = 0.4f;

    public bool movingUp;
    public bool movingDown;
    
    private Menu() { }
    
    public void Initialize() {
        var games = Client.getGames().ToList();
        for (int i = 0; i < games.Count; i++) {
            MenuCard card = new (i, games[i].name, null);
            cards.Add(games[i].name, card);
        }
        
        Client.onBannerFinished += (_, game) => {
            if (cards.ContainsKey(game.name)) {
                Devcade.instance.loadTextureFromFile($"/tmp/devcade/{game.name}/banner.png");
            }
        };
        
        // Container.OnContainerBuilt += (_, args) => {
        //     logger.Info("Running game");
        //     Container.runContainer(args);
        // };
    }
    
    public void LoadContent(ContentManager contentManager) {
        // Setup banner finished callback
        Client.onBannerFinished += (_, game) => {
            Devcade.instance.loadTextureFromFile($"/tmp/devcade/{game.name}/banner.png").ContinueWith(t => {
                if (t.IsCompletedSuccessfully && t.Result.is_ok() && cards.ContainsKey(game.name)) {
                    cards[game.name].setTexture(t.Result.unwrap());
                    return;
                }

                if (!t.IsCompletedSuccessfully) {
                    logger.Error($"Download thread failed: {t.Exception}");
                    return;
                }
                if (!t.Result.is_ok()) {
                    logger.Error($"Download returned error: {t.Result.unwrap_err()}");
                    return;
                }
                logger.Warn($"Attempted to load banner for non-existent game {game.name}");
            });
        };

        var games = Client.getGames().ToList();
    }
    
    public void Update(GameTime gameTime) {
        // TODO
    }
    
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        // TODO
    }
    
    public void Unload() {
        // TODO
    }
}