using Godot;
using MyProject;
using System;
using System.Text;

public partial class DetailsWeapon : Control
{
    [Export] public TextureRect[] Rarity;
    [Export] public Label nameLable;
    [Export] public Label damageLable;
    [Export] public Label baseDamageLable;
    [Export] public Label strongAddLabel;
    [Export] public Label speedAddLabel;
    [Export] public Label brainAddLabel;
    [Export] public Label effectLabel;
    [Export] public Label noEffectLabel;
    [Export] public Control stateTips;
    [Export] public Button BG;

    public override void _Ready()
	{
        BG.Pressed += () => UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsWeapon.tscn");
    }
    public void InitialTips(int ID) 
    {
        
        //获取装备ID
        int equipID = ConfigManager.Instance.itemDic[ID].EquipID;

        //获取装备本身
        Equip equip = ConfigManager.Instance.equipDic[equipID];

        //初始化可见性
        noEffectLabel.Visible = false;
        effectLabel.Visible = true;

        //选择稀有度
        for (int i = 0; i < Rarity.Length; i++)
        {
            Rarity[i].Visible = false;
        }
        Rarity[ConfigManager.Instance.itemDic[ID].Rarity - 1].Visible = true;

        //文本赋值
        nameLable.Text = equip.Name;
        if (ConfigManager.Instance.equipDic[equipID].EffectDes!="")
        {
            effectLabel.Text = equip.EffectDes;
        }
        else
        {
            noEffectLabel.Visible = true;
            effectLabel.Visible = false;
        }
        baseDamageLable.Text = equip.Damage.ToString();
        strongAddLabel.Text = "x" + equip.SpeedAdd.ToString();
        speedAddLabel.Text = "x" + equip.StrongAdd.ToString();
        brainAddLabel.Text = "x" + equip.BrainAdd.ToString();


    }


}
