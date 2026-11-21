using Godot;

public partial class WorkstationUI : Control
{
    [Export] public Button closeBtn;
    [Export] public TextureButton upgrade;
    [Export] public TextureButton make;
    [Export] public Control upgradePanel;
    [Export] public Control makePanel;
    private Texture2D _tabActive;
    private Texture2D _tabInactive;
    private static readonly Color ActiveTabColor = new(0.96f, 0.98f, 1f);
    private static readonly Color InactiveTabColor = new(0.58f, 0.76f, 0.92f);

    public override void _Ready()
    {
        closeBtn.Pressed += OnCloseButtonPressed;
        upgrade.Pressed += OnUpgradePressed;
        make.Pressed += OnMakePressed;

        _tabActive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/building/tab_selected.png");
        _tabInactive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/building/tab_unselected.png");

        // 默认显示升级界面
        CallDeferred(nameof(SwitchToUpgrade));
    }

    private void OnCloseButtonPressed()
    {
        // 调用 UIManager 隐藏自己
        UIManager.Instance.HideUI(Paths.WorkstationUI);
    }

    private void OnUpgradePressed()
    {
        SwitchToUpgrade();
    }

    private void OnMakePressed()
    {
        
        if (PlayerManager.Instance == null || PlayerManager.Instance.WorkStationLevel <= 1)
        {
            UIManager.Instance?.ShowCommonTips2("请先解锁工作台");
            return;
        }
        SwitchToMake();
    }

    // 升级界面
    private void SwitchToUpgrade()
    {
        if (upgradePanel != null) upgradePanel.Visible = true;
        if (makePanel != null) makePanel.Visible = false;

        SetTabStyle(upgrade, true);
        SetTabStyle(make, false);
    }

    //制作界面
    private void SwitchToMake()
    {
        if (upgradePanel != null) upgradePanel.Visible = false;
        if (makePanel != null) makePanel.Visible = true;

        SetTabStyle(upgrade, false);
        SetTabStyle(make, true);
    }

    private void SetTabStyle(TextureButton tab, bool active)
    {
        tab.TextureNormal = active ? _tabActive : _tabInactive;
        tab.GetNode<Label>("Label").AddThemeColorOverride("font_color", active ? ActiveTabColor : InactiveTabColor);
    }

    internal static Texture2D GetMaterialIcon(int itemID)
    {
        return itemID switch
        {
            7 => GD.Load<Texture2D>("res://Assets/Images/UI/building/plant/wood.png"),
            9 => GD.Load<Texture2D>("res://Assets/Images/UI/building/plant/parts.png"),
            10015 or 10016 => GD.Load<Texture2D>("res://Assets/Images/UI/stamina.png"),
            _ => UIManager.Instance?.GetItemIcon(itemID)
        };
    }
}
