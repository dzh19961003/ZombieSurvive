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
		var list = ConfigManager.Instance?.roofWorkstationList;
		if (list == null) return null;
		return list.FirstOrDefault(r => r.Level == currentLevel);
	}

	private void RefreshDisplay()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

		//以PlayerManager中的真实等级为准
		currentLevel = PlayerManager.Instance.WorkStationLevel;
		nextLevel = currentLevel + 1;

		//额外功能解锁状态展示
		RefreshExtraFun();

		// 等级标题
	
		string levelText = $"等级{currentLevel}";
		string nextText = $"等级{nextLevel}";
		if (_gradeTitle != null) _gradeTitle.Text = levelText;
		if (_makeLevelValue != null) _makeLevelValue.Text = levelText;
		if (_makeNextLevelValue != null) _makeNextLevelValue.Text = nextText;
		var cfg = FindUpgradeConfig();
		if (currentLevel==5)
		{
			_makeNextLevelValue.Visible=false;
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

		if (upgrade != null) upgrade.Disabled = false;

		// 动态填充三个需求槽位
		int slotCount = Mathf.Min(_reqLabels.Length, _reqIcons.Length);
		for (int i = 0; i < slotCount; i++)
		{
			if (_reqLabels[i] == null || _reqIcons[i] == null) continue;

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
		if (_cookDesc == null || _decomposeDesc == null) return;

		bool cookUnlocked = PlayerManager.Instance.WorkStationLevel >= CookUnlockLevel;
		bool decomposeUnlocked = PlayerManager.Instance.WorkStationLevel >= DecomposeUnlockLevel;

		_cookDesc.AddThemeColorOverride("font_color", cookUnlocked ? Colors.White : LockedFunColor);
		if (_cookLock != null) _cookLock.Visible = !cookUnlocked;

		_decomposeDesc.AddThemeColorOverride("font_color", decomposeUnlocked ? Colors.White : LockedFunColor);
		if (_decomposeLock != null) _decomposeLock.Visible = !decomposeUnlocked;
	}

	private void OnUpgradePressed()
	{
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;

		var cfg = FindUpgradeConfig();
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
					// 物品：走 RemoveItem（钳制到 0，触发 GetItem 事件）

					
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
					// 统一走 AddItem扣除
					PlayerManager.Instance.AddItem(10015, -need);
					break; // 体力只扣一次
				}
			}
		}

		currentLevel++;
		nextLevel++;
		GD.Print($"[WsUpgradeUi] 升级成功，当前等级：{currentLevel}");
		PlayerManager.Instance.SetWorkStationLevel(currentLevel);

		RefreshDisplay();
	}

	// 判断 itemID:need 这一项是否充足（物品 / 属性统一入口）
	// 判断材料充足
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
