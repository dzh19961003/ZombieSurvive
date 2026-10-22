
using System;
using Godot;
using Godot.Collections;
using MyProject;

public partial class PlantManager : Node, ISaveable
{
    public static PlantManager Instance { get; private set; }
    public string SaveKey => GetPath();

    private int _plantLevel = 1;
    private bool _plantUnlocked = false;
    //种植区槽位
    private Dictionary<int, int> plantSlotDic = new Dictionary<int, int>();
    //种植进度
    private Dictionary<int, int> plantProgressDic = new Dictionary<int, int>();

    public int PlantLevel => _plantLevel;
    public bool PlantUnlocked => _plantUnlocked;

    public void SetPlantLevel(int level)
    {
        _plantLevel = Mathf.Max(1, level);
        NotifyChanged();
    }
    public void SetPlantUnlocked(bool value)
    {
        _plantUnlocked = value;
        NotifyChanged();
    }

    //当前等级可种植数量
    public int GetPlantSlotCount()
    {
        var cfg = ConfigManager.Instance?.roofPlantsList?.Find(p => p.Level == _plantLevel);
        return cfg?.GrowNum ?? 0;
    }

    //当前等级生长速度
    public int GetPlantGrowSpeed()
    {
        var cfg = ConfigManager.Instance?.roofPlantsList?.Find(p => p.Level == _plantLevel);
        return cfg?.GrowSpeed ?? 0;
    }

    //已种植数量
    public int GetPlantedCount() => plantSlotDic.Count;

    //已种植是否已满
    public bool IsPlantSlotFull() => plantSlotDic.Count >= GetPlantSlotCount();

    //查询槽位
    public int GetPlantSlot(int slot, out int progress)
    {
        progress = 0;
        if (!plantSlotDic.ContainsKey(slot)) return -1;
        progress = plantProgressDic.ContainsKey(slot) ? plantProgressDic[slot] : 0;
        return plantSlotDic[slot];
    }

    //该植物是否已种
    public bool HasPlant(int cfgID)
    {
        foreach (var kv in plantSlotDic)
        {
            if (kv.Value == cfgID) return true;
        }
        return false;
    }

    //该植物是否成熟
    public bool HasRipePlant(int cfgID)
    {
        var dic = ConfigManager.Instance?.roofPlantsItemDic;
        if (dic == null || !dic.ContainsKey(cfgID)) return false;
        int totalProgress = dic[cfgID].Progress;
        foreach (var kv in plantSlotDic)
        {
            if (kv.Value == cfgID && plantProgressDic.ContainsKey(kv.Key) && plantProgressDic[kv.Key] >= totalProgress)
            {
                return true;
            }
        }
        return false;
    }

    //种植
    public bool TryPlant(int cfgID)
    {
        if (IsPlantSlotFull()) return false;
        var dic = ConfigManager.Instance?.roofPlantsItemDic;
        if (dic == null || !dic.ContainsKey(cfgID)) return false;
        var cfg = dic[cfgID];
        //基地体力不足
        if (PlayerManager.Instance == null || PlayerManager.Instance.BaseStamina < cfg.Stamina) return false;

        //找第一个空槽位
        int slotCount = GetPlantSlotCount();
        for (int i = 0; i < slotCount; i++)
        {
            if (!plantSlotDic.ContainsKey(i))
            {
                plantSlotDic[i] = cfgID;
                plantProgressDic[i] = 0;
                PlayerManager.Instance.AddItem(10015, -cfg.Stamina); //内部会触发 GetItem
                return true;
            }
        }
        return false;
    }

    //收获
    public int HarvestPlant(int cfgID)
    {
        var dic = ConfigManager.Instance?.roofPlantsItemDic;
        if (dic == null || !dic.ContainsKey(cfgID) || PlayerManager.Instance == null) return 0;
        var cfg = dic[cfgID];

        //先拷贝keys，避免遍历时修改字典
        var keys = new Array<int>();
        foreach (var kv in plantSlotDic)
        {
            if (kv.Value == cfgID && plantProgressDic.ContainsKey(kv.Key) && plantProgressDic[kv.Key] >= cfg.Progress)
            {
                keys.Add(kv.Key);
            }
        }

        int total = 0;
        foreach (var slot in keys)
        {
            plantSlotDic.Remove(slot);
            plantProgressDic.Remove(slot);
            total += cfg.Num;
        }
        if (total > 0)
        {
            PlayerManager.Instance.AddItem(cfg.ItemID, total); //内部会触发 GetItem
        }
        return total;
    }

    //每日生长
    public void GrowPlants()
    {
        if (plantSlotDic.Count == 0) return;
        int growSpeed = GetPlantGrowSpeed();
        if (growSpeed <= 0) return;

        //先拷贝keys，避免遍历时修改字典
        var keys = new Array<int>();
        foreach (var k in plantSlotDic.Keys) keys.Add(k);
        foreach (var slot in keys)
        {
            plantProgressDic[slot] = (plantProgressDic.ContainsKey(slot) ? plantProgressDic[slot] : 0) + growSpeed;
        }
        NotifyChanged();
        GD.Print("[PlantManager] 种植区作物每日生长结算完成");
    }

    //事件通知UI刷新
    private void NotifyChanged()
    {
        PlayerManager.Instance?.NotifyChanged();
    }

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("[PlantManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;
        AddToGroup("Save");
    }

    #region 存档相关
    public Dictionary GetSaveData()
    {
        return new Dictionary
        {
            {"plantLevel", _plantLevel},
            {"plantUnlocked", _plantUnlocked},
            {"plantSlotDic", plantSlotDic},
            {"plantProgressDic", plantProgressDic}
        };
    }
    public void LoadSaveData(Dictionary data)
    {
        _plantLevel = data.ContainsKey("plantLevel") ? (int)data["plantLevel"] : 1;
        _plantUnlocked = data.ContainsKey("plantUnlocked") && (bool)data["plantUnlocked"];

        // 注意：JSON 序列化后字典 key 会变成 string，需要手动转回 int
        plantSlotDic = new Dictionary<int, int>();
        plantProgressDic = new Dictionary<int, int>();

        if (data.ContainsKey("plantSlotDic"))
        {
            var rawSlotDic = data["plantSlotDic"].AsGodotDictionary();
            foreach (var key in rawSlotDic.Keys)
            {
                int slotKey = key.VariantType == Variant.Type.Int ? key.AsInt32() : int.Parse(key.AsString());
                plantSlotDic[slotKey] = (int)rawSlotDic[key];
            }
        }

        if (data.ContainsKey("plantProgressDic"))
        {
            var rawProgressDic = data["plantProgressDic"].AsGodotDictionary();
            foreach (var key in rawProgressDic.Keys)
            {
                int slotKey = key.VariantType == Variant.Type.Int ? key.AsInt32() : int.Parse(key.AsString());
                plantProgressDic[slotKey] = (int)rawProgressDic[key];
            }
        }

        NotifyChanged();
        GD.Print($"[PlantManager] 数据恢复完成：Level={_plantLevel}, Unlocked={_plantUnlocked}, Slots={plantSlotDic.Count}");
    }
    #endregion
}
