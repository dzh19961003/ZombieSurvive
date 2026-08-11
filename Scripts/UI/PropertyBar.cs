using Godot;
using System;

public partial class PropertyBar : Button
{
    [Export] public Label _strengthLabel;
    [Export] public Label _agilityLabel;
    [Export] public Label _intelligenceLabel;
    [Export] public TextureProgressBar _healthBar;
    [Export] public Label _healthText;

    public override void _Ready()
    {
        
        // 延迟到当前帧末尾：保证 PlayerManager._Ready 已执行，Instance 已赋值
        CallDeferred(nameof(DeferredSetup));
    }
    public void RefreshDisplay()
    {
        var pm = PlayerManager.Instance;
        if (pm == null) return;
        SetAttrDisplay(_strengthLabel, pm.Strength);
        SetAttrDisplay(_agilityLabel, pm.Agility);
        SetAttrDisplay(_intelligenceLabel, pm.Intelligence);

        if (_healthBar != null)
        {
            _healthBar.MaxValue = pm.MaxHp;
            _healthBar.Value = Mathf.Clamp(pm.Hp, 0, pm.MaxHp);
        }
        if (_healthText != null)
        {
            _healthText.Text = $"{pm.Hp}/{pm.MaxHp}";
        }
    }
    private void DeferredSetup()
    {
        var pm = PlayerManager.Instance;
        if (pm == null)
        {
            GD.PrintErr("[MainUI] PlayerManager.Instance 仍为 null，初始化失败！确认场景里已挂载 PlayerManager 节点。");
            return;
        }
        RefreshDisplay();
        pm.GetItem += RefreshDisplay;

    }

    public override void _ExitTree()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.GetItem -= RefreshDisplay;
        }
    }

    private void SetAttrDisplay(Label numLabel, int attrValue)
    {
        if (numLabel != null) numLabel.Text = attrValue.ToString();

    }


}

