using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace onboard
{
    public class Menu
    {
        private readonly GraphicsDeviceManager _device;

        public static Menu instance;

        // Synonymous with cards list from my project
        // Will have to convert this to a list of MenuCards
        public List<DevcadeGame> gameTitles { get; set; }
        private Dictionary<string, MenuCard> cards { get; set; } = new Dictionary<string, MenuCard>();
        public int itemSelected = 0;

        private int loadingCol = 0;
        private int loadingRow = 0;
        private float offset = 0;

        private int _sWidth;
        private int _sHeight;
        private double scalingAmount = 0;

        private const float moveTime = 0.15f; // This is the total time the scrolling animation takes
        private float timeRemaining = 0f;

        private float descX; // The X position of the description box is saved here. Used to animate the box
        private float descOpacity = 0f; // The opacity of the description box
        private const float descFadeTime = 0.4f; // Time it takes to make the description box fade in/out

        public bool movingUp;
        public bool movingDown;

        public Menu(GraphicsDeviceManager graph)
        {
            _device = graph;
            instance = this;
        }

        public void updateDims(GraphicsDeviceManager _graphics)
        {
            // Get the screen width and height. If none are set, set the to the default values
            _sWidth = Int32.Parse(Environment.GetEnvironmentVariable("VIEW_WIDTH") ?? "1080");
            _sHeight = Int32.Parse(Environment.GetEnvironmentVariable("VIEW_HEIGHT") ?? "2560");

            scalingAmount = Math.Sqrt((_sHeight * _sWidth) / (double)(1920 * 1080)); // This is a constant value that is used to scale the UI elements

            _graphics.PreferredBackBufferHeight = _sHeight;
            _graphics.PreferredBackBufferWidth = _sWidth;
            _graphics.ApplyChanges();
        }

        // Empties the gameTitles and cards lists. Called when the reload buttons are pressed
        public void clearGames()
        {
            try
            {
                gameTitles.Clear();
                cards.Clear();
                itemSelected = 0;
            }
            catch (System.NullReferenceException e)
            {
                Console.WriteLine($"No game titles or cards yet. {e}");
            }
        }

        public bool reloadGames(GraphicsDevice device, DevcadeClient client, bool clear = true)
        {
            if (clear)
                clearGames();

            try
            {
                gameTitles = client.GetGames();
                setCards(client, device);
            }
            catch (System.AggregateException e)
            {
                Console.WriteLine($"Failed to fetch games: {e}");
                return false;
            }
            return true;
        }

        public void setCards(DevcadeClient _client, GraphicsDevice graphics)
        {
            for (int i = 0; i < gameTitles.Count; i++)
            {
                DevcadeGame game = gameTitles[i];
                // Start downloading the textures
                _client.getBannerAsync(game);
                // check if /tmp/ has the banner
                string bannerPath = $"/tmp/{game.id}Banner";
                if (File.Exists(bannerPath))
                {
                    try
                    {
                        Texture2D banner = Texture2D.FromStream(graphics, File.OpenRead(bannerPath));
                        cards.Add(game.id, new MenuCard(i * -1, game.name, banner));
                    }
                    catch (System.InvalidOperationException e)
                    {
                        Console.WriteLine($"Unable to set card.{e}");
                        cards.Add(game.id, new MenuCard(i * -1, game.name, null));
                    }
                }
                else
                {
                    // If the banner doesn't exist, use a placeholder until it can be downloaded later.
                    cards.Add(game.id, new MenuCard(i * -1, game.name, null));
                }

            }
            MenuCard.cardX = 0;
            descX = _sWidth * 1.5f;
        }

        public void notifyTextureAvailable(string gameID)
        {
            if (!cards.ContainsKey(gameID)) return;
            // check if file exists
            string bannerPath = $"/tmp/{gameID}Banner";
            if (!File.Exists(bannerPath))
            {
                Console.WriteLine("Bozo");
                return;
            }
            // load texture
            Texture2D banner = Texture2D.FromStream(_device.GraphicsDevice, File.OpenRead(bannerPath));
            // set texture
            cards[gameID].setTexture(banner);
        }

        public DevcadeGame gameSelected()
        {
            return gameTitles.ElementAt(itemSelected);
        }

        public void drawBackground(SpriteBatch _spriteBatch, Texture2D BGgradient, Texture2D icon, float col, GameTime gameTime)
        {
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

            // Idea for scrolling icons: This surprisingly worked with no hassle
            // For row in range (# of rows)
            // For column in range (# of cols)
            // Draw a single icon. The location is based on it's row & column. Both of these values will incremement by 150 (150 is the size of the icon sprite)
            // Added to the X and Y values will be it's offset, which is calculated by 150 * time elapsed. Making it move 150 px in one second
            // Once offset reaches 150, it goes back to zero. 

            offset += (150 * (float)gameTime.ElapsedGameTime.TotalSeconds) / 2; // Divided by two to make the animation a little slower
            if (offset > 150)
            {
                offset = 0;
            }

            int numColumns = (_sWidth / 150) + 1;
            int numRows = (_sHeight / 150) + 1;

            for (int row = -150; row <= numRows * 150; row += 150) // Starts at -150 to draw an extra row above the screen
            {
                for (int column = 0; column <= numColumns * 150; column += 150)
                {
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

        public void drawTitle(SpriteBatch _spriteBatch, Texture2D titleTexture, float col)
        {
            float scaling = (float)_sWidth / titleTexture.Width; // The title will always be scaled to fit the width of the screen. The height follows scaling based on how much the title was stretched horizontally
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

        public void drawInstructions(SpriteBatch _spriteBatch, SpriteFont font)
        {
            List<string> instructions = wrapText("Press the Red button to play! Press both Black Buttons to refresh", 25);
            float instructSize = font.MeasureString("Press the Red button to play! Press both Black Buttons to refresh").Y;
            int lineNum = 0;

            foreach (string line in instructions)
            {
                writeString(_spriteBatch,
                    font,
                    line,
                    new Vector2(_sWidth / 2.0f, (int)(500 * scalingAmount) + (instructSize * lineNum)), // 500 is the height of the title, this string goes right beneath that
                    1f
                );
                lineNum++;
            }
        }


        public void drawError(SpriteBatch _spriteBatch, SpriteFont font)
        {
            string error = "Error: Could not get game list. Is API Down? Press both black buttons to reload.";
            List<string> instructions = wrapText(error, 25);
            float instructSize = font.MeasureString(error).Y;
            int lineNum = 0;

            foreach (string line in instructions)
            {
                writeString(_spriteBatch,
                    font,
                    line,
                    new Vector2(_sWidth / 2.0f, (int)(500 * scalingAmount) + (instructSize * lineNum)), // 500 is the height of the title, this string goes right beneath that
                    1f,
                    Color.Red
                );
                lineNum++;
            }
        }

        public void drawLoading(SpriteBatch _spriteBatch, Texture2D loadingSpin, float col)
        {
            if (loadingCol > 4)
            {
                loadingCol = 0;
                loadingRow++;
                if (loadingRow > 4)
                {
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

        public void descFadeIn(GameTime gameTime)
        {
            // This does the slide in animation, starting off screen and moving to the middle over 0.8 seconds
            if (!(descOpacity < 1)) return;
            descX -= (_sWidth) / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
            descOpacity += (1 / descFadeTime) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void descFadeOut(GameTime gameTime)
        {
            // This does the slide out animation, starting in the middle of the screen and moving it off over 0.8 seconds
            if (!(descOpacity > 0)) return;
            descX += (_sWidth) / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
            descOpacity -= (1 / descFadeTime) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void cardFadeIn(GameTime gameTime)
        {
            if (!(MenuCard.cardOpacity < 1)) return;
            MenuCard.cardX += (_sWidth) / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
            MenuCard.cardOpacity += (1 / descFadeTime) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void cardFadeOut(GameTime gameTime)
        {
            if (!(MenuCard.cardOpacity > 0)) return;
            MenuCard.cardX -= (_sWidth) / descFadeTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
            MenuCard.cardOpacity -= (1 / descFadeTime) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void drawDescription(SpriteBatch _spriteBatch, Texture2D descTexture, SpriteFont titleFont, SpriteFont descFont)
        {
            // First, draw the backdrop of the description
            // I'm not sure why I added descTexture.Height/6 to the position. It is to make the image draw slightly below the center of the screen, but there is probably a better way to do this?
            Vector2 descPos = new Vector2(descX, _sHeight / 2 + (int)((descTexture.Height * scalingAmount) / 6));

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
            foreach (string line in wrapDesc)
            {
                writeString(_spriteBatch,
                descFont,
                line,
                new Vector2(descPos.X, (float)(descPos.Y - (descTexture.Height * scalingAmount) / 5 +
                                                descHeight * lineNum)),
                descOpacity
                );
                lineNum++;
            }

            // Write the game's title
            writeString(_spriteBatch,
                titleFont,
                gameSelected().name,
                new Vector2(descPos.X, descPos.Y - (int)((descTexture.Height * scalingAmount) / 2.5f)),
                descOpacity
            );

            String author = (gameSelected().user.user_type == "CSH") ? gameSelected().user.id : gameSelected().user.email.Remove(gameSelected().user.email.IndexOf('@'));
            // Write the game's author
            writeString(_spriteBatch,
                descFont,
                "By: " + author,
                new Vector2(descPos.X, descPos.Y - (int)((descTexture.Height * scalingAmount) / 3)),
                descOpacity
            );

            // Instructions to go back
            writeString(_spriteBatch,
                descFont,
                "Press the Blue button to return",
                new Vector2(descPos.X, descPos.Y + (int)((descTexture.Height * scalingAmount) / 2 - descHeight)),
                descOpacity
            );

        }

        public static List<string> wrapText(string desc, int lineLimit)
        {
            // This function should take in a description and return a list of lines to print to the screen
            string[] words = desc.Split(' '); // Split the description up by words
            List<string> lines = new() { ' '.ToString() }; // Create a list to return 

            int currentLine = 0;
            foreach (string word in words)
            {
                // For each word in the description, we add it to a line. 
                lines[currentLine] += word + ' ';
                // Once that line is over the limit of  characters, we move to the next line
                if (lines[currentLine].Length <= lineLimit) continue;
                currentLine++;
                lines.Add(' '.ToString());
            }

            return lines;
        }

        public void writeString(SpriteBatch _spriteBatch, SpriteFont font, string str, Vector2 pos, float opacity, Color color)
        {
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

        public void writeString(SpriteBatch _spriteBatch, SpriteFont font, string str, Vector2 pos, float opacity)
        {
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

        public void drawCards(SpriteBatch _spriteBatch, Texture2D cardTexture, SpriteFont font)
        {
            // I still have no idea why the layerDepth does not work
            foreach (MenuCard card in cards.Values.Where(card => Math.Abs(card.listPos) == 4))
            {
                card.DrawSelf(_spriteBatch, cardTexture, font, _sHeight, scalingAmount);
            }
            foreach (MenuCard card in cards.Values.Where(card => Math.Abs(card.listPos) == 3))
            {
                card.DrawSelf(_spriteBatch, cardTexture, font, _sHeight, scalingAmount);
            }
            foreach (MenuCard card in cards.Values.Where(card => Math.Abs(card.listPos) == 2))
            {
                card.DrawSelf(_spriteBatch, cardTexture, font, _sHeight, scalingAmount);
            }
            foreach (MenuCard card in cards.Values.Where(card => Math.Abs(card.listPos) == 1))
            {
                card.DrawSelf(_spriteBatch, cardTexture, font, _sHeight, scalingAmount);
            }
            foreach (MenuCard card in cards.Values.Where(card => Math.Abs(card.listPos) == 0))
            {
                card.DrawSelf(_spriteBatch, cardTexture, font, _sHeight, scalingAmount);
            }
        }

        public void beginAnimUp()
        {
            if (movingUp || movingDown || itemSelected >= gameTitles.Count - 1) return; // scrolling beginds only if it is not already moving, and not at bottom of list
            foreach (MenuCard card in cards.Values)
            {
                card.listPos++;
                //card.layer = (float)Math.Abs(card.listPos) / 4;
            }
            timeRemaining = moveTime; // Time remaining in the animation begins at the total expected move time
            movingUp = true;
            itemSelected++; // Update which game is currently selected, so the proper one will be launched
        }

        public void beginAnimDown()
        {
            if (movingDown || movingUp || itemSelected <= 0) return; // scrolling begins only if it is not already moving, and not at the top of the list
            foreach (MenuCard card in cards.Values)
            {
                card.listPos--;
                //card.layer = (float)Math.Abs(card.listPos) / 4;
            }
            timeRemaining = moveTime; // Time remaining in the animation begins at the total expected move time
            movingDown = true;
            itemSelected--;
        }

        public void animate(GameTime gameTime)
        {
            if (timeRemaining > 0) // Continues to execute the following code as long as the animation is playing AND max time isn't reached
            {
                if (movingUp)
                {
                    foreach (MenuCard card in cards.Values)
                    {
                        card.moveUp(gameTime);
                    }
                }

                else if (movingDown)
                {
                    foreach (MenuCard card in cards.Values)
                    {
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
}
