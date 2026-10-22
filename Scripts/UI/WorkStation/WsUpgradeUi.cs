using Godot;
using System;
using System.Linq;
using MyProject;

public partial class WsUpgradeUi : Control
{
	[Export] Button upgrade;

	//额外功能解锁等级：做饭3级、拆解5级
	private const int CookUnlockLevel = 3;
	private const int DecomposeUnlockLevel = 5;
	//未解锁功能名称灰色（与场景默认灰色一致）
	private static readonly Color LockedFunColor = new(0.5958281f, 0.5958281f, 0.5958281f, 1f);
	private const int MaxTextureLevel = 5;
	private const int MaxLevel = 5;


	// 当前工作台等级
	private int currentLevel = 1;
	private int nextLevel = 2;


	private Label _gradeTitle;
	private Label _makeLevelValue;
	private Label _makeNextLevelValue;
	private Label _maxTip;
	private Label[] _reqLabels;
	private TextureRect[] _reqIcons;
	//额外功能展示
	private Label _cookDesc;
	private Label _decomposeDesc;
	private TextureRect _cookLock;
	private TextureRect _decomposeLock;
	//升级界面工作台预览图
	private TextureRect _wsTexture;

	public override void _Ready()
	{
		_gradeTitle     = GetNode<Label>("GradeTitle");
		_makeLevelValue = GetNode<Label>("MakeLevel/MakeLevelValue");
		_maxTip=GetNode<Label>("UpgradeRequ/maxTip");
		_makeNextLevelValue = GetNode<Label>("MakeLevel/MakeNextLevelValue");
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
		//额外功能：厨房（等级3）与拆解（等级5），Lock2在厨房行、Lock在拆解行
		_cookDesc      = GetNode<Label>("ExtraFun/ExtraFunDescription");
		_decomposeDesc = GetNode<Label>("ExtraFun/ExtraFunDescription2");
		_cookLock      = GetNode<TextureRect>("ExtraFun/Lock2");
		_decomposeLock = GetNode<TextureRect>("ExtraFun/Lock");
		_wsTexture     = GetNode<TextureRect>("WSTexture");

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
			GD.PrintErr("[WsUpgradeUi] PlayerManager 或 ConfigManager 未就绪");
			return;
		}
		PlayerManager.Instance.GetItem += OnPlayerDataChanged;
		RefreshDisplay();
	}

	private void OnPlayerDataChanged()
	{
		// 玩家数据变化时刷新需求显示
		CallDeferred(nameof(RefreshDisplay));
	}

	
	private RoofWorkstation FindUpgradeConfig()
	{
		var dic = ConfigManager.Instance?.roofWorkstationDic;
		if (dic == null) return null;
		return dic.ContainsKey(currentLevel) ? dic[currentLevel] : null;
	}

	private RoofWorkstation FindNextUpgradeConfig()
	{
		var dic = ConfigManager.Instance?.roofWorkstationDic;
		if (dic == null) return null;
		return dic.ContainsKey(currentLevel + 1) ? dic[currentLevel + 1] : null;
	}

	private void RefreshDisplay()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

		//工作台实际等级
		currentLevel = PlayerManager.Instance.WorkStationLevel;
		nextLevel = currentLevel + 1;
		//玩家等级
		int playerMakeLevel =PlayerManager.Instance.PlayerMakeLevel;
		// 制作等级
		int sumLevel=currentLevel+playerMakeLevel;
		//额外功能解锁状态展示
		RefreshExtraFun();
		//根据等级切换工作台预览图
		RefreshWsTexture();
		
		string wsLevelText = $"等级{currentLevel}";
		string levelText = $"等级{sumLevel}";
		string nextText = $"等级{sumLevel+1}";
		if (_gradeTitle != null) _gradeTitle.Text = wsLevelText;

		// 等级1未解锁：查当前等级配置；已解锁或等级>1：查下一等级配置
		bool isUnlockStep = currentLevel == 1 && !PlayerManager.Instance.WorkstationUnlocked;
		var cfg = isUnlockStep ? FindUpgradeConfig() : FindNextUpgradeConfig();

		// 制作等级数值显示：三种状态
		if (isUnlockStep)
		{
			// 解锁前：不显示具体数值
			if (_makeLevelValue != null)
			{
				_makeLevelValue.Visible = true;
				_makeLevelValue.Text = "???";
			}
			if (_makeNextLevelValue != null)
			{
				_makeNextLevelValue.Visible = false;
				_makeNextLevelValue.Text = "???";
			}
		}
		else
		{
			// 解锁后：显示当前数值
			if (_makeLevelValue != null) _makeLevelValue.Text = levelText;

			if (currentLevel >= MaxLevel && PlayerManager.Instance.WorkstationUnlocked)
			{
				// 满级：不显示下一级预览
				if (_makeNextLevelValue != null) _makeNextLevelValue.Visible = false;
			}
			else
			{
				// 正常升级：显示下一级预览
				if (_makeNextLevelValue != null)
				{
					_makeNextLevelValue.Visible = true;
					_makeNextLevelValue.Text = nextText;
				}
			}
		}

		if (currentLevel >= MaxLevel && PlayerManager.Instance.WorkstationUnlocked)
		{
			// 已达最高等级：清空需求并禁用按钮
			for (int i = 0; i < _reqLabels.Length; i++)
			{
				_reqIcons[i].Visible=false;
				_reqLabels[i].Visible=false;
			}
			if (upgrade != null) upgrade.Icon = GD.Load<Texture2D>("res://Assets/Images/UI/button_cancel.png");
			_maxTip.Visible=true;
			return;
		}

		if (upgrade != null)
		{
			upgrade.Disabled = false;
			upgrade.Text = (currentLevel == 1 && !PlayerManager.Instance.WorkstationUnlocked) ? "解锁" : "升级";
		}

		// 动态填充三个需求槽位
		int slotCount = Mathf.Min(_reqLabels.Length, _reqIcons.Length);
		for (int i = 0; i < slotCount; i++)
		{
			if (_reqLabels[i] == null || _reqIcons[i] == null) continue;

			int itemID = (cfg.ItemID != null && i < cfg.ItemID.Count) ? cfg.ItemID[i] : 0;
			int need   = (cfg.ItemNum != null && i < cfg.ItemNum.Count) ? cfg.ItemNum[i] : 0;

			if (itemID == 0 || need == 0)
			{
				_reqIcons[i].Visible = false;
				_reqLabels[i].Visible = false;
				continue;
			}

			_reqIcons[i].Visible = true;
			_reqLabels[i].Visible = true;

			if (UIManager.Instance != null && itemID < 10000)
			{
				var icon = UIManager.Instance.GetItemIcon(itemID);
				if (icon != null) _reqIcons[i].Texture = icon;
			}
			else if (itemID == 10015)
			{
				var staminaIcon = GD.Load<Texture2D>("res://Assets/Images/UI/stamina.png");
				if (staminaIcon != null) _reqIcons[i].Texture = staminaIcon;
			}

			bool insufficient = !IsEnough(itemID, need);
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
		if (_cookDesc == null || _decomposeDesc == null) return;

		bool cookUnlocked = PlayerManager.Instance.WorkStationLevel >= CookUnlockLevel;
		bool decomposeUnlocked = PlayerManager.Instance.WorkStationLevel >= DecomposeUnlockLevel;

		_cookDesc.AddThemeColorOverride("font_color", cookUnlocked ? Colors.White : LockedFunColor);
		if (_cookLock != null) _cookLock.Visible = !cookUnlocked;

		_decomposeDesc.AddThemeColorOverride("font_color", decomposeUnlocked ? Colors.White : LockedFunColor);
		if (_decomposeLock != null) _decomposeLock.Visible = !decomposeUnlocked;
	}

	//根据等级切换图
	private void RefreshWsTexture()
	{
		if (_wsTexture == null || PlayerManager.Instance == null) return;

		int level = Mathf.Clamp(PlayerManager.Instance.WorkStationLevel, 1, MaxTextureLevel);
		var tex = GD.Load<Texture2D>($"res://Assets/Images/Base/workspace_{level}.png");
		if (tex != null)
		{
			_wsTexture.Texture = tex;
		}
	}

	private void OnUpgradePressed()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

		bool isUnlock = currentLevel == 1 && !PlayerManager.Instance.WorkstationUnlocked;

		var cfg = isUnlock ? FindUpgradeConfig() : FindNextUpgradeConfig();
		if (cfg == null)
		{	
			UIManager.Instance.ShowCommonTips2("工作台已达最高等级，无法升级");
			return;
		}

		//校验所有材料是否充足
		if (cfg.ItemID != null && cfg.ItemNum != null)
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
			PlayerManager.Instance.SetWorkstationUnlocked(true);
			GD.Print("[WsUpgradeUi] 工作台已解锁");
		}
		else
		{
			currentLevel++;
			nextLevel++;
			GD.Print($"[WsUpgradeUi] 升级成功，当前等级：{currentLevel}");
			PlayerManager.Instance.SetWorkStationLevel(currentLevel);
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
