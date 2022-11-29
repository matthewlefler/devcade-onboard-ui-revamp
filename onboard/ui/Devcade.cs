using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using onboard.devcade;
using onboard.util;

namespace onboard.ui; 

public class Devcade : Game {

    private static ILog logger = LogManager.GetLogger("onboard.ui.Devcade");
    
    public static Devcade instance { get; set; }
    
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private IMenu menu = Menu.instance;
    
    public Devcade() {
        this.graphics = new GraphicsDeviceManager(this);
    }
    
    protected override void Initialize() {
        // TODO: Add your initialization logic here
        var sWidth = Env.get("VIEW_WIDTH");
        var sHeight = Env.get("VIEW_HEIGHT");
        if (sWidth.is_none()) {
            logger.Warn("VIEW_WIDTH not set. Using default 1080");
        }
        if (sHeight.is_none()) {
            logger.Warn("VIEW_HEIGHT not set. Using default 2560");
        }
        int width = sWidth.map_or(1080, int.Parse);
        int height = sHeight.map_or(2560, int.Parse);
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
        graphics.ApplyChanges();
        
        menu.Initialize();

        instance = this;

        base.Initialize();
    }
    
    protected override void LoadContent() {
        // TODO: use this.Content to load your game content here

        menu.LoadContent(Content);
        
        this.spriteBatch = new SpriteBatch(this.graphics.GraphicsDevice);
        base.LoadContent();
    }
    
    protected override void Update(GameTime gameTime) {
        // TODO: Add your update logic here

        menu.Update(gameTime);
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime) {
        // TODO: Add your drawing code here
        GraphicsDevice.Clear(Color.CornflowerBlue);

        menu.Draw(this.spriteBatch, gameTime);

        base.Draw(gameTime);
    }

    public Task<Result<Texture2D, Exception>> loadTextureFromFile(string path) {
        if (!System.IO.File.Exists(path)) {
            return Task.FromResult(Result<Texture2D, Exception>.Err(new System.IO.FileNotFoundException("File not found", path)));
        }
        return Task.Run(() => {
            try {
                Texture2D tex = Texture2D.FromFile(this.graphics.GraphicsDevice, path);
                return Result<Texture2D, Exception>.Ok(tex);
            }
            catch (Exception e) {
                return Result<Texture2D, Exception>.Err(e);
            }
        });
    }
}