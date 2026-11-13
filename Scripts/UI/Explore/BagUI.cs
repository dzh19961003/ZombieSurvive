using Godot;
using MyProject;
using System.Collections.Generic;

public partial class BagUI : Control
{
    [Export] public Button closeBtn;
    [Export] public GridContainer itemList;
    [Export] public Label titleLabel;

    // 每行格子数
    private const int Columns = 4;
    private BagItemSlot selectedSlot;

    public override void _Ready()
    {
        closeBtn.Pressed += () => UIManager.Instance.DeleteUI(this);
        Refresh();
    }

    // 刷新背包物品格子（4列一行，格子数读取 BagNum）
    public void Refresh()
    {
        foreach (Node child in itemList.GetChildren())
        {
            child.QueueFree();
        }
        selectedSlot = null;

        int bagNum = PlayerManager.Instance.BagNum;
        int rows = (bagNum + Columns - 1) / Columns;
        int totalSlots = rows * Columns;

        // 先放满空格子
        PackedScene slotScene = GD.Load<PackedScene>("res://UI/Explore/bagItemSlot.tscn");
        for (int i = 0; i < totalSlots; i++)
        {
            BagItemSlot slot = slotScene.Instantiate<BagItemSlot>();
            itemList.AddChild(slot);
            slot.SetEmpty();
            slot.OnSlotClicked = OnSlotClicked;
        }

        // 再把实际物品填进前 N 格（按ID排序保证稳定）
        var bagItems = GameManager.Instance.BagItemDic;
        var ids = new List<int>(bagItems.Keys);
        ids.Sort();
        for (int i = 0; i < ids.Count && i < totalSlots; i++)
        {
            if (itemList.GetChild(i) is BagItemSlot slot)
            {
                slot.SetItem(ids[i], bagItems[ids[i]]);
            }
        }
    }

    // 点击格子切换选中态：橙色边框表示选中，再点一次取消
    private void OnSlotClicked(BagItemSlot slot)
    {
        if (slot.ItemID == 0) return;
        if (selectedSlot != null) selectedSlot.SetSelected(false);
        if (selectedSlot == slot)
        {
            selectedSlot = null;
            return;
        }
        slot.SetSelected(true);
        selectedSlot = slot;
    }
}
