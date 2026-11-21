using Godot;
using MyProject;

public partial class PlantUpgradeUI : Control
{
    [Export] TextureButton upgrade;
    [Export] private ShaderMaterial _disabledActionMaterial;

    private const int MaxLevel = 4;

    private static readonly Color LockedFunColor = new(0.4862745f, 0.61960787f, 0.72156864f, 1f);
    private static readonly Color ValueColor = new(1f, 0.97f, 0.89f);
    private static readonly Color InsufficientColor = new(1f, 0.48f, 0.41f, 1f);

    private int currentLevel = 1;
    private int _statFontSize;

    private Label _lvCur;
    private Label _lvNext;
    private TextureRect _levelArrow;
    private Label _countCur;
    private Label _countNext;
    private TextureRect _countArrow;
    private Label _speedCur;
    private Label _speedNext;
    private TextureRect _speedArrow;
    private Label _speedUnit;
    private Label _upgradeHint;
    private Label _reqTitle;
    private Control _reqRow;
    private Label _maxTip;
    private Label _upgradeLabel;
    private Control[] _reqSlots;
    private Control[] _reqDividers;
    private TextureRect[] _reqIcons;
    private Label[] _reqNames;
    private Label[] _reqCounts;

    public override void _Ready()
    {
        _lvCur          = GetNode<Label>("Info/LevelRow/LvCur");
        _lvNext         = GetNode<Label>("Info/LevelRow/LvNext");
        _levelArrow     = GetNode<TextureRect>("Info/LevelRow/LevelArrow");
        _countCur       = GetNode<Label>("Info/CountRow/CountCur");
        _countNext      = GetNode<Label>("Info/CountRow/CountNext");
        _countArrow     = GetNode<TextureRect>("Info/CountRow/CountArrow");
        _speedCur       = GetNode<Label>("Info/SpeedRow/SpeedCur");
        _speedNext      = GetNode<Label>("Info/SpeedRow/SpeedNext");
        _speedArrow     = GetNode<TextureRect>("Info/SpeedRow/SpeedArrow");
        _speedUnit      = GetNode<Label>("Info/SpeedUnit");
        _statFontSize   = _countCur.GetThemeFontSize("font_size");
        _upgradeHint    = GetNode<Label>("UpgradeHint");
        _reqTitle       = GetNode<Label>("ReqTitle");
        _reqRow         = GetNode<Control>("ReqRow");
        _maxTip         = GetNode<Label>("MaxTip");

        _reqSlots = new Control[]
        {
            GetNode<Control>("ReqRow/Req1"),
            GetNode<Control>("ReqRow/Req2"),
            GetNode<Control>("ReqRow/Req3")
        };
        _reqDividers = new Control[]
        {
            GetNode<Control>("ReqRow/ReqDivider1"),
            GetNode<Control>("ReqRow/ReqDivider2")
        };
        _reqIcons = new TextureRect[]
        {
            GetNode<TextureRect>("ReqRow/Req1/IconWrap/Icon"),
            GetNode<TextureRect>("ReqRow/Req2/IconWrap/Icon"),
            GetNode<TextureRect>("ReqRow/Req3/IconWrap/Icon")
        };
        _reqNames = new Label[]
        {
            GetNode<Label>("ReqRow/Req1/Name"),
            GetNode<Label>("ReqRow/Req2/Name"),
            GetNode<Label>("ReqRow/Req3/Name")
        };
        _reqCounts = new Label[]
        {
            GetNode<Label>("ReqRow/Req1/Count"),
            GetNode<Label>("ReqRow/Req2/Count"),
            GetNode<Label>("ReqRow/Req3/Count")
        };

        if (upgrade != null)
        {
            _upgradeLabel = upgrade.GetNodeOrNull<Label>("Label");
            upgrade.Pressed += OnUpgradePressed;
        }

        CallDeferred(nameof(DeferredInit));
    }

    public override void _ExitTree()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.GetItem -= OnPlayerDataChanged;
        }
        VisibilityChanged -= OnVisibilityChanged;
    }

    private void DeferredInit()
    {
        if (PlayerManager.Instance == null || ConfigManager.Instance == null)
        {
            GD.PrintErr("[PlantUpgradeUI] PlayerManager 或 ConfigManager 未就绪");
            return;
        }
        PlayerManager.Instance.GetItem += OnPlayerDataChanged;
        VisibilityChanged += OnVisibilityChanged;
        RefreshDisplay();
    }

    private void OnPlayerDataChanged()
    {
        CallDeferred(nameof(RefreshDisplay));
    }

    private void OnVisibilityChanged()
    {
        if (Visible) RefreshDisplay();
    }

    // 当前等级配置
    private RoofPlants FindConfig()
    {
        var dic = ConfigManager.Instance?.roofPlantsDic;
        if (dic == null) return null;
        return dic.ContainsKey(currentLevel) ? dic[currentLevel] : null;
    }

    // 下一等级配置
    private RoofPlants FindNextConfig()
    {
        var dic = ConfigManager.Instance?.roofPlantsDic;
        if (dic == null) return null;
        return dic.ContainsKey(currentLevel + 1) ? dic[currentLevel + 1] : null;
    }

    private void RefreshDisplay()
    {
        if (PlantManager.Instance == null || ConfigManager.Instance == null) return;

        currentLevel = PlantManager.Instance.PlantLevel;
        bool isUnlockStep = currentLevel == 1 && !PlantManager.Instance.PlantUnlocked;
        bool isMaxLevel = currentLevel >= MaxLevel && PlantManager.Instance.PlantUnlocked;

        var curCfg = FindConfig();
        var nextCfg = FindNextConfig();
        bool showNext = !isUnlockStep && !isMaxLevel;

        RefreshLevel(showNext);
        RefreshStats(curCfg, nextCfg, isUnlockStep, showNext);
        _upgradeHint.Visible = !isMaxLevel;
        _upgradeHint.Text = isUnlockStep ? "解锁后即可升级种植区。" : "升级可增加种植位并加快生长。";
        RefreshRequirements(isUnlockStep ? curCfg : nextCfg, isUnlockStep, isMaxLevel);
    }

    // 等级：Lv.当前 → Lv.下一级
    private void RefreshLevel(bool showNext)
    {
        _lvCur.Text = $"Lv.{currentLevel}";
        _lvNext.Visible = showNext;
        _levelArrow.Visible = showNext;
        if (showNext) _lvNext.Text = $"Lv.{currentLevel + 1}";
    }

    // 同时种植数量 / 生长速度
    private void RefreshStats(RoofPlants curCfg, RoofPlants nextCfg, bool isUnlockStep, bool showNext)
    {
        _countCur.AddThemeColorOverride("font_color", isUnlockStep ? LockedFunColor : ValueColor);
        _speedCur.AddThemeColorOverride("font_color", isUnlockStep ? LockedFunColor : ValueColor);
        _countCur.AddThemeFontSizeOverride("font_size", isUnlockStep ? 42 : _statFontSize);
        _speedCur.AddThemeFontSizeOverride("font_size", isUnlockStep ? 42 : _statFontSize);
        _countCur.Text = isUnlockStep ? "解锁后查看" : (curCfg?.GrowNum ?? 0).ToString();
        _speedCur.Text = isUnlockStep ? "解锁后查看" : (curCfg?.GrowSpeed ?? 0).ToString();

        _countNext.Visible = showNext;
        _countArrow.Visible = showNext;
        _speedNext.Visible = showNext;
        _speedArrow.Visible = showNext;

        if (showNext)
        {
            _countNext.Text = (nextCfg?.GrowNum ?? 0).ToString();
            _speedNext.Text = (nextCfg?.GrowSpeed ?? 0).ToString();
        }

        _speedUnit.Visible = !isUnlockStep;
    }

    // 升级需求：图标 + 名称 + 拥有/需求
    private void RefreshRequirements(RoofPlants cfg, bool isUnlockStep, bool isMaxLevel)
    {
        if (isMaxLevel)
        {
            _maxTip.Visible = true;
            _reqTitle.Visible = false;
            _reqRow.Visible = false;
            if (upgrade != null)
            {
                upgrade.Disabled = true;
                upgrade.Material = _disabledActionMaterial;
            }
            if (_upgradeLabel != null) _upgradeLabel.Text = "已满级";
            return;
        }

        _maxTip.Visible = false;
        _reqTitle.Visible = true;
        _reqRow.Visible = true;
        _reqTitle.Text = isUnlockStep ? "解锁需求" : "升级需求";
        if (upgrade != null)
        {
            upgrade.Disabled = false;
            upgrade.Material = null;
        }
        if (_upgradeLabel != null) _upgradeLabel.Text = isUnlockStep ? "解锁" : "升级";

        if (cfg == null) return;

        bool hasPreviousRequirement = false;
        for (int i = 0; i < _reqSlots.Length; i++)
        {
            int itemID = (cfg.ItemID != null && i < cfg.ItemID.Count) ? cfg.ItemID[i] : 0;
            int need = (cfg.ItemNum != null && i < cfg.ItemNum.Count) ? cfg.ItemNum[i] : 0;

            bool hasRequirement = itemID != 0 && need != 0;
            _reqSlots[i].Visible = hasRequirement;
            if (i > 0)
                _reqDividers[i - 1].Visible = hasRequirement && hasPreviousRequirement;
            if (!hasRequirement) continue;

            hasPreviousRequirement = true;
            _reqNames[i].Text = GetItemName(itemID);

            var icon = GetRequirementIcon(itemID);
            if (icon != null) _reqIcons[i].Texture = icon;

            int have = GetItemHave(itemID);
            _reqCounts[i].Text = $"{have}/{need}";
            _reqCounts[i].AddThemeColorOverride("font_color",
                have < need ? InsufficientColor : ValueColor);
        }
    }

    private Texture2D GetRequirementIcon(int itemID)
    {
        return itemID switch
        {
            7 => GD.Load<Texture2D>("res://Assets/Images/UI/building/plant/wood.png"),
            9 => GD.Load<Texture2D>("res://Assets/Images/UI/building/plant/parts.png"),
            10015 or 10016 => GD.Load<Texture2D>("res://Assets/Images/UI/stamina.png"),
            _ => UIManager.Instance?.GetItemIcon(itemID)
        };
    }

    private string GetItemName(int itemID)
    {
        if (itemID < 10000)
        {
            var itemDic = ConfigManager.Instance?.itemDic;
            return itemDic != null && itemDic.ContainsKey(itemID) ? itemDic[itemID].Name : "";
        }

        var effectDic = ConfigManager.Instance?.effectTypeDic;
        return effectDic != null && effectDic.ContainsKey(itemID) ? effectDic[itemID].Type : "";
    }

    private int GetItemHave(int itemID)
    {
        if (PlayerManager.Instance == null) return 0;

        if (itemID < 10000) return PlayerManager.Instance.GetItemCount(itemID);

        switch (itemID)
        {
            case 10001: return (int)PlayerManager.Instance.Hp;
            case 10002: return (int)PlayerManager.Instance.MaxHp;
            case 10003: return PlayerManager.Instance.Strength;
            case 10004: return PlayerManager.Instance.Agility;
            case 10005: return PlayerManager.Instance.Intelligence;
            case 10015: return PlayerManager.Instance.BaseStamina;
            case 10016: return PlayerManager.Instance.ExploreStamina;
            default: return 0;
        }
    }

    private void OnUpgradePressed()
    {
        if (PlantManager.Instance == null || ConfigManager.Instance == null) return;

        bool isUnlock = currentLevel == 1 && !PlantManager.Instance.PlantUnlocked;

        var cfg = isUnlock ? FindConfig() : FindNextConfig();
        if (cfg == null)
        {
            UIManager.Instance.ShowCommonTips2("种植区已达最高等级，无法升级");
            return;
        }

        // 校验材料充足
        if (cfg.ItemID != null && cfg.ItemNum != null)
        {
            for (int i = 0; i < cfg.ItemID.Count; i++)
            {
                int itemID = cfg.ItemID[i];
                int need = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
                if (!IsEnough(itemID, need))
                {
                    UIManager.Instance.ShowCommonTips2(isUnlock ? "解锁材料不足" : "升级材料不足");
                    return;
                }
            }
        }

        // 扣材料
        if (cfg.ItemID != null && cfg.ItemNum != null)
        {
            for (int i = 0; i < cfg.ItemID.Count; i++)
            {
                int itemID = cfg.ItemID[i];
                int need = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
                if (itemID == 0 || need == 0) continue;

                if (itemID < 10000)
                {
                    PlayerManager.Instance.RemoveItem(itemID, need);
                }
                else
                {
                    // 属性类通过 AddItem 扣减
                    PlayerManager.Instance.AddItem(itemID, -need);
                }
            }
        }

        if (isUnlock)
        {
            PlantManager.Instance.SetPlantUnlocked(true);
            GD.Print("[PlantUpgradeUI] 种植区已解锁");
        }
        else
        {
            int newLevel = currentLevel + 1;
            PlantManager.Instance.SetPlantLevel(newLevel);
            GD.Print($"[PlantUpgradeUI] 升级成功，当前等级：{newLevel}");
        }

        RefreshDisplay();
    }

    // 判断材料是否充足（支持物品ID和属性ID）
    private bool IsEnough(int itemID, int need)
    {
        if (need == 0) return true;
        return GetItemHave(itemID) >= need;
    }
}
