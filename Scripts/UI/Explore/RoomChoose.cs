using Godot;
using MyProject;
using System;

public partial class RoomChoose : Control
{
    [Export] public TextureButton roomChooseBtn;
	[Export] public Label nameLabel;
	[Export] public TextureRect roomIcon;
    [Export] public TextureRect food;
    [Export] public TextureRect medic;
    [Export] public TextureRect equip;
    [Export] public TextureRect material;
    [Export] public Label exploreProgress;
    [Export] public Button button;
   
    public int ID;
    ExploreUI exploreUI;
    public override void _Ready()
	{
        //强行向上找三级，层级修改这里也要改
        Node explore = GetParent().GetParent().GetParent(); 
        exploreUI = explore as ExploreUI;

        button.Pressed += InitialExplore;

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
    private void InitialExplore()
    {
        GameManager.Instance.LoadEvent(ID);
        GameManager.Instance.exploreState = 2;
        exploreUI.exploreChoose.Visible = true;
        exploreUI.roomChoose.Visible = false;
        TextTyper.TypeText(exploreUI.desLabel, ConfigManager.Instance.roomDic[ID].Des);
    }


}
