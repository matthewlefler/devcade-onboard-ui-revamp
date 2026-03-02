using Godot;
using onboard.devcade;
using onboard.devcade.GUI;
using onboard.util;
using System;

public partial class SupervisorButtonHint : MarginContainer
{
    [Export]
    private double inactiveMaxTime = 60.0; // time in seconds
    double time = 0.0;

    public override void _Ready()
    {
        this.Hide();
    }

    public override void _Process(double delta)
    {
        if(Input.IsAnythingPressed() || !Client.gameLauched)
        {
            time = 0.0;
            this.Hide();
            return;
        }

        time+=delta;
        if(time >= inactiveMaxTime)
        {
            this.Show();
        }
    }
}
