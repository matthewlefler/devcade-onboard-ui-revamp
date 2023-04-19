using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// Change to onboard.ui when changes merge
namespace onboard;

public class TagCard {

    private static SpriteFont font;
    private static Texture2D texture;
    
    private Vector2 pos;
    private static float defaultxVel = 100f;
    private float xVel = defaultxVel;
    // Because the distance travelled is based of gameTime elapsed, this is a constant value that will increase the amount it moves every frame
    // This value is also not affected by the decelleration. Without this the xAccel would make the cards move way too slowly
    // Just feels better than using some large number like 10000 for the velocity
    private static float xSpeed = 100f; 
    private float xAccel = 0.85f;
    private float xShowing;
    private float xHidden;

    private bool isSelected = false;

    private float scale;
    private static float unhighlightedScale = 0.6f;
    private static float highlightedScale = 0.75f;
    private float scaleVel = 0.1f;
    private float scaleAccel = 0.5f;

    private static Color color = new Color(150, 0 ,0);
    private string name;

    public TagCard(Texture2D texture, SpriteFont font, Vector2 startPos, string name, float hiddenOffset) {
        TagCard.texture = texture;
        TagCard.font = font;

        this.pos = startPos;
        this.xHidden = startPos.X;
        this.xShowing = startPos.X - hiddenOffset;
        this.scale = unhighlightedScale;
        this.name = name;
    }

    public string getName() { return this.name; }
    public void setSelected(bool selected) { this.isSelected = selected; }

    public void updateScale( GameTime gameTime ) {
        // This is a new system for animating on screen elements. I think it's a little bit cleaner and easier to understand than what I previously had
        // I will update Menu.cs and MenuCard.cs to do something like this instead

        // The scale will increase or decrease depending on whether it is being selected or deselected
        if(isSelected) {
            // If the scale has yet to reach it's target
            if (scale < highlightedScale) {
                // gradually increase scale each frame
                scale += scaleVel * (float)gameTime.ElapsedGameTime.TotalSeconds;

            } else {
                // Otherwise, it has reached it's target, so just reset the velocity and set scale to what it should be
                scale = highlightedScale;
                scaleVel = 0.1f;
            }
        } else {
            if (scale > unhighlightedScale) {
                scale -= scaleVel * (float)gameTime.ElapsedGameTime.TotalSeconds;

            } else {
                scale = unhighlightedScale;
                scaleVel = 0.1f;
            }
        }

        // if scaleAccel is too high, the scale of the button will actually increase so fast that it goes past what it should, and is forced back down.
        // It gives the animations a sort of bounce, I like it so I'm keeping it

        // The amount that the scale changes every frame is increasing every frame, 
        // Gives the animation a less linear look
        scaleVel += scaleAccel;
    }

    // These two methods are similar to the one above, where the X position of the cards gradually changes each frame when switching between tags and games menu.
    public void scrollRight( GameTime gameTime ) {
        // With these two, I have them slow down as they slide. So, if the xVel reaches zero, just snap them to where they need to be
        if (pos.X < xHidden && xVel > 0) {
            pos.X += xVel * xSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            xVel *= xAccel;
        } else {
            pos.X = xHidden;
            xVel = defaultxVel;
        }
    }

    public void scrollLeft( GameTime gameTime ) {
        if (pos.X > xShowing && xVel > 0) {
            pos.X -= xVel * xSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            xVel *= xAccel;
        } else {
            pos.X = xShowing;
            xVel = defaultxVel;
        }
    }

    public void resetxVel() { this.xVel = defaultxVel; }

    public void DrawSelf(SpriteBatch _spriteBatch, double scalingAmount) {

        _spriteBatch.Draw(
            texture,
            pos,
            null,
            color,
            0f,
            new Vector2(texture.Width/2, texture.Height/2),
            (float)(scale * scalingAmount),
            SpriteEffects.None,
            0f
        );

        Vector2 strSize = font.MeasureString(name);

        _spriteBatch.DrawString(font,
            name,
            pos,
            Color.White,
            0f,
            new Vector2(strSize.X / 2, strSize.Y / 2),
            (float)(scale * 2 * scalingAmount),
            SpriteEffects.None,
            0f
        );
    }

}
