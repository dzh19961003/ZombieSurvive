using Godot;
using System;
using MyProject;

namespace MyProject{
public partial class MainUI : Control
{
	[Export] public Button propertyBtn;
        private Label _strengthLabel;
        private Label _agilityLabel;
        private Label _intelligenceLabel;
        private TextureProgressBar _healthBar;
        private Label _healthText;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		propertyBtn.Pressed += EnterPro;

		_strengthLabel      = GetNodeOrNull<Label>("PropertyBar/strength/Label");
		_agilityLabel       = GetNodeOrNull<Label>("PropertyBar/agile/Label3");
		_intelligenceLabel  = GetNodeOrNull<Label>("PropertyBar/intelligence/Label2");
		_healthBar          = GetNodeOrNull<TextureProgressBar>("Health/HealthProgressBar");
		_healthText         = GetNodeOrNull<Label>("Health/Label");

		// 延迟到当前帧末尾：保证 PlayerManager._Ready 已执行，Instance 已赋值
		CallDeferred(nameof(DeferredSetup));
	}

	/// <summary>当前帧所有节点 _Ready 结束后才执行，此时 PlayerManager.Instance 已就绪。</summary>
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

	/// <summary>
	/// 从 PlayerManager 读取三维属性和生命值，刷新到 UI。
	/// 订阅 GetItem 事件，玩家数据变化时自动触发。
	/// </summary>
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

	private void SetAttrDisplay(Label numLabel ,int attrValue)
        {
            if (numLabel != null) numLabel.Text = attrValue.ToString();

        }

	private void EnterPro()
	{
		UIManager.Instance?.ShowUI(Paths.PropertyUI);
	}
}
}
