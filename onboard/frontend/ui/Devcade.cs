using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Devcade;
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

    private Menu menu;
    private bool demo;

    private SpriteFont _devcadeMenuBig;
    private SpriteFont _devcadeMenuTitle;

    private bool _loading;

    private MenuState state = MenuState.Launch;
    private float fadeColor;

    private KeyboardState lastState;

    private enum MenuState {
        Launch,
        Loading,
        Input,
        Descritpion,
        Tags
    }

    private Texture2D cardTexture;
    private Texture2D loadingSpin;
    private Texture2D BGgradient;
    private Texture2D icon;
    private Texture2D titleTexture;
    private Texture2D titleDevTexture;
    private Texture2D titleTextureWhite;
    private Texture2D descriptionTexture;

    // If we can't fetch the game list (like if the API is down)
    private bool _cantFetch;

    public Devcade() {
        this.graphics = new GraphicsDeviceManager(this);
    }

    protected override void Initialize() {
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

        // if the DEMO_MODE environment variable is true, sorting by tags is disabled, and only curated games are shown
        demo = Env.get("DEMO_MODE").map_or(false, bool.Parse);

        this.menu = new Menu(this.graphics);

        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
        graphics.ApplyChanges();

        menu.Initialize();

        instance = this;
        
        //--------------------------------------------
        // Testing API routes to make sure they work
        // There's no code that uses these routes yet
        // so I'm just testing them here
        //--------------------------------------------
        
        // TODO proper tests? how test in C#?
        // no cargo test in C# :(

        // Run in thread so we don't block the main thread
        new Thread(() => {
            try {
                List<Tag> tags = new();
                var tagResult = Client.getTags();
                tagResult.ContinueWith(res => {
                    if (!res.IsCompletedSuccessfully) {
                        logger.Warn("Failed to fetch tags (Task failed)");
                        return;
                    }

                    var tagRes = res.Result.into_result<List<Tag>>();
                    if (tagRes.is_err()) {
                        logger.Warn($"Failed to fetch tags (API error): {tagRes.unwrap_err()}");
                        return;
                    }

                    tags = tagRes.unwrap();
                    logger.Debug($"Successfully fetched tags (Got {tags.Count} tags)");
                }).Wait();

                List<DevcadeGame> games = new();
                var gamesByTagResult = Client.getGamesWithTag(tags[0]);
                gamesByTagResult.ContinueWith(res => {
                    if (!res.IsCompletedSuccessfully) {
                        logger.Warn("Failed to fetch games by tag (Task failed)");
                        return;
                    }

                    var gamesRes = res.Result.into_result<List<DevcadeGame>>();
                    if (gamesRes.is_err()) {
                        logger.Warn($"Failed to fetch games by tag (API error): {gamesRes.unwrap_err()}");
                        return;
                    }

                    games = gamesRes.unwrap();
                    logger.Debug($"Successfully fetched games by tag (Got {games.Count} games)");
                }).Wait();

                logger.Debug($"The following games have the tag {tags[0].name}:");
                foreach (DevcadeGame game in games) {
                    logger.Debug($"- {game.name}");
                }

                User joe = new();
                var userResult = Client.getUser("joeneil"); // Who else would I use but myself?
                userResult.ContinueWith(res => {
                    if (!res.IsCompletedSuccessfully) {
                        logger.Warn("Failed to fetch user (Task failed)");
                        return;
                    }
                    
                    var userRes = res.Result.into_result<User>();
                    if (userRes.is_err()) {
                        logger.Warn($"Failed to fetch user (API error): {userRes.unwrap_err()}");
                        return;
                    }
                    
                    joe = userRes.unwrap();
                    logger.Debug($"Successfully fetched user (Got user {joe.id} aka {joe.first_name} {joe.last_name})");
                }).Wait();
            } catch (Exception e) {
                // This is just a test, so we don't want to crash the game
                logger.Warn("Failed to test API routes", e);
            }
        }).Start();
        
        // End of testing API routes
        
        base.Initialize();
    }

    protected override void LoadContent() {
        Content.RootDirectory = "Content";
        menu.LoadContent(Content);

        this.spriteBatch = new SpriteBatch(this.graphics.GraphicsDevice);

        _devcadeMenuBig = Content.Load<SpriteFont>("devcade-menu-big");
        _devcadeMenuTitle = Content.Load<SpriteFont>("devcade-menu-title");

        cardTexture = Content.Load<Texture2D>("card");
        titleTexture = Content.Load<Texture2D>("transparent-logo");
        titleDevTexture = Content.Load<Texture2D>("transparent-dev-logo");
        titleTextureWhite = Content.Load<Texture2D>("transparent-logo-white");

        descriptionTexture = Content.Load<Texture2D>("description");

        BGgradient = Content.Load<Texture2D>("OnboardBackgroundGradient");
        icon = Content.Load<Texture2D>("CSH");

        loadingSpin = Content.Load<Texture2D>("loadingSheet");

        // TODO: use this.Content to load your game content here

        if (!menu.reloadGames(GraphicsDevice, false)) {
            state = MenuState.Loading;
            _cantFetch = true; 
        }

        // Create instances related to the tags menu
        menu.initializeTagsMenu(cardTexture, _devcadeMenuBig);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime) {
        menu.Update(gameTime);
        // Update inputs
        KeyboardState myState = Keyboard.GetState();
        Input.Update(); // Controller update

        // Keyboard only to exit menu as it should never exit in prod
        if (Keyboard.GetState().IsKeyDown(Keys.Tab)) {
            Exit();
        }

        // If the state is loading, it is still taking input as though it is in the input state..?
        switch (state) {
            // Fade in when the app launches
            case MenuState.Launch:
                if (fadeColor < 1f) {
                    fadeColor += (float)(gameTime.ElapsedGameTime.TotalSeconds);
                }
                else {
                    // Once the animation completes, begin tracking input
                    state = MenuState.Input;
                }

                break;

            case MenuState.Loading:

                if (_cantFetch && (myState.IsKeyDown(Keys.Space) ||
                                (Input.GetButton(1, Input.ArcadeButtons.Menu) &&
                                Input.GetButtonDown(2, Input.ArcadeButtons.Menu)) ||
                                (Input.GetButtonDown(1, Input.ArcadeButtons.Menu) &&
                                Input.GetButton(2, Input.ArcadeButtons.Menu)))) {
                    try {
                        menu.reloadGames(GraphicsDevice);
                        _cantFetch = false;
                        state = MenuState.Input;
                    }
                    catch (AggregateException e) {
                        logger.Error($"Failed to fetch games: {e}");
                        state = MenuState.Loading;
                        _cantFetch = true;
                    }
                }

                // TODO - Fix this to work with new client
                // if (_client.DownloadFailed)
                // {
                //     _loading = false;
                //     _client.DownloadFailed = false;
                // }

                if (fadeColor < 1f) {
                    fadeColor += (float)(gameTime.ElapsedGameTime.TotalSeconds);
                }

                if (!_loading) {
                    fadeColor = 0f;
                    state = MenuState.Launch;
                }

                break;

            // In this state, the user is able to scroll through the menu and launch games
            case MenuState.Input:
                menu.descFadeOut(gameTime);
                menu.cardFadeIn(gameTime);
                menu.updateTagsMenu(myState, lastState, gameTime);

                if ((myState.IsKeyDown(Keys.Space) || (Input.GetButton(1, Input.ArcadeButtons.Menu) &&
                                                      Input.GetButtonDown(2, Input.ArcadeButtons.Menu)) ||
                                                      (Input.GetButtonDown(1, Input.ArcadeButtons.Menu) &&
                                                      Input.GetButton(2, Input.ArcadeButtons.Menu))) &&
                    !menu.reloadGames(GraphicsDevice)) {
                    state = MenuState.Loading;
                    _cantFetch = true;
                }


                if (myState.IsKeyDown(Keys.Z) || (Input.GetButtonDown(1, Input.ArcadeButtons.B4) &&
                                                  Input.GetButton(2, Input.ArcadeButtons.B4)) || 
                                                  (Input.GetButton(1, Input.ArcadeButtons.B4) &&
                                                  Input.GetButtonDown(2, Input.ArcadeButtons.B4))) {
                    // Switch to dev/prod
                    Client.setProduction(!Client.isProduction).Wait();

                    // And reload
                    if (!menu.reloadGames(GraphicsDevice)) {
                        state = MenuState.Loading;
                        _cantFetch = true;
                    }
                }

                if (((myState.IsKeyDown(Keys.Down)) || // Keyboard down
                     Input.GetButton(1, Input.ArcadeButtons.StickDown) || // or joystick down
                     Input.GetButton(2, Input.ArcadeButtons.StickDown))) // of either player
                {
                    menu.beginAnimUp();
                }

                if (((myState.IsKeyDown(Keys.Up)) || // Keyboard up
                     Input.GetButton(1, Input.ArcadeButtons.StickUp) || // or joystick up
                     Input.GetButton(2,
                         Input.ArcadeButtons.StickUp))) // of either player																			 // and not at top of list
                {
                    menu.beginAnimDown();
                }

                if ((myState.IsKeyDown(Keys.Enter) && lastState.IsKeyUp(Keys.Enter)) || // Keyboard Enter
                    Input.GetButtonDown(1, Input.ArcadeButtons.A1) || // or A1 button
                    Input.GetButtonDown(2, Input.ArcadeButtons.A1)) // of either player
                {
                    state = MenuState.Descritpion;
                }

                if ((myState.IsKeyDown(Keys.R) && lastState.IsKeyUp(Keys.R)) || // Keyboard R
                    (Input.GetButton(1, Input.ArcadeButtons.Menu) &&
                     Input.GetButton(2, Input.ArcadeButtons.Menu) && // OR Both Menu Buttons
                     Input.GetButton(1, Input.ArcadeButtons.B4))) // and Player 1 B4
                {
                    menu.reloadGames(GraphicsDevice);

                    state = MenuState.Input;
                }

                if (((myState.IsKeyDown(Keys.Right) && lastState.IsKeyUp(Keys.Right)) || // Keyboard Right
                    Input.GetButtonDown(1, Input.ArcadeButtons.StickRight) ||           
                    Input.GetButtonDown(2, Input.ArcadeButtons.StickRight)) &&           // OR either right stick
                    !demo)                                                               // AND demo mode is off
                {
                    menu.showTags();
                    state = MenuState.Tags;
                }

                menu.animate(gameTime);
                break;

            case MenuState.Descritpion:
                menu.descFadeIn(gameTime);
                menu.cardFadeOut(gameTime);

                if ((myState.IsKeyDown(Keys.Enter) && lastState.IsKeyUp(Keys.Enter)) || // Keyboard Enter
                    Input.GetButtonDown(1, Input.ArcadeButtons.A1) || // or A1 button
                    Input.GetButtonDown(2, Input.ArcadeButtons.A1)) // of either player
                {
                    if (menu.gameSelected().id == "error") {
                        // Don't launch the default error game
                        logger.Info("Someone tried to launch the placeholder error game");
                        break;
                    }
                    logger.Info("Launching game: " + menu.gameSelected().id + " - " + menu.gameSelected().name);
                    Client.launchGame(
                        menu.gameSelected().id
                    ).ContinueWith(res => {
                        if (res.IsCompletedSuccessfully) {
                            state = MenuState.Input;
                        }
                        else {
                            logger.Error("Failed to launch game: " + res.Exception);
                            state = MenuState.Input;
                        }
                    });

                    fadeColor = 0f;
                    _loading = true;
                    state = MenuState.Loading;
                }
                else if ((myState.IsKeyDown(Keys.RightShift) &&
                          lastState.IsKeyUp(Keys.RightShift)) || // Keyboard Rshift
                         Input.GetButtonDown(1, Input.ArcadeButtons.A2) || // or A2 button
                         Input.GetButtonDown(2, Input.ArcadeButtons.A2)) // of either player
                {
                    state = MenuState.Input;
                }

                break;
            
            case MenuState.Tags:
                menu.cardFadeOut(gameTime);

                if( ((myState.IsKeyDown(Keys.Left) && lastState.IsKeyUp(Keys.Left)) ||  // Keyboard Left
                    Input.GetButtonDown(1, Input.ArcadeButtons.StickLeft) ||            // OR Stick Left
                    Input.GetButtonDown(2, Input.ArcadeButtons.StickLeft)) &&           // of either player
                    menu.getTagCol() == 0 )                                             // AND if we are already on the left column of tags
                {
                    menu.hideTags();
                    state = MenuState.Input;
                }

                if((myState.IsKeyDown(Keys.Enter) && lastState.IsKeyUp(Keys.Enter)) ||  // Keyboard Enter
                    Input.GetButtonDown(1, Input.ArcadeButtons.A1) ||                   // OR A1
                    Input.GetButtonDown(2, Input.ArcadeButtons.A1))                     // of either player
                {
                    menu.updateTag();
                    menu.hideTags();
                    state = MenuState.Input;
                }

                menu.updateTagsMenu(myState, lastState, gameTime);

                break;
        }

        lastState = Keyboard.GetState();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        this.spriteBatch.Begin();
        GraphicsDevice.Clear(Color.Black);

        switch (state) {
            case MenuState.Launch:
            case MenuState.Input:
            case MenuState.Descritpion:
                menu.drawBackground(this.spriteBatch, BGgradient, icon, fadeColor, gameTime);
                menu.drawTitle(this.spriteBatch, Client.isProduction ? titleTexture : titleDevTexture, fadeColor);
                menu.drawCards(this.spriteBatch, cardTexture, _devcadeMenuBig);
                menu.drawDescription(this.spriteBatch, descriptionTexture, _devcadeMenuTitle, _devcadeMenuBig);
                menu.drawInstructions(this.spriteBatch, _devcadeMenuBig);
                menu.drawTagsMenu(this.spriteBatch, _devcadeMenuBig);
                break;

            case MenuState.Loading:
                menu.drawLoading(this.spriteBatch, loadingSpin, fadeColor);
                menu.drawTitle(this.spriteBatch, titleTextureWhite, fadeColor);
                if (_cantFetch)
                    menu.drawError(this.spriteBatch, _devcadeMenuBig);
                break;
            
            case MenuState.Tags:
                menu.drawBackground(this.spriteBatch, BGgradient, icon, fadeColor, gameTime);
                menu.drawTitle(this.spriteBatch, Client.isProduction ? titleTexture : titleDevTexture, fadeColor);
                menu.drawTagsMenu(this.spriteBatch, _devcadeMenuBig);
                break;
        }

        // Draw a string in the top left showing the current state. Used for debugging. TODO: Use debug tags
        //this.spriteBatch.DrawString(_devcadeMenuBig, state, new Vector2(0, 0), Color.White);

        // TODO - Fix this to work with the new Client
        // if (_client.DownloadFailed)
        //     this.spriteBatch.DrawString(
        //         _devcadeMenuBig,
        //         "There was a problem running the game.",
        //         new Vector2(10, 400),
        //         Color.Red
        //     );

        this.spriteBatch.End();

        base.Draw(gameTime);
    }

    public Task<Result<Texture2D, Exception>> loadTextureFromFile(string path) {
        if (!System.IO.File.Exists(path)) {
            return Task.FromResult(
                Result<Texture2D, Exception>.Err(new System.IO.FileNotFoundException("File not found", path)));
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
