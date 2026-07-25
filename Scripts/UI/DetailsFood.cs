using Godot;
using Godot.Collections;
using MyProject;
using System;
using System.Text;

public partial class DetailsFood : Control
{
    [Export] public Label nameLable;
    [Export] public Label numLabel;
    [Export] public Label stateLabel;
    [Export] public Label stateNameLabel;
	[Export] public Label noStateLabel;
    [Export] public Control stateTips;
    [Export] public Button BG;
    [Export] public TextureRect[] Rarity;

    private State state;
    public override void _Ready()
	{
        BG.Pressed += () => UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsFood.tscn");
    }

	public void InitialTips(int ID) 
	{
        //初始化可见性
        noStateLabel.Visible = false;
        stateLabel.Visible = true;
        stateNameLabel.Visible = true;

        //选择稀有度
        for (int i = 0; i < Rarity.Length; i++)
        {
            Rarity[i].Visible = false;
        }
        Rarity[ConfigManager.Instance.itemDic[ID].Rarity - 1].Visible = true;

        //设置文本
        nameLable.Text = ConfigManager.Instance.itemDic[ID].Name;
        numLabel.Text = ConfigManager.Instance.itemDic[ID].Num.ToString();

        if (ConfigManager.Instance.itemDic[ID].BuffID != 0 )
		{
			state = ConfigManager.Instance.stateDic[ConfigManager.Instance.itemDic[ID].BuffID];
            stateNameLabel.Text = state.Name;
            //更改状态字体颜色
            if (state.Positive == 0)
            {
                AddThemeColorOverride(stateNameLabel.Name, Colors.Red);
            }
            else
            {
                AddThemeColorOverride(stateNameLabel.Name, Colors.Green);
            }
            StateTips tips = stateTips as StateTips;
            tips.Initail(state.ID);
        }		        
		else
		{
			noStateLabel.Visible = true;
			stateLabel.Visible = false;
			stateNameLabel.Visible = false;
        }

        //是否展示状态说明
        if (ConfigManager.Instance.itemDic[ID].BuffID == 0)
		{
			stateTips.Visible = false;
		}
		else 
		{
            stateTips.Visible = true;
        }
    }

}
