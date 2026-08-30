using Godot;
using MyProject;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class WarehouseUI : Control
{
	[Export] public GridContainer ItemList;
	[Export] public Button closeBtn;
    [Export] public TextureButton[] tabButton;

    private List<int> ItemID = new List<int>();
    // 当前选中的页签类型（0=全部，1=食物，2=药品，3=装备，4=材料）
    private int currentType = 0;

    private int gridLength = 5;
    // 最少显示的格子数量，铺满至少一整屏
    private int minNum = 35;


    public override void _Ready()
	{
		closeBtn.Pressed += () => UIManager.Instance.HideUI(Paths.WarehouseUI);

        for (int i = 0; i < tabButton.Length; i++)
        {
            int index = i;
            tabButton[i].Pressed += () => SwitchBtn(index);
        }
        tabButton[0].TextureNormal = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/New/switchBtn_2.png");
        // _Ready顺序不确定，延迟初始化避免PlayerManager未就绪
        CallDeferred(nameof(DeferredInit));
    }

    public override void _ExitTree()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.GetItem -= OnPlayerDataChanged;
        }
    }

    private void DeferredInit()
    {
        if (PlayerManager.Instance == null)
        {
            GD.PrintErr("[WarehouseUI] PlayerManager 未就绪");
            return;
        }
        // 订阅玩家数据变化，物品增减时自动刷新仓库
        PlayerManager.Instance.GetItem += OnPlayerDataChanged;
        RefreshWarehouse();
    }

    private void OnPlayerDataChanged()
    {
        CallDeferred(nameof(RefreshWarehouse));
    }

    // 从 PlayerManager 读取最新持有物品并重建当前页签的物品栏
    private void RefreshWarehouse()
    {
        if (PlayerManager.Instance == null) return;
        ItemID = new List<int>(PlayerManager.Instance.GetAllItemIDs());
        ClearItemList();
        SpawnItemList(ItemID, currentType);
    }

    //清空物品栏所有格子
    private void ClearItemList()
    {
        for (int i = 0; i < ItemList.GetChildCount(); i++)
        {
            ItemList.GetChild(i).QueueFree();
        }
    }

    //生成物品栏方法
    private void SpawnItemList(List<int> AllItemList,int type)
    {
        //记录当前页签类型，供数据变化刷新时保持页签
        currentType = type;
        List<int> itemList = GetSortedItemID(AllItemList);
        // 当物品种类小于最小数量时，最低生成42个格子
        if (ItemID.Count <= minNum)
        {
            int skipNum = 0;
            for (int i = 0; i < itemList.Count; i++)
            {
                if (ConfigManager.Instance.itemDic[itemList[i]].Type!=type && type != 0)
                {
                    skipNum += 1;
                    continue;
                }
                var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                Item item = itemScene.Instantiate<Item>();
                ItemList.AddChild(item);
                item.ID = itemList[i];
                item.num = PlayerManager.Instance.GetItemCount(itemList[i]);
                item.InitialItem();
            }
            for (int i = 0; i < (minNum+ skipNum - itemList.Count); i++)
            {
                var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                Item item = itemScene.Instantiate<Item>();
                ItemList.AddChild(item);
                item.ID = 0;
                item.InitialItem();
            }
        }
        // 高于最低数量时，根据取余将格子生成满
        else
        {
            int skipNum = 0;
            for (int i = 0; i < itemList.Count; i++)
            {
                if (ConfigManager.Instance.itemDic[itemList[i]].Type != type && type != 0)
                {
                    skipNum += 1;
                    continue;
                }
                var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                Item item = itemScene.Instantiate<Item>();
                ItemList.AddChild(item);
                item.ID = ItemID[i];
                item.num = PlayerManager.Instance.GetItemCount(itemList[i]);
                item.InitialItem();
            }
            // 补齐最后一行不满的部分
            if (itemList.Count- skipNum>=minNum)
            {
                int remainder = (itemList.Count - skipNum) % gridLength;
                int fillCount = (remainder == 0) ? 0 : (gridLength - remainder);
                for (int i = 0; i < fillCount; i++)
                {
                    var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                    Item item = itemScene.Instantiate<Item>();
                    ItemList.AddChild(item);
                    item.ID = 0;
                    item.InitialItem();
                }
            }
            else
            {
                for (int i = 0; i < minNum- (itemList.Count - skipNum); i++)
                {
                    var itemScene = GD.Load<PackedScene>("res://UI/item.tscn");
                    Item item = itemScene.Instantiate<Item>();
                    ItemList.AddChild(item);
                    item.ID = 0;
                    item.InitialItem();
                }
            }

        }

        // GridContainer 在动态添加子节点后，需要手动让它重新计算自身的最小尺寸，
        // 这样 ScrollContainer 才能根据 GridContainer 的实际内容高度来设置滚动范围。
        // 不调用的话，ScrollContainer 可能拿到的还是旧的最小尺寸，导致无法滚到底部。
        ItemList.UpdateMinimumSize();
    }

    //点击页签按钮后根据类型生成物品栏
    private void SwitchBtn(int index)
    {
        for (int i = 0; i < tabButton.Length; i++)
        {
            tabButton[i].TextureNormal = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/New/switchBtn.png");
        }
        tabButton[index].TextureNormal = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/New/switchBtn_2.png");
        for (int i = 0; i < ItemList.GetChildCount(); i++)
        {
            ItemList.GetChild(i).QueueFree();
        }
        switch (index)
        {
            case 0:
                SpawnItemList(ItemID, 0);
                break;
            case 1:
                SpawnItemList(ItemID, 1);
                break;
            case 2:
                SpawnItemList(ItemID, 2);
                break;
            case 3:
                SpawnItemList(ItemID, 3);
                break;
            case 4:
                SpawnItemList(ItemID, 4);
                break;
            default:
                SpawnItemList(ItemID, 0);
                break;
        }
    }

    //按照类型、稀有度、ID的顺序排行
    public List<int> GetSortedItemID(List<int> itemList)
    {
        List<int> sortedList = itemList;

        return sortedList
            .OrderBy(id => ConfigManager.Instance.itemDic[id].Type)
            .ThenByDescending(id => ConfigManager.Instance.itemDic[id].Rarity)
            .ThenBy(id => id)
            .ToList();
    }
}
