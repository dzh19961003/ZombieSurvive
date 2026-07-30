using Godot;
using System;
using MyProject;

public partial class DetailsState : Control
{
    [Export] public Label nameLabel;
    [Export] public Label desLabel;
    [Export] public Label timeLabel;
    [Export] public Button BG;
    public override void _Ready()
    {
        BG.Pressed += () => UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsState.tscn");
    }
    public void Initail(int ID)
    {
        nameLabel.Text = ConfigManager.Instance.stateDic[ID].Name;
        if (ConfigManager.Instance.stateDic[ID].Positive == 0)
        {
            nameLabel.AddThemeColorOverride("font_color", Colors.Red);
        }
        else
        {
            nameLabel.AddThemeColorOverride("font_color", Colors.Green);
        }
        desLabel.Text = ConfigManager.Instance.stateDic[ID].Effect;
        timeLabel.Text = ConfigManager.Instance.stateDic[ID].Time.ToString() + "天";
    }
}
