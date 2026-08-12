using Godot;
using System;

public partial class WorkstationUI : Control
{
    [Export] public Button closeBtn;
    [Export] public TextureButton upgrade;
    [Export] public TextureButton make;
    [Export] public Control upgradePanel;
    [Export] public Control makePanel;
    private Texture2D _tabActive;
    private Texture2D _tabInactive;

    public override void _Ready()
    {
        closeBtn.Pressed += OnCloseButtonPressed;
        upgrade.Pressed += OnUpgradePressed;
        make.Pressed += OnMakePressed;

        _tabActive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/tab_active.png");
        _tabInactive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/tab_inactive.png");

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
        SwitchToMake();
    }

    // 升级界面
    private void SwitchToUpgrade()
    {
        if (upgradePanel != null) upgradePanel.Visible = true;
        if (makePanel != null) makePanel.Visible = false;

        if (upgrade != null) upgrade.TextureNormal = _tabActive;
        if (make != null) make.TextureNormal = _tabInactive;
    }

    //制作界面
    private void SwitchToMake()
    {
        if (upgradePanel != null) upgradePanel.Visible = false;
        if (makePanel != null) makePanel.Visible = true;

        if (upgrade != null) upgrade.TextureNormal = _tabInactive;
        if (make != null) make.TextureNormal = _tabActive;
    }
}
