using Godot;
using onboard;
using onboard.devcade;

public partial class DevcadeIcon : TextureRect
{
    [Export]
    public Texture2D prodTexture;
    [Export]
    public Texture2D devTexture;

    public override void _Ready()
    {
        GuiManagerGlobal.instance.gameTitlesUpdated += setTexture;
    }

    private void setTexture()
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
