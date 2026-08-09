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
    private Dictionary<int, int> ItemDic = new Dictionary<int, int>();
   //天赋列表
    private Array<int> talentID = new Array<int>() { 1,2};
    private Dictionary<int,int> ItemArray=new Dictionary<int, int>() { };
//测试数据
    private Array<int> stateArray = new Array<int>() { };
   
    private int hpBase = 100;
    private int maxHpBase = 100;
    private int strengthBase = 10;
    private int agilityBase = 10;
    private int intelligenceBase = 10;
     private int strength_exp = 0;
    private int agility_exp = 0;
    private int intelligence_exp = 0;
    private int exp_acq_rate = 1;
    private int armor = 0;
    private int max_armor = 100;
    private int attack_limb_weight = 0;
    private int attack_head_weight = 0;
    private int attack_body_weight = 0;
    private int baseStamina = 10;
    private int exploreStamina = 10;
    private int maxBaseStamina = 10;
    private int maxexploreStamina = 10;
    public int HpBase {get{return hpBase + GetAddition(10001);}private set{}}
    public int MaxHpBase {get{return maxHpBase + GetAddition(10002);}private set{}}
    public int StrengthBase {get{return strengthBase + GetAddition(10003);}private set{}}
    public int AgilityBase {get{return agilityBase + GetAddition(10004);}private set{}}
    public int IntelligenceBase {get{return intelligenceBase + GetAddition(10005);}private set{}}
    public int Strength_exp {get{return strength_exp + GetAddition(10006);}private set{}}
    public int Agility_exp {get{return agility_exp + GetAddition(10007);}private set{}}
    public int Intelligence_exp {get{return intelligence_exp + GetAddition(10008);}private set{}}
    public int Exp_acq_rate {get{return exp_acq_rate + GetAddition(10009);}private set{}}
    public int Armor {get{return armor + GetAddition(10010);}private set{}}
    public int MaxArmor {get{return max_armor + GetAddition(10011);}private set{}}
    public int Attack_limb_weight {get{return attack_limb_weight + GetAddition(10012);}private set{}}
    public int Attack_head_weight {get{return attack_head_weight + GetAddition(10013);}private set{}}
    public int Attack_body_weight {get{return attack_body_weight + GetAddition(10014);}private set{}}
    public int BaseStamina {get{return baseStamina + GetAddition(10015);}private set{}}
    public int ExploreStamina {get{return exploreStamina + GetAddition(10016);}private set{}}
    public int MaxBaseStamina {get{return maxBaseStamina + GetAddition(10017);}private set{}}
    public int MaxexploreStamina {get{return maxexploreStamina + GetAddition(10018);}private set{}}
    
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
    // 三项属性对应的经验值。满 ExpMax 时，对应属性 +1 并清零。
    public const int ExpMax = 100;
    //获取属性id
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
    //增加基础属性值
    public void AddItem(int id,int amount)
    {   
        //加属性
        if(id>10000){
        switch ( id)
        {
            case 10001:
                // 生命值钳制在 [0, MaxHpBase]（含状态加成的当前上限）
                hpBase = Mathf.Clamp(hpBase + amount, 0, MaxHpBase);
                break;
            case 10002:
                maxHpBase += amount;
                // 最大生命值变化后，当前生命值不能超过新上限
                hpBase = Mathf.Min(hpBase, MaxHpBase);
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
                strength_exp += amount;
                while (strength_exp >= ExpMax)
                {
                    strength_exp -= ExpMax;
                    AddItem(10003, 1);
                    GD.Print($"[PlayerManager] 强健经验满，强健+1 → {StrengthBase}");
                }
                break;
            case 10007:
                if (amount <= 0) return;
                agility_exp += amount;
                while (agility_exp >= ExpMax)
                {
                    agility_exp -= ExpMax;
                    AddItem(10004, 1);
                    GD.Print($"[PlayerManager] 速度经验满，速度+1 → {AgilityBase}");
                }
                break;
            case 10008:
                if (amount <= 0) return;
                intelligence_exp += amount;
                while (intelligence_exp >= ExpMax)
                {
                    intelligence_exp -= ExpMax;
                    AddItem(10005, 1);
                    GD.Print($"[PlayerManager] 智力经验满，智力+1 → {IntelligenceBase}");
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
                // 基地体力钳制在 [0, MaxBaseStamina]
                baseStamina = Mathf.Clamp(baseStamina + amount, 0, MaxBaseStamina);
                break;
            case 10016:
                // 探索体力钳制在 [0, MaxexploreStamina]
                exploreStamina = Mathf.Clamp(exploreStamina + amount, 0, MaxexploreStamina);
                break;
            case 10017:
                maxBaseStamina += amount;
                // 最大值变化后，当前体力不能超过新上限
                baseStamina = Mathf.Min(baseStamina, MaxBaseStamina);
                break;
            case 10018:
                maxexploreStamina += amount;
                exploreStamina = Mathf.Min(exploreStamina, MaxexploreStamina);
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
    }
    #region 存档相关
       public Dictionary GetSaveData()
    {
        // 存私有 base 字段（基础值，不含 GetAddition 加成）。
        // 加成由 stateArray 单独存档，读档后 getter 自动重新计算，避免重复叠加。
        return new Dictionary
        {
            { "hpBase", hpBase},
            { "maxHpBase", maxHpBase},
            { "strengthBase", strengthBase},
            { "agilityBase", agilityBase},
            { "intelligenceBase", intelligenceBase},
            { "strength_exp", strength_exp},
            { "agility_exp", agility_exp},
            { "intelligence_exp", intelligence_exp},
            { "ItemArray", ItemArray},
            { "ItemDic",ItemDic},
            { "talentID", talentID },
            { "stateArray", stateArray},
            {"exp_acq_rate", exp_acq_rate},
            {"armor", armor},
            {"max_armor", max_armor},
            {"attack_limb_weight", attack_limb_weight},
            {"attack_head_weight", attack_head_weight},
            {"attack_body_weight", attack_body_weight},
            {"baseStamina", baseStamina},
            {"exploreStamina", exploreStamina},
            {"maxBaseStamina", maxBaseStamina},
            {"maxexploreStamina", maxexploreStamina}
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
        exp_acq_rate        = data.ContainsKey("exp_acq_rate")        ? (int)data["exp_acq_rate"]        : 1;
        armor               = data.ContainsKey("armor")               ? (int)data["armor"]               : 0;
        max_armor           = data.ContainsKey("max_armor")           ? (int)data["max_armor"]           : 100;
        attack_limb_weight  = data.ContainsKey("attack_limb_weight")  ? (int)data["attack_limb_weight"]  : 0;
        attack_head_weight  = data.ContainsKey("attack_head_weight")  ? (int)data["attack_head_weight"]  : 0;
        attack_body_weight  = data.ContainsKey("attack_body_weight")  ? (int)data["attack_body_weight"]  : 0;
        baseStamina         = data.ContainsKey("baseStamina")         ? (int)data["baseStamina"]         : 10;
        exploreStamina      = data.ContainsKey("exploreStamina")      ? (int)data["exploreStamina"]      : 10;
        maxBaseStamina      = data.ContainsKey("maxBaseStamina")      ? (int)data["maxBaseStamina"]      : 10;
        maxexploreStamina   = data.ContainsKey("maxexploreStamina")   ? (int)data["maxexploreStamina"]   : 10;
        // ItemArray = data.ContainsKey("ItemArray") ? (Array<int>)data["ItemArray"] : new Array<int>{1,2};
        ItemDic   = data.ContainsKey("ItemDic")   ? (Dictionary<int, int>)data["ItemDic"]   : new Dictionary<int, int> { };
        talentID  = data.ContainsKey("talentID")  ? (Array<int>)data["talentID"]            : new Array<int> { };
        stateArray = data.ContainsKey("stateArray") ? (Array<int>)data["stateArray"]        : new Array<int> { };
        GD.Print($"[PlayerManager] 数据恢复完成：HP={HpBase}/{MaxHpBase}, Str={StrengthBase}({strength_exp}/{ExpMax}), Agi={AgilityBase}({agility_exp}/{ExpMax}), Int={IntelligenceBase}({intelligence_exp}/{ExpMax})");
    }
    #endregion
}
