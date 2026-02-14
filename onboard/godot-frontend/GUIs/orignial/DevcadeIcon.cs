using Godot;
using onboard.devcade;

public partial class DevcadeIcon : TextureRect
{
    [Export]
    public Texture2D prodTexture;
    [Export]
    public Texture2D devTexture;

    public override void _Ready()
    {
        if(Client.isProduction)
        {
            setTextureToProd();
        }
        else
        {
            setTextureToDev();
        }
    }

    public override void _Process(double delta)
    {
        if(Client.isProduction)
        {
            setTextureToProd();
        }
        else
        {
            setTextureToDev();
        }
    }

    void setTextureToProd()
    {
        this.Texture = prodTexture;
    }

    void setTextureToDev()
    {
        this.Texture = devTexture;
    }
}
