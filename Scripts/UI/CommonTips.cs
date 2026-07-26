using Godot;
using MyProject;
using System;

public partial class CommonTips : Control
{
	[Export] public Button closeBtn;
    [Export] public TextureButton confirmBtn;
    [Export] public TextureButton cancelBtn;
    [Export] public Label titleLabel;
    [Export] public Label tipsLabel;
    public override void _Ready()
	{
        closeBtn.Pressed += () => UIManager.Instance.HideUI("res://UI/CommonTips.tscn");
        confirmBtn.Pressed += () => UIManager.Instance.HideUI("res://UI/CommonTips.tscn");
        cancelBtn.Pressed += () => UIManager.Instance.HideUI("res://UI/CommonTips.tscn");
    }

    public void SpawnTips(string title,string tips) 
    {
        titleLabel.Text = title;
        tipsLabel.Text = tips;
    }   

}
