using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace onboard.ui
{
    // if the size of the textures of the Menu Cards are different, it will most likely cause issues as it has otherwise been constant in every instance 
    public abstract class MenuCardABS
    {
        protected Texture2D texture;

        public int listPos; // Tracks the card's current position on the screen

        /// <summary>
        /// The current rotation of the menu card in radians 
        /// </summary>
        protected float rotation;

        /// <summary>
        /// The menu card will rotate around this point, <br />
        /// It is in local space, relative to the top left point of the un-rotated menu card texture <br />
        /// (0, 0) would refer to the top left of the texture <br />
        /// (texture.width, texture.height) would refer to the bottom right of the texture <br />
        /// </summary>
        protected Vector2 origin; 

        protected Vector2 position;

        /// <summary>
        /// The current scale of the menu card <br />
        /// Scales relative to the origin <br />
        /// Any float value works
        /// </summary>
        protected float scale = 1f;

        /// <summary>
        /// The opacity of the menu card <br /> 
        /// accepts values from zero to one inclusive
        /// </summary>
        public float cardOpacity = 1f;

        /// <summary>
        /// A reference to the game that this menu card represents
        /// </summary>
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

        /// <summary>
        /// Sets the Menu Card's position/rotation/scale/opacity to match the given position 
        /// </summary>
        /// <param name="pos"> The index of the Menu Card reative to the selected element at Zero <br /> Positive refers to above, Negative refers to below </param>
        public abstract void setListPos(int pos);

        /// <summary>
        /// This function should move the Menu Card up to its final position after any animation would be completed <br />
        /// Therefore it should change the position/rotation/scale/opacity to do so
        /// </summary>
        public abstract void moveUp();

        /// <summary>
        /// This function should move the Menu Card down to its final position after any animation would be completed <br />
        /// Therefore it should change the position/rotation/scale/opacity to do so
        /// </summary>
        public abstract void moveDown();

        /// <summary>
        /// This function should move the Menu Card left to its final position after any animation would be completed <br />
        /// Therefore it should change the position/rotation/scale/opacity to do so
        /// </summary>
        public abstract void moveLeft();

        /// <summary>
        /// This function should move the Menu Card right to its final position after any animation would be completed <br />
        /// Therefore it should change the position/rotation/scale/opacity to do so
        /// </summary>
        public abstract void moveRight();

        /// <summary>
        /// Adds the menu card to the sprite batch 
        /// </summary>
        /// <param name="_spriteBatch"> The sprite batch </param>
        /// <param name="cardTexture"> Default texture if the menu card does not have one assigned </param>
        /// <param name="scalingAmount"> parameter to affect the scaling of the menu card </param> though it might be better to just change the menu card's scale value instead 
        /// (could be marked virtual in the future to allow for custom implementations)
        public void DrawSelf(SpriteBatch _spriteBatch, Texture2D cardTexture, double scalingAmount)
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
        /// Changes the texture of the menu card to the given texture 
        /// </summary>
        /// <param name="texture"></param>
        public void setTexture(Texture2D texture) {
            this.texture = texture;
        }
    }
}
