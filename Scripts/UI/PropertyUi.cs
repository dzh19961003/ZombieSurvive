using Godot;
using System;

public partial class PropertyUi : Control
{
    [Export] public  Button close_button;

    public override void _Ready()
    {

        close_button.Pressed += OnCloseButtonPressed;
      
    }

    private void OnCloseButtonPressed()
    {
        // 调用 UIManager 隐藏自己
        UIManager.Instance.HideUI(Paths.PropertyUI);
    }
}