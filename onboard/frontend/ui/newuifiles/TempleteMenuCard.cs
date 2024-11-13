// These are only here as templates / documentation and SHOULD NOT be used or referenced in any other code in this project

// This is an 2 wide by (number of games / 2 rounded up) tall grid of game menu cards 
// and therefore the cards cannot move left to right. so, the moveLeft and moveRight functions do not matter


// these two are requried
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// this included some nice math functions similar to MathF or others, 
// but is not nessesary
using System;

namespace onboard.ui
{
    internal class TemplateMenuCard : MenuCardABS
    {
        /// <summary>
        /// this is some padding so the Menu Cards do not sit right next to each other <br />
        /// gives them some breathing room
        /// </summary>
        private const float padding = 10f;

        public TemplateMenuCard(int initialPos, Texture2D cardTexture, devcade.DevcadeGame game) : base(initialPos, cardTexture, game)
        {

        }

        /// <summary>
        /// This function is called when the Menu Card should move up
        /// or in most cases when the Menu Card selection should move down <br />
        /// Therefore it should change the position/rotation/scale/opacity to do so
        /// </summary>
        public override void moveUp()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// This function is called when the Menu Card should move down
        /// or in most cases when the Menu Card selection should move up <br />
        /// Therefore it should change the position/rotation/scale/opacity to do so
        /// </summary>
        public override void moveDown()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// sets the position of the Menu Card, 
        /// </summary>
        /// <param name="pos"> the position of the menu card in the order of menu cards, 0 refers to the current selected element, negative is below, positive above</param>
        public override void setListPos(int pos)
        {
            // is the Menu card on the left side of the two columns 
            int onLeftSide = pos % 2;
            // take absolute value to deal with negative values
            onLeftSide = Math.Abs(onLeftSide);

            // get the row number by dividing by 2
            // side note: division by 2 of integers will result in the floor of the result if they were floats
            int rowNumber = pos / 2;

            float x = onLeftSide * texture.Width;
            // set y position to 
            float y = rowNumber * texture.Height;
            this.position = new Vector2(onLeftSide * texture.Width + onLeftSide * padding + padding,  + rowNumber * padding + padding);
        }

        // not used: 
        // add bool to disable?
        //          or at least tell the menu that these are disabled
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


