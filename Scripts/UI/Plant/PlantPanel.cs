using Godot;
using System.Linq;
using MyProject;

public partial class PlantPanel : Control
{
    [Export] private ShaderMaterial _disabledActionMaterial;

    private Control _rowTemplate;
    private VBoxContainer _rowList;
    private Label _levelValue;
    private Label _plantedValue;
    private const string CropArtPath = "res://Assets/Images/UI/building/";

    private static readonly Color InfoColor = new(0.65882355f, 0.83137256f, 0.91764706f, 1f);
    private static readonly Color RipeColor = new(0.48f, 0.92f, 0.51f, 1f);
    private static readonly Color DangerColor = new(1f, 0.48f, 0.41f, 1f);

    //向上取整
    private static int CeilDiv(int a, int b) => b > 0 ? (a + b - 1) / b : 0;

    public override void _Ready()
    {
        _rowList = GetNode<VBoxContainer>("CropScroll/RowList");
        _rowTemplate = _rowList.GetNode<Control>("Row1");
        _levelValue = GetNode<Label>("Summary/Pad/Row/LeftPart/LevelValue");
        _plantedValue = GetNode<Label>("Summary/Pad/Row/RightPart/PlantedValue");

        _rowTemplate.Hide();

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
            GD.PrintErr("[PlantPanel] PlayerManager 或 ConfigManager 未就绪");
            return;
        }
        PlayerManager.Instance.GetItem += OnPlayerDataChanged;

        VisibilityChanged += OnVisibilityChanged;
        RefreshRows();
    }

    private void OnPlayerDataChanged()
    {
        CallDeferred(nameof(RefreshRows));
    }

    private void OnVisibilityChanged()
    {
        if (Visible) RefreshRows();
    }

    // 植物列表
    private void RefreshRows()
    {
        if (ConfigManager.Instance == null || PlayerManager.Instance == null || PlantManager.Instance == null) return;
        var dic = ConfigManager.Instance.roofPlantsItemDic;
        if (dic == null || _rowTemplate == null || _rowList == null) return;

        //清空旧行
        foreach (var child in _rowList.GetChildren())
        {
            if (child == _rowTemplate) continue;
            _rowList.RemoveChild(child);
            child.QueueFree();
        }

        //按配置ID顺序生成行
        foreach (var kv in dic.OrderBy(k => k.Key))
        {
            var row = (Control)_rowTemplate.Duplicate();
            FillRow(row, kv.Key, kv.Value);
            _rowList.AddChild(row);
            row.Show();
        }

        RefreshSummary();
    }

    private void RefreshSummary()
    {
        int level = PlantManager.Instance.PlantLevel;
        _levelValue.Text = $"Lv.{level}";
        _plantedValue.Text = $"{PlantManager.Instance.GetPlantedCount()}/{PlantManager.Instance.GetPlantSlotCount()}";

    }

    //填充行
    private void FillRow(Control row, int cfgID, RoofPlantsItem cfg)
    {
        var icon = row.GetNode<TextureRect>("IconFrame/IconWrap/Icon");
        var nameLabel = row.GetNode<Label>("Name");
        var yieldLabel = row.GetNode<Label>("YieldLabel");
        var stateTip = row.GetNode<Label>("StateTip");
        var divider = row.GetNode<Control>("Divider");
        var plantState = row.GetNode<Control>("PlantState");
        var staminaLabel = row.GetNode<Label>("PlantState/StaminaNum");
        var plantBtn = row.GetNode<Button>("PlantState/PlantBtn");
        var statusTip = row.GetNode<Label>("PlantState/StatusTip");
        var growState = row.GetNode<Control>("GrowState");
        var progress = row.GetNode<ProgressBar>("GrowState/Progress");
        var progressLabel = row.GetNode<Label>("GrowState/Progress/ProgressLabel");
        var growLabel = row.GetNode<Label>("GrowState/GrowLabel");
        var ripeState = row.GetNode<Control>("RipeState");
        var harvestBtn = row.GetNode<Button>("RipeState/HarvestBtn");

        //按钮事件
        plantBtn.Pressed += () => OnPlantPressed(cfgID);
        harvestBtn.Pressed += () => OnHarvestPressed(cfgID);

        //图标、收获数量、名称
        icon.Texture = cfg.ItemID switch
        {
            11 => GD.Load<Texture2D>(CropArtPath + "plant_1.png"),
            12 => GD.Load<Texture2D>(CropArtPath + "plant_2.png"),
            13 => GD.Load<Texture2D>(CropArtPath + "plant_3.png"),
            _ => UIManager.Instance?.GetItemIcon(cfg.ItemID)
        };
        yieldLabel.Text = $"×{cfg.Num}";
        nameLabel.Text = ConfigManager.Instance.itemDic.ContainsKey(cfg.ItemID)
            ? ConfigManager.Instance.itemDic[cfg.ItemID].Name
            : "???";

        //查询该作物占用的种植位与进度
        int slotProgress = 0;
        int slotCfgID = -1;
        int slotCount = PlantManager.Instance.GetPlantSlotCount();
        for (int slot = 0; slot < slotCount; slot++)
        {
            int slotProg;
            if (PlantManager.Instance.GetPlantSlot(slot, out slotProg) == cfgID)
            {
                slotCfgID = cfgID;
                slotProgress = slotProg;
                break;
            }
        }

        int growSpeed = PlantManager.Instance.GetPlantGrowSpeed();
        bool growing = slotCfgID >= 0 && slotProgress < cfg.Progress;
        bool ripe = slotCfgID >= 0 && slotProgress >= cfg.Progress;

        //三种状态互斥展示
        plantState.Visible = slotCfgID < 0;
        growState.Visible = growing;
        ripeState.Visible = ripe;
        divider.Visible = !growing;
        stateTip.Visible = !growing;
        nameLabel.Position = new Vector2(nameLabel.Position.X, growing ? 80f : 111f);

        if (growing)
        {
            int remainDays = CeilDiv(cfg.Progress - slotProgress, growSpeed);
            progress.MaxValue = cfg.Progress;
            progress.Value = slotProgress;
            progressLabel.Text = $"{slotProgress}/{cfg.Progress}";
            growLabel.Text = growSpeed > 0 ? $"生长中 · 剩余 {remainDays} 天" : "升级后继续生长";
        }
        else if (ripe)
        {
            stateTip.Text = "已成熟";
            stateTip.AddThemeColorOverride("font_color", RipeColor);
        }
        else
        {
            int days = CeilDiv(cfg.Progress, growSpeed);
            stateTip.Text = growSpeed > 0 ? $"预计时间：{days}天" : "升级后可种植";
            stateTip.AddThemeColorOverride("font_color", InfoColor);

            staminaLabel.Text = $"体力 ×{cfg.Stamina}";

            bool slotFull = PlantManager.Instance.IsPlantSlotFull();
            bool noStamina = PlayerManager.Instance.BaseStamina < cfg.Stamina;
            plantBtn.Disabled = slotFull || noStamina;
            plantBtn.Material = plantBtn.Disabled ? _disabledActionMaterial : null;
            staminaLabel.AddThemeColorOverride("font_color", noStamina ? DangerColor : Colors.White);
            statusTip.Visible = slotFull || noStamina;
            statusTip.Text = slotCount == 0 ? "暂无种植位" : slotFull ? "种植位已满" : "体力不足";
            statusTip.AddThemeColorOverride("font_color", noStamina && !slotFull ? DangerColor : InfoColor);
        }
    }

    private void OnPlantPressed(int cfgID)
    {
        if (PlantManager.Instance == null) return;

        //已种植已满
        if (PlantManager.Instance.IsPlantSlotFull())
        {
            UIManager.Instance?.ShowCommonTips2("可种植数量已满，无法继续种植！");
            return;
        }

        if (!PlantManager.Instance.TryPlant(cfgID))
        {
            UIManager.Instance?.ShowCommonTips2("体力不足");
            return;
        }
        RefreshRows();
    }

    private void OnHarvestPressed(int cfgID)
    {
        if (PlantManager.Instance == null) return;

        var dic = ConfigManager.Instance.roofPlantsItemDic;
        if (dic == null || !dic.ContainsKey(cfgID)) return;

        int total = PlantManager.Instance.HarvestPlant(cfgID);
        if (total > 0 && UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatTips(dic[cfgID].ItemID, $"x{total}");
        }
        RefreshRows();
    }
}
