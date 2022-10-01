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

        private Menu _mainMenu;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _mainMenu = new Menu(_graphics);
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
            _mainMenu.getGames();
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
            _mainMenu.updateDims();

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            _mainMenu.drawTitle(_devcadeMenuBig, _spriteBatch);
            _mainMenu.drawGames(_devcadeMenuBig, _spriteBatch);

            _spriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
