using System.Collections.Generic;
using Godot;
using onboard;
using onboard.devcade;

public partial class GamesContainer : Control
{
    [Export] float side_game_card_overdraw = 1.5f;

    // seconds it takes for the game cards to move from one position to another
    [Export] float game_card_transistion_time = 0.3f;

    [Export] private Label game_name_label = null;
    [Export] private Label game_author_label = null;
    [Export] private Label game_description_label = null;

    private ReferenceRect lleft, left, middle, right, rright;

    private List<TextureRect> game_cards = new List<TextureRect>();
    private List<DevcadeGame> games = new List<DevcadeGame>();
    private int game_card_index = 0;

    public override void _Ready()
    {
        create_reference_rectangles();

        GuiManagerGlobal.instance.gameTitlesUpdated += () =>
        {
            create_game_cards(GuiManagerGlobal.gameTitles);
            set_card_rects();
        };
        this.Resized += () => {
            create_reference_rectangles();
            set_card_rects();
        };
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // stick right
        if (@event.IsActionPressed("Player1_StickRight") || @event.IsActionPressed("Player2_StickRight"))
        {
            if(++game_card_index > game_cards.Count - 1)
            {
                game_card_index = game_cards.Count - 1;
                return;   
            }
            set_card_rects();
        }

        // stick left
        if (@event.IsActionPressed("Player1_StickLeft") || @event.IsActionPressed("Player2_StickLeft"))
        {
            if(--game_card_index < 0)
            {
                game_card_index = 0;
                return;
            }
            set_card_rects();
        }

        // enter button (red button)
        if (@event.IsActionPressed("Player1_A1") || @event.IsActionPressed("Player2_A1"))
        {
            // launch currently selected game
            _ = GuiManagerGlobal.instance.launchGame(games[game_card_index]);
        }
    }

    private void create_game_cards(List<DevcadeGame> games)
    {
        foreach(Node child in this.GetChildren())
        {
            this.RemoveChild(child);
        }

        game_cards = new List<TextureRect>(games.Count);

        for(int i = 0; i < games.Count; ++i)
        {
            DevcadeGame game = games[i];
            TextureRect rect = new TextureRect
            {
                Texture = game.banner,
                Position = new Vector2(this.Size.X + 400, 0),
                ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional    
            };
            
            rect.SetSize(new Vector2(300, this.Size.Y));
            rect.Hide();

            this.AddChild(rect);
            game_cards.Add(rect);
            games.Add(game);
        }
    }

    static readonly float banner_aspect_ratio = 1.777777777f; // width 800 x height 450
    private void create_reference_rectangles()
    {
        float middle_box_height = this.Size.Y;
        float middle_box_width = middle_box_height * banner_aspect_ratio;
        float middle_box_x_pos = (this.Size.X - middle_box_width) / 2.0f;

        float side_box_height = middle_box_x_pos / banner_aspect_ratio * side_game_card_overdraw;
        float side_box_width = middle_box_x_pos * side_game_card_overdraw;
        float side_box_y_pos = (middle_box_height - side_box_height) / 2.0f;

        middle = new ReferenceRect
        {
            Size = new Vector2(middle_box_width, middle_box_height),
            Position = new Vector2(middle_box_x_pos,0)
        };

        left = new ReferenceRect
        {
            Size = new Vector2(side_box_width, side_box_height),
            Position = new Vector2(middle_box_x_pos - side_box_width,side_box_y_pos)
        };
        right = new ReferenceRect
        {
            Size = new Vector2(side_box_width, side_box_height),
            Position = new Vector2(middle_box_x_pos + middle_box_width,side_box_y_pos)
        };
        lleft = new ReferenceRect
        {
            Size = new Vector2(side_box_width, side_box_height),
            Position = new Vector2(middle_box_x_pos - (2*side_box_width),side_box_y_pos)
        };
        rright = new ReferenceRect
        {
            Size = new Vector2(side_box_width, side_box_height),
            Position = new Vector2(middle_box_x_pos + middle_box_width + side_box_width,side_box_y_pos)
        };
    }

    private void set_card_rects()
    {
        for(int i = 0; i < this.game_cards.Count; ++i)
        {
            TextureRect rect = this.game_cards[i];
            rect.Hide();
            int index = i - game_card_index;
            if(index == -2)
            {
                Tween tween = rect.CreateTween().SetTrans(Tween.TransitionType.Linear).SetParallel(true);
                tween.TweenProperty(rect, "position", lleft.Position, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_maximum_size", lleft.Size, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_minimum_size", lleft.Size, game_card_transistion_time);
                rect.Show();
            }
            else if(index == -1)
            {
                Tween tween = rect.CreateTween().SetTrans(Tween.TransitionType.Linear).SetParallel(true);
                tween.TweenProperty(rect, "position", left.Position, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_maximum_size", left.Size, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_minimum_size", left.Size, game_card_transistion_time);
                rect.Show();
            }
            else if(index == 0)
            {
                Tween tween = rect.CreateTween().SetTrans(Tween.TransitionType.Linear).SetParallel(true);
                tween.TweenProperty(rect, "position", middle.Position, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_maximum_size", middle.Size, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_minimum_size", middle.Size, game_card_transistion_time);
                rect.Show();

                if(game_author_label != null)
                {
                    game_author_label.Text = $"Author: {games[i].author}";
                }
                if(game_name_label != null)
                {
                    game_name_label.Text = games[i].name;
                }
                if(game_description_label != null)
                {
                    game_description_label.Text = games[i].description;
                }
            }
            else if(index == 1)
            {
                Tween tween = rect.CreateTween().SetTrans(Tween.TransitionType.Linear).SetParallel(true);
                tween.TweenProperty(rect, "position", right.Position, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_maximum_size", right.Size, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_minimum_size", right.Size, game_card_transistion_time);
                rect.Show();
            }
            else if(index == 2)
            {
                Tween tween = rect.CreateTween().SetTrans(Tween.TransitionType.Linear).SetParallel(true);
                tween.TweenProperty(rect, "position", rright.Position, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_maximum_size", rright.Size, game_card_transistion_time);
                tween.TweenProperty(rect, "custom_minimum_size", rright.Size, game_card_transistion_time);
                rect.Show();
            }
        }
    }
}
