using Godot;
using Godot.Collections;
using MyProject;
using System;
using System.Collections.Generic;

public partial class WarehouseUI : Control
{
	[Export] public GridContainer ItemList;
	[Export] public Button closeBtn;
    [Export] public TextureButton[] tabButton;

    private List<int> ItemID = new List<int>();

    // gridLength = 每行列数，和 tscn 中 columns 保持一致
    private int gridLength = 6;
    // 最少显示的格子数量，铺满至少一整屏
    private int minNum = 42;

    private int buttonIndex = 1;


    public override void _Ready()
	{
		closeBtn.Pressed += () => UIManager.Instance.HideUI(Paths.WarehouseUI);
        //for (int i = 0; i < tabButton.Length; i++)
        //{
        //    int index = i; 
        //    tabButton[i].Pressed += () => SwitchBtn(index);
        //}

        //测试用，先手动添加ID为1、2的物品
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(1);
        ItemID.Add(2);
        ItemID.Add(1);
        ItemID.Add(2);

        SpawnItemList(ItemID);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    /// <summary>
    /// 根据物品ID列表，在 GridContainer 中生成物品格子。
    /// 不足 minNum 时补空格子；超过时按 gridLength 取余补齐最后一行。
    /// </summary>
    private void SpawnItemList(List<int> ItemID)
    {
        // 当物品种类小于最小数量时，最低生成42个格子
        if (ItemID.Count <= minNum)
        {
            for (int i = 0; i < ItemID.Count; i++)
            {
                var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                Item item = itemScene.Instantiate<Item>();
                ItemList.AddChild(item);
                item.ID = ItemID[i];
                item.InitialItem();
            }
            for (int i = 0; i < (minNum - ItemID.Count); i++)
            {
                var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                Item item = itemScene.Instantiate<Item>();
                ItemList.AddChild(item);
                item.ID = 0;
                item.InitialItem();
            }
        }
        // 高于最低数量时，根据取余将格子生成满
        else
        {
            for (int i = 0; i < ItemID.Count; i++)
            {
                var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                Item item = itemScene.Instantiate<Item>();
                ItemList.AddChild(item);
                item.ID = ItemID[i];
                item.InitialItem();
            }
            // 补齐最后一行不满的部分
            int remainder = ItemID.Count % gridLength;
            int fillCount = (remainder == 0) ? 0 : (gridLength - remainder);
            for (int i = 0; i < fillCount; i++)
            {
                var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                Item item = itemScene.Instantiate<Item>();
                ItemList.AddChild(item);
                item.ID = 0;
                item.InitialItem();
            }
        }

        // GridContainer 在动态添加子节点后，需要手动让它重新计算自身的最小尺寸，
        // 这样 ScrollContainer 才能根据 GridContainer 的实际内容高度来设置滚动范围。
        // 不调用的话，ScrollContainer 可能拿到的还是旧的最小尺寸，导致无法滚到底部。
        ItemList.UpdateMinimumSize();
    }

    public void LoadSaveData(Dictionary data)
    {
        
    }
}
