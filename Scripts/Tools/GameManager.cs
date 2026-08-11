using System;
using Godot;
using Godot.Collections;
using MyProject;

public partial class GameManager : Node2D,ISaveable
{
    /*当前游戏环节
    
    写日记 = 1,
    随机事件 = 2,
    早晨 = 3,
    探索 = 4,
    夜晚 = 5 */
    public int gameState = 1;

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

    //各建筑已触发支线
    public Dictionary<int, Array<int>> subTaskDic = new Dictionary<int, Array<int>>();

    //当前直线
    public int currentSubTask=0;

    //建筑探索噪音值
    public int exploreNoise=0;

    //当前事件
    public Array<int> carefulEventArray = new Array<int>();
    public Array<int> quickEventArray = new Array<int>();


    public static GameManager Instance { get; private set; }

    #region 存档相关
    public string SaveKey => GetPath();

    public Dictionary GetSaveData()
    {
        return new Dictionary
        {
            { "gameState", gameState },
            { "exploreProgress",exploreProgress},
            { "timePeriod", currentTimePeriod },
            { "dayCount", dayCount }
        };
    }

    public void LoadSaveData(Dictionary data)
    {
        gameState = data.ContainsKey("gameState") ? (int)data["gameState"] : 1;
        exploreProgress =  data.ContainsKey("exploreProgress") ? (Dictionary<int,int>)data["exploreProgress"] : new Dictionary<int, int>();
        currentTimePeriod = data.ContainsKey("timePeriod") ? (int)data["timePeriod"] : 0;
        dayCount = data.ContainsKey("dayCount") ? (int)data["dayCount"] : 1;
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

    public void LoadEvent(int roomID)
    {
        Array<int> eventArray = new Array<int>();
        Array<int> eventArray2 = new Array<int>();

        foreach (var item in ConfigManager.Instance.roomDic[roomID].EventPool)
        {
            
            foreach (var item2 in ConfigManager.Instance.eventPoolDic[item].Event)
            {
                if (ConfigManager.Instance.eventDic[item2].HeadType == 1 && !eventArray.Contains(item2))
                {
                    eventArray.Add(item2);
                }
            }
        }
        foreach (var item in ConfigManager.Instance.roomDic[roomID].EventPool2)
        {

            foreach (var item2 in ConfigManager.Instance.eventPoolDic[item].Event)
            {
                if (ConfigManager.Instance.eventDic[item2].HeadType == 1 && !eventArray2.Contains(item2))
                {
                    eventArray2.Add(item2);
                }
            }
        }
        carefulEventArray = eventArray;
        quickEventArray = eventArray2;
    }

}
