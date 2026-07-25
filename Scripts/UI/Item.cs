using Godot;
using Godot.Collections;
using MyProject;
using System;
using System.Diagnostics;

public partial class Item : Control
{
	public int ID;
    public int num;

	[Export] public TextureRect[] Rarity;
	[Export] public TextureButton BG;
    [Export] public TextureRect itemIcon;
    [Export] public Label numLabel;

    public override void _Ready()
	{
		BG.Pressed += OnItemClick;
        InitialItem();
    }

	public void InitialItem() 
	{
		if (ID == 0)
		{          
            itemIcon.Visible = false;
            for (int i = 0; i < Rarity.Length; i++)
            {
                Rarity[i].Visible = false;
            }
            numLabel.Text = "";
        }
		else
		{
            itemIcon.Visible = true;
            //读取物品图标
            itemIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Items/" + ConfigManager.Instance.itemDic[ID].Icon + ".png");

            //读取稀有度
            for (int i = 0; i < Rarity.Length; i++)
            {
                Rarity[i].Visible = false;
            }
            Rarity[ConfigManager.Instance.itemDic[ID].Rarity - 1].Visible = true;
            numLabel.Text = num.ToString();
        }
		       
    }
	private void OnItemClick()
	{
        if (ID==0)
        {
            return;
        }
        switch (ConfigManager.Instance.itemDic[ID].Type)
        {
            case 1:
                DetailsFood foodTips = (DetailsFood)UIManager.Instance.ShowUI("res://UI/DetailsTag/DetailsFood.tscn");
                foodTips.InitialTips(ID);
                SetSpwanPosition(foodTips);
                break;
            case 2:
                DetailsFood medicTips = (DetailsFood)UIManager.Instance.ShowUI("res://UI/DetailsTag/DetailsMedic.tscn");
                medicTips.InitialTips(ID);
                SetSpwanPosition(medicTips);
                break;
            case 3:
                switch (ConfigManager.Instance.itemDic[ID].EquipID)
                {
                    case 1:
                        DetailsWeapon weaponTips=(DetailsWeapon)UIManager.Instance.ShowUI("res://UI/DetailsTag/DetailsWeapon.tscn");
                        weaponTips.InitialTips(ID);
                        SetSpwanPosition(weaponTips);
                        break;
                    default:
                        break;
                }
                break;

            default:
                break;
        }
    }

    //设置标签位置的方法
    private void SetSpwanPosition(Control control) 
    {

        Vector2 itemGlobalPos = this.GlobalPosition;    
        
        float itemRight = itemGlobalPos.X + Size.X;  
        float itemTop = itemGlobalPos.Y; 


        Vector2 controlSize = control.Size;
        if (controlSize == Vector2.Zero) 
        { 
            controlSize = new Vector2(400, 500);
        }
            

        float controlX, controlY;

        // 屏幕宽度
        float screenWidth = GetViewport().GetVisibleRect().Size.X;

        // 右侧放得下
        if (itemRight + controlSize.X <= screenWidth)
        {
            controlX = itemRight;
        }
        //放不下
        else
        {
            controlX = itemGlobalPos.X - controlSize.X;
        }

        // 竖直方向：
        controlY = itemTop;
        if (controlY < 0)
        { 
            controlY = 0;
        } 
        if (controlY + controlSize.Y > GetViewport().GetVisibleRect().Size.Y)
        { 
            controlY = GetViewport().GetVisibleRect().Size.Y - controlSize.Y;
        }

        // 4) 赋值
        control.Position = new Vector2(controlX, controlY);
    }
}
