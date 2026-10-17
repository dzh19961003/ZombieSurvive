using Godot;
using System;
using System.Linq;
using MyProject;

public partial class PlantUpgradeUI : Control
{
    [Export] Button upgrade;

    private const int MaxLevel = 4;
    private const int MaxTextureLevel = 4;

    private int currentLevel = 1;

    private TextureRect _plantTexture;
    private Label _gradeTitle;
    private Label _curVal1;
    private Label _nextVal1;
    private Label _curVal2;
    private Label _nextVal2;
    private Label _maxTip;
    private TextureRect _arrow1;
    private TextureRect _arrow2;
    private TextureRect[] _reqIcons;
    private Label[] _reqLabels;
    private Control _upgradeRequ;

    public override void _Ready()
    {
        _plantTexture = GetNode<TextureRect>("PlantTexture");
        _gradeTitle = GetNode<Label>("GradeTitle");
        _curVal1 = GetNode<Label>("PlantNum/CurVal1");
        _nextVal1 = GetNode<Label>("PlantNum/NextVal1");
        _arrow1 = GetNode<TextureRect>("PlantNum/Arrow1");
        _curVal2 = GetNode<Label>("GrowSpeed/CurVal2");
        _nextVal2 = GetNode<Label>("GrowSpeed/NextVal2");
        _arrow2 = GetNode<TextureRect>("GrowSpeed/Arrow2");
        _maxTip = GetNode<Label>("MaxTip");
        _upgradeRequ = GetNode<Control>("UpgradeRequ");
        _reqIcons = new TextureRect[]
        {
            GetNode<TextureRect>("UpgradeRequ/ReqIcon1"),
            GetNode<TextureRect>("UpgradeRequ/ReqIcon2"),
            GetNode<TextureRect>("UpgradeRequ/ReqIcon3")
        };
        _reqLabels = new Label[]
        {
            GetNode<Label>("UpgradeRequ/ReqLabel1"),
            GetNode<Label>("UpgradeRequ/ReqLabel2"),
            GetNode<Label>("UpgradeRequ/ReqLabel3")
        };

        if (upgrade != null)
        {
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

        // 等级标题
        if (_gradeTitle != null) _gradeTitle.Text = $"等级{currentLevel}";

        RefreshPlantTexture();

        // 植物数值显示
        if (isUnlockStep)
        {
            // 解锁前
            if (_curVal1 != null) _curVal1.Text = "解锁后查看";
            if (_curVal2 != null) _curVal2.Text = "解锁后查看";
            if (_nextVal1 != null)
            {
                _nextVal1.Visible = false;
                _nextVal1.Text = "???";
            }
            if (_nextVal2 != null)
            {
                _nextVal2.Visible = false;
                _nextVal2.Text = "???";
            }
            if (_arrow1 != null) _arrow1.Visible = false;
            if (_arrow2 != null) _arrow2.Visible = false;
        }
        else
        {
            // 解锁后
            if (_curVal1 != null) _curVal1.Text = curCfg != null ? curCfg.GrowNum.ToString() : "0";
            if (_curVal2 != null) _curVal2.Text = curCfg != null ? curCfg.GrowSpeed.ToString() : "0";

            if (isMaxLevel)
            {
                // 满级
                if (_nextVal1 != null) _nextVal1.Visible = false;
                if (_nextVal2 != null) _nextVal2.Visible = false;
                if (_arrow1 != null) _arrow1.Visible = false;
                if (_arrow2 != null) _arrow2.Visible = false;
            }
            else
            {
                // 正常升级
                if (_nextVal1 != null)
                {
                    _nextVal1.Visible = true;
                    _nextVal1.Text = nextCfg != null ? nextCfg.GrowNum.ToString() : "0";
                }
                if (_nextVal2 != null)
                {
                    _nextVal2.Visible = true;
                    _nextVal2.Text = nextCfg != null ? nextCfg.GrowSpeed.ToString() : "0";
                }
                if (_arrow1 != null) _arrow1.Visible = true;
                if (_arrow2 != null) _arrow2.Visible = true;
            }
        }

        // 满级处理
        if (isMaxLevel)
        {
            if (_maxTip != null) _maxTip.Visible = true;
            if (_upgradeRequ != null) _upgradeRequ.Visible = false;
            if (upgrade != null)
            {
                upgrade.Icon = GD.Load<Texture2D>("res://Assets/Images/UI/button_cancel.png");
                upgrade.Disabled = true;
                upgrade.Text = "满级";
            }
            return;
        }

        if (_maxTip != null) _maxTip.Visible = false;
        if (_upgradeRequ != null) _upgradeRequ.Visible = true;

        if (upgrade != null)
        {
            upgrade.Disabled = false;
            upgrade.Text = isUnlockStep ? "解锁" : "升级";
        }

        // 材料需求
        var reqCfg = isUnlockStep ? curCfg : nextCfg;
        RefreshRequirements(reqCfg);
    }

    private void RefreshPlantTexture()
    {
        if (_plantTexture == null || PlayerManager.Instance == null) return;

        int level = Mathf.Clamp(PlantManager.Instance.PlantLevel, 1, MaxTextureLevel);
        var tex = GD.Load<Texture2D>($"res://Assets/Images/Base/plant_{level}.png");
        if (tex != null)
        {
            _plantTexture.Texture = tex;
        }
    }

    private void RefreshRequirements(RoofPlants cfg)
    {
        if (cfg == null) return;

        int slotCount = Mathf.Min(_reqLabels.Length, _reqIcons.Length);
        for (int i = 0; i < slotCount; i++)
        {
            if (_reqLabels[i] == null || _reqIcons[i] == null) continue;

            int itemID = (cfg.ItemID != null && i < cfg.ItemID.Count) ? cfg.ItemID[i] : 0;
            int need = (cfg.ItemNum != null && i < cfg.ItemNum.Count) ? cfg.ItemNum[i] : 0;

            if (itemID == 0 || need == 0)
            {
                _reqIcons[i].Visible = false;
                _reqLabels[i].Visible = false;
                continue;
            }

            _reqIcons[i].Visible = true;
            _reqLabels[i].Visible = true;

            if (UIManager.Instance != null && itemID != 0 && itemID < 10000)
            {
                var icon = UIManager.Instance.GetItemIcon(itemID);
                if (icon != null) _reqIcons[i].Texture = icon;
            }
            else if (itemID == 10015)
            {
                var staminaIcon = GD.Load<Texture2D>("res://Assets/Images/UI/stamina.png");
                if (staminaIcon != null) _reqIcons[i].Texture = staminaIcon;
            }

            bool insufficient = !IsEnough(itemID, need);
            _reqLabels[i].Text = $"×{need}";
            _reqLabels[i].AddThemeColorOverride("font_color", insufficient ? Colors.Red : Colors.White);
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
                    // 属性类
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

    // 判断材料是否充足
    private bool IsEnough(int itemID, int need)
    {
        if (PlayerManager.Instance == null) return false;
        if (need == 0) return true;

        if (itemID >= 10000)
        {
          
            switch (itemID)
            {
                case 10001: return PlayerManager.Instance.Hp >= need;
                case 10002: return PlayerManager.Instance.MaxHp >= need;
                case 10003: return PlayerManager.Instance.Strength >= need;
                case 10004: return PlayerManager.Instance.Agility >= need;
                case 10005: return PlayerManager.Instance.Intelligence >= need;
                case 10015: return PlayerManager.Instance.BaseStamina >= need;
                case 10016: return PlayerManager.Instance.ExploreStamina >= need;
                default: return false;
            }
        }
        return PlayerManager.Instance.GetItemCount(itemID) >= need;
    }
}
