using Godot;
using MyProject;
using System;

public partial class BuildingTips : Control
{
	[Export] Label nameLabel;
	[Export] Control star;
	[Export] Label desLabel;
	[Export] TextureButton confirmBtn;
    [Export] Button cancelBtn;

    public override void _Ready()
	{
		cancelBtn.Pressed += () => UIManager.Instance.HideUI("res://UI/BuildingTips.tscn");
	}
	public void InitialTips(int ID) 
	{
		Building building = ConfigManager.Instance.buildingDic[ID];

        nameLabel.Text = building.Name;
		Stars stars = star as Stars;
		stars.ShowStars(building.Stars);

		desLabel.Text = building.Des;
	}

}
