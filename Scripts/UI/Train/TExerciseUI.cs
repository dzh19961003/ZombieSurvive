using Godot;
using System;
using System.Linq;
using MyProject;


public partial class TExerciseUI : Control
{
	//家具解锁所需自律区等级
	private const int DumbbellUnlockLevel = 1;
	private const int TreadmillUnlockLevel = 2;
	private const int BookshelfUnlockLevel = 3;

	private static readonly Color EnabledBtnColor = new(1f, 1f, 1f, 1f);
	private static readonly Color DisabledBtnColor = new(0.45f, 0.45f, 0.45f, 1f);


	private class Furniture
	{
		public string ExpLabel;                                  
		public string StatName;                                 
		public int UnlockLevel;                                  
		public PlayerManager.TrainStatType StatType;             
	}

	private readonly Furniture[] _furnitures = new Furniture[]
	{
		new Furniture { ExpLabel = "力量经验", StatName = "力量", UnlockLevel = DumbbellUnlockLevel,  StatType = PlayerManager.TrainStatType.Strength },
		new Furniture { ExpLabel = "速度经验", StatName = "速度", UnlockLevel = TreadmillUnlockLevel, StatType = PlayerManager.TrainStatType.Agility },
		new Furniture { ExpLabel = "智力经验", StatName = "智力", UnlockLevel = BookshelfUnlockLevel, StatType = PlayerManager.TrainStatType.Intelligence },
	};

	
	private class RowUi
	{
		public Label ExpText;
		public Label LockTip;
		public Label Times;
		public TextureRect StaminaIcon;
		public Label StaminaNum;
		public Button ActionBtn;
	}
	private readonly RowUi[] _rows = new RowUi[3];

	public override void _Ready()
	{
		for (int i = 0; i < _rows.Length; i++)
		{
			string row = $"Row{i + 1}";
			int index = i; 
			var ui = new RowUi
			{
				ExpText = GetNode<Label>($"{row}/ExpText"),
				LockTip = GetNode<Label>($"{row}/LockTip"),
				Times = GetNode<Label>($"{row}/Times"),
				StaminaIcon = GetNode<TextureRect>($"{row}/StaminaIcon"),
				StaminaNum = GetNode<Label>($"{row}/StaminaNum"),
				ActionBtn = GetNode<Button>($"{row}/ActionBtn"),
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
		//单次锻炼消耗的玩家体力（10027）
		int staminaCost = PlayerManager.Instance.TrainStaminaCostValue;

		for (int i = 0; i < _furnitures.Length; i++)
		{
			Furniture f = _furnitures[i];
			RowUi row = _rows[i];
			bool unlocked = trainLevel >= f.UnlockLevel;

			//未解锁时候的显示
			row.ExpText.Visible = unlocked;
			row.Times.Visible = unlocked;
			row.StaminaIcon.Visible = unlocked;
			row.StaminaNum.Visible = unlocked;
			row.ActionBtn.Visible = unlocked;
			row.LockTip.Visible = !unlocked;
			if (!unlocked) continue;

			//经验文案
			row.ExpText.Text = $"{f.ExpLabel}+{Consts.trainExp}";

			//今日次数
			int used = PlayerManager.Instance.GetTrainTimes(i);
			row.Times.Text = $"({used}/{Consts.maxTrainTimes})";

			//玩家体力(10027)消耗
			row.StaminaNum.Text = $"×{staminaCost}";
			bool noStamina = !PlayerManager.Instance.CanConsumeTrainStamina;
			row.StaminaNum.AddThemeColorOverride("font_color", noStamina ? Colors.Red : Colors.White);

			//次数用尽或体力不足
			bool timesUp = used >= Consts.maxTrainTimes;
			row.ActionBtn.Disabled = timesUp || noStamina;
			row.ActionBtn.Modulate = row.ActionBtn.Disabled ? DisabledBtnColor : EnabledBtnColor;
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
		//扣玩家体力(10027)
		if (!PlayerManager.Instance.TryConsumeTrainStamina())
		{
			UIManager.Instance?.ShowCommonTips2("体力不足，无法锻炼");
			return;
		}

		//发放最终经验
		int finalExp = CalcFinalExp();
		PlayerManager.Instance.GainTrainExp(f.StatType, finalExp);
		PlayerManager.Instance.AddTrainTimes(index);
		GD.Print($"[TExerciseUI] {f.StatName}锻炼完成，获得{finalExp}经验");
		UIManager.Instance?.ShowCommonTips2($"{f.StatName}经验+{finalExp}");
	}

	//最终锻炼经验 = trainExp × (1 + 智力加成% + 经验效率buff% + 自律区等级加成%)
	private int CalcFinalExp()
	{
		//自律区当前等级提供的经验加成
		var cfg = ConfigManager.Instance.roofTrainList?
			.FirstOrDefault(r => r.Level == PlayerManager.Instance.TrainLevel);
		int trainBonusPercent = cfg != null ? cfg.ExpAdd : 0;

		//玩家经验获取效率：
		
		double playerBonusPercent = PlayerManager.Instance.Intelligence * Consts.expPerIntellect
		                          + (PlayerManager.Instance.Exp_acq_rate - 1.0) * 100.0;

		return (int)Math.Round(Consts.trainExp * (1.0 + (playerBonusPercent + trainBonusPercent) / 100.0));
	}
}
