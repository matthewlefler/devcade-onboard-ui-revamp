using System.Collections.Generic;
using Godot;
using onboard;
using onboard.devcade;

public partial class GamesContainer : Control
{
    private ReferenceRect left, middle, right;

    public override void _Ready()
    {
        create_reference_rectangles();

        GuiManagerGlobal.instance.gameTitlesUpdated += () =>
        {
            create_game_cards(GuiManagerGlobal.gameTitles);
        };
    }

    private void create_game_cards(List<DevcadeGame> games)
    {
        foreach(Node child in this.GetChildren())
        {
            this.RemoveChild(child);
        }

        for(int i = 0; i < games.Count; ++i)
        {
            DevcadeGame game = games[i];
            TextureRect rect = new TextureRect
            {
                Texture = game.banner,
                Position = new Vector2(i * 400, 0),
                ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional    
            };
            
            rect.SetSize(new Vector2(300, this.Size.Y));
            rect.Hide();

            if(i == 0)
            {
                rect.Size = middle.Size;
                rect.Position = middle.Position;
                rect.Show();
            }
            if(i == 1)
            {
                rect.Size = right.Size;
                rect.Position = right.Position;
                rect.Show();
            }

            this.AddChild(rect);
        }
    }

    static readonly float banner_aspect_ratio = 1.777777777f; // width 800 x height 450
    private void create_reference_rectangles()
    {
        float middle_box_height = this.Size.Y;
        float middle_box_width = middle_box_height * banner_aspect_ratio;
        float middle_box_x_pos = (this.Size.X - middle_box_width) / 2.0f;

        float side_box_height = middle_box_x_pos / banner_aspect_ratio;
        float side_box_width = middle_box_x_pos;

        middle = new ReferenceRect
        {
            Size = new Vector2(middle_box_width, middle_box_height),
            Position = new Vector2(middle_box_x_pos,0)
        };

        left = new ReferenceRect
        {
            Size = new Vector2(side_box_width, side_box_height),
            Position = new Vector2(0,0)
        };
        right = new ReferenceRect
        {
            Size = new Vector2(side_box_width, side_box_height),
            Position = new Vector2(middle_box_x_pos + middle_box_width,0)
        };
    }
}
