using Godot;
using Godot.Collections;
using MyProject;
using System.Collections.Generic;

public partial class ExploreUI : Control
{
    [Export] public TextureRect bg;
    [Export] public TextureRect image;
    [Export] public Label desLabel;
    [Export] public NinePatchRect noiseBar;
    [Export] public Label exploreProgressLabel;
    [Export] public Label noiseProgressLabel;
    [Export] public HBoxContainer iconContainer;
    [Export] public NinePatchRect exploreProgressBar;

    private RoomChooseBar _roomChooseBar;
    private ExploreChooseBar _exploreChooseBar;

    //判断本次探索是否首次进入房间选择
    private bool firstRoomTimes = true;

    //判断本次探索是否首次进入探索选择
    private bool firstExploreTimes = true;

    public override void _Ready()
    {
        TextTyper.OnTypeEnd += GetItem;
    }

    public void RefreshExplore()
    {        
        Building building = ConfigManager.Instance.buildingDic[GameManager.Instance.currentBuildingID];

        //获取最大房间层级并给每层房间赋值
        int maxLayer = 0;

        foreach (var item in building.RoomID)
        {
            if (ConfigManager.Instance.roomDic[item].RoomLayer > maxLayer)
            {
                maxLayer = ConfigManager.Instance.roomDic[item].RoomLayer;
            }
        }

        Array<int>[] LayerArray = new Array<int>[maxLayer];
        for (int i = 0; i < LayerArray.Length; i++)
        {
            LayerArray[i] = new Array<int>();
        }
        for (int i = 1; i < LayerArray.Length + 1; i++)
        {
            foreach (var item in building.RoomID)
            {
                if (ConfigManager.Instance.roomDic[item].RoomLayer == i)
                {
                    LayerArray[i - 1].Add(item);
                }
            }
        }
        RefreshExploreUI(false);
        image.Visible = false;
        //初始化房间
        _roomChooseBar = (RoomChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/RoomChooseBar.tscn");

        if (GameManager.Instance.exploreLayer <= maxLayer)
        {
            if (firstRoomTimes == true) 
            { 
                TextTyper.TypeText(desLabel, building.Des);
                firstRoomTimes = false;
            }
            else
            {
                desLabel.Text = building.Des;
            }           
            _roomChooseBar.Init(this, LayerArray[GameManager.Instance.exploreLayer - 1], maxLayer);
        }
        else
        {
            TextTyper.TypeText(desLabel, "已经到头，没什么好探索的了");
            _roomChooseBar.Init(this);
        }
    }
    public void RefreshExploreUI(bool showProgress)
    {
        
        exploreProgressBar.Visible = showProgress;
        if (GameManager.Instance.exploreProgress.ContainsKey(GameManager.Instance.roomID))
        {
            exploreProgressLabel.Text = GameManager.Instance.exploreProgress[GameManager.Instance.roomID].ToString() + "%";
        }
        else
        {
            GameManager.Instance.exploreProgress.Add(GameManager.Instance.roomID, 0);
            exploreProgressLabel.Text = GameManager.Instance.exploreProgress[GameManager.Instance.roomID].ToString() + "%";
        }
        noiseProgressLabel.Text = GameManager.Instance.exploreNoise.ToString() + "%";

        ExploreEvent exploreEvent = ConfigManager.Instance.exploreEventDic[GameManager.Instance.currentEventID];
        GD.Print("获得物品数量" + exploreEvent.ItemID.Count);
    }
    //探索获得物品
    public void GetItem()
    {
        iconContainer.Visible = true;
        foreach (var item in iconContainer.GetChildren())
        {
            item.QueueFree();
        }
        ExploreEvent exploreEvent = ConfigManager.Instance.exploreEventDic[GameManager.Instance.currentEventID];
        for (int i = 0; i < exploreEvent.ItemID.Count; i++)
        {
            if (exploreEvent.ItemID[i] < 10000)
            {
                Godot.Collections.Dictionary<int, int> itemDictionary = new Godot.Collections.Dictionary<int, int>();
                for (int j = 0; j < exploreEvent.ItemNum[i]; j++)
                {
                    int itemID = Tools.GetRandomNumber(ConfigManager.Instance.itemPoolDic[exploreEvent.ItemID[i]].Item);
                    PlayerManager.Instance.AddItem(itemID, 1);
                    if (itemDictionary.ContainsKey(itemID))
                        itemDictionary[itemID]++;
                    else
                        itemDictionary[itemID] = 1;
                }
                foreach (var kvp in itemDictionary)
                {
                    var scene1 = GD.Load<PackedScene>("res://UI/Explore/exploreIcon.tscn");
                    ExploreIcon exploreIcon1 = scene1.Instantiate<ExploreIcon>();
                    iconContainer.AddChild(exploreIcon1);
                    exploreIcon1.Initial(kvp.Key, kvp.Value);
                }
            }
            else
            {
                PlayerManager.Instance.AddItem(exploreEvent.ItemID[i], exploreEvent.ItemNum[i]);
                var scene = GD.Load<PackedScene>("res://UI/Explore/exploreIcon.tscn");
                ExploreIcon exploreIcon = scene.Instantiate<ExploreIcon>();
                iconContainer.AddChild(exploreIcon);
                exploreIcon.Initial(exploreEvent.ItemID[i], exploreEvent.ItemNum[i]);
            }               
        }
    }
    // 销毁房间选择面板，加载事件，创建搜索策略面板
    public void OnRoomSelected(int roomID,bool finish)
    {
        GameManager.Instance.roomID = roomID;

        if (_roomChooseBar != null)
        {
            UIManager.Instance.DeleteUI(_roomChooseBar);
            _roomChooseBar = null;
        }

        GameManager.Instance.LoadEvent(roomID);
        GameManager.Instance.exploreState = 2;
        if (finish==false)
        {
            if (firstExploreTimes == true)
            {
                TextTyper.TypeText(desLabel, ConfigManager.Instance.roomDic[roomID].Des);
                firstExploreTimes = false;
            }
            else
            {
                desLabel.Text = ConfigManager.Instance.roomDic[roomID].Des;
            }          
        }
        else
        {
            TextTyper.TypeText(desLabel, "这个区域已经没有什么好探索的了");
        }
        

        // 创建独立的搜索策略面板
        _exploreChooseBar = (ExploreChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/ExploreChooseBar.tscn");
        _exploreChooseBar.Init(this,finish);
        image.Visible = false;
    }

    // 离开探索：销毁所有探索相关面板和自身
    public void LeaveExplore()
    {
        if (_roomChooseBar != null)
        {
            UIManager.Instance.DeleteUI(_roomChooseBar);
            _roomChooseBar = null;
        }
        UIManager.Instance.DeleteUI(this);
        GameManager.Instance.exploreNoise = 0;
        TextTyper.OnTypeEnd -= GetItem;
    }
    public void LeaveRoom()
    {
        if (_exploreChooseBar != null)
        {
            UIManager.Instance.DeleteUI(_exploreChooseBar);
            _exploreChooseBar = null;
        }
        GameManager.Instance.exploreState = 1;
        GameManager.Instance.exploreLayer += 1;
        firstExploreTimes = true;
        RefreshExplore();
    }
    public int BattleEventReady(int type)
    {
        //获取怪物ID
        EnemyPool enemyPool = ConfigManager.Instance.enemyPoolDic[ConfigManager.Instance.roomDic[GameManager.Instance.roomID].EnemyPool];
        GameManager.Instance.enemyID = Tools.GetRandomNumber(enemyPool.Enemy, enemyPool.Weight);
        //探索随机遇到战斗
        if (type == 1)
        {
            Array<int> eventArray = new Array<int>() { 10001, 10002 };
            Array<int> weightArray = new Array<int>() { 50, 50 };
            Tools.GetRandomNumber(eventArray, weightArray);
            return 10002;
        }
        //探索强制遇到战斗
        else if (type == 2)
        {
            return 10002;
        }
        //事件遇到指定战斗事件(走探索自己逻辑，不取事件ID)
        else if (type == 3)
        {
            return 10001;
        }
        else
        {
            return 10002;
        }
    }
}
