using Godot;
using MyProject;
using System;

public partial class DetailsTalent : Control
{
    [Export] public TextureRect[] Rarity;
    [Export] public Label nameLable;
    [Export] public Label descriptino;
    [Export] public Button BG;
    
    public override void _Ready()
    {
        BG.Pressed += () =>
        {
            UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsTalent.tscn");
            
        };
    }
    public void InitialTips(int ID)
    {
        for (int i = 0; i < Rarity.Length; i++)
        {
            Rarity[i].Visible = false;
        }
        Rarity[ConfigManager.Instance.talentDic[ID].Rarity - 1].Visible = true;
        nameLable.Text = ConfigManager.Instance.talentDic[ID].Name;
        descriptino.Text = ConfigManager.Instance.talentDic[ID].Effect;
        
    }
}
