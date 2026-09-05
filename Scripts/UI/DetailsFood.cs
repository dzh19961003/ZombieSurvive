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
    [Export] public TextureRect typeIcon;
    [Export] public TextureRect itemIcon;

    private State state;
    public override void _Ready()
	{
        BG.Pressed += () => { UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsFood.tscn"); UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsMedic.tscn"); UIManager.Instance.item.edge.Visible = false; };
    }

	public void InitialTips(int ID) 
	{
        //初始化可见性
        noStateLabel.Visible = false;
        stateLabel.Visible = true;
        stateNameLabel.Visible = true;

        itemIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Items/" + ConfigManager.Instance.itemDic[ID].Icon + ".png");
        //选择稀有度
        for (int i = 0; i < Rarity.Length; i++)
        {
            Rarity[i].Visible = false;
        }
        Rarity[ConfigManager.Instance.itemDic[ID].Rarity - 1].Visible = true;

        //设置文本
        nameLable.Text = ConfigManager.Instance.itemDic[ID].Name;
        UIManager.Instance.SetLabelRarityColor(nameLable, ConfigManager.Instance.itemDic[ID].Rarity);
        numLabel.Text = ConfigManager.Instance.itemDic[ID].Num.ToString();

        if (ConfigManager.Instance.itemDic[ID].BuffID != 0 )
		{
			state = ConfigManager.Instance.stateDic[ConfigManager.Instance.itemDic[ID].BuffID];
            stateNameLabel.Text = state.Name;
            //更改状态字体颜色
            if (state.Positive == 0)
            {
                stateNameLabel.AddThemeColorOverride("font_color", Color.FromHtml("#ee634e"));
            }
            else
            {
                stateNameLabel.AddThemeColorOverride("font_color", Color.FromHtml("#9fce94"));
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
        typeIcon.Texture = UIManager.Instance.SetItemRarityType(ID);
    }

}
