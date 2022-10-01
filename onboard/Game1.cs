using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace onboard
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteFont _devcadeMenuBig;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _devcadeMenuBig = Content.Load<SpriteFont>("devcade-menu-big");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DeepPink);

            // TODO: Add your drawing code here

            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            _spriteBatch.Begin();

            string welcome = "Welcome to Devcade";
            Vector2 welcomeSize = _devcadeMenuBig.MeasureString(welcome);
            _spriteBatch.DrawString(_devcadeMenuBig, "Welcome to Devcade", new Vector2(screenWidth/2 - welcomeSize.X/2, screenHeight/2 - welcomeSize.Y), Color.White);

            _spriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
