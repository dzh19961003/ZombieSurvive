using Godot;

public partial class PlantUI : Control
{
    [Export] public Button closeBtn;
    [Export] public TextureButton upgrade;
    [Export] public TextureButton plant;
    [Export] public Control upgradePanel;
    [Export] public Control plantPanel;

    private Texture2D _tabActive;
    private Texture2D _tabInactive;
    private static readonly Color ActiveTabColor = new(0.96f, 0.98f, 1f);
    private static readonly Color InactiveTabColor = new(0.58f, 0.76f, 0.92f);

    public override void _Ready()
    {
        closeBtn.Pressed += OnCloseButtonPressed;
        upgrade.Pressed += OnUpgradePressed;
        plant.Pressed += OnPlantPressed;

        _tabActive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/building/tab_selected.png");
        _tabInactive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/building/tab_unselected.png");

        // 默认显示升级界面
        CallDeferred(nameof(SwitchToUpgrade));
    }

    private void OnCloseButtonPressed()
    {
        UIManager.Instance.HideUI(Paths.PlantUI);
    }

    private void OnUpgradePressed()
    {
        SwitchToUpgrade();
    }

    private void OnPlantPressed()
    {
        if (PlantManager.Instance == null ||
            (PlantManager.Instance.PlantLevel <= 1 && !PlantManager.Instance.PlantUnlocked))
        {
            UIManager.Instance?.ShowCommonTips2("请先解锁！");
            return;
        }
        SwitchToPlant();
    }

    // 升级界面
    private void SwitchToUpgrade()
    {
        if (upgradePanel != null) upgradePanel.Visible = true;
        if (plantPanel != null) plantPanel.Visible = false;

        SetTabStyle(upgrade, true);
        SetTabStyle(plant, false);
    }

    // 种植界面
    private void SwitchToPlant()
    {
        if (upgradePanel != null) upgradePanel.Visible = false;
        if (plantPanel != null) plantPanel.Visible = true;

        SetTabStyle(upgrade, false);
        SetTabStyle(plant, true);
    }

    private void SetTabStyle(TextureButton tab, bool active)
    {
        tab.TextureNormal = active ? _tabActive : _tabInactive;
        tab.GetNode<Label>("Label").AddThemeColorOverride("font_color", active ? ActiveTabColor : InactiveTabColor);
    }
}
