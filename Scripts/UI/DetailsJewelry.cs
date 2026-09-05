using Godot;
using MyProject;
using System;

public partial class DetailsJewelry : Control
{
    [Export] public TextureRect[] Rarity;
    [Export] public Label nameLable;
    [Export] public Button BG;
    [Export] public TextureRect typeIcon;
    [Export] public TextureButton equipBtn;
    [Export] public Label effectLable;
    [Export] public Control stateTips;
    [Export] public TextureRect itemIcon;
    public override void _Ready()
    {
        BG.Pressed += () => 
        { 
            UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsJewelry.tscn");
            UIManager.Instance.item.edge.Visible = false;
        };
    }

    public void InitialTips(int ID)
    {
        for (int i = 0; i < Rarity.Length; i++)
        {
            Rarity[i].Visible = false;
        }
        itemIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Items/" + ConfigManager.Instance.itemDic[ID].Icon + ".png");
        Rarity[ConfigManager.Instance.itemDic[ID].Rarity - 1].Visible = true;
        nameLable.Text = ConfigManager.Instance.itemDic[ID].Name;
        UIManager.Instance.SetLabelRarityColor(nameLable, ConfigManager.Instance.itemDic[ID].Rarity);
        typeIcon.Texture = UIManager.Instance.SetItemRarityType(ID);
        effectLable.Text = ConfigManager.Instance.equipDic[ConfigManager.Instance.itemDic[ID].EquipID].EffectDes;
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
