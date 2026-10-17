using Godot;
using System;
using System.Linq;
using MyProject;

public partial class PlantPanel : Control
{
    private Control _rowTemplate;   
    private Control _rowList;       
    private Label _plantedLabel;

    //向上取整
    private static int CeilDiv(int a, int b) => b > 0 ? (a + b - 1) / b : 0;

    public override void _Ready()
    {
        _rowList = GetNode<Control>("RowList");
        _rowTemplate = GetNode<Control>("RowList/Row1");
        _plantedLabel = GetNode<Label>("PlantedLabel");

        _rowList.RemoveChild(_rowTemplate);

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
        if (ConfigManager.Instance == null || PlayerManager.Instance == null) return;
        var dic = ConfigManager.Instance.roofPlantsItemDic;
        if (dic == null || _rowTemplate == null || _rowList == null) return;

        //清空旧行
        foreach (var child in _rowList.GetChildren())
        {
            child.QueueFree();
        }

        //按配置ID顺序生成行
        foreach (var kv in dic.OrderBy(k => k.Key))
        {
            var row = (Control)_rowTemplate.Duplicate();
            _rowList.AddChild(row);
            FillRow(row, kv.Key, kv.Value);
        }

        
        _plantedLabel.Text = $"已种植：{PlantManager.Instance.GetPlantedCount()}/{PlantManager.Instance.GetPlantSlotCount()}";
    }

    //填充行
    private void FillRow(Control row, int cfgID, RoofPlantsItem cfg)
    {
        var icon = row.GetNode<TextureRect>("Icon");
        var numLabel = row.GetNode<Label>("NumLabel");
        var nameLabel = row.GetNode<Label>("NameLabel");
        var plantState = row.GetNode<Control>("PlantState");
        var staminaLabel = row.GetNode<Label>("PlantState/StaminaLabel");
        var timeLabel = row.GetNode<Label>("PlantState/TimeLabel");
        var plantBtn = row.GetNode<Button>("PlantState/PlantBtn");
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
        if (UIManager.Instance != null)
        {
            var tex = UIManager.Instance.GetItemIcon(cfg.ItemID);
            if (tex != null) icon.Texture = tex;
        }
        numLabel.Text = $"x{cfg.Num}";
        nameLabel.Text = ConfigManager.Instance.itemDic.ContainsKey(cfg.ItemID)
            ? ConfigManager.Instance.itemDic[cfg.ItemID].Name
            : "???";

        
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

        bool growing = slotCfgID >= 0 && slotProgress < cfg.Progress;
        bool ripe = slotCfgID >= 0 && slotProgress >= cfg.Progress;

        //三种状态
        plantState.Visible = slotCfgID < 0;
        growState.Visible = growing;
        ripeState.Visible = ripe;

        if (growing)
        {
            //进度条显示
            progress.MaxValue = cfg.Progress;
            progress.Value = slotProgress;
            progressLabel.Text = $"{slotProgress}/{cfg.Progress}";
            int remainDays = CeilDiv(cfg.Progress - slotProgress, PlantManager.Instance.GetPlantGrowSpeed());
            growLabel.Text = $"生长中，剩余{remainDays}天";
        }
        else if (slotCfgID < 0)
        {
            //未种植时的显示
            staminaLabel.Text = $"x{cfg.Stamina}";
            int days = CeilDiv(cfg.Progress, PlantManager.Instance.GetPlantGrowSpeed());
            timeLabel.Text = $"预计时间：{days}天";
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
