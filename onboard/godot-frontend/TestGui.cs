using Godot;
using System;

using onboard.devcade;
using System.Collections.Generic;

namespace GodotFrontend;

public partial class TestGui : Control
{
    public List<DevcadeGame> gameTitles;

    [Export]
    public GridContainer gridContainer;

    public override void _Ready()
    {
        
    }

    public void make_buttons(List<DevcadeGame> gameTitles)
    {
        this.gameTitles = gameTitles;
        foreach(DevcadeGame game in gameTitles)
        {
            Button b = new Button();
            b.Text = game.name;
            gridContainer.CallDeferred("add_child", b);
        }
    }

    private void pressed(DevcadeGame game)
    {
        GD.Print(game.name);
    }
}
