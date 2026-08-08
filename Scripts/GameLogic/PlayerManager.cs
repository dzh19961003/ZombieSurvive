// ============================================================
//  PlayerManager — 玩家数据管理（使用 SaveManager 的完整示例）
//
//  要参与存档只需要做三件事：
//   1. 实现 ISaveable 接口
//   2. 在 _Ready() 里 AddToGroup("Persist")
//   3. 写 GetSaveData() 和 LoadSaveData()
// ============================================================

using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;
using MyProject;

public partial class PlayerManager : Node, ISaveable
{
    public static PlayerManager Instance { get; private set; }
    public string SaveKey => GetPath();

    //仓库
    public Dictionary<int, int> ItemDic = new Dictionary<int, int>();
 
   
    private int hp = 100;
    private int maxHpBase = 100;
    private int strengthBase = 10;
    private int agilityBase = 10;
    private int intelligenceBase = 10;
    public int StrengthBase
    {
        get
        {
            return strengthBase + GetAddition(10003);
        }
        private set
        {

        }
    }
    // ===== 玩家属性 =====
    // 外部可读，私有可写；修改请走下方提供的方法，确保逻辑统一
    public int HP
    {
        get
        {
            return hp + GetAddition(10002);
        }
        private set
        {

        }
    }
    public int GetAddition(int statetype)
    {
        int v = 0;
        foreach (var item in stateArray)
        {
            if (statetype == ConfigManager.Instance.stateDic[item].EffctType)
            {
                v += ConfigManager.Instance.stateDic[item].EffctNum;
            }

        }

        return v;
    }

    public int MaxHP { get; private set; } = 100;
    public int Strength { get; private set; } = 10;       // 力量
    public int Agility { get; private set; } = 10;        // 敏捷
    public int Intelligence { get; private set; } = 10;   // 智力
    
    // 三项属性对应的经验值（进度条）。满 ExpMax 时，对应属性 +1 并清零。
    public int StrengthExp { get; private set; } = 0;
    public int AgilityExp { get; private set; } = 0;
    public int IntelligenceExp { get; private set; } = 0;
    // 经验条上限（与 PropertyUI 进度条 max 一致）
    public const int ExpMax = 100;
   
  
   //天赋列表
    public Array<int> talentID = new Array<int>() { 1,2};
    public Dictionary<int,int> ItemArray=new Dictionary<int, int>() { };
//测试数据
    private Array<int> stateArray = new Array<int>() { };
    public void GetState(int ID)
    {
        stateArray.Add(ID);
    }
    public void RemoveState(int ID)
    {
        if (stateArray.Contains(ID))
        {
            stateArray.Remove(ID);
        }    
    }
    //增加基础属性值
    public void AddItem(int id,int amount)
    {   
        //加属性
        if(id>10000){
        switch ( id)
        {
            case 10001:
            hp+=amount;
        
                break;
            case 10006:
            if (amount <= 0) return;
            StrengthExp += amount;
            while (StrengthExp >= ExpMax)
            {

            StrengthExp -= ExpMax;
            AddItem(10003,1);            
            GD.Print($"[PlayerManager] 力量经验满，力量+1 → {Strength}");
            }
                break;
            default:

                break;
        }
        }
        // 加物品
        else
        {
            if (!ItemDic.ContainsKey(id))
            {
                ItemDic.Add(id,amount);
            }
            else
            {
                ItemDic[id]+=amount;
            }
        }
        
    }
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("[PlayerManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;

        AddToGroup("Save");
        SaveManager.Instance.Save();
        SaveManager.Instance.Load();
        GD.Print(HP);
        GD.Print(StrengthBase);
    }


  
 
 
    #region 存档相关
       public Dictionary GetSaveData()
    {
        return new Dictionary
        {
            { "hp", HP},
            { "maxHP", MaxHP},
            { "strength", Strength},
            { "agility", Agility},
            { "intelligence", Intelligence},
            { "strengthExp", StrengthExp},
            { "agilityExp", AgilityExp},
            { "intelligenceExp", IntelligenceExp},
            { "ItemArray", ItemArray},
            { "ItemDic",ItemDic},
            { "talentID", talentID },
            { "stateArray", stateArray}
        };
    }
    public void LoadSaveData(Dictionary data)
    {
        HP            = data.ContainsKey("hp")           ? (int)data["hp"]           : 100;
        MaxHP         = data.ContainsKey("maxHP")        ? (int)data["maxHP"]        : 100;
        Strength      = data.ContainsKey("strength")     ? (int)data["strength"]     : 10;
        Agility       = data.ContainsKey("agility")      ? (int)data["agility"]      : 10;
        Intelligence  = data.ContainsKey("intelligence") ? (int)data["intelligence"] : 10;
        StrengthExp     = data.ContainsKey("strengthExp")     ? (int)data["strengthExp"]     : 0;
        AgilityExp      = data.ContainsKey("agilityExp")      ? (int)data["agilityExp"]      : 0;
        IntelligenceExp = data.ContainsKey("intelligenceExp") ? (int)data["intelligenceExp"] : 0;
        // ItemArray = data.ContainsKey("ItemArray") ? (Array<int>)data["ItemArray"] : new Array<int>{1,2};
        ItemDic = data.ContainsKey("ItemDic") ? (Dictionary<int, int>)data["ItemDic"] : new Dictionary<int, int> { };
        talentID = data.ContainsKey("talentID") ? (Array<int>)data["talentID"] : new Array<int> { };
        stateArray = data.ContainsKey("stateArray") ? (Array<int>)data["stateArray"] : new Array<int> { };
        GD.Print($"[PlayerManager] 数据恢复完成：HP={HP}, MaxHP={MaxHP}, Str={Strength}({StrengthExp}/{ExpMax}), Agi={Agility}({AgilityExp}/{ExpMax}), Int={Intelligence}({IntelligenceExp}/{ExpMax})");
    }
    #endregion
}
