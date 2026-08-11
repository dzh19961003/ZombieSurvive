// ============================================================
//  PlayerManager — 玩家数据管理（使用 SaveManager 的完整示例）
//
//  要参与存档只需要做三件事：
//   1. 实现 ISaveable 接口
//   2. 在 _Ready() 里 AddToGroup("Persist")
//   3. 写 GetSaveData() 和 LoadSaveData()
// ============================================================

using System;
using Godot;
using Godot.Collections;
using MyProject;

public partial class PlayerManager : Node, ISaveable
{
   

    public static PlayerManager Instance { get; private set; }
    public string SaveKey => GetPath();
    public event Action GetItem;
    public event Action<Array<int>> GetItem2;

    // 三项属性对应的经验值。满 ExpMax 时，对应属性 +1 并清零。
    public const int ExpMax = 100;
    public const int MaxHunger = 3;
    //hunger 值 → 状态表 ID 映射（对应 state.json 中 ID 5/6/7/8）
    private static readonly int[] HungerStateIDs = new int[] { 5, 6, 7, 8 };
    //仓库
    private Dictionary<int, int> ItemDic = new Dictionary<int, int>();
   //天赋列表
    private Array<int> talentID = new Array<int>() { 1,2};
    private Dictionary<int,int> ItemArray=new Dictionary<int, int>() { };
   //测试数据
    private Array<int> stateArray = new Array<int>() { };
    //状态剩余天数表：key=状态ID，value=剩余天数（每日结算-1，归0移除）
    private Dictionary<int, int> StateTimeDic = new Dictionary<int, int>();
    //当前自然日内已推进的时段数（0~3）。每满4个时段算完整一天，触发 OnDayEnd。
    private int timePeriodsElapsed = 0;
    public const int PeriodsPerDay = 4;
    
    private int hpBase = 100;
    private int maxHpBase = 100;
    private int strengthBase = 10;
    private int agilityBase = 10;
    private int intelligenceBase = 10;
    private int strength_exp = 0;
    private int agility_exp = 0;
    private int intelligence_exp = 0;
    private double exp_acq_rate = 1.0;
    private int armor = 0;
    private int max_armor = 100;
    private int attack_limb_weight = 100;
    private int attack_head_weight = 100;
    private int attack_body_weight = 100;
    private int baseStamina = 10;
    private int exploreStamina = 10;
    private int maxBaseStamina = 10;
    private int maxexploreStamina = 10;
    private int hunger = 3;
    public int Hp {get{return hpBase + GetAddition(10001);}private set{}}
    public int MaxHp {get{return maxHpBase + GetAddition(10002);}private set{}}
    public int Strength {get{return strengthBase + GetAddition(10003);}private set{}}
    public int Agility {get{return agilityBase + GetAddition(10004);}private set{}}
    public int Intelligence {get{return intelligenceBase + GetAddition(10005);}private set{}}
    public int Strength_exp {get{return (int)(strength_exp + GetAddition(10006)* Exp_acq_rate); }private set{}}
    public int Agility_exp {get{return (int)(agility_exp + GetAddition(10007) * Exp_acq_rate); }private set{}}
    public int Intelligence_exp {get{return (int)(intelligence_exp + GetAddition(10008) * Exp_acq_rate); }private set{}}
    public double Exp_acq_rate {get{return exp_acq_rate + GetAddition(10009);}private set{}}
    public int Armor {get{return armor + GetAddition(10010);}private set{}}
    public int MaxArmor {get{return max_armor + GetAddition(10011);}private set{}}
    public int Attack_limb_weight {get{return attack_limb_weight + GetAddition(10012);}private set{}}
    public int Attack_head_weight {get{return attack_head_weight + GetAddition(10013);}private set{}}
    public int Attack_body_weight {get{return attack_body_weight + GetAddition(10014);}private set{}}
    public int BaseStamina {get{return baseStamina + GetAddition(10015);}private set{}}
    public int ExploreStamina {get{return exploreStamina + GetAddition(10016);}private set{}}
    public int MaxBaseStamina {get{return maxBaseStamina + GetAddition(10017);}private set{}}
    public int MaxexploreStamina {get{return maxexploreStamina + GetAddition(10018);}private set{}}
    public int Hunger {get{return hunger + GetAddition(10019);}private set{}}
    
    //增量计算
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

    //获得状态：状态唯一，重复获得时重置为初始天数而非叠加
    public void GetState(int ID)
    {
        if (!ConfigManager.Instance.stateDic.ContainsKey(ID))
        {
            GD.PrintErr($"[PlayerManager] 状态ID不存在：{ID}");
            return;
        }
        int initTime = ConfigManager.Instance.stateDic[ID].Time;
        if (StateTimeDic.ContainsKey(ID))
        {
            //已有该状态，重置为初始天数
            StateTimeDic[ID] = initTime;
        }
        else
        {
            StateTimeDic.Add(ID, initTime);
            stateArray.Add(ID);
        }
        GetItem?.Invoke();
        GetItem2?.Invoke(stateArray);
    }
    public void RemoveState(int ID)
    {
        if (StateTimeDic.ContainsKey(ID))
        {
            StateTimeDic.Remove(ID);
        }
        if (stateArray.Contains(ID))
        {
            stateArray.Remove(ID);
        }
    }

    public void OnTimeAdvanced()
    {
        timePeriodsElapsed++;
        GD.Print($"[PlayerManager] 时段累计 {timePeriodsElapsed}/{PeriodsPerDay}");
        if (timePeriodsElapsed >= PeriodsPerDay)
        {
            timePeriodsElapsed = 0;
            GD.Print("[PlayerManager] 完整一天结束，开始每日结算");
            OnDayEnd();
        }
    }

    //每日结算：所有状态剩余天数-1，归0时移除状态
    public void OnDayEnd()
    {
        //先拷贝keys，避免遍历时修改字典
        var keys = new Array<int>();
        foreach (var k in StateTimeDic.Keys) keys.Add(k);

        var expired = new Array<int>();
        foreach (var id in keys)
        {
            StateTimeDic[id] -= 1;
            if (StateTimeDic[id] <= 0)
            {
                expired.Add(id);
            }
        }
        //移除过期状态
        foreach (var id in expired)
        {
            StateTimeDic.Remove(id);
            if (stateArray.Contains(id))
            {
                stateArray.Remove(id);
            }
            GD.Print($"[PlayerManager] 状态到期移除：ID={id} ({ConfigManager.Instance.stateDic[id].Name})");
        }

        if (keys.Count > 0)
        {
            GetItem?.Invoke();
            GetItem2?.Invoke(stateArray);
        }
    }
    //只读访问：返回副本，外部无法修改内部数组
    public Array<int> GetStateArray()
    {
        var copy = new Array<int>();
        foreach (var id in stateArray) copy.Add(id);
        return copy;
    }
    public Array<int> GetTalentID()
    {
        var copy = new Array<int>();
        foreach (var id in talentID) copy.Add(id);
        return copy;
    }
    
    //获取指定状态的剩余天数
   
    public int GetStateRemainingDays(int stateID)
    {
        if (StateTimeDic.ContainsKey(stateID))
        {
            return StateTimeDic[stateID];
        }
        return 0;
    }
    private void SyncHungerState(int hunger)
    {
        // 1. 移除所有饥饿类状态（避免多个饥饿状态同时存在）
        foreach (var sid in HungerStateIDs)
        {
            if (StateTimeDic.ContainsKey(sid))
            {
                RemoveState(sid);
            }
        }
        // 2. 施加当前 hunger 对应的状态（HungerStateIDs[0..3] 对应 hunger 0..3）
        int idx = Mathf.Clamp(hunger, 0, MaxHunger);
        int targetID = HungerStateIDs[idx];
        if (ConfigManager.Instance != null &&
            ConfigManager.Instance.stateDic.ContainsKey(targetID))
        {
            GetState(targetID);
            GD.Print($"[PlayerManager] 饥饿状态同步：hunger={hunger} → 施加状态 ID={targetID}（{ConfigManager.Instance.stateDic[targetID].Name}）");
        }
        else
        {
            GD.PrintErr($"[PlayerManager] SyncHungerState 失败：状态表找不到 ID={targetID}");
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
                // 生命值钳制在 [0, MaxHp]（含状态加成的当前上限）
                hpBase = Mathf.Clamp(hpBase + amount, 0, MaxHp);
                break;
            case 10002:
                maxHpBase += amount;
                // 最大生命值变化后，当前生命值不能超过新上限
                hpBase = Mathf.Min(hpBase, MaxHp);
                break;
            case 10003:
                strengthBase += amount;
                break;
            case 10004:
                agilityBase += amount;
                break;
            case 10005:
                intelligenceBase += amount;
                break;
            case 10006:
                if (amount <= 0) return;
                strength_exp += (int)(amount*Exp_acq_rate);
                while (strength_exp >= ExpMax)
                {
                    strength_exp -= ExpMax;
                    AddItem(10003, 1);
                    GD.Print($"[PlayerManager] 强健经验满，强健+1 → {Strength}");
                }
                break;
            case 10007:
                if (amount <= 0) return;
                agility_exp += (int)(amount * Exp_acq_rate);
                while (agility_exp >= ExpMax)
                {
                    agility_exp -= ExpMax;
                    AddItem(10004, 1);
                    GD.Print($"[PlayerManager] 速度经验满，速度+1 → {Agility}");
                }
                break;
            case 10008:
                if (amount <= 0) return;
                intelligence_exp += (int)(amount * Exp_acq_rate);
                while (intelligence_exp >= ExpMax)
                {
                    intelligence_exp -= ExpMax;
                    AddItem(10005, 1);
                    GD.Print($"[PlayerManager] 智力经验满，智力+1 → {Intelligence}");
                }
                break;
            case 10009:
                exp_acq_rate += amount;
                break;
            case 10010:
                armor += amount;
                break;
            case 10011:
                max_armor += amount;
                break;
            case 10012:
                attack_limb_weight += amount;
                break;
            case 10013:
                attack_head_weight += amount;
                break;
            case 10014:
                attack_body_weight += amount;
                break;
            case 10015:
                baseStamina = Mathf.Clamp(baseStamina + amount, 0, MaxBaseStamina);
                break;
            case 10016:
                exploreStamina = Mathf.Clamp(exploreStamina + amount, 0, MaxexploreStamina);
                break;
            case 10017:
                maxBaseStamina += amount;
                baseStamina = Mathf.Min(baseStamina, MaxBaseStamina);
                break;
            case 10018:
                maxexploreStamina += amount;
                exploreStamina = Mathf.Min(exploreStamina, MaxexploreStamina);
                break;
            case 10019:

                hunger = Mathf.Clamp(hunger + amount, 0, MaxHunger);
                SyncHungerState(hunger);
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
        GetItem?.Invoke();
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
        // 读档后 hunger 已恢复，同步一次初始饥饿状态到 stateArray
        // （否则初始 stateArray 里没有饱腹状态，PropertyUi 打开看不到）
        // 用 CallDeferred：保证 ConfigManager 已初始化（SyncHungerState 内部要读表）
        CallDeferred(nameof(DeferredInitHungerState));
        GetState(2);
    }

    private void DeferredInitHungerState()
    {
        if (ConfigManager.Instance == null)
        {
            GD.PrintErr("[PlayerManager] ConfigManager 未就绪，饥饿状态初始化失败");
            return;
        }
        SyncHungerState(hunger);
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            AddItem(10001, -15);
            AddItem(10002, 20);
            AddItem(10003, 10);
            // GetState(2);
            AddItem(10019, -1);
            GameManager.Instance.AdvanceTime();
        }
    }
    #region 存档相关
    public Dictionary GetSaveData()
    {
        // 存私有 base 字段（基础值，不含 GetAddition 加成）。
        // 加成由 stateArray 单独存档，读档后 getter 自动重新计算，避免重复叠加。
        return new Dictionary
        {
            { "hp", hpBase},
            { "maxHp", maxHpBase},
            { "strength", strengthBase},
            { "agility", agilityBase},
            { "intelligence", intelligenceBase},
            { "strength_exp", strength_exp},
            { "agility_exp", agility_exp},
            { "intelligence_exp", intelligence_exp},
            { "ItemArray", ItemArray},
            { "ItemDic",ItemDic},
            { "talentID", talentID },
            { "stateArray", stateArray},
            { "stateTimeDic", StateTimeDic},
            { "timePeriodsElapsed", timePeriodsElapsed},
            {"exp_acq_rate", exp_acq_rate},
            {"armor", armor},
            {"max_armor", max_armor},
            {"attack_limb_weight", attack_limb_weight},
            {"attack_head_weight", attack_head_weight},
            {"attack_body_weight", attack_body_weight},
            {"baseStamina", baseStamina},
            {"exploreStamina", exploreStamina},
            {"maxBaseStamina", maxBaseStamina},
            {"maxexploreStamina", maxexploreStamina},
            {"hunger",hunger}
        };
    }
    public void LoadSaveData(Dictionary data)
    {
        // 直接赋值私有 base 字段（属性 set 是空实现，赋给属性会失效）。
        // key 与 GetSaveData 一一对应；缺字段时回退到字段默认初始值。
        hpBase              = data.ContainsKey("hpBase")              ? (int)data["hpBase"]              : 100;
        maxHpBase           = data.ContainsKey("maxHpBase")           ? (int)data["maxHpBase"]           : 100;
        strengthBase        = data.ContainsKey("strengthBase")        ? (int)data["strengthBase"]        : 10;
        agilityBase         = data.ContainsKey("agilityBase")         ? (int)data["agilityBase"]         : 10;
        intelligenceBase    = data.ContainsKey("intelligenceBase")    ? (int)data["intelligenceBase"]    : 10;
        strength_exp        = data.ContainsKey("strength_exp")        ? (int)data["strength_exp"]        : 0;
        agility_exp         = data.ContainsKey("agility_exp")         ? (int)data["agility_exp"]         : 0;
        intelligence_exp    = data.ContainsKey("intelligence_exp")    ? (int)data["intelligence_exp"]    : 0;
        exp_acq_rate        = data.ContainsKey("exp_acq_rate")        ? (int)data["exp_acq_rate"]        : 1.0;
        armor               = data.ContainsKey("armor")               ? (int)data["armor"]               : 0;
        max_armor           = data.ContainsKey("max_armor")           ? (int)data["max_armor"]           : 100;
        attack_limb_weight  = data.ContainsKey("attack_limb_weight")  ? (int)data["attack_limb_weight"]  : 100;
        attack_head_weight  = data.ContainsKey("attack_head_weight")  ? (int)data["attack_head_weight"]  : 100;
        attack_body_weight  = data.ContainsKey("attack_body_weight")  ? (int)data["attack_body_weight"]  : 100;
        baseStamina         = data.ContainsKey("baseStamina")         ? (int)data["baseStamina"]         : 10;
        exploreStamina      = data.ContainsKey("exploreStamina")      ? (int)data["exploreStamina"]      : 10;
        maxBaseStamina      = data.ContainsKey("maxBaseStamina")      ? (int)data["maxBaseStamina"]      : 10;
        maxexploreStamina   = data.ContainsKey("maxexploreStamina")   ? (int)data["maxexploreStamina"]   : 10;
        hunger              = data.ContainsKey("hunger")              ? (int)data["hunger"]              : 3;
        // ItemArray = data.ContainsKey("ItemArray") ? (Array<int>)data["ItemArray"] : new Array<int>{1,2};
        ItemDic   = data.ContainsKey("ItemDic")   ? (Dictionary<int, int>)data["ItemDic"]   : new Dictionary<int, int> { };
        talentID  = data.ContainsKey("talentID")  ? (Array<int>)data["talentID"]            : new Array<int> { };
        stateArray = data.ContainsKey("stateArray") ? (Array<int>)data["stateArray"]        : new Array<int> { };
        StateTimeDic = data.ContainsKey("stateTimeDic") ? (Dictionary<int, int>)data["stateTimeDic"] : new Dictionary<int, int> { };
        timePeriodsElapsed = data.ContainsKey("timePeriodsElapsed") ? (int)data["timePeriodsElapsed"] : 0;
        GD.Print($"[PlayerManager] 数据恢复完成：HP={Hp}/{MaxHp}, Str={Strength}({strength_exp}/{ExpMax}), Agi={Agility}({agility_exp}/{ExpMax}), Int={Intelligence}({intelligence_exp}/{ExpMax})");
    }
    #endregion
}
