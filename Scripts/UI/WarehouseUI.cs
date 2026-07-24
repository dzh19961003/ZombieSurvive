using Godot;
using MyProject;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class WarehouseUI : Control
{
	[Export] public GridContainer ItemList;
	[Export] public Button closeBtn;
    [Export] public TextureButton[] tabButton;

    private List<int> ItemID = new List<int>();
    private Dictionary<int, int> ItemDic = new Dictionary<int, int>();

    private int gridLength = 6;
    // 最少显示的格子数量，铺满至少一整屏
    private int minNum = 42;

    private int buttonIndex = 1;


    public override void _Ready()
	{
		closeBtn.Pressed += () => UIManager.Instance.HideUI(Paths.WarehouseUI);

        for (int i = 0; i < tabButton.Length; i++)
        {
            int index = i; 
            tabButton[i].Pressed += () => SwitchBtn(index);
        }

        //测试用，先手动添加物品及数量
        ItemDic.Add(1, 15);
        ItemDic.Add(2, 18);
        ItemDic.Add(3, 16);

        ItemID = ItemDic.Keys.ToList<int>();

        SpawnItemList(ItemID,0);
    }

	public override void _Process(double delta)
	{
	}

    /// <summary>
    /// 根据物品ID列表，在 GridContainer 中生成物品格子。
    /// 不足 minNum 时补空格子；超过时按 gridLength 取余补齐最后一行。
    /// </summary>
    private void SpawnItemList(List<int> AllItemList,int type)
    {
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
                item.num = ItemDic[itemList[i]];
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
                item.num = ItemDic[itemList[i]];
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
    private void SwitchBtn(int index) 
    {
        for (int i = 0; i < tabButton.Length; i++)
        {
            tabButton[i].TextureNormal = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/tab_inactive.png");
        }
        tabButton[index].TextureNormal = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/tab_active.png");
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

    public List<int> GetSortedItemID(List<int> itemList)
    {
        List<int> sortedList = itemList;

        return sortedList
            .OrderBy(id => ConfigManager.Instance.itemDic[id].Type)
            .ThenBy(id => ConfigManager.Instance.itemDic[id].Rarity)
            .ThenBy(id => id)  // 最后按ID排序
            .ToList();
    }
}
