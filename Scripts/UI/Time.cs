using Godot;
using System;

public partial class Time : TextureButton
{
	private Label _timeLabel;
	private TextureButton _day;
	private Label _dayCount;

	public override void _Ready()
	{
		_timeLabel = GetNodeOrNull<Label>("Label");
		_day       = GetNodeOrNull<TextureButton>("Day");
		_dayCount  = GetNodeOrNull<Label>("Day/DayCount");
		CallDeferred(nameof(DeferredSetup));
	}

	private void DeferredSetup()
	{
		var gm = GameManager.Instance;
		if (gm == null)
		{
			GD.PrintErr("[Time] GameManager.Instance 仍为 null，初始化失败！");
			return;
		}
		RefreshTimeDisplay();
		gm.TimeChanged += RefreshTimeDisplay;
	}

	public override void _ExitTree()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.TimeChanged -= RefreshTimeDisplay;
		}
	}
	private void RefreshTimeDisplay(int period, int day)
	{
		if (_timeLabel != null)
		{
			_timeLabel.Text = GameManager.Instance?.GetTimePeriodName(period) ?? "";
		}
		if (_dayCount != null)
		{
			_dayCount.Text = $"第{day}天";
		}
	}

	private void RefreshTimeDisplay()
	{
		var gm = GameManager.Instance;
		if (gm == null) return;
		RefreshTimeDisplay(gm.CurrentTimePeriod, gm.DayCount);
	}
}