using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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

        public void drawTitle(SpriteFont font, SpriteBatch _spriteBatch)
        {
            string welcome = "Welcome to Devcade";
            Vector2 welcomeSize = font.MeasureString(welcome);
            _spriteBatch.DrawString(font, welcome, new Vector2(_sWidth / 2 - welcomeSize.X / 2, _sHeight / 5 - welcomeSize.Y), Color.White);
        }

        public void drawGames(SpriteFont font, SpriteBatch _spriteBatch)
        {
            int index = 0;
            foreach (String gameTitle in gameTitles)
            {
                Vector2 gameTitleSize = font.MeasureString(gameTitle);
                _spriteBatch.DrawString(font, gameTitle, new Vector2(_sWidth / 2 - gameTitleSize.X / 2, ((_sHeight / 5) * 2) + ((_sHeight / 10) * index)), Color.White);
                index++;
            }    
        }
    }
}