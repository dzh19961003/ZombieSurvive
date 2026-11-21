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
    public bool EnableBattleMedicine { get; set; }
    private Button medicineButton;
    private Label medicineHint;
    private ConfirmationDialog medicineConfirm;
    private int pendingMedicine;

    public override void _Ready()
    {
        closeBtn.Pressed += () => UIManager.Instance.DeleteUI(this);
        if (EnableBattleMedicine)
            CreateMedicineControls();
        Refresh();
    }

    // 刷新背包物品格子（4列一行，格子数读取 BagNum）
    public void Refresh()
    {
        foreach (Node child in itemList.GetChildren())
        {
            itemList.RemoveChild(child);
            child.QueueFree();
        }
        selectedSlot = null;

        int bagNum = PlayerManager.Instance.BagNum;
        int rows = (bagNum + Columns - 1) / Columns;
        var bagItems = GameManager.Instance.BagItemDic;
        int totalSlots = System.Math.Max(rows, (bagItems.Count + Columns - 1) / Columns) * Columns;

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
        var ids = new List<int>(bagItems.Keys);
        ids.Sort();
        for (int i = 0; i < ids.Count && i < totalSlots; i++)
        {
            if (itemList.GetChild(i) is BagItemSlot slot)
            {
                slot.SetItem(ids[i], bagItems[ids[i]]);
            }
        }
        RefreshMedicineControls();
    }

    // 点击格子切换选中态：橙色边框表示选中，再点一次取消
    private void OnSlotClicked(BagItemSlot slot)
    {
        if (slot.ItemID == 0) return;
        if (selectedSlot != null) selectedSlot.SetSelected(false);
        if (selectedSlot == slot)
        {
            selectedSlot = null;
            RefreshMedicineControls();
            return;
        }
        slot.SetSelected(true);
        selectedSlot = slot;
        RefreshMedicineControls();
    }

    private void CreateMedicineControls()
    {
        medicineHint = new Label
        {
            Position = new Vector2(205, 1602), Size = new Vector2(695, 48),
            HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore
        };
        medicineHint.AddThemeFontSizeOverride("font_size", 27);
        AddChild(medicineHint);
        medicineButton = new Button
        {
            Name = "UseMedicine", Text = "使用药品", Position = new Vector2(393, 1651),
            Size = new Vector2(294, 63), MouseDefaultCursorShape = CursorShape.PointingHand
        };
        medicineButton.AddThemeFontSizeOverride("font_size", 27);
        medicineButton.Pressed += ConfirmMedicine;
        AddChild(medicineButton);
        medicineConfirm = new ConfirmationDialog
        {
            Name = "ConfirmMedicine", Title = "使用药品", OkButtonText = "使用", CancelButtonText = "取消",
            Exclusive = true
        };
        medicineConfirm.Confirmed += UseMedicine;
        AddChild(medicineConfirm);
    }

    private static MyProject.Item FindMedicine(int id)
    {
        return ConfigManager.Instance.itemDic.TryGetValue(id, out var item)
            && item.Type == 2 && item.EffectType == 10001 && item.Num > 0 ? item : null;
    }

    private void RefreshMedicineControls()
    {
        if (!EnableBattleMedicine)
            return;
        var item = FindMedicine(selectedSlot?.ItemID ?? 0);
        bool full = PlayerManager.Instance.Hp >= PlayerManager.Instance.MaxHp;
        medicineButton.Disabled = item == null || full;
        medicineHint.Text = selectedSlot == null ? "战斗中只可使用恢复生命的药品"
            : item == null ? "这件物品在战斗中不可使用"
            : full ? "生命值已满" : $"{item.Name} · 恢复 {item.Num} 点生命";
    }

    private void ConfirmMedicine()
    {
        if (medicineButton.Disabled)
            return;
        pendingMedicine = selectedSlot?.ItemID ?? 0;
        var item = FindMedicine(pendingMedicine);
        if (item == null)
            return;
        medicineConfirm.DialogText = $"使用 1 个{item.Name}，恢复最多 {item.Num} 点生命？";
        medicineConfirm.PopupCentered();
    }

    private void UseMedicine()
    {
        var item = FindMedicine(pendingMedicine);
        var player = PlayerManager.Instance;
        if (item != null && player.Hp < player.MaxHp && GameManager.Instance.RemoveBagItem(pendingMedicine, 1) == 1)
            player.AddItem(10001, item.Num);
        pendingMedicine = 0;
        Refresh();
    }
}
