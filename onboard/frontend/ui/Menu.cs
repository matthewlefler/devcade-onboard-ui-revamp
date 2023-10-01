using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using onboard.devcade;
using onboard.util;

using Microsoft.Xna.Framework.Input;

namespace onboard.ui;

public class Menu : IMenu {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);

    public static Menu instance { get; private set; }

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
    public int itemSelected { get; set; }
    private Dictionary<string, MenuCard> cards { get; } = new();

    private int loadingCol;
    private int loadingRow;
    private float offset;

    private int _sWidth;
    private int _sHeight;
    private double scalingAmount;

    private const float moveTime = 0.15f;
    private float timeRemaining;

    private float descX;
    private float descOpacity;
    private const float descFadeTime = 0.4f;

    private bool movingUp;
    private bool movingDown;

    private string devcadePath;

    public Menu(GraphicsDeviceManager _device) {
        instance = this;
        this._device = _device;
    }

    public void Initialize() {
        // Container.OnContainerBuilt += (_, args) => {
        //     logger.Info("Running game");
        //     Container.runContainer(args);
        // };
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
        try {
            var gameTask = Client.getGameList();
            // wait for the task to finish or timeout
            if (!gameTask.Wait(TimeSpan.FromSeconds(2))) {
                logger.Error("Failed to fetch game list: Timed out");
                gameTitles = new List<DevcadeGame> { defaultGame };
            }
            else {
                gameTitles = gameTask.Result.into_result<List<DevcadeGame>>()
                    .unwrap_or(new List<DevcadeGame> { defaultGame });
            }

            setCards(device);
        }
        catch (AggregateException e) {
            logger.Error($"Failed to fetch game list: {e}");
            return false;
        }

        return true;
    }
    
    public void setCards(GraphicsDevice graphics) {
        // Get all of the tags from the API
        tags = Client.getTags().Result.into_result<List<devcade.Tag>>().unwrap_or(new List<devcade.Tag>());
        tags.Insert(0, allTag); // Make all tag appear at the top of the list
        foreach(devcade.Tag tag in tags) {
            tagLists.Add(tag.name, new List<MenuCard>());
        }

        for (int i = 0; i < gameTitles.Count; i++) {
            devcade.DevcadeGame game = gameTitles[i];

            MenuCard newCard;

            // Start downloading the textures
            if (game.id != "error") {
                // don't download the banner for the default game
                Client.downloadBanner(game.id);
            } // check if /tmp/ has the banner

            string bannerPath = $"/tmp/devcade/{game.id}/banner.png";
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
        List<string> instructions = wrapText("Press the Red button to play! Press both Black Buttons to refresh", 25);
        float instructSize = font.MeasureString("Press the Red button to play! Press both Black Buttons to refresh").Y;
        int lineNum = 0;

        foreach (string line in instructions) {
            writeString(_spriteBatch,
                font,
                line,
                new Vector2(_sWidth / 2.0f,
                    (int)(500 * scalingAmount) +
                    instructSize * lineNum), // 500 is the height of the title, this string goes right beneath that
                1f
            );
            lineNum++;
        }
    }


    public void drawError(SpriteBatch _spriteBatch, SpriteFont font) {
        const string error = "Error: Could not get game list. Is API Down? Press both black buttons to reload.";
        var instructions = wrapText(error, 25);
        float instructSize = font.MeasureString(error).Y;
        int lineNum = 0;

        foreach (string line in instructions) {
            writeString(_spriteBatch,
                font,
                line,
                new Vector2(_sWidth / 2.0f,
                    (int)(500 * scalingAmount) +
                    instructSize * lineNum), // 500 is the height of the title, this string goes right beneath that
                1f,
                Color.Red
            );
            lineNum++;
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

    public void cardFadeIn(GameTime gameTime) {
        if (MenuCard.cardOpacity >= 1) return;
        MenuCard.cardX += _sWidth / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
        MenuCard.cardOpacity += 1 / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void cardFadeOut(GameTime gameTime) {
        if (MenuCard.cardOpacity <= 0) return;
        MenuCard.cardX -= _sWidth / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
        MenuCard.cardOpacity -= 1 / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

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
        List<string> wrapDesc = wrapText(gameSelected().description, 25);
        float descHeight = descFont.MeasureString(gameSelected().description).Y;

        int lineNum = 0;
        foreach (string line in wrapDesc) {
            writeString(_spriteBatch,
                descFont,
                line,
                new Vector2(descPos.X, (float)(descPos.Y - descTexture.Height * scalingAmount / 5 +
                                               descHeight * lineNum)),
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

    public static List<string> wrapText(string desc, int lineLimit) {
        // This function should take in a description and return a list of lines to print to the screen
        string[] words = desc.Split(' '); // Split the description up by words
        List<string> lines = new() { ' '.ToString() }; // Create a list to return 

        int currentLine = 0;
        StringBuilder currentLineStr = new();
        foreach (string word in words) {
            // For each word in the description, we add it to a line. 
            currentLineStr.Append(word + ' ');
            // Once that line is over the limit of  characters, we move to the next line
            if (currentLineStr.Length <= lineLimit) {
                continue;
            }

            lines[currentLine] = currentLineStr.ToString();
            currentLineStr.Clear();
            currentLine++;
            lines.Add(' '.ToString());
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
        // I still have no idea why the layerDepth does not work\
        foreach (MenuCard card in tagLists[currentTag].Where(card => Math.Abs(card.listPos) == 4))
        {
            card.DrawSelf(_spriteBatch, cardTexture, _sHeight, scalingAmount);
        }
        foreach (MenuCard card in tagLists[currentTag].Where(card => Math.Abs(card.listPos) == 3))
        {
            card.DrawSelf(_spriteBatch, cardTexture, _sHeight, scalingAmount);
        }
        foreach (MenuCard card in tagLists[currentTag].Where(card => Math.Abs(card.listPos) == 2))
        {
            card.DrawSelf(_spriteBatch, cardTexture, _sHeight, scalingAmount);
        }
        foreach (MenuCard card in tagLists[currentTag].Where(card => Math.Abs(card.listPos) == 1))
        {
            card.DrawSelf(_spriteBatch, cardTexture, _sHeight, scalingAmount);
        }
        foreach (MenuCard card in tagLists[currentTag].Where(card => Math.Abs(card.listPos) == 0))
        {
            card.DrawSelf(_spriteBatch, cardTexture, _sHeight, scalingAmount);
        }
    }

    public void beginAnimUp() {
        // scrolling beginds only if it is not already moving, and not at bottom of list
        if (movingUp || movingDown || itemSelected >= tagLists[currentTag].Count - 1) return; 
        
        foreach (MenuCard card in tagLists[currentTag]) {
            card.listPos++;
            //card.layer = (float)Math.Abs(card.listPos) / 4;
        }

        timeRemaining = moveTime; // Time remaining in the animation begins at the total expected move time
        movingUp = true;
        itemSelected++; // Update which game is currently selected, so the proper one will be launched
    }

    public void beginAnimDown() {
        // scrolling begins only if it is not already moving, and not at the top of the list
        if (movingDown || movingUp || itemSelected <= 0) return; 
        foreach (MenuCard card in tagLists[currentTag]) {
            card.listPos--;
            //card.layer = (float)Math.Abs(card.listPos) / 4;
        }

        timeRemaining = moveTime; // Time remaining in the animation begins at the total expected move time
        movingDown = true;
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

            timeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds; // Decrement time until it reaches zero
        }

        else // Once timeleft reaches 0, finish anim. 
        {
            movingUp = false;
            movingDown = false;
        }
    }
}
