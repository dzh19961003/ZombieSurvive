using Godot;
using System;

public partial class TrainUI : Control
{
    [Export] public Button closeBtn;
    [Export] public TextureButton upgrade;
    [Export] public TextureButton exercise;
    [Export] public Control upgradePanel;
    [Export] public Control exercisePanel;
    private Texture2D _tabActive;
    private Texture2D _tabInactive;
    private static readonly Color ActiveTabColor = new(0.96f, 0.98f, 1f);
    private static readonly Color InactiveTabColor = new(0.58f, 0.76f, 0.92f);

    public override void _Ready()
    {
        closeBtn.Pressed += OnCloseButtonPressed;
        upgrade.Pressed += OnUpgradePressed;
        exercise.Pressed += OnExercisePressed;

        _tabActive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/building/tab_selected.png");
        _tabInactive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/building/tab_unselected.png");

        // 默认显示升级界面
        CallDeferred(nameof(SwitchToUpgrade));
    }

    private void OnCloseButtonPressed()
    {
        // 调用 UIManager 隐藏自己
        UIManager.Instance.HideUI(Paths.TrainUI);
    }

    private void OnUpgradePressed()
    {
        SwitchToUpgrade();
    }

    private void OnExercisePressed()
    {
        
        if (PlayerManager.Instance == null || PlayerManager.Instance.TrainLevel <= 1)
        {
            UIManager.Instance?.ShowCommonTips2("请先解锁自律区");
            return;
        }
        SwitchToExercise();
    }

    // 升级界面
    private void SwitchToUpgrade()
    {
        if (upgradePanel != null) upgradePanel.Visible = true;
        if (exercisePanel != null) exercisePanel.Visible = false;

        SetTabStyle(upgrade, true);
        SetTabStyle(exercise, false);
    }

    // 锻炼界面
    private void SwitchToExercise()
    {
        if (upgradePanel != null) upgradePanel.Visible = false;
        if (exercisePanel != null) exercisePanel.Visible = true;

        SetTabStyle(upgrade, false);
        SetTabStyle(exercise, true);
    }

    private void SetTabStyle(TextureButton tab, bool active)
    {
        tab.TextureNormal = active ? _tabActive : _tabInactive;
        tab.GetNode<Label>("Label").AddThemeColorOverride("font_color", active ? ActiveTabColor : InactiveTabColor);
    }
}
