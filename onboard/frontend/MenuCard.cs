using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace onboard
{
    public class MenuCard
    {
        private const float moveTime = 0.15f; // Time it takes to finish scrolling anim

        private float rotation = 0f; // Initial pos
        private static readonly float rotation_amt = MathHelper.ToRadians(25f); // Amount the card moves when scrolling

        private Texture2D texture;

        public int listPos; // Tracks the card's current position on the screen
        private string name; // Each game name will taken from gameTitles list in Menu.cs

        // Same as rotation variables, but for scale, color
        private float scale = 1f;
        private const float scale_amt = 0.05f;

        public static float cardOpacity = 1f;
        public static float cardX = 0f;

        // Constants that determine the rate at which the rotation, color, scale change.
        private static readonly float rotationSpeed = rotation_amt / moveTime;
        private const float scaleSpeed = scale_amt / moveTime;

        public MenuCard(int initialPos, string theName, Texture2D cardTexture)
        {
            this.listPos = initialPos;
            this.name = theName;
            this.texture = cardTexture;

            while (initialPos > 0)
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

        public void DrawSelf(SpriteBatch _spriteBatch, Texture2D cardTexture, SpriteFont font, int _sHeight, double scalingAmount)
        {
            _spriteBatch.Draw(
                texture ?? cardTexture,
                new Vector2(cardX, (int)(_sHeight / 2.0 + (cardTexture.Height * scalingAmount) / 2)),
                null,
                new Color(cardOpacity, cardOpacity, cardOpacity, cardOpacity),
                rotation,
                new Vector2(0, cardTexture.Height / 2.0f),
                (float)(scale * scalingAmount),
                SpriteEffects.None,
                0f
            );
        }

        public void setTexture(Texture2D texture)
        {
            this.texture = texture;
        }
    }
}
