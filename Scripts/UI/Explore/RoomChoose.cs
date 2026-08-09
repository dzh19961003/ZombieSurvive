using Godot;
using Godot.Collections;
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
    RoomChooseBar _bar;
    bool finish = false;
    public override void _Ready()
	{
        // 向上找两级：RoomChoose -> roomContainer -> RoomChooseBar
        Node parent = GetParent().GetParent();
        _bar = parent as RoomChooseBar;
        button.Pressed += InitialExplore;

	}     
    public void InitialRoom(int roomID) 
    {
        Room room = ConfigManager.Instance.roomDic[roomID];
        Dictionary<int, int> progress = GameManager.Instance.exploreProgress;
        nameLabel.Text = room.Name;
        if (room.Food == 0) food.Visible = false;
        if (room.Medic == 0) medic.Visible = false;
        if (room.Equip == 0) equip.Visible = false;
        if (room.Material == 0) material.Visible = false;

        roomIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Building/"+room.Image+".png");
        if (!progress.ContainsKey(roomID))
        {
            progress.Add(roomID, 0);
            exploreProgress.Text = progress[roomID].ToString() + "%";
        }
        else
        {            
            exploreProgress.Text = progress[roomID].ToString() + "%";
        }
        
    }
    private void InitialExplore()
    {
        if (GameManager.Instance.exploreProgress[GameManager.Instance.roomID]>=100)
        {
            finish = true;
        }
        _bar.exploreUI.OnRoomSelected(ID,false);
        _bar.exploreUI.RefreshExploreUI(true);
    }


}
