using Godot;
using System;
using MyProject;

public partial class TExerciseUI : Control
{
	// 家具解锁所需自律区等级
	private const int DumbbellUnlockLevel = 1;
	private const int TreadmillUnlockLevel = 2;
	private const int BookshelfUnlockLevel = 3;

	[Export] private ShaderMaterial _disabledActionMaterial;

	private static readonly Color LockedIconColor = new(0.70f, 0.76f, 0.88f);
	private static readonly Color InsufficientColor = new(1f, 0.47f, 0.42f);

	private class Furniture
	{
		public string DisplayName;
		public string ExpLabel;
		public string ActionText;
		public int UnlockLevel;
		public PlayerManager.TrainStatType StatType;
	}

	private readonly Furniture[] _furnitures = new Furniture[]
	{
		new Furniture { DisplayName = "哑铃", ExpLabel = "力量经验", ActionText = "撸铁", UnlockLevel = DumbbellUnlockLevel, StatType = PlayerManager.TrainStatType.Strength },
		new Furniture { DisplayName = "跑步机", ExpLabel = "速度经验", ActionText = "跑步", UnlockLevel = TreadmillUnlockLevel, StatType = PlayerManager.TrainStatType.Agility },
		new Furniture { DisplayName = "书柜", ExpLabel = "智力经验", ActionText = "阅读", UnlockLevel = BookshelfUnlockLevel, StatType = PlayerManager.TrainStatType.Intelligence },
	};

	private class RowUi
	{
		public Label Name;
		public RichTextLabel ExpText;
		public Label LockTip;
		public TextureRect Icon;
		public TextureRect LockIcon;
		public RichTextLabel Times;
		public TextureRect StaminaIcon;
		public Label StaminaNum;
		public Button ActionBtn;
		public Label StatusTip;
	}

	private readonly RowUi[] _rows = new RowUi[3];

	private Label _levelValue;
	private Label _expValue;

	public override void _Ready()
	{
		_levelValue = GetNode<Label>("Summary/Pad/Row/LeftPart/LevelValue");
		_expValue = GetNode<Label>("Summary/Pad/Row/RightPart/ExpValue");

		for (int i = 0; i < _rows.Length; i++)
		{
			string row = $"Row{i + 1}";
			int index = i;
			var ui = new RowUi
			{
				Name = GetNode<Label>($"{row}/Name"),
				ExpText = GetNode<RichTextLabel>($"{row}/ExpText"),
				LockTip = GetNode<Label>($"{row}/LockTip"),
				Icon = GetNode<TextureRect>($"{row}/Icon"),
				LockIcon = GetNode<TextureRect>($"{row}/LockIcon"),
				Times = GetNode<RichTextLabel>($"{row}/Times"),
				StaminaIcon = GetNode<TextureRect>($"{row}/StaminaIcon"),
				StaminaNum = GetNode<Label>($"{row}/StaminaNum"),
				ActionBtn = GetNode<Button>($"{row}/ActionBtn"),
				StatusTip = GetNode<Label>($"{row}/StatusTip"),
			};
			ui.ActionBtn.Pressed += () => OnTrainPressed(index);
			_rows[i] = ui;
		}

		CallDeferred(nameof(DeferredInit));
	}

	public override void _ExitTree()
	{
		if (PlayerManager.Instance != null)
		{
			PlayerManager.Instance.GetItem -= OnPlayerDataChanged;
		}
		VisibilityChanged -= OnVisibilityChanged;
	}

	private void DeferredInit()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null)
		{
			GD.PrintErr("[TExerciseUI] PlayerManager 或 ConfigManager 未就绪");
			return;
		}
		PlayerManager.Instance.GetItem += OnPlayerDataChanged;
		VisibilityChanged += OnVisibilityChanged;
		RefreshDisplay();
	}

	private void OnPlayerDataChanged()
	{
		CallDeferred(nameof(RefreshDisplay));
	}

	private void OnVisibilityChanged()
	{
		if (Visible) RefreshDisplay();
	}

	private void RefreshDisplay()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

		int trainLevel = PlayerManager.Instance.TrainLevel;
		// 单次锻炼消耗的玩家体力（10027）
		int staminaCost = PlayerManager.Instance.TrainStaminaCostValue;
		int finalExp = CalcFinalExp();

		RefreshSummary(trainLevel);
		RefreshRows(trainLevel, staminaCost, finalExp);
	}

	// 顶部概要：自律区当前等级 + 当前经验效率
	private void RefreshSummary(int trainLevel)
	{
		_levelValue.Text = $"Lv.{trainLevel}";

		var cfg = GetLevelConfig(trainLevel);
		int equipPercent = cfg != null ? cfg.ExpAdd : 0;
		_expValue.Text = $"+{GetPlayerExpBonusPercent() + equipPercent}%";
	}

	private void RefreshRows(int trainLevel, int staminaCost, int finalExp)
	{
		for (int i = 0; i < _furnitures.Length; i++)
		{
			Furniture f = _furnitures[i];
			RowUi row = _rows[i];
			bool unlocked = trainLevel >= f.UnlockLevel;

			row.Name.Text = f.DisplayName;
			row.ActionBtn.Text = f.ActionText;
			row.Icon.SelfModulate = unlocked ? Colors.White : LockedIconColor;
			row.LockIcon.Visible = !unlocked;

			// 未解锁：只展示名称与解锁条件
			row.ExpText.Visible = unlocked;
			row.Times.Visible = unlocked;
			row.StaminaIcon.Visible = unlocked;
			row.StaminaNum.Visible = unlocked;
			row.ActionBtn.Visible = unlocked;
			row.StatusTip.Visible = false;
			row.LockTip.Visible = !unlocked;
			if (!unlocked)
			{
				row.LockTip.Text = $"自律区 Lv.{f.UnlockLevel} 解锁";
				continue;
			}

			// 经验文案：展示实际可获得经验
			row.ExpText.Text = $"{f.ExpLabel} [color=#ffb35c]+{finalExp}[/color]";

			// 今日次数
			int used = PlayerManager.Instance.GetTrainTimes(i);
			row.Times.Text = $"今日 [color=#ffb35c]{used}/{Consts.maxTrainTimes}[/color]";

			// 体力消耗
			row.StaminaNum.Text = $"体力 ×{staminaCost}";
			bool noStamina = !PlayerManager.Instance.CanConsumeTrainStamina;
			row.StaminaNum.AddThemeColorOverride("font_color", noStamina ? InsufficientColor : Colors.White);

			// 次数用尽或体力不足
			bool timesUp = used >= Consts.maxTrainTimes;
			row.ActionBtn.Disabled = timesUp || noStamina;
			row.ActionBtn.Material = row.ActionBtn.Disabled ? _disabledActionMaterial : null;
			row.StatusTip.Text = timesUp ? "今日次数已用尽" : (noStamina ? "体力不足" : "");
			row.StatusTip.Visible = timesUp || noStamina;
		}
	}

	private void OnTrainPressed(int index)
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;
		Furniture f = _furnitures[index];

		if (PlayerManager.Instance.TrainLevel < f.UnlockLevel)
		{
			UIManager.Instance?.ShowCommonTips2("升级自律区以解锁该家具");
			return;
		}
		if (PlayerManager.Instance.GetTrainTimes(index) >= Consts.maxTrainTimes)
		{
			UIManager.Instance?.ShowCommonTips2("今日锻炼次数已用尽");
			return;
		}
		// 扣玩家体力（10027）
		if (!PlayerManager.Instance.TryConsumeTrainStamina())
		{
			UIManager.Instance?.ShowCommonTips2("体力不足，无法锻炼");
			return;
		}

		// 发放最终经验
		int finalExp = CalcFinalExp();
		PlayerManager.Instance.GainTrainExp(f.StatType, finalExp);
		PlayerManager.Instance.AddTrainTimes(index);
		GD.Print($"[TExerciseUI] {f.DisplayName}锻炼完成，获得{finalExp}经验");
		UIManager.Instance?.ShowCommonTips2($"{f.ExpLabel}+{finalExp}");
	}

	// 自律区当前等级配置
	private RoofTrain GetLevelConfig(int level)
	{
		var dic = ConfigManager.Instance?.roofTrainDic;
		if (dic == null) return null;
		return dic.ContainsKey(level) ? dic[level] : null;
	}

	// 玩家经验获取效率加成：智力加成 + 状态/天赋等效率加成
	private int GetPlayerExpBonusPercent()
	{
		if (PlayerManager.Instance == null) return 0;
		double bonus = PlayerManager.Instance.Intelligence * Consts.expPerIntellect
		             + (PlayerManager.Instance.Exp_acq_rate - 1.0) * 100.0;
		return (int)Math.Round(bonus);
	}

	// 最终锻炼经验 = trainExp × (1 + 玩家加成% + 自律区等级加成%)
	private int CalcFinalExp()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return Consts.trainExp;

		var cfg = GetLevelConfig(PlayerManager.Instance.TrainLevel);
		int trainBonusPercent = cfg != null ? cfg.ExpAdd : 0;

		double playerBonusPercent = PlayerManager.Instance.Intelligence * Consts.expPerIntellect
		                          + (PlayerManager.Instance.Exp_acq_rate - 1.0) * 100.0;

		return (int)Math.Round(Consts.trainExp * (1.0 + (playerBonusPercent + trainBonusPercent) / 100.0));
	}
}
