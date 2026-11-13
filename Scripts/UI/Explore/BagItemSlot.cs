using Godot;
using MyProject;
using System;

public partial class BagItemSlot : Control
{
    [Export] public TextureButton bg;
    [Export] public TextureRect itemIcon;
    [Export] public Label numLabel;
    [Export] public TextureRect chooseEdge;

    // 当前格子的物品ID，0 表示空格
    public int ItemID = 0;
    public int Num = 0;

    // 点击格子时的回调（由父面板赋值）
    public Action<BagItemSlot> OnSlotClicked;

    public override void _Ready()
    {
        bg.Pressed += () => OnSlotClicked?.Invoke(this);
    }

    // 设置格子的物品内容
    public void SetItem(int itemID, int num)
    {
        ItemID = itemID;
        Num = num;
        SetSelected(false);
        if (itemID == 0)
        {
            itemIcon.Visible = false;
            numLabel.Text = "";
        }
        else
        {
            itemIcon.Visible = true;
            itemIcon.Texture = ResourceLoader.Load<Texture2D>("res://Assets/Images/Items/" + ConfigManager.Instance.itemDic[itemID].Icon + ".png");
            numLabel.Text = num.ToString();
        }
    }

    // 选中态：显示橙色选中边框（itemChoose.png），和未选中的深蓝格子（bagItem.png）区分开
    public bool IsSelected = false;
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (chooseEdge != null) chooseEdge.Visible = selected;
    }

    // 设置空格
    public void SetEmpty()
    {
        SetItem(0, 0);
        SetSelected(false);
    }
}
