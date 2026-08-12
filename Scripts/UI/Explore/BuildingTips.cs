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
    [Export] Control foodStars;
    [Export] Control medicStars;
    [Export] Control equipStars;
    [Export] Control materialStars;
    private int buildingID;

    public override void _Ready()
    {
        cancelBtn.Pressed += () => UIManager.Instance.HideUI("res://UI/Explore/BuildingTips.tscn");
        confirmBtn.Pressed += () =>
        {
            if (GameManager.Instance.CurrentTimePeriod == 1)
            {
                EnterExplore();
                GameManager.Instance.AdvanceTime();
            }
            else if (GameManager.Instance.CurrentTimePeriod == 2)
            {
                Node tips = UIManager.Instance.ShowCommonTips("探索提示", "确定进入今天的第二次探索吗？临近黄昏丧尸的能力会显著提升");
                CommonTips control = (CommonTips)tips;
                control.OnConfirm = EnterExplore;
                GameManager.Instance.AdvanceTime();
            }
            else
            {
                Node tips = UIManager.Instance.ShowCommonTips("探索提示", "今天已经太晚了，明天再来继续探索吧");
                CommonTips control = (CommonTips)tips;
                control.OnConfirm = () =>
                { 
                    UIManager.Instance.HideUI("res://UI/CommonTips.tscn");
                    UIManager.Instance.HideUI("res://UI/Explore/BuildingTips.tscn");
                };
            }

        };
    }
    public void InitialTips(int ID) 
	{
        buildingID = ID;
		Building building = ConfigManager.Instance.buildingDic[ID];
        nameLabel.Text = building.Name;
        Stars stars = star as Stars;
        stars.ShowStars(building.Stars);

        desLabel.Text = building.Des;

		Stars food = foodStars as Stars;
        food.ShowStars(building.Food);
        Stars medic = medicStars as Stars;
        medic.ShowStars(building.Medic);
        Stars equip = equipStars as Stars;
        equip.ShowStars(building.Equip);
        Stars material = materialStars as Stars;
        material.ShowStars(building.Material);
    }
    public void EnterExplore() 
    {
        UIManager.Instance.HideUI("res://UI/Explore/BuildingTips.tscn");
        GameManager.Instance.currentBuildingID = buildingID;
        // 用 CreateUI 创建全新实例（不走缓存），每次打开 ExploreUI 都会重新执行 _Ready
        ExploreUI explore = (ExploreUI)UIManager.Instance.CreateUI("res://UI/Explore/ExploreUI.tscn");
        GameManager.Instance.exploreState = 1;
        GameManager.Instance.exploreLayer = 1;
        explore.RefreshExplore();       
    }
}
