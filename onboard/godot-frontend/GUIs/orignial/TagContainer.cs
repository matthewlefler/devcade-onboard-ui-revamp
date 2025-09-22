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

    public void updateTags(List<Tag> tagList, Action<Tag> on_tag_pressed)
    {
        foreach (Node child in this.GetChildren())
        {
            this.RemoveChild(child);
        }

        if (tagList.Count <= 0)
        {
            return;
        }

        tagButtons = new Button[tagList.Count];

        currentHoveredTag = tagList[0];

        for (int i = 0; i < tagList.Count; i++)
        {
            Button button = new Button();
            button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            button.SizeFlagsVertical = SizeFlags.ExpandFill;

            button.Theme = tagButtonTheme;

            button.Text = tagList[i].name;

            Tag tag = tagList[i];
            button.Pressed += () => on_tag_pressed(tag);
            button.FocusEntered += () => currentHoveredTag = tag;

            tagButtons[i] = button;

            MarginContainer marginContainer = new MarginContainer();
            marginContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            marginContainer.SizeFlagsVertical = SizeFlags.ExpandFill;

            marginContainer.Theme = tagButtonTheme;

            marginContainer.AddChild(button);

            this.AddChild(marginContainer);
        }
    }

    public void grabFocus()
    {
        this.tagButtons[0].CallDeferred("grab_focus");
    }
}
