using Godot;
using MyProject;
using System;

public partial class Item : Control
{
	public int ID;

	[Export] public Sprite2D[] Rarity;
	[Export] public TextureButton BG;
    [Export] public TextureRect itemIcon;


    public override void _Ready()
	{
		BG.Pressed += OnItemClick;
        InitialItem();
    }

	public void InitialItem() 
	{
		if (ID!=0)
		{
            GD.Print(ConfigManager.Instance.itemDic[ID].Icon);
            //读取物品图标
            itemIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Items/" + ConfigManager.Instance.itemDic[ID].Icon+".png");

            //读取稀有度
            for (int i = 0; i < Rarity.Length; i++)
            {
                Rarity[i].Visible = false;
            }
            Rarity[ConfigManager.Instance.itemDic[ID].Rarity - 1].Visible = true;
        }
		else
		{
            for (int i = 0; i < Rarity.Length; i++)
            {
                Rarity[i].Visible = false;
            }
        }
		       
    }
	private void OnItemClick()
	{
		
	}
}
