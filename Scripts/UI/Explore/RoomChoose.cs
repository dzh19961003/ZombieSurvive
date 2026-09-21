using Godot;
using Godot.Collections;
using MyProject;
using System;
using System.Diagnostics;

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
    [Export] public NinePatchRect selectEdge;   //选中边框
   
    public int ID;
    RoomChooseBar _bar;
    GameManager gm = GameManager.Instance;
    int currentRoomID;
    public override void _Ready()
	{
        // 向上找两级：RoomChoose -> roomContainer -> RoomChooseBar
        Node parent = GetParent().GetParent();
        _bar = parent as RoomChooseBar;
        button.Pressed += ShowExplore;

	}     
    public void InitialRoom(int roomID) 
    {
        Room room = ConfigManager.Instance.roomDic[roomID];
        currentRoomID = roomID;

        nameLabel.Text = room.Name;
        if (room.Food == 0) food.Visible = false;
        if (room.Medic == 0) medic.Visible = false;
        if (room.Equip == 0) equip.Visible = false;
        if (room.Material == 0) material.Visible = false;

        roomIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Building/"+room.Image+".png");       
    }
    //由 RoomChooseBar 调用，切换本房间的选中边框显示
    public void SetSelected(bool selected)
    {
        selectEdge.Visible = selected;
    }
    private void ShowExplore()
    {
        //通知父面板选中自己，由父面板统一保证同时只有一个房间被选中
        _bar.SelectRoom(this);

        Dictionary<int, int> progress = GameManager.Instance.exploreProgress;
        _bar.detailsPanel.Visible = true;
        if (!progress.ContainsKey(gm.roomID))
        {
            progress.Add(gm.roomID, 0);
            _bar.exploreProgress.Text = progress[gm.roomID].ToString() + "%";
        }
        else
        {
            _bar.exploreProgress.Text = progress[gm.roomID].ToString() + "%";
        }
        GameManager.Instance.roomID = currentRoomID;
    }
}
