using Godot;
using System;
using MyProject;

public partial class TentUpgradeUI : Control
{
    [Export] TextureButton upgrade;

    private const int MaxLevel = 3;
    private const int MaxTextureLevel = 3;

    private static readonly Color NormalColor = new(1f, 1f, 1f, 1f);

    private int currentLevel = 1;

    private TextureRect _tentTexture;
    private Label _lvCur;
    private Label _lvNext;
    private TextureRect _levelArrow;
    private Label _curVal1;
    private Label _nextVal1;
    private Label _curVal2;
    private Label _nextVal2;
    private Label _maxTip;
    private Label _upgradeLabel;
    private TextureRect _arrow1;
    private TextureRect _arrow2;
    private TextureRect[] _reqIcons;
    private Label[] _reqNames;
    private Label[] _reqCounts;
    private Control _upgradeRequ;

    public override void _Ready()
    {
        _tentTexture = GetNode<TextureRect>("TentTexture");
        _lvCur = GetNode<Label>("LevelBlock/LevelRow/LvCur");
        _lvNext = GetNode<Label>("LevelBlock/LevelRow/LvNext");
        _levelArrow = GetNode<TextureRect>("LevelBlock/LevelRow/LevelArrow");
        _curVal1 = GetNode<Label>("Stats/Row1/HBox/CurVal1");
        _nextVal1 = GetNode<Label>("Stats/Row1/HBox/NextVal1");
        _arrow1 = GetNode<TextureRect>("Stats/Row1/HBox/Arrow1");
        _curVal2 = GetNode<Label>("Stats/Row2/HBox/CurVal2");
        _nextVal2 = GetNode<Label>("Stats/Row2/HBox/NextVal2");
        _arrow2 = GetNode<TextureRect>("Stats/Row2/HBox/Arrow2");
        _maxTip = GetNode<Label>("MaxTip");
        _upgradeRequ = GetNode<Control>("UpgradeRequ");
        string reqRoot = "UpgradeRequ/Pad/Body/ReqList";
        _reqIcons = new TextureRect[]
        {
            GetNode<TextureRect>($"{reqRoot}/Req1/IconWrap/Icon"),
            GetNode<TextureRect>($"{reqRoot}/Req2/IconWrap/Icon"),
            GetNode<TextureRect>($"{reqRoot}/Req3/IconWrap/Icon")
        };
        _reqNames = new Label[]
        {
            GetNode<Label>($"{reqRoot}/Req1/Name"),
            GetNode<Label>($"{reqRoot}/Req2/Name"),
            GetNode<Label>($"{reqRoot}/Req3/Name")
        };
        _reqCounts = new Label[]
        {
            GetNode<Label>($"{reqRoot}/Req1/Count"),
            GetNode<Label>($"{reqRoot}/Req2/Count"),
            GetNode<Label>($"{reqRoot}/Req3/Count")
        };

        if (upgrade != null)
        {
            _upgradeLabel = upgrade.GetNodeOrNull<Label>("Label");
            upgrade.Pressed += OnUpgradePressed;
        }

        var closeBtn = GetNodeOrNull<Button>("CloseBtn");
        if (closeBtn != null)
        {
            closeBtn.Pressed += OnClosePressed;
        }

        CallDeferred(nameof(DeferredInit));
    }

    public override void _ExitTree()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.GetItem -= OnPlayerDataChanged;
        }
    }

    private void DeferredInit()
    {
        if (PlayerManager.Instance == null || ConfigManager.Instance == null)
        {
            GD.PrintErr("[TentUpgradeUI] PlayerManager 或 ConfigManager 未就绪");
            return;
        }
        PlayerManager.Instance.GetItem += OnPlayerDataChanged;
        RefreshDisplay();
    }

    private void OnPlayerDataChanged()
    {
        CallDeferred(nameof(RefreshDisplay));
    }

    private void OnClosePressed()
    {
        UIManager.Instance.HideUI(Paths.TentUI);
    }

    // 当前等级配置
    private RoofTent FindConfig()
    {
        var dic = ConfigManager.Instance?.roofTentDic;
        if (dic == null) return null;
        return dic.ContainsKey(currentLevel) ? dic[currentLevel] : null;
    }

    // 下一等级配置
    private RoofTent FindNextConfig()
    {
        var dic = ConfigManager.Instance?.roofTentDic;
        if (dic == null) return null;
        return dic.ContainsKey(currentLevel + 1) ? dic[currentLevel + 1] : null;
    }

    private void RefreshDisplay()
    {
        if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

        currentLevel = PlayerManager.Instance.TentLevel;
        bool isUnlockStep = currentLevel == 1 && !PlayerManager.Instance.TentUnlocked;
        bool isMaxLevel = currentLevel >= MaxLevel && PlayerManager.Instance.TentUnlocked;

        var curCfg = FindConfig();
        var nextCfg = FindNextConfig();

        // 等级标题：Lv.当前 → Lv.下一级
        _lvCur.Text = $"Lv.{currentLevel}";
        bool hasNextLevel = !isUnlockStep && !isMaxLevel;
        _lvNext.Visible = hasNextLevel;
        _levelArrow.Visible = hasNextLevel;
        if (hasNextLevel) _lvNext.Text = $"Lv.{currentLevel + 1}";

        RefreshTentTexture();
        RefreshStamina(curCfg, nextCfg, isUnlockStep, isMaxLevel);
        RefreshRequirements(isUnlockStep ? curCfg : nextCfg, isUnlockStep, isMaxLevel);
    }

    // 基地体力 / 探索体力 的当前值与升级后数值
    private void RefreshStamina(RoofTent curCfg, RoofTent nextCfg, bool isUnlockStep, bool isMaxLevel)
    {
        if (isUnlockStep)
        {
            // 解锁前不展示具体数值
            _curVal1.Text = "???";
            _curVal2.Text = "???";
            _nextVal1.Visible = false;
            _nextVal2.Visible = false;
            _arrow1.Visible = false;
            _arrow2.Visible = false;
            return;
        }

        _curVal1.Text = curCfg != null ? curCfg.RoofStamina.ToString() : "0";
        _curVal2.Text = curCfg != null ? curCfg.ExploreStamina.ToString() : "0";

        bool hasNext = !isMaxLevel;
        _nextVal1.Visible = hasNext;
        _nextVal2.Visible = hasNext;
        _arrow1.Visible = hasNext;
        _arrow2.Visible = hasNext;
        if (hasNext)
        {
            _nextVal1.Text = nextCfg != null ? nextCfg.RoofStamina.ToString() : "0";
            _nextVal2.Text = nextCfg != null ? nextCfg.ExploreStamina.ToString() : "0";
        }
    }

    private void RefreshTentTexture()
    {
        if (_tentTexture == null || PlayerManager.Instance == null) return;

        int level = Mathf.Clamp(PlayerManager.Instance.TentLevel, 1, MaxTextureLevel);
        var tex = GD.Load<Texture2D>($"res://Assets/Images/Base/tent_{level}.png");
        if (tex != null)
        {
            _tentTexture.Texture = tex;
        }
    }

    // 升级需求：图标 + 名称 + 拥有/需求
    private void RefreshRequirements(RoofTent cfg, bool isUnlockStep, bool isMaxLevel)
    {
        if (isMaxLevel)
        {
            _maxTip.Visible = true;
            _upgradeRequ.Visible = false;
            if (upgrade != null)
            {
                upgrade.Disabled = true;
                upgrade.Modulate = new Color(1f, 1f, 1f, 0.5f);
            }
            if (_upgradeLabel != null) _upgradeLabel.Text = "已满级";
            return;
        }

        _maxTip.Visible = false;
        _upgradeRequ.Visible = true;
        if (upgrade != null)
        {
            upgrade.Disabled = false;
            upgrade.Modulate = NormalColor;
        }
        if (_upgradeLabel != null) _upgradeLabel.Text = isUnlockStep ? "解锁" : "升级";

        if (cfg == null) return;

        int slotCount = Mathf.Min(_reqNames.Length, Mathf.Min(_reqIcons.Length, _reqCounts.Length));
        for (int i = 0; i < slotCount; i++)
        {
            int itemID = (cfg.ItemID != null && i < cfg.ItemID.Count) ? cfg.ItemID[i] : 0;
            int need = (cfg.ItemNum != null && i < cfg.ItemNum.Count) ? cfg.ItemNum[i] : 0;

            if (itemID == 0 || need == 0)
            {
                SetReqVisible(i, false);
                continue;
            }

            SetReqVisible(i, true);
            _reqNames[i].Text = GetItemName(itemID);

            var icon = GetRequirementIcon(itemID);
            if (icon != null) _reqIcons[i].Texture = icon;

            int have = GetItemHave(itemID);
            bool insufficient = have < need;
            _reqCounts[i].Text = $"{have}/{need}";
            // 资源不足红色，充足白色
            _reqCounts[i].AddThemeColorOverride("font_color", insufficient ? Colors.Red : Colors.White);
        }
    }

    private void SetReqVisible(int index, bool visible)
    {
        _reqIcons[index].Visible = visible;
        _reqNames[index].Visible = visible;
        _reqCounts[index].Visible = visible;
    }

    // 需求图标：体力类属性复用黄色闪电图标，其余走通用图标
    private Texture2D GetRequirementIcon(int itemID)
    {
        if (itemID == 10015 || itemID == 10016)
        {
            return GD.Load<Texture2D>("res://Assets/Images/UI/stamina.png");
        }
        return UIManager.Instance?.GetItemIcon(itemID);
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
        if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

        bool isUnlock = currentLevel == 1 && !PlayerManager.Instance.TentUnlocked;

        var cfg = isUnlock ? FindConfig() : FindNextConfig();
        if (cfg == null)
        {
            UIManager.Instance.ShowCommonTips2("帐篷已达最高等级，无法升级");
            return;
        }

        // 校验所有材料是否充足
        if (cfg.ItemID != null && cfg.ItemNum != null)
        {
            for (int i = 0; i < cfg.ItemID.Count; i++)
            {
                int itemID = cfg.ItemID[i];
                int need = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
                if (GetItemHave(itemID) < need)
                {
                    UIManager.Instance.ShowCommonTips2(isUnlock ? "解锁材料不足" : "升级材料不足");
                    return;
                }
            }
        }

        // 扣材料（物品 + 属性类）
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
                    // 属性类（10000+）：通过 AddItem 扣减
                    PlayerManager.Instance.AddItem(itemID, -need);
                }
            }
        }

        if (isUnlock)
        {
            PlayerManager.Instance.SetTentUnlocked(true);
            GD.Print("[TentUpgradeUI] 帐篷已解锁");
        }
        else
        {
            int newLevel = currentLevel + 1;
            PlayerManager.Instance.SetTentLevel(newLevel);
            GD.Print($"[TentUpgradeUI] 升级成功，当前等级：{newLevel}");
        }

        RefreshDisplay();
    }
}
