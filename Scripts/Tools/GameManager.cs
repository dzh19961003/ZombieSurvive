using System;
using Godot;
using Godot.Collections;
using MyProject;

public partial class GameManager : Node2D,ISaveable
{
    /*当前探索环节
    选择房间 = 1,
    选择事件 = 2，
    探索完毕 = 3 */
    public int exploreState = 1;

    //当前建筑ID
    public int currentBuildingID = 1;

    //当前事件ID
    public int currentEventID = 1;

    //当前探索层级，与房间所在层级相同
    public int exploreLayer = 1;

    //当前房间ID
    public int roomID=1;

    //已完成的支线
    public Array<int> finishedEvent = new Array<int>();

    //各建筑探索进度
    public Dictionary<int, int> exploreProgress = new Dictionary<int, int>();

    // 当前剧本ID（游戏开始前随机 1~4，随存档保存/读取）
    public int currentScriptID = 0;

    //各建筑已触发支线
    public Dictionary<int, Array<int>> subTaskDic = new Dictionary<int, Array<int>>();

    //当前直线
    public int currentSubTask=0;

    //建筑探索噪音值
    public int exploreNoise=0;

    //局内探索背包物品（不接入存档，只在一次外出探索中使用）
    //key=物品ID，value=数量；一格放一种，数量不限
    public Dictionary<int, int> BagItemDic = new Dictionary<int, int>();

    //各房间剩余可探索次数（key=房间ID），接入存档
    //房间第一次被读到时会初始化成 room 表里的 exploreNum
    public Dictionary<int, int> roomExploreNum = new Dictionary<int, int>();

    //末层所有房间都探索完毕后置位，用于房间选择界面显示"这次已经探索完毕了"
    //不进存档，只在一次外出探索流程内有效（早晨恢复时清空）
    public bool lastLayerFinished = false;

    //当前事件
    public Array<int> carefulEventArray = new Array<int>();
    public Array<int> quickEventArray = new Array<int>();

    //怪物ID
    public int enemyID=0;
    public static GameManager Instance { get; private set; }

    #region 存档相关
    // 保存：剧本ID、当前天数、各房间剩余探索次数。
    // 时段固定回到"早晨"；除 roomExploreNum 外的探索流程状态不保存。
    public string SaveKey => GetPath();

    public Dictionary GetSaveData()
    {
        // Json.Stringify 只认 Godot 的 Dictionary，所以把 int 型的 key 转成字符串再存
        var roomSave = new Godot.Collections.Dictionary();
        foreach (var kv in roomExploreNum)
        {
            roomSave[kv.Key.ToString()] = kv.Value;
        }

        return new Dictionary
        {
            { "dayCount", dayCount },
            { "currentScriptID", currentScriptID },
            { "roomExploreNum", roomSave }
        };
    }

    public void LoadSaveData(Dictionary data)
    {
        dayCount = data.ContainsKey("dayCount") ? (int)data["dayCount"] : 1;
        currentScriptID = data.ContainsKey("currentScriptID") ? (int)data["currentScriptID"] : 0;

        roomExploreNum.Clear();
        if (data.ContainsKey("roomExploreNum"))
        {
            var roomSave = data["roomExploreNum"].AsGodotDictionary();
            foreach (var key in roomSave.Keys)
            {
                roomExploreNum[int.Parse(key.ToString())] = (int)roomSave[key];
            }
        }
    }
    #endregion

    public override void _Ready()
	{
        if (Instance != null)
        {
            GD.PrintErr("[GameManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;

        this.AddToGroup("Save");
    }

    #region 时间演化系统
    // 时段最大值
    public const int MaxTimePeriod = 3;
    // 时段中文名称数组
    private static readonly string[] TimePeriodNames = { "早晨", "上午", "下午", "夜晚" };
    private int currentTimePeriod = 0;
    private int dayCount = 1;
    //当前时段
    public int CurrentTimePeriod {get { return currentTimePeriod; }}
    //当前天数
    public int DayCount {get {return dayCount;}}
    /// <summary>当前时段中文名称</summary>
    public string CurrentTimePeriodName {get {return TimePeriodNames[currentTimePeriod];}}
    public event Action<int, int> TimeChanged;
    //时间推进
    public void AdvanceTime()
    {
        if (currentTimePeriod >= MaxTimePeriod)
        {
            // 跨天
            currentTimePeriod = 0;
            dayCount++;
        }
        else
        {
            currentTimePeriod++;
        }
        
        PlayerManager.Instance?.OnTimeAdvanced();
        TimeChanged?.Invoke(currentTimePeriod, dayCount);
        GD.Print($"[GameManager] 时间推进 → 第{dayCount}天 {CurrentTimePeriodName}");
    }
    public void SetTimePeriod(int period, int day = -1)
    {
        currentTimePeriod = Mathf.Clamp(period, 0, MaxTimePeriod);
        if (day > 0)
        {
            dayCount = day;
        }
        TimeChanged?.Invoke(currentTimePeriod, dayCount);
    }
    public string GetTimePeriodName(int period)
    {
        if (period < 0 || period > MaxTimePeriod) return "";
        return TimePeriodNames[period];
    }
    #endregion

    #region 探索进度读写
    // 统一用这三个方法读写 exploreProgress，不要在别处直接 exploreProgress[房间ID]
    //（直接查的话，房间还没记录过就会报 KeyNotFoundException）

    // 取某个房间的探索进度，没记录过就当 0
    public int GetExploreProgress(int roomID)
    {
        if (exploreProgress.TryGetValue(roomID, out int value)) return value;
        return 0;
    }

    // 给某个房间加探索进度（没记录过会自动建一条），结果夹在 0~100
    public void AddExploreProgress(int roomID, int add)
    {
        exploreProgress[roomID] = Mathf.Clamp(GetExploreProgress(roomID) + add, 0, 100);
    }

    // 直接设定某个房间的探索进度，结果夹在 0~100
    public void SetExploreProgress(int roomID, int value)
    {
        exploreProgress[roomID] = Mathf.Clamp(value, 0, 100);
    }
    #endregion

    #region 房间剩余探索次数（roomExploreNum）
    // 取某房间剩余可探索次数；没记录过就按配置表的 exploreNum 初始化一条
    public int GetRoomExploreNum(int roomID, int maxNum)
    {
        if (!roomExploreNum.ContainsKey(roomID))
        {
            roomExploreNum[roomID] = maxNum;
        }
        return roomExploreNum[roomID];
    }

    // 每完成一次探索，剩余次数减 1（最低到 0，不会变负）
    public void CostRoomExploreNum(int roomID)
    {
        if (!roomExploreNum.ContainsKey(roomID)) return;
        roomExploreNum[roomID] = Mathf.Max(0, roomExploreNum[roomID] - 1);
    }

    // 直接设定剩余次数（关键支线保留一次探索机会时会用到）
    public void SetRoomExploreNum(int roomID, int value)
    {
        roomExploreNum[roomID] = Mathf.Max(0, value);
    }
    #endregion

    #region 局内背包（BagItemDic）操作
    // 获取背包内某物品数量，没有则为 0
    public int GetBagItemCount(int itemID)
    {
        return BagItemDic.TryGetValue(itemID, out int count) ? count : 0;
    }

    // 判断背包当前是否已满（物品种类 >= BagNum）
    public bool IsBagFull()
    {
        return BagItemDic.Count >= PlayerManager.Instance.BagNum;
    }

    // 往背包加物品。返回 true=成功；false=背包已满且不是已有物品类型
    public bool AddBagItem(int itemID, int amount)
    {
        if (amount <= 0) return true;
        // 已存在的物品类型不受容量限制，只有新种类才检查
        if (!BagItemDic.ContainsKey(itemID) && IsBagFull())
        {
            return false;
        }
        BagItemDic[itemID] = GetBagItemCount(itemID) + amount;
        return true;
    }

    // 从背包移除物品，数量不足则移除全部并返回实际移除量
    public int RemoveBagItem(int itemID, int amount)
    {
        if (!BagItemDic.ContainsKey(itemID)) return 0;
        int have = BagItemDic[itemID];
        int remove = Mathf.Min(have, amount);
        BagItemDic[itemID] = have - remove;
        if (BagItemDic[itemID] <= 0)
        {
            BagItemDic.Remove(itemID);
        }
        return remove;
    }

    // 清空背包（探索结束把东西放回仓库前调用）
    public void ClearBag()
    {
        BagItemDic.Clear();
    }
    #endregion

    public void LoadEvent(int roomID)
    {
        Array<int> eventArray = new Array<int>();
        Array<int> eventArray2 = new Array<int>();

        foreach (var item in ConfigManager.Instance.roomDic[roomID].EventPool)
        {
            
            foreach (var item2 in ConfigManager.Instance.eventPoolDic[item].Event)
            {
                if (ConfigManager.Instance.eventDic[item2].HeadType == 1 && !eventArray.Contains(item2) && ConfigManager.Instance.eventDic[item2].EventType!=102 && ConfigManager.Instance.eventDic[item2].EventType != 103)
                {
                    eventArray.Add(item2);
                }
            }
        }
        foreach (var item in ConfigManager.Instance.roomDic[roomID].EventPool2)
        {

            foreach (var item2 in ConfigManager.Instance.eventPoolDic[item].Event)
            {
                if (ConfigManager.Instance.eventDic[item2].HeadType == 1 && !eventArray2.Contains(item2) && ConfigManager.Instance.eventDic[item2].EventType != 102 && ConfigManager.Instance.eventDic[item2].EventType != 103)
                {
                    eventArray2.Add(item2);
                }
            }
        }
        carefulEventArray = eventArray;
        quickEventArray = eventArray2;
    }

    #region 剧本与日记
    /// <summary>
    /// 新游戏开始时调用：从 script 表随机一个剧本ID（当前为 1~4），
    /// 同时重置天数与探索进度，把GameManager还原到初始状态。
    /// </summary>
    public void NewGameStart()
    {
        // 从 scriptList 里取一个随机ID
        if (ConfigManager.Instance.scriptList == null || ConfigManager.Instance.scriptList.Count == 0)
        {
            GD.PrintErr("[GameManager] script表为空，无法随机剧本，默认用 1");
            currentScriptID = 1;
        }
        else
        {
            int r = GD.RandRange(0, ConfigManager.Instance.scriptList.Count - 1);
            currentScriptID = ConfigManager.Instance.scriptList[r].ID;
            GD.Print($"[GameManager] 新游戏 → 随机剧本ID = {currentScriptID}");
        }

        // 重置时间到第1天早晨
        SetTimePeriod(0, 1);

        // 清空探索进度与支线（新游戏从零开始，房间探索次数也要重置）
        exploreProgress.Clear();
        subTaskDic.Clear();
        finishedEvent.Clear();
        roomExploreNum.Clear();
        lastLayerFinished = false;

        GD.Print("[GameManager] 新游戏初始化完成");
    }

    /// <summary>
    /// 继续游戏时调用：把 GameManager 重置为"当前这一天的早晨"。
    /// 只负责时间/天数回到当天早晨，并清空当天会重新跑的探索流程状态。
    /// 玩家自身的体力、饥饿、物品等由调用方（StartMenu）决定怎么处理，
    /// 这里不动 PlayerManager。
    /// </summary>
    public void RestoreToMorning()
    {
        // 时段固定回到早晨，天数保持 LoadSaveData 恢复的值
        SetTimePeriod(0, dayCount);

        // 探索流程不保存，醒来就清空重新来
        exploreProgress.Clear();
        subTaskDic.Clear();
        finishedEvent.Clear();
        exploreState = 1;
        exploreLayer = 1;
        currentBuildingID = 1;
        currentEventID = 1;
        roomID = 1;
        exploreNoise = 0;
        currentSubTask = 0;
        carefulEventArray = new Array<int>();
        quickEventArray = new Array<int>();
        lastLayerFinished = false;

        GD.Print($"[GameManager] 已恢复到第{dayCount}天早晨（剧本 {currentScriptID}）");
    }

    /// <summary>
    /// 取当前天数 + 当前剧本 对应的日记文本。
    /// 同一天的同一条目可能有多份（type、scriptID 不同），
    /// 会筛选出 Day == day 且 ScriptID 包含当前剧本ID 的第一条返回。
    /// 找不到返回空字符串（调用方自行决定是否弹出）。
    /// </summary>
    public string GetDiaryTextForCurrentDay()
    {
        int day = dayCount;
        int scriptID = currentScriptID;
        if (ConfigManager.Instance.diaryList == null) return "";

        foreach (var d in ConfigManager.Instance.diaryList)
        {
            if (d.Day == day && d.ScriptID != null && d.ScriptID.Contains(scriptID))
            {
                return d.Diaries ?? "";
            }
        }
        return "";
    }

    /// <summary>
    /// 指定天数的日记（按当前剧本筛选）。供非"当前天"的场景调用。
    /// </summary>
    public string GetDiaryTextForDay(int day)
    {
        if (ConfigManager.Instance.diaryList == null) return "";
        int scriptID = currentScriptID;
        foreach (var d in ConfigManager.Instance.diaryList)
        {
            if (d.Day == day && d.ScriptID != null && d.ScriptID.Contains(scriptID))
            {
                return d.Diaries ?? "";
            }
        }
        return "";
    }
    #endregion

}
