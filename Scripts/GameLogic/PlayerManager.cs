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
    //天赋列表
    public Array<int> talentID = new Array<int>() { 1,2};
   

    // ===== 玩家属性 =====
    // 外部可读，私有可写；修改请走下方提供的方法，确保逻辑统一
    public int HP { get; private set; } = 100;
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


    public Array<int> ItemArray=new Array<int>() { 2,3};
//测试数据
    public Array<int> stateArray = new Array<int>() { 1, 2 };
    
    // ===== 属性修改方法 =====
    // HP 相关
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        HP = Mathf.Max(0, HP - amount);
    }
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        HP = Mathf.Min(MaxHP, HP + amount);
    }
    public void SetMaxHP(int value, bool refill = false)
    {
        MaxHP = Mathf.Max(1, value);
        if (HP > MaxHP) HP = MaxHP;
        if (refill) HP = MaxHP;
    }

    // 力量 / 敏捷 / 智力 修改
    public void ModifyStrength(int delta)     => Strength     = Mathf.Max(0, Strength + delta);
    public void ModifyAgility(int delta)      => Agility      = Mathf.Max(0, Agility + delta);
    public void ModifyIntelligence(int delta) => Intelligence = Mathf.Max(0, Intelligence + delta);

    // 直接设定某项属性（如初始化、Buff 覆盖）
    public void SetStrength(int value)     => Strength     = Mathf.Max(0, value);
    public void SetAgility(int value)      => Agility      = Mathf.Max(0, value);
    public void SetIntelligence(int value) => Intelligence = Mathf.Max(0, value);

    // ===== 经验值相关 =====
    // 增加经验，满 ExpMax 时对应属性 +1，多余经验保留到下一轮。
    public void AddStrengthExp(int amount)
    {
        if (amount <= 0) return;
        StrengthExp += amount;
        while (StrengthExp >= ExpMax)
        {
            StrengthExp -= ExpMax;
            ModifyStrength(1);
            GD.Print($"[PlayerManager] 力量经验满，力量+1 → {Strength}");
        }
    }
    public void AddAgilityExp(int amount)
    {
        if (amount <= 0) return;
        AgilityExp += amount;
        while (AgilityExp >= ExpMax)
        {
            AgilityExp -= ExpMax;
            ModifyAgility(1);
            GD.Print($"[PlayerManager] 敏捷经验满，敏捷+1 → {Agility}");
        }
    }
    public void AddIntelligenceExp(int amount)
    {
        if (amount <= 0) return;
        IntelligenceExp += amount;
        while (IntelligenceExp >= ExpMax)
        {
            IntelligenceExp -= ExpMax;
            ModifyIntelligence(1);
            GD.Print($"[PlayerManager] 智力经验满，智力+1 → {Intelligence}");
        }
    }

    // ===== 天赋应用相关 =====
    // 属性名 → 配置 effect 字段中使用的中文别名
    // 配置里 "强健" 对应 Strength，"敏捷" 对应 Agility，"智力" 对应 Intelligence

    /// <summary>
    /// 重置三项基础属性为同一初始值（便于重新计算或测试）。
    /// </summary>
    public void ResetAttributes(int baseValue = 10)
    {
        Strength = baseValue;
        Agility = baseValue;
        Intelligence = baseValue;
    }

    /// <summary>
    /// 根据 talentID 列表，从 ConfigManager 读取天赋配置并应用到玩家属性。
    /// 解析 talent.json 的 effect 字段，格式如 "强健+3"、"敏捷-2"。
    /// </summary>
    public void ApplyTalents()
    {
        if (ConfigManager.Instance?.talentDic == null)
        {
            GD.PrintErr("[PlayerManager] ConfigManager 未就绪，无法应用天赋");
            return;
        }

        foreach (int id in talentID)
        {
            if (!ConfigManager.Instance.talentDic.TryGetValue(id, out var talent)) continue;
            if (!TryParseEffect(talent.Effect, out var attrName, out var delta)) continue;

            switch (attrName)
            {
                case "强健":
                    ModifyStrength(delta);
                    GD.Print($"[天赋] {talent.Name}: 力量 {(delta >= 0 ? "+" : "")}{delta}");
                    break;
                case "敏捷":
                    ModifyAgility(delta);
                    GD.Print($"[天赋] {talent.Name}: 敏捷 {(delta >= 0 ? "+" : "")}{delta}");
                    break;
                case "智力":
                    ModifyIntelligence(delta);
                    GD.Print($"[天赋] {talent.Name}: 智力 {(delta >= 0 ? "+" : "")}{delta}");
                    break;
                default:
                    GD.PrintErr($"[PlayerManager] 未知属性名 '{attrName}' (天赋: {talent.Name})");
                    break;
            }
        }
    }

    /// <summary>
    /// 解析 effect 字符串，格式："属性名±数值"，例如 "强健+3"、"强健-3"。
    /// </summary>
    private static bool TryParseEffect(string effect, out string attrName, out int delta)
    {
        attrName = null;
        delta = 0;
        if (string.IsNullOrEmpty(effect)) return false;

        var match = Regex.Match(effect, @"^(.+?)([+-]\d+)$");
        if (!match.Success) return false;

        attrName = match.Groups[1].Value;
        delta = int.Parse(match.Groups[2].Value);
        return true;
    }

    // ===== 物品相关 =====
    public void GetItem(int id)
    {

    }
    //获得天赋：ID
    //获得状态：ID
 
    #region 存档相关
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

        // ===== 测试代码：根据天赋（力大无穷等）改变玩家属性 =====
        GD.Print("===== [测试] 天赋属性应用测试 =====");
        ResetAttributes();
        GD.Print($"[测试] 应用前: 力量={Strength}, 敏捷={Agility}, 智力={Intelligence}");
        ApplyTalents();
        GD.Print($"[测试] 应用后: 力量={Strength}, 敏捷={Agility}, 智力={Intelligence}");
        GD.Print("================================");

        // ===== 测试代码：模拟击杀怪物增加力量经验，验证自动升级 =====
        GD.Print("===== [测试] 击杀怪物经验测试 =====");
        GD.Print($"[测试] 初始: 力量={Strength}, 力量经验={StrengthExp}/{ExpMax}");
        // 模拟击杀4只怪物，每次+30力量经验（共120，应触发1次升级：力量+1，经验余20）
        for (int i = 1; i <= 4; i++)
        {
            AddStrengthExp(30);
            GD.Print($"[测试] 击杀第{i}只怪物(+30): 力量={Strength}, 力量经验={StrengthExp}/{ExpMax}");
        }
        // 不自动打开面板：玩家主动打开 PropertyUI 时，VisibilityChanged 会自动触发 RefreshAttributes 显示最新值
        GD.Print($"[测试] 完成: 力量={Strength}, 力量经验={StrengthExp}/{ExpMax}（手动打开属性面板可查看 UI 刷新）");
        GD.Print("================================================");
    }
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
        ItemArray = data.ContainsKey("ItemArray") ? (Array<int>)data["ItemArray"] : new Array<int>{1,2};
        ItemDic = data.ContainsKey("ItemDic") ? (Dictionary<int, int>)data["ItemDic"] : new Dictionary<int, int> { };

        talentID = data.ContainsKey("talentID") ? (Array<int>)data["talentID"] : new Array<int> { };
        stateArray = data.ContainsKey("stateArray") ? (Array<int>)data["stateArray"] : new Array<int> { };
        GD.Print($"[PlayerManager] 数据恢复完成：HP={HP}, MaxHP={MaxHP}, Str={Strength}({StrengthExp}/{ExpMax}), Agi={Agility}({AgilityExp}/{ExpMax}), Int={Intelligence}({IntelligenceExp}/{ExpMax})");
    }
    #endregion
}
