using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class ExploreMap : Control
{
	[Export] public Control[] buildings;
	[Export] public TextureButton backBtn;
    public override void _Ready()
	{
		backBtn.Pressed += () => 
		{
            CommonTips tips = UIManager.Instance.ShowCommonTips("返回基地", "确定返回基地吗？（时间将切换到夜晚）");
			tips.OnConfirm = () =>
			{
				UIManager.Instance.HideUI("res://UI/Explore/ExploreMap.tscn");
				UIManager.Instance.ShowUI(Paths.MainUI);
				GameManager.Instance.gameState = 5;
			};

        };

        Array<int> buildArray = new Array<int>();
		for (int i = 5; i > 0; i--)
		{
            buildArray.Add(i);
        }
		InitialMap(buildArray);

    }
	public void InitialMap(Array<int> buildingArry) 
	{
		for (int i = 0; i < buildingArry.Count; i++)
		{
			Buildings building = buildings[i] as Buildings;
			building.InitialBuilding(buildingArry[i]);
			building.ID = buildingArry[i];
        }
	}

}
