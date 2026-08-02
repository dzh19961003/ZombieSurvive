using Godot;
using MyProject;
using System;

public partial class RoomChoose : Control
{
	[Export] public Label nameLabel;
	[Export] public TextureRect roomIcon;
    [Export] public TextureRect food;
    [Export] public TextureRect medic;
    [Export] public TextureRect equip;
    [Export] public TextureRect material;
    [Export] public Label exploreProgress;

    public override void _Ready()
	{
	}
    public void InitialRoom(int roomID) 
    {       
        Room room = ConfigManager.Instance.roomDic[roomID];
        nameLabel.Text = room.Name;
        if (room.Food == 0) food.Visible = false;
        if (room.Medic == 0) medic.Visible = false;
        if (room.Equip == 0) equip.Visible = false;
        if (room.Material == 0) material.Visible = false;

        roomIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Building/"+room.Image+".png");
    }


}
