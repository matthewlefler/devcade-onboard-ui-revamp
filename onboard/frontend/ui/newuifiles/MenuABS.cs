using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using onboard.devcade;
using onboard.util;

using Microsoft.Xna.Framework.Input;

namespace onboard.ui;

public abstract class MenuABS : IMenu {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);

    public static MenuABS instance { get; private set; }

    // The instance of TagsMenu that will be used to draw the area where the user can sort by tag
    private static TagsMenu tagsMenu;
    private static devcade.Tag allTag = new devcade.Tag("All Games", "View all available games");
    private string currentTag = allTag.name;
    // A list of all the tags
    private List<devcade.Tag> tags;
    // A dictionary that will map the current tag to a list of the cards that have that tag
    private Dictionary<string, List<MenuCard>> tagLists = new Dictionary<string, List<MenuCard>>();

    private readonly GraphicsDeviceManager _device;

    public List<DevcadeGame> gameTitles { get; private set; }
    private DevcadeGame defaultGame;

    /// <summary>
    /// The Menu Card selected
    /// </summary>
    public int itemSelected { get; set; }

    /// <summary>
    /// A dictionary of names and corresponding Menu Cards
    /// </summary>
    private Dictionary<string, MenuCard> cards { get; } = new();

    private int loadingCol;
    private int loadingRow;
    private float offset;

    /// <summary>
    /// screen width in pixels
    /// </summary>
    private int _sWidth;

    /// <summary>
    /// screen height in pixels
    /// </summary>
    private int _sHeight;
    private double scalingAmount;

    private float descX;
    private float descOpacity;
    private const float descFadeTime = 0.4f;

    private const float moveTime = 0.15f;
    private float timeRemaining;

    /// <summary>
    /// Are the Menu Cards moving up?
    /// </summary>
    private bool movingUp;
    
    /// <summary>
    /// Are the Menu Cards moving down?
    /// </summary>
    private bool movingDown;

    /// <summary>
    /// Are the Menu Cards moving left?
    /// </summary>
    private bool movingLeft;

    /// <summary>
    /// Are the Menu Cards moving right?
    /// </summary>
    private bool movingRight;

    private string devcadePath;

    //Determines how far apart each line of text is drawn vertically
    private float yscaleInstructions;

    //Both descriptions and error messages
    private float yscaleDesc;

    public MenuABS(GraphicsDeviceManager _device) {
        instance = this;
        this._device = _device;
    }

    public void Initialize() {
        // Container.OnContainerBuilt += (_, args) => {
        //     logger.Info("Running game");
        //     Container.runContainer(args);
        // };
        
        yscaleInstructions = Env.get("Y_SCALE_INSTRUCTIONS").map_or(0.4f, float.Parse);
        yscaleDesc = Env.get("Y_SCALE_DESC").map_or(0.4f, float.Parse);
        devcadePath = Env.get("DEVCADE_PATH").unwrap_or("/tmp/devcade");
        defaultGame = new DevcadeGame {
            name = "Error",
            description = "There was a problem loading games from the API. Please check the logs for more information.",
            id = "error",
            author = "None",
        };
        updateDims(_device);
    }

    public void LoadContent(ContentManager contentManager) {
        // Setup banner finished callback
        Client.onBannerFinished += (_, game) => {
            Devcade.instance.loadTextureFromFile($"{devcadePath}/{game.id}/banner.png").ContinueWith(t => {
                if (t.IsCompletedSuccessfully && t.Result.is_ok() && cards.ContainsKey(game.id)) {
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
    }

    public void Update(GameTime gameTime) {
        // Comment to make the linter happy
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        // Comment to make the linter happy
    }

    public void Unload() {
        // Comment to make the linter happy
    }

    public void updateDims(GraphicsDeviceManager _graphics) {
        // Get the screen width and height. If none are set, set the to the default values
        _sWidth = Env.get("VIEW_WIDTH").map_or_else(() => 1920, int.Parse);
        _sHeight = Env.get("VIEW_HEIGHT").map_or_else(() => 1080, int.Parse);


        // This is a constant value that is used to scale the UI elements if the resolution is smaller than 2560x1080. Results may vary if the same aspect ratio is not kept
        scalingAmount = Math.Sqrt(_sHeight * _sWidth / (double)(1080 * 2560)); 


        _graphics.PreferredBackBufferHeight = _sHeight;
        _graphics.PreferredBackBufferWidth = _sWidth;
        _graphics.ApplyChanges();
    }

    // Empties the gameTitles and cards lists. Called when the reload buttons are pressed
    public void clearGames() {
        gameTitles?.Clear();
        cards?.Clear();
        tagLists.Clear();
        itemSelected = 0;
    }

    public bool reloadGames(GraphicsDevice device, bool clear = true) {
        if (clear)
            clearGames();
        // Reload the .env file every time the games are reloaded to make sure that the demo mode is up to date
        Env.load("../.env");
        itemSelected = 0;
        
        var errorList = new List<DevcadeGame> { defaultGame };
        
        setTags();
        
        // Public access to state is definitely a good idea (this whole thing needs a refactor)
        Devcade.instance.state = Devcade.MenuState.Loading;
        Devcade.instance._loading = true;

        // gameTask is 'never used' but tasks in C# are eager, so it doesn't need to be awaited to run. 
        Task gameTask = Client.getGameList()
            .ContinueWith(t => {
                if (!t.IsCompletedSuccessfully) {
                    logger.Error($"Failed to fetch game list: {t.Exception}");
                    gameTitles = errorList;
                    return;
                }

                var res = t.Result.into_result<List<DevcadeGame>>();
                if (!res.is_ok()) {
                    logger.Error($"Failed to fetch game list: {res.err().unwrap()}");
                    gameTitles = errorList;
                    return;
                }

                logger.Info("Got game list, setting titles");
                gameTitles = res.unwrap();
            })
            .ContinueWith(_ => {
                logger.Info("Setting cards");
                setCards(device);
                Devcade.instance.state = Devcade.MenuState.Input;
                Devcade.instance._loading = false;
            })
            .WaitAsync(TimeSpan.FromSeconds(10))
            .ContinueWith(t => {
                if (t.IsCompletedSuccessfully) return;
                // Take timed out, so we need to set the state back to input and game titles to the error list
                Devcade.instance.state = Devcade.MenuState.Input;
                Devcade.instance._loading = false;
                gameTitles = errorList;
                setCards(device);
            });

        // Since this is now done asynchronously, the return means nothing.
        return true;
    }

    public void setTags() {
        if (tags == null || tags.Count == 0) {
            logger.Info("Getting tags from API (this should be only once, but maybe every reload of the game list?)");
            tags = Client.getTags().Result.into_result<List<devcade.Tag>>().unwrap_or(new List<devcade.Tag>());
            tags.Insert(0, allTag); // Make all tag appear at the top of the list
        }

        if (tagLists.Keys.Count != 0) return;
        
        // tagLists gets cleared every time the games are reloaded?!
        foreach (Tag tag in tags) {
            tagLists.Add(tag.name, new List<MenuCard>());
        }
    }
    
    public void setCards(GraphicsDevice graphics) {
        for (int i = 0; i < gameTitles.Count; i++) {
            devcade.DevcadeGame game = gameTitles[i];

            MenuCard newCard;                                           // TODO: Change this based on the selected THEME

            // Start downloading the textures
            if (game.id != "error") {
                // don't download the banner for the default game
                Client.downloadBanner(game.id);
            } // check if /tmp/ has the banner
            
            string bannerPath = $"{Env.get("DEVCADE_PATH").unwrap_or_else(() => Env.get("HOME").unwrap() + "/.devcade")}/{game.id}/banner.png";
            if (File.Exists(bannerPath)) {
                try {
                    Texture2D banner = Texture2D.FromStream(graphics, File.OpenRead(bannerPath));   
                    newCard = new MenuCard(i * -1, banner, game);
                }
                catch (InvalidOperationException e) {
                    logger.Warn($"Unable to set card.{e}");
                    newCard = new MenuCard(i * -1, null, game);
                }
            }
            else {
                // If the banner doesn't exist, use a placeholder until it can be downloaded later.
                newCard = new MenuCard(i * -1, null, game);
            }

            cards.Add(game.id, newCard);

            // Add the reference to the card to the proper lists within the tag dictionary
            foreach(devcade.Tag tag in game.tags) {
                tagLists[tag.name].Add(newCard);
            }  

            tagLists[allTag.name].Add(newCard);
        }
        
        // shuffle lists
        Random rand = new Random();
        foreach(string key in tagLists.Keys) {
            tagLists[key] = tagLists[key].OrderBy(a => rand.Next()).ToList();
        }

        // If demo mode is on, then set the tag to be curated instead of all
        if (Env.get("DEMO_MODE").map_or(false, bool.Parse)) {
            updateTag("Curated");
        } else {
            updateTag(currentTag);
        }

        MenuCard.cardX = 0;
        descX = _sWidth * 1.5f;
    }

    public devcade.DevcadeGame gameSelected() {
        return tagLists[currentTag].ElementAt(itemSelected).game;
    }

    /* 
    * Tags Menu Related Functions
    */
    
    // MAKE FONTS, TEXTURES, AND DIMS FIELDS WITHIN TAGS MENU
    public void initializeTagsMenu(Texture2D cardTexture, SpriteFont font) {
        tagsMenu = new TagsMenu(tags.ToArray(), cardTexture, font, new Vector2(_sWidth, _sHeight), scalingAmount);
    }

    public void drawTagsMenu(SpriteBatch spriteBatch, SpriteFont font) {
        tagsMenu.Draw(spriteBatch, font, new Vector2(_sWidth, _sHeight));
    }

    public void updateTagsMenu(KeyboardState currentState, KeyboardState lastState, GameTime gameTime) {
        tagsMenu.Update(currentState, lastState, gameTime);
    }

    public int getTagCol() { return tagsMenu.getCurrentCol(); }

    public void updateTag(string tag) {
        this.currentTag = tag;

        // Reset the listPos of each card within the list of currently visible cards
        List<MenuCard> visibleCards = tagLists[currentTag];
        for (int i=0; i<visibleCards.Count; i++) {
            visibleCards[i].setListPos(i * -1);
        }
        itemSelected = 0;
    }

    public void updateTag() { updateTag(tagsMenu.getCurrentTag().name); }

    public void showTags() {
        tagsMenu.setIsShowing(true);
        tagsMenu.resetxVel();
    }

    public void hideTags() {
        tagsMenu.setIsShowing(false);
        tagsMenu.resetxVel();
    }

    public void drawBackground(SpriteBatch _spriteBatch, Texture2D BGgradient, Texture2D icon, float col,
        GameTime gameTime) {
        _spriteBatch.Draw(
            BGgradient,
            new Rectangle(0, 0, _sWidth, _sHeight),
            null,
            new Color(col, col, col),
            0f,
            new Vector2(0, 0),
            SpriteEffects.None,
            0f
        );

        offset += 150 * (float)gameTime.ElapsedGameTime.TotalSeconds / 2; // Divided by two to make the animation a little slower
        if (offset > 150) {
            offset = 0;
        }

        int numColumns = _sWidth / 150 + 1;
        int numRows = _sHeight / 150 + 1;

        for (int row = -150; row <= numRows * 150; row += 150) // Starts at -150 to draw an extra row above the screen
        {
            for (int column = 0; column <= numColumns * 150; column += 150) {
                _spriteBatch.Draw(
                    icon,
                    new Vector2(column - offset, row + offset),
                    null,
                    new Color(col, col, col),
                    0f,
                    new Vector2(0, 0),
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }

    public void drawTitle(SpriteBatch _spriteBatch, Texture2D titleTexture, float col) {
        // The title will always be scaled to fit the width of the screen. The height follows scaling based on how
        // much the title was stretched horizontally
        float scaling = (float)_sWidth / titleTexture.Width;
        _spriteBatch.Draw(
            titleTexture,
            new Rectangle(0, 0, _sWidth, (int)(titleTexture.Height * scaling)),
            null,
            new Color(col, col, col),
            0f,
            new Vector2(0, 0),
            SpriteEffects.None,
            0f
        );
    }

    public void drawInstructions(SpriteBatch _spriteBatch, SpriteFont font) {
        List<string> instructions = wrapText("Press the Red button to play! Press both Black Buttons to refresh", 35);
        float instructSize = font.MeasureString(instructions[0]).Y;
        float yPos = (float)(500 * scalingAmount);
        for (int i = 0; i < instructions.Count; i++) {
            writeString(_spriteBatch, font, instructions[i], new Vector2(_sWidth / 2.0f, yPos + instructSize * i * yscaleInstructions), 1f);
        }
    }


    public void drawError(SpriteBatch _spriteBatch, SpriteFont font) {
        const string error = "Error: Could not get game list. Is API Down? Press both black buttons to reload.";
        var wrappedError = wrapText(error, 35); 
        float errorSize = font.MeasureString(wrappedError[0]).Y;
        float yPos = (float)(500 * scalingAmount);

        for (int i = 0; i < wrappedError.Count; i++) {
            writeString(_spriteBatch, font, wrappedError[i], new Vector2(_sWidth / 2.0f, yPos + errorSize * i * yscaleDesc), 1f, Color.Red);
        }
    }


    public void drawLoading(SpriteBatch _spriteBatch, Texture2D loadingSpin, float col) {
        if (loadingCol > 4) {
            loadingCol = 0;
            loadingRow++;
            if (loadingRow > 4) {
                loadingRow = 0;
            }
        }

        // Creates a boundary to get the right spot on the spritesheet to be drawn
        Rectangle spriteBounds = new(
            600 * loadingCol,
            600 * loadingRow,
            600,
            600
        );

        _spriteBatch.Draw(
            loadingSpin,
            new Vector2(_sWidth / 2.0f, _sHeight / 2.0f + 150),
            spriteBounds,
            new Color(col, col, col),
            0f,
            new Vector2(300, 300),
            1.5f,
            SpriteEffects.None,
            0f
        );

        loadingCol++;
    }

    public void descFadeIn(GameTime gameTime) {
        // This does the slide in animation, starting off screen and moving to the middle over 0.8 seconds
        if (descOpacity >= 1) return;
        descX -= _sWidth / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
        descOpacity += 1 / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void descFadeOut(GameTime gameTime) {
        // This does the slide out animation, starting in the middle of the screen and moving it off over 0.8 seconds
        if (descOpacity <= 0) return;
        descX += _sWidth / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
        descOpacity -= 1 / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public abstract void cardFadeIn(GameTime gameTime);

    public abstract void cardFadeOut(GameTime gameTime);

    public void drawDescription(SpriteBatch _spriteBatch, Texture2D descTexture, SpriteFont titleFont,
        SpriteFont descFont) {
        // First, draw the backdrop of the description
        Vector2 descPos = new Vector2(descX, _sHeight / 2 + (int)(descTexture.Height * scalingAmount / 6));

        _spriteBatch.Draw(descTexture,
            descPos,
            null,
            new Color(descOpacity, descOpacity, descOpacity, descOpacity),
            0f,
            new Vector2(descTexture.Width / 2.0f, descTexture.Height / 2.0f),
            (float)(1f * scalingAmount),
            SpriteEffects.None,
            0f
        );

        // Wraps the description text to fit within the box
        // Then draws the description
        List<string> wrapDesc = wrapText(gameSelected().description, 35);
        float descHeight = descFont.MeasureString(gameSelected().description).Y;

        int lineNum = 0;
        foreach (string line in wrapDesc) {
            writeString(_spriteBatch,
                descFont,
                line,
                new Vector2(descPos.X, (float)(descPos.Y - descTexture.Height * scalingAmount / 5 +
                                               descHeight * lineNum * yscaleDesc)),
                descOpacity
            );
            lineNum++;
        }

        // Write the game's title
        writeString(_spriteBatch,
            titleFont,
            gameSelected().name,
            new Vector2(descPos.X, descPos.Y - (int)(descTexture.Height * scalingAmount / 2.5f)),
            descOpacity
        );

        // String author = (gameSelected().user.user_type == UserType.CSH) ? gameSelected().user.id : gameSelected().user.email.Remove(gameSelected().user.email.IndexOf('@'));
        // Write the game's author
        writeString(_spriteBatch,
            descFont,
            "By: " + gameSelected().author,
            new Vector2(descPos.X, descPos.Y - (int)(descTexture.Height * scalingAmount / 3)),
            descOpacity
        );

        // Instructions to go back
        writeString(_spriteBatch,
            descFont,
            "Press the Blue button to return",
            new Vector2(descPos.X, descPos.Y + (int)(descTexture.Height * scalingAmount / 2 - descHeight)),
            descOpacity
        );
    }

    /// <summary>
    /// Takes an input string and returns a list of strings <br />
    /// Each string in the list is not longer than the line limit, but can be less 
    /// </summary>
    /// <param name="text"> input string </param>
    /// <param name="lineLimit"> the number of characters per line </param>
    /// <returns></returns>
    public static List<string> wrapText(string text, int lineLimit) {
        List<string> lines = new List<string>();
        StringBuilder currentLine = new StringBuilder();
        string[] words = text.Split(' ');

        foreach (string word in words) {
            if (currentLine.Length + word.Length + 1 > lineLimit) {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
            }
            currentLine.Append(word + " ");
        }

        if (currentLine.Length > 0) {
            lines.Add(currentLine.ToString());
        }

        return lines;
    }


    public void writeString(SpriteBatch _spriteBatch, SpriteFont font, string str, Vector2 pos, float opacity,
        Color color) {
        Vector2 strSize = font.MeasureString(str);

        _spriteBatch.DrawString(font,
            str,
            pos,
            color,
            0f,
            new Vector2(strSize.X / 2, strSize.Y / 2),
            (float)(1f * scalingAmount),
            SpriteEffects.None,
            0f
        );
    }

    public void writeString(SpriteBatch _spriteBatch, SpriteFont font, string str, Vector2 pos, float opacity) {
        Vector2 strSize = font.MeasureString(str);

        _spriteBatch.DrawString(font,
            str,
            pos,
            new Color(opacity, opacity, opacity, opacity),
            0f,
            new Vector2(strSize.X / 2, strSize.Y / 2),
            (float)(1f * scalingAmount),
            SpriteEffects.None,
            0f
        );
    }

    public void drawCards(SpriteBatch _spriteBatch, Texture2D cardTexture, SpriteFont font) {
        //TODO: spritebatch sorting options
        foreach (MenuCard card in tagLists[currentTag])
        {
            card.DrawSelf(_spriteBatch, cardTexture, _sHeight, scalingAmount);
        }
    }

    // to add an animation, add a bool for if the animation is running
    // add a function to begin it
    // then add an if statment in the animate function to execute the animation 

    public void beginAnimUp() {
        // scrolling beginds only if it is not already moving, and not at bottom of list
        if (movingUp || movingDown || movingLeft || movingRight || itemSelected >= tagLists[currentTag].Count - 1) return; 
        
        foreach (MenuCard card in tagLists[currentTag]) {
            card.listPos++;
            //card.layer = (float)Math.Abs(card.listPos) / 4;
        }

        movingUp = true;
        itemSelected++; // Update which game is currently selected, so the proper one will be launched
    }

    public void beginAnimDown() {
        // scrolling begins only if it is not already moving, and not at the top of the list
        if (movingDown || movingUp || movingLeft || movingRight || itemSelected <= 0) return; 
        foreach (MenuCard card in tagLists[currentTag]) {
            card.listPos--;
            //card.layer = (float)Math.Abs(card.listPos) / 4;
        }

        movingDown = true;
        itemSelected--;
    }

    public void beginAnimLeft() {
        // scrolling beginds only if it is not already moving, and not at bottom of list
        if (movingUp || movingDown || movingLeft || movingRight || itemSelected >= tagLists[currentTag].Count - 1) return; 
        
        foreach (MenuCard card in tagLists[currentTag]) {
            card.listPos++;
            //card.layer = (float)Math.Abs(card.listPos) / 4;
        }

        movingLeft = true;
        itemSelected++; // Update which game is currently selected, so the proper one will be launched
    }

    public void beginAnimRight() {
        // scrolling begins only if it is not already moving, and not at the top of the list
        if (movingDown || movingUp || movingLeft || movingRight || itemSelected <= 0) return; 
        foreach (MenuCard card in tagLists[currentTag]) {
            card.listPos--;
            //card.layer = (float)Math.Abs(card.listPos) / 4;
        }

        movingRight = true;
        itemSelected--;
    }

    public void animate(GameTime gameTime) {
        if (timeRemaining >
            0) // Continues to execute the following code as long as the animation is playing AND max time isn't reached
        {
            if (movingUp) {
                foreach (MenuCard card in cards.Values) {
                    card.moveUp(gameTime);
                }
            }

            else if (movingDown) {
                foreach (MenuCard card in cards.Values) {
                    card.moveDown(gameTime);
                }
            }

            else if (movingLeft) {
                foreach (MenuCard card in cards.Values) {
                    card.moveLeft(gameTime);
                }
            }
            
            else if (movingRight) {
                foreach (MenuCard card in cards.Values) {
                    card.moveRight(gameTime);
                }
            }

        }

        else // Once timeleft reaches 0, finish anim. 
        {
            movingUp = false;
            movingDown = false;
            movingLeft = false;
            movingRight = false;
        }
    }
}