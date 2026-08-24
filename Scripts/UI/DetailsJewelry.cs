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
        Rarity[ConfigManager.Instance.itemDic[ID].Rarity - 1].Visible = true;
        nameLable.Text = ConfigManager.Instance.itemDic[ID].Name;
        typeIcon.Texture = UIManager.Instance.SetItemRarityType(ID);
        effectLable.Text = ConfigManager.Instance.equipDic[ConfigManager.Instance.itemDic[ID].EquipID].EffectDes;
    }
}
