using Godot;
using MyProject;
using System;

public partial class Buildings : Control
{
	[Export] TextureRect buildingImage;
	[Export] Control stars;
	[Export] Label nameLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	
    }
	public void InitialBuilding(int buildingID)
	{
        Building building = ConfigManager.Instance.buildingDic[buildingID];
        buildingImage.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Building/" + building.Image + ".png");
		nameLabel.Text = building.Name;
		Stars star = stars as Stars;
		star.ShowStars(building.Stars);
    }
}
