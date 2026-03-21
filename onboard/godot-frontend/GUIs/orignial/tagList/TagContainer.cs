using Godot;
using System;
using System.Collections.Generic;

namespace onboard.devcade.GUI.originalGUI;

public partial class TagContainer : GridContainer
{
    /// <summary>
    /// the custom theme for the tag buttons
    /// as well as any game that does not have a banner texture
    /// </summary>
    [Export]
    public Theme tagButtonTheme;

    public Tag currentHoveredTag;

    private Button[] tagButtons;

    public int numberOfTags {get; private set;} = 1;

    private int maxX;
    private int maxY;

    public int currentX {get; private set;} = 0;
    public int currentY {get; private set;} = 0;

    /// <summary>
    /// Attempt to select the tag above the current one
    /// </summary>
    public void selectUp()
    {
        select(currentX, currentY - 1);
    }

    /// <summary>
    /// Attempt to select the tag below the current one
    /// </summary>
    public void selectDown()
    {
        select(currentX, currentY + 1);
    }
    
    /// <summary>
    /// Attempt to select the tag left of the current one
    /// </summary>
    public void selectLeft()
    {
        select(currentX - 1, currentY);
    }
    
    /// <summary>
    /// Attempt to select the tag right of the current one
    /// </summary>
    public void selectRight()
    {
        select(currentX + 1, currentY);
    }

    public void select(int x, int y)
    {
        if(x < 0)
        {
            x = 0;
        }
        if(x > maxX)
        {
            x = maxX;
        }
        if(y < 0)
        {
            y = 0;
        }
        if(y > maxY)
        {
            y = maxY;
        }

        if((y * Columns + x) > (numberOfTags - 1))
        {
            currentX = (numberOfTags - 1) % Columns;
            currentY = (numberOfTags - 1) / Columns;
            
            this.tagButtons[numberOfTags - 1].CallDeferred("grab_focus");

            return;
        }

        currentX = x;
        currentY = y;

        this.tagButtons[y * Columns + x].CallDeferred("grab_focus");
    }

    public void updateTags(List<Tag> tagList, Action<Tag> on_tag_pressed)
    {
        if(tagList == null) {return;}
        
        foreach (Node child in this.GetChildren())
        {
            this.RemoveChild(child);
        }

        if (tagList.Count <= 0)
        {
            return;
        }

        numberOfTags = tagList.Count;

        tagButtons = new Button[tagList.Count];

        currentHoveredTag = tagList[0];

        for (int i = 0; i < tagList.Count; i++)
        {
            Button button = new Button
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,

                Theme = tagButtonTheme,

                Text = tagList[i].name,
                // Text = i.ToString(),
            };

            Tag tag = tagList[i];
            button.Pressed += () => on_tag_pressed(tag);
            button.FocusEntered += () => currentHoveredTag = tag;

            tagButtons[i] = button;

            MarginContainer marginContainer = new MarginContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,

                Theme = tagButtonTheme,

                
            };

            marginContainer.CallDeferred(Node.MethodName.AddChild, button);

            this.AddChild(marginContainer);

            // Ignores basic ui_"whatever" input actions
            // while still allowing them to be focused
            marginContainer.FocusMode = FocusModeEnum.Click;
            button.FocusMode = FocusModeEnum.Click;
        }

        maxX = Columns - 1;
        maxY = (int) Mathf.Ceil(numberOfTags / (float) Columns) - 1;

        currentX = 0;
        currentY = 0;
    }

    public void grabFocus()
    {
        select(0, 0);
    }
}
