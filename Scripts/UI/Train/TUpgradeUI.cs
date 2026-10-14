using Godot;
using System;
using System.Linq;
using MyProject;

public partial class TUpgradeUI : Control
{
	[Export] Button upgrade;
	private const int MaxTextureLevel = 5;
	
	private const int RunningmachineUnlockLevel = 2;
	private const int BookshelfUnlockLevel = 3;
	private const int MaxLevel = 5;
	private static readonly Color LockedFunColor = new(0.5958281f, 0.5958281f, 0.5958281f, 1f);
	
	private int currentLevel = 1;
	
	private TextureRect _trainTexture;
	private Label _gradeTitle;
	private Label _expELevelValue;
	private Label _expENextLevelValue;
	private TextureRect _arrow;
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
		_arrow = GetNode<TextureRect>("ExpEfficiency/Arrow");
		_trainTexture     = GetNode<TextureRect>("TrainTexture");
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
		var dic = ConfigManager.Instance?.roofTrainDic;
		if (dic == null) return null;
		return dic.ContainsKey(currentLevel) ? dic[currentLevel] : null;
	}

	//下一等级配置
	private RoofTrain FindNextUpgradeConfig()
	{
		var dic = ConfigManager.Instance?.roofTrainDic;
		if (dic == null) return null;
		return dic.ContainsKey(currentLevel + 1) ? dic[currentLevel + 1] : null;
	}

	private void RefreshDisplay()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

		currentLevel = PlayerManager.Instance.TrainLevel;
		//额外功能解锁状态展示
		RefreshExtraFun();
		RefreshTrainTexture();
		string levelText = $"等级{currentLevel}";
		if (_gradeTitle != null) _gradeTitle.Text = levelText;

		var cfg = FindUpgradeConfig();
		var nextCfg = FindNextUpgradeConfig();

		// 等级1未解锁：材料查当前等级配置；已解锁或等级>1：查下一等级配置
		bool isUnlockStep = currentLevel == 1 && !PlayerManager.Instance.TrainUnlocked;
		var reqCfg = isUnlockStep ? cfg : nextCfg;

		// 经验效率
		if (isUnlockStep)
		{
			// 解锁前
			if (_expELevelValue != null)
			{
				_expELevelValue.Visible = true;
				_expELevelValue.Text = "解锁后查看";
			}
			if (_expENextLevelValue != null)
			{
				_expENextLevelValue.Visible = false;
				_expENextLevelValue.Text = "解锁后查看";
			}
			if (_arrow != null) _arrow.Visible = false;
		}
		else
		{
			// 解锁后
			if (_expELevelValue != null)
			{
				_expELevelValue.Visible = true;
				_expELevelValue.Text = cfg != null ? $"+{cfg.ExpAdd+PlayerManager.Instance.Exp_acq_rate*100:F2}%" : "+0%";
			}

			if (currentLevel >= MaxLevel)
			{
				// 满级
				if (_expENextLevelValue != null) _expENextLevelValue.Visible = false;
				if (_arrow != null) _arrow.Visible = false;
			}
			else
			{
				// 正常升级
				if (_expENextLevelValue != null)
				{
					_expENextLevelValue.Visible = true;
					_expENextLevelValue.Text = nextCfg != null ? $"+{nextCfg.ExpAdd+PlayerManager.Instance.Exp_acq_rate*100:F2}%" : "+0%";
				}
				if (_arrow != null) _arrow.Visible = true;
			}
		}

		if (currentLevel >= MaxLevel && PlayerManager.Instance.TrainUnlocked)
		{
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
		_maxTip.Visible = false;
		if (upgrade != null)
		{
			upgrade.Disabled = false;
			upgrade.Text = (currentLevel == 1 && !PlayerManager.Instance.TrainUnlocked) ? "解锁" : "升级";
		}

		// 动态填充三个需求槽位
		int slotCount = Mathf.Min(_reqLabels.Length, _reqIcons.Length);
		for (int i = 0; i < slotCount; i++)
		{
			if (_reqLabels[i] == null || _reqIcons[i] == null) continue;
			_reqIcons[i].Visible = true;
			_reqLabels[i].Visible = true;

			int itemID = (reqCfg.ItemID != null && i < reqCfg.ItemID.Count) ? reqCfg.ItemID[i] : 0;
			int need   = (reqCfg.ItemNum != null && i < reqCfg.ItemNum.Count) ? reqCfg.ItemNum[i] : 0;
			if (UIManager.Instance != null && itemID != 0 && itemID < 10000)
			{
				var icon = UIManager.Instance.GetItemIcon(itemID);
				if (icon != null) _reqIcons[i].Texture = icon;
			}
			else if (itemID >= 10000)
			{
				var icon = UIManager.Instance.GetItemIcon(itemID);
				if (icon != null) _reqIcons[i].Texture = icon;
			}

			bool insufficient = !IsEnough(itemID, need);
			_reqLabels[i].Text = $"×{need}";
			SetReqColor(_reqLabels[i], insufficient);
		}
	}

	private void RefreshTrainTexture()
	{
		if (_trainTexture == null || PlayerManager.Instance == null) return;

		int level = Mathf.Clamp(PlayerManager.Instance.TrainLevel, 1, MaxTextureLevel);
		var tex = GD.Load<Texture2D>($"res://Assets/Images/Base/train_{level}.png");
		if (tex != null)
		{
			_trainTexture.Texture = tex;
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

		bool isUnlock = currentLevel == 1 && !PlayerManager.Instance.TrainUnlocked;

		var cfg = isUnlock ? FindUpgradeConfig() : FindNextUpgradeConfig();
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
					UIManager.Instance.ShowCommonTips2(isUnlock ? "解锁材料不足" : "升级材料不足");
					return;
				}
			}
		}

		// 扣材料（物品 + 属性类）
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
				else
				{
					// 属性类（10000+）：通过 AddItem 扣减
					PlayerManager.Instance.AddItem(itemID, -need);
				}
			}
		}

		if (isUnlock)
		{
			PlayerManager.Instance.SetTrainUnlocked(true);
			GD.Print("[TUpgradeUI] 自律区已解锁");
		}
		else
		{
			currentLevel++;
			GD.Print($"[TUpgradeUI] 升级成功，当前等级：{currentLevel}");
			PlayerManager.Instance.SetTrainLevel(currentLevel);
		}

		RefreshDisplay();
	}
	// 判断材料是否充足（支持物品ID和属性ID）
	private bool IsEnough(int itemID, int need)
	{
		if (PlayerManager.Instance == null) return false;
		if (need == 0) return true;

		if (itemID >= 10000)
		{
			// 属性类：通过 AddItem 对应的属性判断
			switch (itemID)
			{
				case 10001: return PlayerManager.Instance.Hp >= need;
				case 10002: return PlayerManager.Instance.MaxHp >= need;
				case 10003: return PlayerManager.Instance.Strength >= need;
				case 10004: return PlayerManager.Instance.Agility >= need;
				case 10005: return PlayerManager.Instance.Intelligence >= need;
				case 10015: return PlayerManager.Instance.BaseStamina >= need;
				case 10016: return PlayerManager.Instance.ExploreStamina >= need;
				default: return false;
			}
		}
		return PlayerManager.Instance.GetItemCount(itemID) >= need;
	}
}
