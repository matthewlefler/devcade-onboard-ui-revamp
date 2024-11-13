using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace onboard.ui
{
    public class MenuCardTest : MenuCardABS
    {
        private const float moveTime = 0.15f; // Time it takes to finish scrolling anim

        private static readonly float rotation_amt = MathHelper.ToRadians(25f); // Amount the card moves when scrolling

        private const float scale_amt = 0.05f;

        // Constants that determine the rate at which the rotation, color, scale change.
        private static readonly float rotationSpeed = rotation_amt / moveTime;
        private const float scaleSpeed = scale_amt / moveTime;

        public MenuCardTest(int initialPos, Texture2D cardTexture, devcade.DevcadeGame game) : base(initialPos, cardTexture, game)
        {
            this.origin = new Vector2(0, texture.Height / 2f);
            this.position = new Vector2(0, 0);
        }

        public override void setListPos(int pos) { 
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

        public override void moveUp()
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

        public override void moveDown()
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

        public override void moveLeft()
        {
            throw new System.NotImplementedException();
        }

        public override void moveRight()
        {
            throw new System.NotImplementedException();
        }
    }
}
