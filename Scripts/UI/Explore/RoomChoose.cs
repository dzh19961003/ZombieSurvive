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
    // 所属的房间选择面板（房间选项现在挂在 RoomChooseBar 里，不再是 ExploreUI 的子节点）
    RoomChooseBar _bar;
    public override void _Ready()
	{
        // 向上找两级：RoomChoose -> roomContainer -> RoomChooseBar
        // 以前是找三级到 ExploreUI，现在房间选项被 RoomChooseBar 管理
        Node parent = GetParent().GetParent();
        _bar = parent as RoomChooseBar;

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
        // 交给 ExploreUI.OnRoomSelected 统一处理：
        // 销毁房间选择面板、加载事件、显示房间描述、创建搜索策略面板
        // （以前在这里直接切 exploreChoose/roomChoose 的 Visible，现在改成创建/销毁面板）
        _bar.exploreUI.OnRoomSelected(ID);
    }


}
