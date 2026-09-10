using Godot;
using System;
using System.Linq;
using MyProject;

public partial class TUpgradeUI : Control
{
	[Export] Button upgrade;
	private const int RunningmachineUnlockLevel = 2;
	private const int BookshelfUnlockLevel = 3;
	private const int MaxLevel = 5;
	private static readonly Color LockedFunColor = new(0.5958281f, 0.5958281f, 0.5958281f, 1f);

	private int currentLevel = 1;

	private Label _gradeTitle;
	private Label _expELevelValue;
	private Label _expENextLevelValue;
	private Label _maxTip;
	private Label[] _reqLabels;
	private TextureRect[] _reqIcons;
	//额外功能展示
	private Label _runningmachineDesc;
	private Label _bookshelfDesc;
	private TextureRect _runningmachineLock;
	private TextureRect _bookshelfLock;

	public override void _Ready()
	{
		_gradeTitle     = GetNode<Label>("GradeTitle");
		_expELevelValue = GetNode<Label>("ExpEfficiency/EELevelValue");
		_maxTip=GetNode<Label>("UpgradeRequ/maxTip");
		_expENextLevelValue = GetNode<Label>("ExpEfficiency/EENextLevelValue");
		_reqLabels = new Label[]
		{
			GetNode<Label>("UpgradeRequ/requirement1"),
			GetNode<Label>("UpgradeRequ/requirement2"),
			GetNode<Label>("UpgradeRequ/requirement3")
		};
		_reqIcons = new TextureRect[]
		{
			GetNode<TextureRect>("UpgradeRequ/TextureRect"),
			GetNode<TextureRect>("UpgradeRequ/TextureRect2"),
			GetNode<TextureRect>("UpgradeRequ/TextureRect3")
		};
		//额外功能
		_runningmachineDesc = GetNode<Label>("ExtraFun/ExtraFunDescription");
		_bookshelfDesc      = GetNode<Label>("ExtraFun/ExtraFunDescription2");
		_runningmachineLock = GetNode<TextureRect>("ExtraFun/Lock2");
		_bookshelfLock      = GetNode<TextureRect>("ExtraFun/Lock");

		if (upgrade != null)
		{
			upgrade.Pressed += OnUpgradePressed;
		}


		CallDeferred(nameof(DeferredInit));
	}

	public override void _ExitTree()
	{
		if (PlayerManager.Instance != null)
		{
			PlayerManager.Instance.GetItem -= OnPlayerDataChanged;
		}
	}

	private void DeferredInit()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null)
		{
			GD.PrintErr("[TUpgradeUI] PlayerManager 或 ConfigManager 未就绪");
			return;
		}
		PlayerManager.Instance.GetItem += OnPlayerDataChanged;
		RefreshDisplay();
	}

	private void OnPlayerDataChanged()
	{
		CallDeferred(nameof(RefreshDisplay));
	}

	//当前等级配置
	private RoofTrain FindUpgradeConfig()
	{
		var list = ConfigManager.Instance?.roofTrainList;
		if (list == null) return null;
		return list.FirstOrDefault(r => r.Level == currentLevel);
	}

	//下一等级配置
	private RoofTrain FindNextUpgradeConfig()
	{
		var list = ConfigManager.Instance?.roofTrainList;
		if (list == null) return null;
		return list.FirstOrDefault(r => r.Level == currentLevel + 1);
	}

	private void RefreshDisplay()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

		currentLevel = PlayerManager.Instance.TrainLevel;
		//额外功能解锁状态展示
		RefreshExtraFun();

		string levelText = $"等级{currentLevel}";
		if (_gradeTitle != null) _gradeTitle.Text = levelText;

		var cfg = FindUpgradeConfig();
		var nextCfg = FindNextUpgradeConfig();

		// 经验效率
		if (_expELevelValue != null)
		{
			_expELevelValue.Visible = true;
			_expELevelValue.Text = cfg != null ? $"+{cfg.ExpAdd+PlayerManager.Instance.Exp_acq_rate*100:F2}%" : "+0%";
		}

		if (currentLevel >= MaxLevel)
		{
			if (_expENextLevelValue != null) _expENextLevelValue.Visible = false;
			// 已达最高等级：清空需求并禁用按钮
			for (int i = 0; i < _reqLabels.Length; i++)
			{
				_reqIcons[i].Visible = false;
				_reqLabels[i].Visible = false;
			}
			if (upgrade != null) upgrade.Icon = GD.Load<Texture2D>("res://Assets/Images/UI/button_cancel.png");
			_maxTip.Visible = true;
			return;
		}

		if (_expENextLevelValue != null)
		{
			_expENextLevelValue.Visible = true;
			_expENextLevelValue.Text = nextCfg != null ? $"+{nextCfg.ExpAdd+PlayerManager.Instance.Exp_acq_rate*100:F2}%" : "+0%";
		}
		_maxTip.Visible = false;
		if (upgrade != null) upgrade.Disabled = false;

		// 动态填充三个需求槽位
		int slotCount = Mathf.Min(_reqLabels.Length, _reqIcons.Length);
		for (int i = 0; i < slotCount; i++)
		{
			if (_reqLabels[i] == null || _reqIcons[i] == null) continue;
			_reqIcons[i].Visible = true;
			_reqLabels[i].Visible = true;

			int itemID = (cfg.ItemID != null && i < cfg.ItemID.Count) ? cfg.ItemID[i] : 0;
			int need   = (cfg.ItemNum != null && i < cfg.ItemNum.Count) ? cfg.ItemNum[i] : 0;
			if (UIManager.Instance != null && itemID != 0 && itemID < 10000)
			{
				var icon = UIManager.Instance.GetItemIcon(itemID);
				if (icon != null) _reqIcons[i].Texture = icon;
			}

			bool insufficient;
			if (itemID >= 10000)
			{

				if (itemID == 10015)
				{
					insufficient = PlayerManager.Instance.BaseStamina < need;
				}
				else
				{
					insufficient = true;
				}
			}
			else
			{
				// 物品类：用持有数量比
				insufficient = PlayerManager.Instance.GetItemCount(itemID) < need;
			}
			_reqLabels[i].Text = $"×{need}";
			SetReqColor(_reqLabels[i], insufficient);
		}
	}

	private void SetReqColor(Label label, bool insufficient)
	{
		if (label == null) return;
		//资源不足红色，充足白色
		label.AddThemeColorOverride("font_color", insufficient ? Colors.Red : Colors.White);
	}

	//额外功能展示：等级不足显示锁图标且名称灰色，解锁后白色并隐藏锁图标
	private void RefreshExtraFun()
	{
		if (_runningmachineDesc == null || _bookshelfDesc == null) return;

		bool runningmachineUnlocked = PlayerManager.Instance.TrainLevel >= RunningmachineUnlockLevel;
		bool bookshelfUnlocked = PlayerManager.Instance.TrainLevel >= BookshelfUnlockLevel;

		_runningmachineDesc.AddThemeColorOverride("font_color", runningmachineUnlocked ? Colors.White : LockedFunColor);
		if (_runningmachineLock != null) _runningmachineLock.Visible = !runningmachineUnlocked;

		_bookshelfDesc.AddThemeColorOverride("font_color", bookshelfUnlocked ? Colors.White : LockedFunColor);
		if (_bookshelfLock != null) _bookshelfLock.Visible = !bookshelfUnlocked;
	}

	private void OnUpgradePressed()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

		var cfg = FindUpgradeConfig();
		if (cfg == null)
		{
			UIManager.Instance.ShowCommonTips2("自律区已达最高等级，无法升级");
			return;
		}

		//校验所有材料是否充足
		if (cfg.ItemID != null && cfg.ItemNum != null )
		{
			for (int i = 0; i < cfg.ItemID.Count; i++)
			{
				int itemID = cfg.ItemID[i];
				int need   = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
				if (!IsEnough(itemID, need))
				{
					UIManager.Instance.ShowCommonTips2("升级材料不足");
					return;
				}
			}
		}

		// 扣物品材料
		if (cfg.ItemID != null && cfg.ItemNum != null)
		{
			for (int i = 0; i < cfg.ItemID.Count; i++)
			{
				int itemID = cfg.ItemID[i];
				int need   = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
				if (itemID == 0 || need == 0) continue;

				if (itemID < 10000)
				{
					PlayerManager.Instance.RemoveItem(itemID, need);
				}
			}
		}
		//扣体力
		if (cfg.ItemID != null && cfg.ItemNum != null)
		{
			for (int i = 0; i < cfg.ItemID.Count; i++)
			{
				int itemID = cfg.ItemID[i];
				int need   = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
				if (itemID == 10015 && need > 0)
				{
					PlayerManager.Instance.AddItem(10015, -need);
					break; 
				}
			}
		}

		currentLevel++;

		GD.Print($"[TUpgradeUI] 升级成功，当前等级：{currentLevel}");
		PlayerManager.Instance.SetTrainLevel(currentLevel);

		RefreshDisplay();
	}
	// 判断材料是否充足
	private bool IsEnough(int itemID, int need)
	{
		if (PlayerManager.Instance == null) return false;
		if (need == 0) return true;

		if (itemID >= 10000)
		{
			if (itemID == 10015) return PlayerManager.Instance.BaseStamina >= need;
			return false;
		}
		return PlayerManager.Instance.GetItemCount(itemID) >= need;
	}
}
