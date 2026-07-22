using Godot;
using MyProject;
using System;
using System.Collections.Generic;

public partial class WarehouseUI : Control
{
	[Export] public GridContainer ItemList;
	[Export] public Button closeBtn;

    List<int> ItemID = new List<int>();

    public override void _Ready()
	{
		closeBtn.Pressed += () => UIManager.Instance.HideUI(Paths.WarehouseUI);

        //测试用，先手动添加ID为1、2的物品    
        ItemID.Add(1);
        ItemID.Add(2);
		SpawnItemList(ItemID);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SpawnItemList(List<int> ItemID) 
	{
        for (int i = 0; i < ItemID.Count; i++)
		{
			var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
			Item item = itemScene.Instantiate<Item>();
            ItemList.AddChild(item);
			item.ID = ItemID[i];
			item.InitialItem();
        }
	}
}
