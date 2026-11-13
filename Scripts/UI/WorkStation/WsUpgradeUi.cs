using Godot;
using System;
using System.Linq;
using MyProject;

public partial class WsUpgradeUi : Control
{
	[Export] TextureButton upgrade;

	//额外功能解锁等级：做饭3级、拆解5级
	private const int CookUnlockLevel = 3;
	private const int DecomposeUnlockLevel = 5;
	private const int MaxTextureLevel = 5;
	private const int MaxLevel = 5;

	private static readonly Color LockedStatusColor = new(0.4862745f, 0.6196079f, 0.7215686f, 1f);
	private static readonly Color UnlockedStatusColor = new(0.66f, 0.7960784f, 0.8784314f, 1f);
	private static readonly Color LockedIconColor = new(0.45f, 0.53f, 0.6f, 1f);
	private static readonly Color NormalColor = new(1f, 1f, 1f, 1f);

	// 当前工作台等级
	private int currentLevel = 1;
	private int nextLevel = 2;

	private Label _lvCur;
	private Label _lvNext;
	private TextureRect _levelArrow;
	private Label _makeLevelValue;
	private Label _makeNextLevelValue;
	private TextureRect _makeArrow;
	private Label _roleBonusCur;
	private Label _stationBonusCur;
	private Label _roleBonusNext;
	private Label _stationBonusNext;
	private Label _maxTip;
	private Label[] _reqNames;
	private Label[] _reqCounts;
	private TextureRect[] _reqIcons;

	//额外功能展示
	private Label _cookDesc;
	private Label _decomposeDesc;
	private TextureRect _cookIcon;
	private TextureRect _decomposeIcon;
	private TextureRect _decomposeLock;
	//升级界面工作台预览图
	private TextureRect _wsTexture;

	public override void _Ready()
	{
		_lvCur = GetNode<Label>("LvCur");
		_lvNext = GetNode<Label>("LvNext");
		_levelArrow = GetNode<TextureRect>("LevelArrow");
		_makeLevelValue = GetNode<Label>("MakeLevelValue");
		_makeNextLevelValue = GetNode<Label>("MakeNextLevelValue");
		_makeArrow = GetNode<TextureRect>("MakeArrow");
		_roleBonusCur = GetNode<Label>("RoleBonusCur");
		_stationBonusCur = GetNode<Label>("StationBonusCur");
		_roleBonusNext = GetNode<Label>("RoleBonusNext");
		_stationBonusNext = GetNode<Label>("StationBonusNext");
		_maxTip = GetNode<Label>("maxTip");

		_reqIcons = new[]
		{
			GetNode<TextureRect>("Req1/Icon"),
			GetNode<TextureRect>("Req2/Icon"),
			GetNode<TextureRect>("Req3/Icon")
		};
		_reqNames = new[]
		{
			GetNode<Label>("Req1/Name"),
			GetNode<Label>("Req2/Name"),
			GetNode<Label>("Req3/Name")
		};
		_reqCounts = new[]
		{
			GetNode<Label>("Req1/Count"),
			GetNode<Label>("Req2/Count"),
			GetNode<Label>("Req3/Count")
		};

		//额外功能：厨房（等级3）与拆解（等级5）
		_cookDesc = GetNode<Label>("FunStatus1");
		_decomposeDesc = GetNode<Label>("FunStatus2");
		_cookIcon = GetNode<TextureRect>("FunIcon1");
		_decomposeIcon = GetNode<TextureRect>("FunIcon2");
		_decomposeLock = GetNode<TextureRect>("FunLock2");
		_wsTexture = GetNode<TextureRect>("WSTexture");

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
		//玩家制作等级
		int playerMakeLevel = PlayerManager.Instance.PlayerMakeLevel;
		int sumLevel = currentLevel + playerMakeLevel;

		RefreshExtraFun();
		RefreshWsTexture();

		bool isUnlockStep = currentLevel == 1 && !PlayerManager.Instance.WorkstationUnlocked;
		var cfg = isUnlockStep ? FindUpgradeConfig() : FindNextUpgradeConfig();

		RefreshLevelInfo(playerMakeLevel, sumLevel, isUnlockStep);
		RefreshRequirements(cfg, isUnlockStep);
	}

	//工作台等级与总制作等级展示
	private void RefreshLevelInfo(int playerMakeLevel, int sumLevel, bool isUnlockStep)
	{
		_lvCur.Text = $"Lv.{currentLevel}";
		bool hasNext = !isUnlockStep && currentLevel < MaxLevel;
		_lvNext.Visible = hasNext;
		_levelArrow.Visible = hasNext;
		if (hasNext) _lvNext.Text = $"Lv.{currentLevel + 1}";

		if (isUnlockStep)
		{
			_makeLevelValue.Text = "???";
			_makeLevelValue.AddThemeColorOverride("font_color", Colors.White);
			_makeNextLevelValue.Visible = false;
			_makeArrow.Visible = false;
			_roleBonusCur.Visible = false;
			_stationBonusCur.Visible = false;
			_roleBonusNext.Visible = false;
			_stationBonusNext.Visible = false;
			return;
		}

		_makeLevelValue.Text = $"{sumLevel}";
		_roleBonusCur.Text = $"角色{playerMakeLevel} +";
		_stationBonusCur.Text = $"工作台{currentLevel}";

		_makeNextLevelValue.Text = $"{sumLevel + 1}";
		_roleBonusNext.Text = $"角色{playerMakeLevel} +";
		_stationBonusNext.Text = $"工作台{currentLevel + 1}";
	}

	private void RefreshRequirements(RoofWorkstation reqCfg, bool isUnlockStep)
	{
		bool isMax = currentLevel >= MaxLevel && PlayerManager.Instance.WorkstationUnlocked;
		if (isMax || reqCfg == null)
		{
			for (int i = 0; i < _reqNames.Length; i++)
			{
				SetReqVisible(i, false);
			}
			_maxTip.Visible = true;
			if (upgrade != null)
			{
				upgrade.Disabled = true;
				upgrade.Modulate = new Color(1f, 1f, 1f, 0.5f);
				GetNode<Label>("UpgradeBtn/Label").Text = "已满级";
			}
			return;
		}

		_maxTip.Visible = false;
		if (upgrade != null)
		{
			upgrade.Disabled = false;
			upgrade.Modulate = NormalColor;
			GetNode<Label>("UpgradeBtn/Label").Text = isUnlockStep ? "解锁" : "升级";
		}

		// 动态填充三个需求槽位
		for (int i = 0; i < _reqNames.Length; i++)
		{
			int itemID = (reqCfg.ItemID != null && i < reqCfg.ItemID.Count) ? reqCfg.ItemID[i] : 0;
			int need = (reqCfg.ItemNum != null && i < reqCfg.ItemNum.Count) ? reqCfg.ItemNum[i] : 0;

			if (itemID == 0 || need == 0)
			{
				SetReqVisible(i, false);
				continue;
			}

			SetReqVisible(i, true);
			_reqNames[i].Text = GetItemName(itemID);

			var icon = UIManager.Instance?.GetItemIcon(itemID);
			if (icon != null) _reqIcons[i].Texture = icon;

			int have = GetItemHave(itemID);
			bool insufficient = have < need;
			_reqCounts[i].Text = $"{have}/{need}";
			//资源不足红色，充足白色
			_reqCounts[i].AddThemeColorOverride("font_color", insufficient ? Colors.Red : Colors.White);
		}
	}

	private void SetReqVisible(int index, bool visible)
	{
		_reqIcons[index].Visible = visible;
		_reqNames[index].Visible = visible;
		_reqCounts[index].Visible = visible;
	}

	private string GetItemName(int itemID)
	{
		if (itemID < 10000)
		{
			var itemDic = ConfigManager.Instance?.itemDic;
			return itemDic != null && itemDic.ContainsKey(itemID) ? itemDic[itemID].Name : "";
		}

		if (itemID == 10015) return "体力";

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

	//额外功能展示：等级不足显示锁图标且状态灰色，解锁后高亮并隐藏锁图标
	private void RefreshExtraFun()
	{
		bool cookUnlocked = PlayerManager.Instance.WorkStationLevel >= CookUnlockLevel;
		bool decomposeUnlocked = PlayerManager.Instance.WorkStationLevel >= DecomposeUnlockLevel;

		_cookDesc.Text = cookUnlocked ? "已解锁" : $"Lv.{CookUnlockLevel}解锁";
		_cookDesc.AddThemeColorOverride("font_color", cookUnlocked ? UnlockedStatusColor : LockedStatusColor);
		if (_cookIcon != null) _cookIcon.SelfModulate = cookUnlocked ? NormalColor : LockedIconColor;

		_decomposeDesc.Text = decomposeUnlocked ? "已解锁" : $"Lv.{DecomposeUnlockLevel}解锁";
		_decomposeDesc.AddThemeColorOverride("font_color", decomposeUnlocked ? UnlockedStatusColor : LockedStatusColor);
		if (_decomposeIcon != null) _decomposeIcon.SelfModulate = decomposeUnlocked ? NormalColor : LockedIconColor;
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
				int need = i < cfg.ItemNum.Count ? cfg.ItemNum[i] : 0;
				if (GetItemHave(itemID) < need)
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
}
