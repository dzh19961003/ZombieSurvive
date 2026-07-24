using Godot;
using System;

public partial class WorkstationUI : Control
{
    [Export] public Button closeBtn;

    public override void _Ready()
    {

        closeBtn.Pressed += OnCloseButtonPressed;

    }

    private void OnCloseButtonPressed()
    {
        // 调用 UIManager 隐藏自己
        UIManager.Instance.HideUI(Paths.WorkstationUI);
    }
}