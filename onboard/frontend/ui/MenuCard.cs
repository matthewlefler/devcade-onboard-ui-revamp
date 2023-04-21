using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace onboard.ui
{
    public class MenuCard
    {
        private const float moveTime = 0.15f; // Time it takes to finish scrolling anim

        private float rotation; // Initial pos
        private static readonly float rotation_amt = MathHelper.ToRadians(25f); // Amount the card moves when scrolling

        private Texture2D texture;

        public int listPos; // Tracks the card's current position on the screen
        
        // Same as rotation variables, but for scale, color
        private float scale = 1f;
        private const float scale_amt = 0.05f;

        public static float cardOpacity = 1f;
        public static float cardX;

        // Constants that determine the rate at which the rotation, color, scale change.
        private static readonly float rotationSpeed = rotation_amt / moveTime;
        private const float scaleSpeed = scale_amt / moveTime;

        // I made each card keep a reference to the game it represents
        // Because when sorting by tags, the positions of the cards will change, so it is easier to launch the currently selected game by first getting the card
        public devcade.DevcadeGame game;

        public MenuCard(int initialPos, Texture2D cardTexture, devcade.DevcadeGame game)
        {
            this.listPos = initialPos;
            this.texture = cardTexture;
            this.game = game;

            while(initialPos > 0)
            {
                rotation -= rotation_amt;
                scale -= scale_amt;

                initialPos--;
            }
            while (initialPos < 0)
            {
                rotation += rotation_amt;
                scale -= scale_amt;

                initialPos++;
            }

        }

        public void setListPos(int pos) { 
            this.listPos = pos; 
            this.rotation = 0f;
            this.scale = 1f;

            while(pos > 0)
            {
                rotation -= rotation_amt;
                scale -= scale_amt;

                pos--;
            }

            while (pos < 0)
            {
                rotation += rotation_amt;
                scale -= scale_amt;

                pos++;
            }
        }

        public void moveUp(GameTime gameTime)
        {
            // The card scales down moving away from the center, otherwise it scales up as it approaches the center 
            if (listPos > 0)
            {
                scale -= scaleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                scale += scaleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            rotation -= rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; // To rotate counter clockwise (aka up), decrease angle
        }

        public void moveDown(GameTime gameTime)
        {
            // The card scales down moving away from the center, otherwise it scales up as it approaches the center 
            if (listPos >= 0)
            {
                scale += scaleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                scale -= scaleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            rotation += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; // To rotate counter counterclockwise (aka down), decrease angle
        }

        public void DrawSelf(SpriteBatch _spriteBatch, Texture2D cardTexture, int _sHeight, double scalingAmount)
        {
            _spriteBatch.Draw(
                texture ?? cardTexture,
                new Vector2(cardX, (int)(_sHeight / 2.0 + (cardTexture.Height * scalingAmount) /2)),
                null,
                new Color(cardOpacity, cardOpacity, cardOpacity, cardOpacity),
                rotation,
                new Vector2(0, cardTexture.Height / 2.0f),
                (float)(scale * scalingAmount), 
                SpriteEffects.None,
                0f
            );
        }

        public void setTexture(Texture2D texture) {
            this.texture = texture;
        }
    }
}
