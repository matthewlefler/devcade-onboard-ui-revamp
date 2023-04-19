using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Devcade;

namespace onboard.ui;

public class TagsMenu 
{

    private TagCard[,] cards;
    private static int rows;
    private static int cols = 2;
    private int currentRow;
    private int currentCol;
    // Jesus this scaling Amount is such a hack, should probably think of a better way to render on non devcade
    // It looks like BUNS but the UI is all visible and functional (it looks better on 2560x1080 I swear ;-;)
    private double scalingAmount;
    private bool isShowing = false;

    public TagsMenu(List<string> tags, Texture2D cardTexture, SpriteFont font, Vector2 dims, double scalingAmount) {

        this.scalingAmount = scalingAmount;

        // cards is a 2D array of every tag in the form [row][col]
        // The # of rows is tags.length / 2 rounded up
        TagsMenu.rows = (int)Math.Ceiling((double)tags.Count/2);
        // # of cols is a constant 2
        cards = new TagCard[rows, cols]; // Surprised that math.floor/ceiling dont return ints

        int currentTag = 0; // Int to keep track of our spot within the tags list

        for (int row=0; row<cards.GetLength(0); row++) {
            for (int col=0; col<cards.GetLength(1); col++) {
                // If the number of tags is ODD, then the 2D array of cards will be LARGER than the number of tags
                // To prevent index OOB, add an additional check to see if we've reached the end of the tags list
                // This also results in the bottom right element of the cards array being NULL, and there needs to be checks for this when moving through the array.
                if (currentTag < tags.Count) {
                    // Create a new tagCard for each tag. 
                    //      The height is dependent on it's row. Starting at 1/3 down the page, its position is lower down depending on it's row
                    float y = (float)(dims.Y/3 + row * 3 * cardTexture.Height/4 * scalingAmount);

                    // The cards are arranged into two columns. The X remains constant, at either 1/4 or 3/4 through the width
                    float x = dims.X / 4;
                    x *= (col == 0) ? 1 : 3;

                    // That card's texture is the generic cardTexture
                    // The card's font is also the same as usual
                    // The card's name is the tag itself
                    TagCard card = new TagCard(
                        cardTexture,
                        font,
                        new Vector2(x + dims.X + 30, y), // I move it a little it extra to the side because it will be slightly visible when on the game menu
                        tags[currentTag],
                        dims.X + 30
                    );
                    cards[row, col] = card;
                }
                currentTag++;
            }
        }

        this.currentRow = 0;
        this.currentCol = 0;
        cards[currentRow, currentCol].setSelected(true);
    }

    public int getCurrentCol() { return this.currentCol; }
    public void setIsShowing(bool isShowing) { this.isShowing = isShowing; }

    public void resetxVel() {
        for (int row=0; row<cards.GetLength(0); row++) {
            for (int col=0; col<cards.GetLength(1); col++) {
                // Check if it's null in the case that the bottom row is not filled
                if (cards[row, col] != null)
                    cards[row, col].resetxVel();
            }
        }
    }

    // Draw method called after update() every frame 
    // Draws all of the necessary elements
    public void Draw(SpriteBatch _spriteBatch, SpriteFont font, Vector2 dims) {
        // Draw the cards
        for (int row=0; row<cards.GetLength(0); row++) {
            for (int col=0; col<cards.GetLength(1); col++) {
                // Check if it's null in the case that the bottom row is not filled
                if (cards[row, col] != null)
                    cards[row, col].DrawSelf(_spriteBatch, scalingAmount);
            }
        }

        if (isShowing) {
            // Draw the Instructions
            Vector2 strSize = font.MeasureString("Select a tag below to search:");
            _spriteBatch.DrawString(font,
                "Select a tag below to search:",
                new Vector2(dims.X/2, (int)(500 * scalingAmount)), // 500 is the height of the title, this string goes right beneath that
                Color.Black,
                0f,
                new Vector2(strSize.X / 2, strSize.Y / 2), 
                (float)(1.2f * scalingAmount),
                SpriteEffects.None,
                0f
            );

            // Display the name of the tag (and probably description later)
            strSize = font.MeasureString(cards[currentRow, currentCol].getName());
            _spriteBatch.DrawString(font,
                cards[currentRow, currentCol].getName(),
                new Vector2(dims.X/2, (int)(500 * scalingAmount) + strSize.Y), // 500 is the height of the title, this string goes right beneath that
                Color.Black,
                0f,
                new Vector2(strSize.X / 2, strSize.Y / 2), 
                (float)(1.2f * scalingAmount),
                SpriteEffects.None,
                0f
            );
        }

    }

    // Update method called every frame
    // Used for input mostly
    public void Update(KeyboardState currentState, KeyboardState lastState, GameTime gameTime) {
        // Track input
        if( isShowing ) {
            if(currentState.IsKeyDown(Keys.Down) && lastState.IsKeyUp(Keys.Down) ||         // Keyboard Down
                Input.GetButtonDown(1, Input.ArcadeButtons.StickDown) ||                    // or joystick down
                Input.GetButtonDown(2, Input.ArcadeButtons.StickDown))                      // of either player
                
                highlightDown();

            if(currentState.IsKeyDown(Keys.Up) && lastState.IsKeyUp(Keys.Up) ||             // Keyboard Up
                Input.GetButtonDown(1, Input.ArcadeButtons.StickUp) ||                      // or joystick up
                Input.GetButtonDown(2, Input.ArcadeButtons.StickUp))                        // of either player
                
                highlightUp();

            if(currentState.IsKeyDown(Keys.Right) && lastState.IsKeyUp(Keys.Right) ||       // Keyboard Right
                Input.GetButtonDown(1, Input.ArcadeButtons.StickRight) ||                   // or joystick right
                Input.GetButtonDown(2, Input.ArcadeButtons.StickRight))                     // of either player
                
                highlightRight();

            if(currentState.IsKeyDown(Keys.Left) && lastState.IsKeyUp(Keys.Left) ||         // Keyboard Left
                Input.GetButtonDown(1, Input.ArcadeButtons.StickLeft) ||                    // or joystick left
                Input.GetButtonDown(2, Input.ArcadeButtons.StickLeft))                      // of either player
                
                highlightLeft();
        }

        for (int row=0; row<cards.GetLength(0); row++) {
            for (int col=0; col<cards.GetLength(1); col++) {
                // Check if it's null in the case that the bottom row is not filled
                if (cards[row, col] != null) {
                    // Play animations for when the user is scrolling through the list
                    cards[row, col].updateScale(gameTime);

                    // Update animations for when switching between cards and tags screen
                    if(isShowing)
                        cards[row, col].scrollLeft(gameTime);
                    else
                        cards[row, col].scrollRight(gameTime); 
                }
            }
        }
    }

    public string getCurrentTag() { return cards[currentRow, currentCol].getName(); }

    public void highlightUp() {
        cards[currentRow, currentCol].setSelected(false);
        
        currentRow -= (currentRow > 0) ? 1 : 0;

        cards[currentRow, currentCol].setSelected(true);
    }

    public void highlightDown() {
        cards[currentRow, currentCol].setSelected(false);
        
        currentRow += (currentRow < rows-1 && cards[currentRow+1, currentCol] != null) ? 1 : 0;

        cards[currentRow, currentCol].setSelected(true);
    }

    public void highlightLeft() {
        cards[currentRow, currentCol].setSelected(false);
        
        currentCol -= (currentCol > 0) ? 1 : 0;

        cards[currentRow, currentCol].setSelected(true);
    }

    public void highlightRight() {
        cards[currentRow, currentCol].setSelected(false);
        
        currentCol += (currentCol < cols-1 && cards[currentRow, currentCol+1] != null) ? 1 : 0;

        cards[currentRow, currentCol].setSelected(true);
    }

}
