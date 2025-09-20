using Godot;
using System;

namespace onboard.devcade.GUI.originalGUI;

public partial class ControlHelpTextTags : Label
{
    [Export]
    public TagContainer tagContainer;

    private String initText;

    public override void _Ready()
    {
        this.initText = this.Text;
        base._Ready();
    }

    public override void _Process(double delta)
    {
        // change text based on currenly seleted tag
        if (tagContainer.currentTag != null)
        {
            this.Text = initText + "\n" + tagContainer.currentTag.description;
        }
        else
        {
            this.Text = initText + "\n" + "No Tag Selected";
        }

        base._Process(delta);
    }
}
