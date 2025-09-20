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

    public Tag currentTag;

    public void updateTags(List<Tag> tagList, Action<Tag> on_tag_pressed)
    {
        foreach(Node child in this.GetChildren())
        {
            this.RemoveChild(child);
        }
        
        if (tagList.Count <= 0)
        {
            return;
        }

        currentTag = tagList[0];

        for (int i = 0; i < tagList.Count; i++)
        {
            Button button = new Button();
            button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            button.SizeFlagsVertical = SizeFlags.ExpandFill;

            button.Theme = tagButtonTheme;

            button.Text = tagList[i].name;

            Tag tag = tagList[i];
            button.Pressed += () => on_tag_pressed(tag);
            button.FocusEntered += () => currentTag = tag;

            MarginContainer marginContainer = new MarginContainer();
            marginContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            marginContainer.SizeFlagsVertical = SizeFlags.ExpandFill;

            marginContainer.Theme = tagButtonTheme;

            marginContainer.AddChild(button);

            this.AddChild(marginContainer);
        }
    }
}
