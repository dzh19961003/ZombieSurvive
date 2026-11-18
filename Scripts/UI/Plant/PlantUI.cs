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

        if (upgrade != null) upgrade.TextureNormal = _tabActive;
        if (plant != null) plant.TextureNormal = _tabInactive;
    }

    // 种植界面
    private void SwitchToPlant()
    {
        if (upgradePanel != null) upgradePanel.Visible = false;
        if (plantPanel != null) plantPanel.Visible = true;

        if (upgrade != null) upgrade.TextureNormal = _tabInactive;
        if (plant != null) plant.TextureNormal = _tabActive;
    }
}
