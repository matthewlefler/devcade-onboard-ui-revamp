using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace onboard
{
    public class Menu
    {
        //private int screenWidth;
        //private int screenHeight;
        private GraphicsDeviceManager _device;

        private string[] gameTitles;
        public Menu(GraphicsDeviceManager graph)
        {
            _device = graph;
        }

        public void getGames() {
            gameTitles = ["Meatball",];
        }

        public void drawTitle(SpriteFont font, SpriteBatch _spriteBatch)
        {
            int screenWidth = _device.GraphicsDevice.Viewport.Width;
            int screenHeight = _device.GraphicsDevice.Viewport.Height;
            string welcome = "Welcome to Devcade";
            Vector2 welcomeSize = font.MeasureString(welcome);
            _spriteBatch.DrawString(font, "Welcome to Devcade", new Vector2(screenWidth / 2 - welcomeSize.X / 2, screenHeight / 5 - welcomeSize.Y), Color.White);
        }
    }
}