using Godot;
using System;

public partial class CommonTips2 : Control
{
	[Export] public Label tipsLabel;

	private Tween _tween;

	public override void _Ready()
	{
		// 初始完全透明，避免首次显示前出现闪烁
		Modulate = new Color(1, 1, 1, 0);
	}

	public void ShowTips(string text)
	{
		// 如果上一个动画还没结束，先杀掉，避免状态混乱
		_tween?.Kill();
		_tween = null;

		tipsLabel.Text = text;
		Modulate = new Color(1, 1, 1, 0);
		Visible = true;

		_tween = CreateTween();
		_tween.SetEase(Tween.EaseType.Out);
		_tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.3f);
		_tween.TweenInterval(1.0f);
        _tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 0.3f);
		_tween.TweenCallback(Callable.From(OnFadeOutComplete));
	}

	private void OnFadeOutComplete()
	{
		UIManager.Instance.HideUI("res://UI/CommonTips2.tscn");
	}
}
