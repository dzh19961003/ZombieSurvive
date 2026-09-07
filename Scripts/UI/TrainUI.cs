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

    public override void _Ready()
    {
        closeBtn.Pressed += OnCloseButtonPressed;
        upgrade.Pressed += OnUpgradePressed;
        exercise.Pressed += OnExercisePressed;

        _tabActive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/tab_active.png");
        _tabInactive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/tab_inactive.png");

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
        SwitchToExercise();
    }

    // 升级界面
    private void SwitchToUpgrade()
    {
        if (upgradePanel != null) upgradePanel.Visible = true;
        if (exercisePanel != null) exercisePanel.Visible = false;

        if (upgrade != null) upgrade.TextureNormal = _tabActive;
        if (exercise != null) exercise.TextureNormal = _tabInactive;
    }

    //制作界面
    private void SwitchToExercise()
    {
        if (upgradePanel != null) upgradePanel.Visible = false;
        if (exercisePanel != null) exercisePanel.Visible = true;

        if (upgrade != null) upgrade.TextureNormal = _tabInactive;
        if (exercise != null) exercise.TextureNormal = _tabActive;
    }
}
