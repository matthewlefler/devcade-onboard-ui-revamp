using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace onboard
{
    public class Menu
    {
        //private int screenWidth;
        //private int screenHeight;
        private GraphicsDeviceManager _device;

        private List<string> gameTitles;

        private int _sWidth;
        private int _sHeight;
        public Menu(GraphicsDeviceManager graph)
        {
            _device = graph;
        }

        public void updateDims() 
        {
            _sWidth = _device.GraphicsDevice.Viewport.Width;
            _sHeight = _device.GraphicsDevice.Viewport.Height;
        }

        public void getGames() {
            gameTitles = new List<String> { "Meatball", "meatball2", "Wilson", "Are you Wilson?", "TSCHOMBPFTHPFHP" };
        }

        public int gamesLen()
        {
            return gameTitles.Count;
        }

        public void drawTitle(SpriteFont font, SpriteBatch _spriteBatch)
        {
            string welcome = "Welcome to Devcade";
            Vector2 welcomeSize = font.MeasureString(welcome);
            _spriteBatch.DrawString(font, welcome, new Vector2(_sWidth / 2 - welcomeSize.X / 2, _sHeight / 5 - welcomeSize.Y), Color.White);
                         

            string wares = "Come enjoy our wares";
            Vector2 waresSize = font.MeasureString(wares);
            _spriteBatch.DrawString(font, wares, new Vector2(_sWidth / 2 + welcomeSize.X / 8, (_sHeight / 4.2f)), Color.Yellow, -0.3f, new Vector2(0, 0), new Vector2(0.5f, 0.5f), SpriteEffects.None, 1);
        }

        public void drawSelection(SpriteBatch _spriteBatch, int menuItemSelected)
        {
            int rectLength = 300;
            int rectHeight = 40;
            RectangleSprite.DrawRectangle(_spriteBatch, new Rectangle(
                    _sWidth / 2 - rectLength/2,
                    ((_sHeight / 5) + (_sHeight / 10)) + ((_sHeight / 10) * menuItemSelected),
                    rectLength,
                    rectHeight
                ),
                Color.White, 3);
        }

        public void drawGames(SpriteFont font, SpriteBatch _spriteBatch)
        {
            int index = 0;
            foreach (String gameTitle in gameTitles)
            {
                Vector2 gameTitleSize = font.MeasureString(gameTitle);
                _spriteBatch.DrawString(font, gameTitle, new Vector2(_sWidth / 2 - gameTitleSize.X / 2, ((_sHeight / 5) + (_sHeight / 10)) + ((_sHeight / 10) * index)), Color.White);
                index++;
            }    
        }
    }
}