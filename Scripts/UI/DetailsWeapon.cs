using Godot;
using MyProject;
using System;
using System.Text;
using static Godot.WebSocketPeer;

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
    [Export] public TextureRect typeIcon;
    [Export] public TextureRect itemIcon;
    [Export] public Label armorDes;
    [Export] public Label weaponDes;

    public override void _Ready()
    {
        BG.Pressed += () => { UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsWeapon.tscn"); UIManager.Instance.HideUI("res://UI/DetailsTag/DetailsArmer.tscn"); UIManager.Instance.item.edge.Visible = false; };
    }
    public void InitialTips(int ID)
    {
        PlayerManager pm = PlayerManager.Instance;
        itemIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Items/" + ConfigManager.Instance.itemDic[ID].Icon + ".png");
        //获取装备ID
        int equipID = ConfigManager.Instance.itemDic[ID].EquipID;

        //获取装备本身
        Equip equip = ConfigManager.Instance.equipDic[equipID];

        //初始化可见性
        noEffectLabel.Visible = false;
        effectLabel.Visible = true;

        UIManager.Instance.SetLabelRarityColor(nameLable, ConfigManager.Instance.itemDic[ID].Rarity);
        //选择稀有度
        for (int i = 0; i < Rarity.Length; i++)
        {
            Rarity[i].Visible = false;
        }
        Rarity[ConfigManager.Instance.itemDic[ID].Rarity - 1].Visible = true;

        //文本赋值
        nameLable.Text = equip.Name;
        if (ConfigManager.Instance.equipDic[equipID].EffectDes != "")
        {
            effectLabel.Text = equip.EffectDes;
        }
        else
        {
            noEffectLabel.Visible = true;
            effectLabel.Visible = false;
        }

        if (equip.Type == 1)
        {
            baseDamageLable.Text = equip.Damage.ToString();
            weaponDes.Visible = true;
            armorDes.Visible = false;
            damageLable.Text = Math.Round(equip.Damage + pm.Strength * equip.StrongAdd + pm.Agility * equip.SpeedAdd + pm.Intelligence * equip.BrainAdd, 1).ToString();
            //UIManager.Instance.SetLabelRarityColor(weaponDes, ConfigManager.Instance.itemDic[ID].Rarity);
        }
        else if (equip.Type == 2 || equip.Type == 3)
        {
            baseDamageLable.Text = equip.Defence.ToString();
            weaponDes.Visible = false;
            armorDes.Visible = true;
            damageLable.Text = Math.Round(equip.Defence + pm.Strength * equip.StrongAdd + pm.Agility * equip.SpeedAdd + pm.Intelligence * equip.BrainAdd, 1).ToString();
            //UIManager.Instance.SetLabelRarityColor(armorDes, ConfigManager.Instance.itemDic[ID].Rarity);
        }
        else
        {
            baseDamageLable.Text = "";
        }

        strongAddLabel.Text = "x" + equip.SpeedAdd.ToString();
        speedAddLabel.Text = "x" + equip.StrongAdd.ToString();
        brainAddLabel.Text = "x" + equip.BrainAdd.ToString();

        if (equip.HasState==1)
        {
            stateTips.Visible = true;
            StateTips tips = stateTips as StateTips;
            tips.Initail(equip.StateID);
        }

        typeIcon.Texture = UIManager.Instance.SetItemRarityType(ID);
    }


}
