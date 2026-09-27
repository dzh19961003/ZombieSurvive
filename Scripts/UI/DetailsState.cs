using Godot;
using System;
using MyProject;

public partial class DetailsState : Control
{
    [Export] public Label nameLabel;
    [Export] public Label desLabel;
    [Export] public Label timeLabel;
    [Export] public Button BG;
    [Export] public Button closeBtn;
    [Export] public TextureRect timeBG;
    [Export] public NinePatchRect desBG;
    public override void _Ready()
    {
        BG.Pressed += () =>
        {
            UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsState.tscn");            
        };
        closeBtn.Pressed += () =>
        {
            UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsState.tscn");
        };
    }
    public void Initail(int ID)
    {
        nameLabel.Text = ConfigManager.Instance.stateDic[ID].Name;
        if (ConfigManager.Instance.stateDic[ID].Positive == 0)
        {
            nameLabel.AddThemeColorOverride("font_color", Color.FromHtml("#c54949"));
            timeLabel.AddThemeColorOverride("font_color", Color.FromHtml("#c54949"));
            timeBG.Texture = GD.Load<Texture2D>("res://Assets/Images/UI/Property/bad_bg.png");
            desBG.Texture = GD.Load<Texture2D>("res://Assets/Images/UI/Property/bad_bg.png");
        }
        else
        {
            nameLabel.AddThemeColorOverride("font_color", Color.FromHtml("#9fce94"));
            timeLabel.AddThemeColorOverride("font_color", Color.FromHtml("#9fce94"));
            timeBG.Texture = GD.Load<Texture2D>("res://Assets/Images/UI/Property/good_bg.png");
            desBG.Texture = GD.Load<Texture2D>("res://Assets/Images/UI/Property/good_bg.png");
        }
        desLabel.Text = ConfigManager.Instance.stateDic[ID].Effect;
        timeLabel.Text = ConfigManager.Instance.stateDic[ID].Time.ToString() + "天";
    }
}
