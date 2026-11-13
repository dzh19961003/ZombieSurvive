using Godot;
using MyProject;
using System.Collections.Generic;
using System.Linq;

public partial class Warehouse2UI : Control
{
    [Export] public Button closeBtn;
    [Export] public GridContainer warehouseList;   // 上方仓库格子（和项目原仓库同一套 item.tscn）
    [Export] public GridContainer bagList;         // 下方临时背包格子
    [Export] public Label bagNumLabel;             // 背包容量显示（如 5/20）
    [Export] public TextureButton[] tabButton;     // 仓库的 5 个分类页签

    private const int BagColumns = 4;
    private const int WhColumns = 5;
    private int currentType = 0;

    // 页签两态：_1 浅蓝(未选中) / _2 橙色(选中)，底框也一起换
    private const string TabNormalPath = "res://Assets/Images/UI/building/warehouse/tabNormal.png";
    private const string TabSelectedPath = "res://Assets/Images/UI/building/warehouse/tabSelectedl.png";
    private static readonly string[] IconNormal =
    {
        "res://Assets/Images/UI/building/warehouse/all_1.png",
        "res://Assets/Images/UI/building/warehouse/food_1.png",
        "res://Assets/Images/UI/building/warehouse/medicine_1.png",
        "res://Assets/Images/UI/building/warehouse/equip_1.png",
        "res://Assets/Images/UI/building/warehouse/tools_1.png"
    };
    private static readonly string[] IconSelected =
    {
        "res://Assets/Images/UI/building/warehouse/all_2.png",
        "res://Assets/Images/UI/building/warehouse/food_2.png",
        "res://Assets/Images/UI/building/warehouse/medicine_2.png",
        "res://Assets/Images/UI/building/warehouse/equip_2.png",
        "res://Assets/Images/UI/building/warehouse/tools_2.png"
    };

    public override void _Ready()
    {
        closeBtn.Pressed += () => UIManager.Instance.DeleteUI(this);
        for (int i = 0; i < tabButton.Length; i++)
        {
            int index = i;
            tabButton[i].Pressed += () => SwitchType(index);
        }
        SetTabState(0);
        // 延迟初始化，确保 PlayerManager / ConfigManager 就绪
        CallDeferred(nameof(Init));
    }

    private void Init()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        RefreshWarehouse();
        RefreshBag();
    }

    // 上方仓库：直接复用项目原本的 item.tscn 格子，只把点击行为换成"装进背包"
    private void RefreshWarehouse()
    {
        ClearGrid(warehouseList);
        var ids = PlayerManager.Instance.GetAllItemIDs().ToList();
        if (currentType != 0)
        {
            ids = ids.Where(id => ConfigManager.Instance.itemDic[id].Type == currentType).ToList();
        }
        ids = ids
            .OrderBy(id => ConfigManager.Instance.itemDic[id].Type)
            .ThenByDescending(id => ConfigManager.Instance.itemDic[id].Rarity)
            .ThenBy(id => id)
            .ToList();

        // 不足 20 格时补空格子，保证滚动区域铺满
        int slotCount = Mathf.Max(ids.Count, 20);
        slotCount = ((slotCount + WhColumns - 1) / WhColumns) * WhColumns;

        PackedScene itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
        for (int i = 0; i < slotCount; i++)
        {
            Item item = itemScene.Instantiate<Item>();
            warehouseList.AddChild(item);
            if (i < ids.Count)
            {
                int id = ids[i];
                item.ID = id;
                item.num = PlayerManager.Instance.GetItemCount(id);
                item.InitialItem();
                item.OnClickOverride = (it) => AddToBag(it.ID);
            }
            else
            {
                item.ID = 0;
                item.InitialItem();
            }
        }
        warehouseList.UpdateMinimumSize();
    }

    // 下方临时背包：格子数读 BagNum
    private void RefreshBag()
    {
        ClearGrid(bagList);
        var bagIds = GameManager.Instance.BagItemDic.Keys.ToList();
        bagIds.Sort();
        int bagNum = PlayerManager.Instance.BagNum;
        int slotCount = Mathf.Max(((bagNum + BagColumns - 1) / BagColumns) * BagColumns, BagColumns);

        PackedScene slotScene = GD.Load<PackedScene>("res://UI/Explore/bagItemSlot.tscn");
        for (int i = 0; i < slotCount; i++)
        {
            BagItemSlot slot = slotScene.Instantiate<BagItemSlot>();
            bagList.AddChild(slot);
            if (i < bagIds.Count)
            {
                int id = bagIds[i];
                slot.SetItem(id, GameManager.Instance.GetBagItemCount(id));
                slot.OnSlotClicked = (s) => RemoveFromBag(s.ItemID);
            }
            else
            {
                slot.SetEmpty();
            }
        }
        bagNumLabel.Text = GameManager.Instance.BagItemDic.Count + "/" + bagNum;
        bagList.UpdateMinimumSize();
    }

    // 点仓库物品：仓库 -1，背包 +1
    private void AddToBag(int itemID)
    {
        if (PlayerManager.Instance.GetItemCount(itemID) <= 0) return;
        if (!GameManager.Instance.BagItemDic.ContainsKey(itemID) && GameManager.Instance.IsBagFull())
        {
            UIManager.Instance.ShowCommonTips2("背包已经塞满了");
            return;
        }
        PlayerManager.Instance.RemoveItem(itemID, 1);
        GameManager.Instance.AddBagItem(itemID, 1);
        RefreshAll();
    }

    // 点背包物品：背包 -1，仓库 +1
    private void RemoveFromBag(int itemID)
    {
        if (!GameManager.Instance.BagItemDic.ContainsKey(itemID)) return;
        GameManager.Instance.RemoveBagItem(itemID, 1);
        PlayerManager.Instance.AddItem(itemID, 1);
        RefreshAll();
    }

    // 切换页签两态外观（底框 + 图标）
    public void SetTabState(int index)
    {
        for (int i = 0; i < tabButton.Length; i++)
        {
            bool selected = (i == index);
            tabButton[i].TextureNormal = ResourceLoader.Load<Texture2D>(selected ? TabSelectedPath : TabNormalPath);
            TextureRect icon = tabButton[i].GetNodeOrNull<TextureRect>("type");
            if (icon != null)
            {
                icon.Texture = ResourceLoader.Load<Texture2D>(selected ? IconSelected[i] : IconNormal[i]);
            }
        }
    }

    private void SwitchType(int index)
    {
        currentType = index;
        SetTabState(index);
        RefreshWarehouse();
    }

    private void ClearGrid(GridContainer grid)
    {
        foreach (Node child in grid.GetChildren())
        {
            child.QueueFree();
        }
    }
}
