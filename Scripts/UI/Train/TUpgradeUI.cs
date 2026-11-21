using Godot;
using System;
using MyProject;

public partial class TUpgradeUI : Control
{
	[Export] TextureButton upgrade;
	[Export] private ShaderMaterial _disabledActionMaterial;

	private const int MaxTextureLevel = 5;
	private const int RunningmachineUnlockLevel = 2;
	private const int BookshelfUnlockLevel = 3;
	private const int MaxLevel = 5;

	private static readonly Color LockedFunColor = new(0.56f, 0.66f, 0.78f);
	private static readonly Color LockedNameColor = new(0.68f, 0.83f, 0.96f);
	private static readonly Color ValueColor = new(1f, 0.97f, 0.89f);
	private static readonly Color InsufficientColor = new(1f, 0.47f, 0.42f);

	private int currentLevel = 1;

	private TextureRect _trainTexture;
	private Label _lvCur;
	private Label _lvNext;
	private TextureRect _levelArrow;
	private Label _expCur;
	private int _expValueFontSize;
	private Label _expNext;
	private Label _expCurBreak;
	private RichTextLabel _expNextBreak;
	private TextureRect _expArrow;
	private Label _featureName1;
	private Label _featureStatus1;
	private Label _featureName2;
	private Label _featureStatus2;
	private Label _reqTitle;
	private Control _reqRow;
	private Label _maxTip;
	private Label _upgradeLabel;
	private Control[] _reqSlots;
	private TextureRect[] _reqIcons;
	private Label[] _reqNames;
	private Label[] _reqCounts;

	public override void _Ready()
	{
		_trainTexture      = GetNode<TextureRect>("TrainTexture");
		_lvCur             = GetNode<Label>("Info/LevelRow/LvCur");
		_lvNext            = GetNode<Label>("Info/LevelRow/LvNext");
		_levelArrow        = GetNode<TextureRect>("Info/LevelRow/LevelArrow");
		_expCur            = GetNode<Label>("Info/ExpRow/ExpCur");
		_expValueFontSize  = _expCur.GetThemeFontSize("font_size");
		_expNext           = GetNode<Label>("Info/ExpRow/ExpNext");
		_expArrow          = GetNode<TextureRect>("Info/ExpRow/ExpArrow");
		_expCurBreak       = GetNode<Label>("Info/ExpCurBreak");
		_expNextBreak      = GetNode<RichTextLabel>("Info/ExpNextBreak");

		_featureName1      = GetNode<Label>("FeatureRow/Feature1/F1Text/F1Name");
		_featureStatus1    = GetNode<Label>("FeatureRow/Feature1/F1Text/F1Status");
		_featureName2      = GetNode<Label>("FeatureRow/Feature2/F2Text/F2Name");
		_featureStatus2    = GetNode<Label>("FeatureRow/Feature2/F2Text/F2Status");

		_reqTitle = GetNode<Label>("ReqTitle");
		_reqRow   = GetNode<Control>("ReqRow");
		_reqSlots = new Control[]
		{
			GetNode<Control>("ReqRow/Req1"),
			GetNode<Control>("ReqRow/Req2"),
			GetNode<Control>("ReqRow/Req3")
		};
		_reqIcons = new TextureRect[]
		{
			GetNode<TextureRect>("ReqRow/Req1/IconWrap/Icon"),
			GetNode<TextureRect>("ReqRow/Req2/IconWrap/Icon"),
			GetNode<TextureRect>("ReqRow/Req3/IconWrap/Icon")
		};
		_reqNames = new Label[]
		{
			GetNode<Label>("ReqRow/Req1/Name"),
			GetNode<Label>("ReqRow/Req2/Name"),
			GetNode<Label>("ReqRow/Req3/Name")
		};
		_reqCounts = new Label[]
		{
			GetNode<Label>("ReqRow/Req1/Count"),
			GetNode<Label>("ReqRow/Req2/Count"),
			GetNode<Label>("ReqRow/Req3/Count")
		};

		_maxTip = GetNode<Label>("MaxTip");

		if (upgrade != null)
		{
			_upgradeLabel = upgrade.GetNodeOrNull<Label>("Label");
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

	// 当前等级配置
	private RoofTrain FindUpgradeConfig()
	{
		var dic = ConfigManager.Instance?.roofTrainDic;
		if (dic == null) return null;
		return dic.ContainsKey(currentLevel) ? dic[currentLevel] : null;
	}

	// 下一等级配置
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
		bool isUnlockStep = currentLevel == 1 && !PlayerManager.Instance.TrainUnlocked;
		bool isMaxLevel = currentLevel >= MaxLevel && PlayerManager.Instance.TrainUnlocked;

		var curCfg = FindUpgradeConfig();
		var nextCfg = FindNextUpgradeConfig();

		RefreshTrainTexture();
		RefreshFeatureUnlock();
		RefreshLevel(curCfg, nextCfg, isUnlockStep, isMaxLevel);
		RefreshRequirements(isUnlockStep ? curCfg : nextCfg, isUnlockStep, isMaxLevel);
	}

	// 等级：Lv.当前 → Lv.下一级
	private void RefreshLevel(RoofTrain curCfg, RoofTrain nextCfg, bool isUnlockStep, bool isMaxLevel)
	{
		_lvCur.Text = $"Lv.{currentLevel}";

		bool hasNext = !isUnlockStep && !isMaxLevel;
		_lvNext.Visible = hasNext;
		_levelArrow.Visible = hasNext;
		if (hasNext) _lvNext.Text = $"Lv.{currentLevel + 1}";

		// 解锁前不展示经验效率
		if (isUnlockStep)
		{
			_expCur.Text = "解锁后查看";
			_expCur.AddThemeColorOverride("font_color", LockedFunColor);
			_expCur.AddThemeFontSizeOverride("font_size", 44);
			_expNext.Visible = false;
			_expArrow.Visible = false;
			_expCurBreak.Visible = false;
			_expNextBreak.Visible = false;
			return;
		}

		int playerPercent = GetPlayerExpBonusPercent();

		// 当前等级：玩家加成 + 器材加成
		int curEquipPercent = curCfg != null ? curCfg.ExpAdd : 0;
		_expCur.AddThemeColorOverride("font_color", ValueColor);
		_expCur.AddThemeFontSizeOverride("font_size", _expValueFontSize);
		_expCur.Text = $"{playerPercent + curEquipPercent}%";
		_expCurBreak.Visible = true;
		_expCurBreak.Text = $"玩家{playerPercent}% +\n器材{curEquipPercent}%";

		if (isMaxLevel)
		{
			_expNext.Visible = false;
			_expArrow.Visible = false;
			_expNextBreak.Visible = false;
			return;
		}

		int nextEquipPercent = nextCfg != null ? nextCfg.ExpAdd : 0;
		_expNext.Visible = true;
		_expArrow.Visible = true;
		_expNextBreak.Visible = true;
		_expNext.Text = $"{playerPercent + nextEquipPercent}%";
		_expNextBreak.Text = $"玩家{playerPercent}% +\n器材[color=#ffb35c]{nextEquipPercent}%[/color]";
	}

	// 玩家经验获取效率加成：智力加成 + 状态/天赋等效率加成
	private int GetPlayerExpBonusPercent()
	{
		if (PlayerManager.Instance == null) return 0;
		double bonus = PlayerManager.Instance.Intelligence * Consts.expPerIntellect
		             + (PlayerManager.Instance.Exp_acq_rate - 1.0) * 100.0;
		return (int)Math.Round(bonus);
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

	// 额外功能解锁：名称保持白色，状态文字显示已解锁/解锁等级
	private void RefreshFeatureUnlock()
	{
		RefreshFeature(_featureName1, _featureStatus1, "跑步机", RunningmachineUnlockLevel);
		RefreshFeature(_featureName2, _featureStatus2, "书柜", BookshelfUnlockLevel);
	}

	private void RefreshFeature(Label name, Label status, string featureName, int unlockLevel)
	{
		if (name == null || status == null) return;

		bool unlocked = PlayerManager.Instance.TrainLevel >= unlockLevel;
		name.Text = featureName;
		name.AddThemeColorOverride("font_color", unlocked ? Colors.White : LockedNameColor);
		status.Text = unlocked ? "已解锁" : $"Lv.{unlockLevel} 解锁";
		status.AddThemeColorOverride("font_color", unlocked ? new Color(0.4509804f, 0.7882353f, 0.9490196f, 1f) : LockedFunColor);
	}

	// 升级需求：图标 + 名称 + 拥有/需求
	private void RefreshRequirements(RoofTrain cfg, bool isUnlockStep, bool isMaxLevel)
	{
		if (isMaxLevel)
		{
			_maxTip.Visible = true;
			_reqTitle.Visible = false;
			_reqRow.Visible = false;
			if (upgrade != null)
			{
				upgrade.Disabled = true;
				upgrade.Material = _disabledActionMaterial;
			}
			if (_upgradeLabel != null) _upgradeLabel.Text = "已满级";
			return;
		}

		_maxTip.Visible = false;
		_reqTitle.Visible = true;
		_reqRow.Visible = true;
		if (upgrade != null)
		{
			upgrade.Disabled = false;
			upgrade.Material = null;
		}
		if (_upgradeLabel != null) _upgradeLabel.Text = isUnlockStep ? "解锁" : "升级";

		if (cfg == null) return;

		int slotCount = Mathf.Min(_reqSlots.Length, Mathf.Min(_reqIcons.Length, Mathf.Min(_reqNames.Length, _reqCounts.Length)));
		for (int i = 0; i < slotCount; i++)
		{
			int itemID = (cfg.ItemID != null && i < cfg.ItemID.Count) ? cfg.ItemID[i] : 0;
			int need = (cfg.ItemNum != null && i < cfg.ItemNum.Count) ? cfg.ItemNum[i] : 0;

			if (itemID == 0 || need == 0)
			{
				_reqSlots[i].Visible = false;
				continue;
			}

			_reqSlots[i].Visible = true;
			_reqNames[i].Text = GetItemName(itemID);

			var icon = GetRequirementIcon(itemID);
			if (icon != null) _reqIcons[i].Texture = icon;

			int have = GetItemHave(itemID);
			bool insufficient = have < need;
			_reqCounts[i].Text = $"{have}/{need}";
			_reqCounts[i].AddThemeColorOverride("font_color", insufficient ? InsufficientColor : ValueColor);
		}

		// 多余槽位隐藏
		for (int i = slotCount; i < _reqSlots.Length; i++)
		{
			_reqSlots[i].Visible = false;
		}
	}

	// 需求图标：体力类属性复用体力图标，其余走通用图标
	private Texture2D GetRequirementIcon(int itemID)
	{
		if (itemID == 10015 || itemID == 10016)
		{
			return GD.Load<Texture2D>("res://Assets/Images/UI/stamina.png");
		}
		return UIManager.Instance?.GetItemIcon(itemID);
	}

	private string GetItemName(int itemID)
	{
		if (itemID < 10000)
		{
			var itemDic = ConfigManager.Instance?.itemDic;
			return itemDic != null && itemDic.ContainsKey(itemID) ? itemDic[itemID].Name : "";
		}

		var effectDic = ConfigManager.Instance?.effectTypeDic;
		return effectDic != null && effectDic.ContainsKey(itemID) ? effectDic[itemID].Type : "";
	}

	private int GetItemHave(int itemID)
	{
		if (PlayerManager.Instance == null) return 0;

		if (itemID < 10000) return PlayerManager.Instance.GetItemCount(itemID);

		switch (itemID)
		{
			case 10001: return (int)PlayerManager.Instance.Hp;
			case 10002: return (int)PlayerManager.Instance.MaxHp;
			case 10003: return PlayerManager.Instance.Strength;
			case 10004: return PlayerManager.Instance.Agility;
			case 10005: return PlayerManager.Instance.Intelligence;
			case 10015: return PlayerManager.Instance.BaseStamina;
			case 10016: return PlayerManager.Instance.ExploreStamina;
			default: return 0;
		}
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

		// 校验所有材料是否充足
		if (cfg.ItemID != null && cfg.ItemNum != null)
		{
			for (int i = 0; i < cfg.ItemID.Count; i++)
			{
				int itemID = cfg.ItemID[i];
				int need = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
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
				int need = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
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
			PlayerManager.Instance.SetTrainLevel(currentLevel);
			GD.Print($"[TUpgradeUI] 升级成功，当前等级：{currentLevel}");
		}

		RefreshDisplay();
	}

	// 判断材料是否充足（支持物品ID和属性ID）
	private bool IsEnough(int itemID, int need)
	{
		if (need == 0) return true;
		return GetItemHave(itemID) >= need;
	}
}
