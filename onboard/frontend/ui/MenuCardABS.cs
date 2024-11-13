using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace onboard.ui
{
    public abstract class MenuCardABS
    {
        private float moveTime; // The amount of time it takes to finish the animation in seconds
        private Texture2D texture;

        public int listPos; // Tracks the card's current position on the screen

        /// <summary>
        /// The current rotation of the menu card in radians 
        /// </summary>
        private float rotation;

        /// <summary>
        /// The absolute amount the menu card moves when the menu selection changes
        /// </summary>
        private float rotation_amt; 

        /// <summary>
        /// the menu card will rotate around this point, 
        /// it is in local space, relative to the un-rotated menu card texture
        /// 0, 0 
        /// </summary>
        private Vector2 origin; 

        private Vector2 position;
        private Vector2 position_amt; // the absolute amount the menu card moves when the menu selection changes

        
        private float scale = 1f;
        private const float scale_amt = 0.05f; // the absolute amount the menu card scales when the menu selection changes
        

        public static float cardOpacity = 1f;
        public static float cardX;

        // Each menu card keep a reference to the game it represents
        // Because when sorting by tags, the positions of the cards will change, so it is easier to launch the currently selected game by first getting the card
        public devcade.DevcadeGame game;

        public MenuCardABS(int initialPos, Texture2D cardTexture, devcade.DevcadeGame game)
        {
            this.listPos = initialPos;
            this.texture = cardTexture;
            this.game = game;

            setListPos(initialPos);
        }

        public abstract void setListPos(int pos);

        /// <summary>
        /// this function is called when the previous menu card should be shown,
        /// therefore it should change the position/rotation/scale to do so
        /// </summary>
        /// <param name="gameTime"> a class that includes mutiple definitions of the elapsed frame time, or delta time </param>
        public abstract void movePrevious(GameTime gameTime);

        /// <summary>
        /// this function is called when the next menu card should be shown,
        /// therefore it should change the position/rotation/scale to do so
        /// </summary>
        /// <param name="gameTime"> a class that includes mutiple definitions of the elapsed frame time, or delta time </param>
        public abstract void moveNext(GameTime gameTime); 

        /// <summary>
        /// Adds the menu card to the sprite batch 
        /// </summary>
        /// <param name="_spriteBatch"> The sprite batch </param>
        /// <param name="cardTexture"> Default texture if the menu card does not have one assigned </param>
        /// <param name="_sHeight"> TODO: figure out what the point of this is </param>
        /// <param name="scalingAmount"> parameter to affect the scaling of the menu card </param> though it might be better to just change the menu card's scale value instead 
        /// (could be marked virtual in the future to allow for custom implementations)
        public void DrawSelf(SpriteBatch _spriteBatch, Texture2D cardTexture, int _sHeight, double scalingAmount)
        {
            _spriteBatch.Draw(
                texture ?? cardTexture,
                position,
                null,
                new Color(cardOpacity, cardOpacity, cardOpacity, cardOpacity),
                rotation,
                origin,
                (float)(scale * scalingAmount), 
                SpriteEffects.None,
                0f
            );
        }

        /// <summary>
        /// changes the texture of the menu card to the given texture 
        /// </summary>
        /// <param name="texture"></param>
        public void setTexture(Texture2D texture) {
            this.texture = texture;
        }
    }
}
