using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MyProject;

public partial class WSMakeUI : Control
{
	[Export] TextureButton make;
	[Export] TextureButton cook;
	[Export] TextureButton disassemb;

	private const int StaminaPropID = 10015;
	//功能解锁等级：做饭3级、拆解5级
	private const int CookUnlockLevel = 3;
	private const int DecomposeUnlockLevel = 5;
	//拆解列表固定行数
	private const int DecomposeRowCount = 5;

	//不可用行灰色
	private static readonly Color DimColor = new(0.45f, 0.52f, 0.6f, 1f);
	//数量不足红色
	private static readonly Color InsufficientColor = new(0.94f, 0.27f, 0.24f, 1f);
	private static readonly Color NormalColor = new(1f, 1f, 1f, 1f);

	private enum TabType { Make = 0, Cook = 1, Decompose = 2 }
	private TabType currentTab = TabType.Make;

	private Texture2D _tabActive;
	private Texture2D _tabInactive;
	private Texture2D _staminaTex;

	private VBoxContainer _recipeList;
	private TextureButton _rowTemplate;
	private Label _infoValue;
	private Label _infoDetail;
	private TextureRect _decompLock;
	private Label _decompLv;

	private Control _detailInfo;
	private Label _detailTip;
	private TextureRect _detailIcon;
	private Label _detailName;
	private TextureButton _actionBtn;
	private Label _actionLabel;

	//当前选中的配置行
	private RoofWorkstationItem _selectedCfg;
	private readonly List<TextureButton> _rows = new();
	private readonly List<RoofWorkstationItem> _rowCfgs = new();

	public override void _Ready()
	{
		_tabActive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/building/tab_selected.png");
		_tabInactive = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/building/tab_unselected.png");
		_staminaTex = ResourceLoader.Load<Texture2D>("res://Assets/Images/UI/stamina.png");

		_infoValue = GetNode<Label>("InfoBar/InfoValue");
		_infoDetail = GetNode<Label>("InfoBar/InfoDetail");
		_decompLock = GetNode<TextureRect>("SubTabBar/TabDecompose/Content/Lock");
		_decompLv = GetNode<Label>("SubTabBar/TabDecompose/Content/Lv");

		_detailInfo = GetNode<Control>("DetailCard/DetailInfo");
		_detailTip = GetNode<Label>("DetailCard/DetailTip");
		_detailIcon = GetNode<TextureRect>("DetailCard/DetailInfo/DetailIconBox/Icon");
		_detailName = GetNode<Label>("DetailCard/DetailInfo/DetailName");
		_actionBtn = GetNode<TextureButton>("DetailCard/ActionBtn");
		_actionLabel = GetNode<Label>("DetailCard/ActionBtn/Label");

		_recipeList = GetNode<VBoxContainer>("RecipeScroll/RecipeList");
		_rowTemplate = GetNode<TextureButton>("RecipeScroll/RecipeList/RowTemplate");
		_recipeList.RemoveChild(_rowTemplate);

		if (make != null) make.Pressed += () => SwitchTab(TabType.Make);
		if (cook != null) cook.Pressed += () => SwitchTab(TabType.Cook);
		if (disassemb != null) disassemb.Pressed += () => SwitchTab(TabType.Decompose);
		if (_actionBtn != null) _actionBtn.Pressed += OnActionPressed;

		//点击空白处清空选中
		var recipeScroll = GetNode<ScrollContainer>("RecipeScroll");
		recipeScroll.GuiInput += OnScrollGuiInput;

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
		if (ConfigManager.Instance == null)
		{
			GD.PrintErr("[WSMakeUI] ConfigManager 未就绪");
			return;
		}
		if (PlayerManager.Instance != null)
		{
			// 玩家物品变化时刷新行的可用状态
			PlayerManager.Instance.GetItem += OnPlayerDataChanged;
		}
		RefreshTabLocks();
		//当前页签未解锁时回到制作页
		if (!IsTabUnlocked(currentTab)) currentTab = TabType.Make;
		SwitchTab(currentTab);
	}

	private void OnPlayerDataChanged()
	{
		CallDeferred(nameof(RefreshInfo));
		CallDeferred(nameof(RefreshTabLocks));
		CallDeferred(nameof(RefreshRows));
		CallDeferred(nameof(RefreshActionBtn));
	}

	//判断子页签是否解锁
	private bool IsTabUnlocked(TabType tab)
	{
		if (PlayerManager.Instance == null)
		{
			return tab == TabType.Make;
		}
		int stationLevel = PlayerManager.Instance.WorkStationLevel;
		switch (tab)
		{
			case TabType.Cook:
				return stationLevel >= CookUnlockLevel;
			case TabType.Decompose:
				return stationLevel >= DecomposeUnlockLevel;
			default:
				return true;
		}
	}

	//顶部总制作等级信息
	private void RefreshInfo()
	{
		if (PlayerManager.Instance == null) return;
		int stationLevel = PlayerManager.Instance.WorkStationLevel;
		int roleLevel = PlayerManager.Instance.PlayerMakeLevel;
		_infoValue.Text = $"{stationLevel + roleLevel}";
		_infoDetail.Text = $"|   角色 {roleLevel} + 工作台 {stationLevel}";
	}

	private void RefreshTabLocks()
	{
		bool cookUnlocked = IsTabUnlocked(TabType.Cook);
		bool decomoseUnlocked = IsTabUnlocked(TabType.Decompose);
		if (cook != null) cook.Modulate = cookUnlocked ? NormalColor : DimColor;
		if (disassemb != null) disassemb.Modulate = decomoseUnlocked ? NormalColor : DimColor;
		if (_decompLock != null) _decompLock.Visible = !decomoseUnlocked;
		if (_decompLv != null)
		{
			_decompLv.Visible = !decomoseUnlocked;
			_decompLv.Text = $"Lv.{DecomposeUnlockLevel}";
		}
	}

	//切换页签
	private void SwitchTab(TabType tab)
	{
		if (!IsTabUnlocked(tab))
		{
			UIManager.Instance.ShowCommonTips2("工作台等级不足");
			return;
		}
		currentTab = tab;
		if (make != null)      make.TextureNormal      = tab == TabType.Make ? _tabActive : _tabInactive;
		if (cook != null)      cook.TextureNormal      = tab == TabType.Cook ? _tabActive : _tabInactive;
		if (disassemb != null) disassemb.TextureNormal = tab == TabType.Decompose ? _tabActive : _tabInactive;

		ClearSelection();
		RefreshInfo();
		RefreshTabLocks();
		RefreshRows();
	}

	//按当前页签筛选配置并重建物品列表
	private void RefreshRows()
	{
		if (ConfigManager.Instance == null) return;
		var all = ConfigManager.Instance.roofWorkstationItemList;
		if (all == null) return;

		//清空旧行
		foreach (var child in _recipeList.GetChildren())
		{
			_recipeList.RemoveChild(child);
			child.QueueFree();
		}
		_rows.Clear();
		_rowCfgs.Clear();

		IEnumerable<RoofWorkstationItem> rows = currentTab switch
		{
			TabType.Make      => all.Where(r => r.Type == 1),
			TabType.Cook      => all.Where(r => r.Type == 2),
			TabType.Decompose => all.Where(r => r.GetID != null && r.GetID.Count > 0),
			_ => all,
		};

		foreach (var cfg in rows)
		{
			var row = (TextureButton)_rowTemplate.Duplicate();
			_recipeList.AddChild(row);
			FillRow(row, cfg);
			row.Pressed += () => SelectRow(cfg);
			_rows.Add(row);
			_rowCfgs.Add(cfg);
		}

		//拆解列表补足空行
		if (currentTab == TabType.Decompose)
		{
			while (_rows.Count < DecomposeRowCount)
			{
				var row = (TextureButton)_rowTemplate.Duplicate();
				_recipeList.AddChild(row);
				FillEmptyRow(row);
				_rows.Add(row);
				_rowCfgs.Add(null);
			}
		}

		RefreshSelectionMark();
	}

	//填充整行
	private void FillRow(TextureButton row, RoofWorkstationItem cfg)
	{
		row.Disabled = false;
		row.Modulate = NormalColor;

		var iconBox = row.GetNode<Panel>("IconBox");
		var icon = row.GetNode<TextureRect>("IconBox/Icon");
		var nameLabel = row.GetNode<Label>("Name");
		var sep = row.GetNode<ColorRect>("Sep");
		var arrow = row.GetNode<Label>("Arrow");
		var lockTip = row.GetNode<HBoxContainer>("LockTip");
		var owned = row.GetNode<Panel>("Owned");

		row.GetNode<Panel>("SelectMark").Visible = false;
		iconBox.Visible = true;
		sep.Visible = false;
		arrow.Visible = false;
		lockTip.Visible = false;
		owned.Visible = false;

		//物品图标与名称
		var tex = UIManager.Instance?.GetItemIcon(cfg.ItemID);
		if (tex != null) icon.Texture = tex;
		nameLabel.Text = GetItemName(cfg.ItemID);

		if (currentTab == TabType.Decompose)
		{
			//拆解：物品 → 产出材料，右侧显示拥有数量
			arrow.Visible = true;
			sep.Visible = true;
			sep.OffsetLeft = 468;
			sep.OffsetRight = 471;

			var getIDs = cfg.GetID ?? new List<int>();
			var getNums = cfg.GetNum ?? new List<int>();
			for (int i = 0; i < 2; i++)
			{
				bool has = i < getIDs.Count;
				int num = (has && i < getNums.Count) ? getNums[i] : 0;
				SetSlot(row, i, has, has ? UIManager.Instance?.GetItemIcon(getIDs[i]) : null, $"×{num}", NormalColor);
			}
			SetSlot(row, 2, false, null, "", NormalColor);

			int have = PlayerManager.Instance?.GetItemCount(cfg.ItemID) ?? 0;
			owned.Visible = true;
			owned.GetNode<Label>("Label").Text = $"拥有×{have}";
			if (have <= 0) row.Modulate = DimColor;
			return;
		}

		//制作/做饭：等级不足显示需求等级
		int totalMakeLevel = (PlayerManager.Instance?.WorkStationLevel ?? 1) + (PlayerManager.Instance?.PlayerMakeLevel ?? 0);
		if (cfg.Level > totalMakeLevel)
		{
			lockTip.Visible = true;
			lockTip.GetNode<Label>("Num").Text = $"{cfg.Level}";
			SetSlot(row, 0, false, null, "", NormalColor);
			SetSlot(row, 1, false, null, "", NormalColor);
			SetSlot(row, 2, false, null, "", NormalColor);
			row.Modulate = DimColor;
			return;
		}

		sep.Visible = true;

		//材料
		var mats = cfg.MaterialID ?? new List<int>();
		var matNums = cfg.MaterialNum ?? new List<int>();
		for (int i = 0; i < 2; i++)
		{
			if (i >= mats.Count)
			{
				SetSlot(row, i, false, null, "", NormalColor);
				continue;
			}
			int need = i < matNums.Count ? matNums[i] : 0;
			bool enough = (PlayerManager.Instance?.GetItemCount(mats[i]) ?? 0) >= need;
			SetSlot(row, i, true, UIManager.Instance?.GetItemIcon(mats[i]), $"×{need}", enough ? NormalColor : InsufficientColor);
		}

		//体力
		int stamina = PlayerManager.Instance?.BaseStamina ?? 0;
		SetSlot(row, 2, true, _staminaTex, $"×{cfg.Stamina}", stamina >= cfg.Stamina ? NormalColor : InsufficientColor);
	}

	//填充一个材料槽位
	private void SetSlot(TextureButton row, int index, bool visible, Texture2D tex, string numText, Color numColor)
	{
		var slot = row.GetNode<Control>($"Slot{index + 1}");
		slot.Visible = visible;
		if (!visible) return;

		var icon = slot.GetNode<TextureRect>("Icon");
		var count = slot.GetNode<Label>("Count");
		if (tex != null) icon.Texture = tex;
		count.Text = numText;
		count.AddThemeColorOverride("font_color", numColor);
	}

	//拆解列表的空行
	private void FillEmptyRow(TextureButton row)
	{
		row.Disabled = true;
		row.GetNode<Panel>("SelectMark").Visible = false;
		row.GetNode<Panel>("IconBox").Visible = false;
		row.GetNode<Label>("Name").Visible = false;
		row.GetNode<ColorRect>("Sep").Visible = false;
		row.GetNode<Label>("Arrow").Visible = false;
		row.GetNode<HBoxContainer>("LockTip").Visible = false;
		row.GetNode<Panel>("Owned").Visible = false;
		row.GetNode<Control>("Slot1").Visible = false;
		row.GetNode<Control>("Slot2").Visible = false;
		row.GetNode<Control>("Slot3").Visible = false;
	}

	//判断一行当前是否可执行
	private bool IsRowAvailable(RoofWorkstationItem cfg)
	{
		if (cfg == null || PlayerManager.Instance == null) return false;

		//等级校验
		if (cfg.Level > PlayerManager.Instance.WorkStationLevel + PlayerManager.Instance.PlayerMakeLevel) return false;

		if (currentTab == TabType.Decompose)
		{
			//拆解需要持有目标物品
			if (PlayerManager.Instance.GetItemCount(cfg.ItemID) <= 0) return false;
		}
		else
		{
			//制作/做饭校验全部材料
			if (cfg.MaterialID != null)
			{
				for (int i = 0; i < cfg.MaterialID.Count; i++)
				{
					int need = (cfg.MaterialNum != null && i < cfg.MaterialNum.Count) ? cfg.MaterialNum[i] : 0;
					if (PlayerManager.Instance.GetItemCount(cfg.MaterialID[i]) < need) return false;
				}
			}
		}

		//体力校验
		if (PlayerManager.Instance.BaseStamina < cfg.Stamina) return false;

		return true;
	}

	//点击行，物品显示到详情区
	private void SelectRow(RoofWorkstationItem cfg)
	{
		_selectedCfg = cfg;

		_detailTip.Visible = false;
		_detailInfo.Visible = true;

		var tex = UIManager.Instance?.GetItemIcon(cfg.ItemID);
		if (tex != null) _detailIcon.Texture = tex;
		_detailName.Text = GetItemName(cfg.ItemID);
		_actionLabel.Text = currentTab == TabType.Decompose ? "拆解" : "制作";

		RefreshActionBtn();
		RefreshSelectionMark();
	}

	//刷新动作按钮：条件不足时视觉变暗
	private void RefreshActionBtn()
	{
		if (_actionBtn == null) return;
		bool enabled = _selectedCfg != null && IsRowAvailable(_selectedCfg);
		_actionBtn.Disabled = !enabled;
		_actionBtn.Modulate = enabled ? NormalColor : new Color(1f, 1f, 1f, 0.45f);
	}

	//刷新行选中高亮
	private void RefreshSelectionMark()
	{
		for (int i = 0; i < _rows.Count; i++)
		{
			var row = _rows[i];
			if (row == null || !IsInstanceValid(row)) continue;
			var mark = row.GetNodeOrNull<Panel>("SelectMark");
			if (mark != null) mark.Visible = _rowCfgs[i] != null && _rowCfgs[i] == _selectedCfg;
		}
	}

	private void ClearSelection()
	{
		_selectedCfg = null;
		if (_detailTip != null) _detailTip.Visible = true;
		if (_detailInfo != null) _detailInfo.Visible = false;
		if (_actionBtn != null)
		{
			_actionBtn.Disabled = true;
			_actionBtn.Modulate = new Color(1f, 1f, 1f, 0.45f);
		}
		RefreshSelectionMark();
	}

	private void OnScrollGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb
			&& mb.ButtonIndex == MouseButton.Left
			&& mb.Pressed
			&& !mb.Canceled)
		{
			ClearSelection();
		}
	}

	//点击制作/拆解按钮
	private void OnActionPressed()
	{
		if (_selectedCfg == null) return;
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;
		var pm = PlayerManager.Instance;
		var cfg = _selectedCfg;

		//等级校验
		if (cfg.Level > pm.WorkStationLevel + pm.PlayerMakeLevel)
		{
			UIManager.Instance.ShowCommonTips2("制作等级不足");
			return;
		}

		//材料/持有物校验
		if (currentTab == TabType.Decompose)
		{
			if (pm.GetItemCount(cfg.ItemID) <= 0)
			{
				UIManager.Instance.ShowCommonTips2($"未持有{GetItemName(cfg.ItemID)}，无法拆解");
				return;
			}
		}
		else
		{
			if (cfg.MaterialID != null)
			{
				for (int i = 0; i < cfg.MaterialID.Count; i++)
				{
					int need = (cfg.MaterialNum != null && i < cfg.MaterialNum.Count) ? cfg.MaterialNum[i] : 0;
					if (pm.GetItemCount(cfg.MaterialID[i]) < need)
					{
						UIManager.Instance.ShowCommonTips2("制作材料不足");
						return;
					}
				}
			}
		}

		//体力校验
		if (pm.BaseStamina < cfg.Stamina)
		{
			UIManager.Instance.ShowCommonTips2("体力不足");
			return;
		}

		bool decompose = currentTab == TabType.Decompose;

		// 扣体力
		if (cfg.Stamina > 0)
		{
			PlayerManager.Instance.AddItem(StaminaPropID, -cfg.Stamina);
		}

		if (decompose)
		{
			// 拆解
			PlayerManager.Instance.RemoveItem(cfg.ItemID, 1);
			if (cfg.GetID != null)
			{
				for (int i = 0; i < cfg.GetID.Count; i++)
				{
					int n = (cfg.GetNum != null && i < cfg.GetNum.Count) ? cfg.GetNum[i] : 0;
					if (n > 0) PlayerManager.Instance.AddItem(cfg.GetID[i], n);
				}
			}
		}
		else
		{
			// 扣材料，产出物品
			if (cfg.MaterialID != null)
			{
				for (int i = 0; i < cfg.MaterialID.Count; i++)
				{
					int need = (cfg.MaterialNum != null && i < cfg.MaterialNum.Count) ? cfg.MaterialNum[i] : 0;
					if (need > 0) PlayerManager.Instance.RemoveItem(cfg.MaterialID[i], need);
				}
			}
			PlayerManager.Instance.AddItem(cfg.ItemID, 1);
		}

		GD.Print($"[WSMakeUI] {(decompose ? "拆解" : "制作")}：{GetItemName(cfg.ItemID)} 完成");

		//刷新列表与按钮状态
		CallDeferred(nameof(RefreshInfo));
		CallDeferred(nameof(RefreshRows));
		CallDeferred(nameof(RefreshActionBtn));
	}

	private string GetItemName(int itemID)
	{
		var itemDic = ConfigManager.Instance?.itemDic;
		if (itemDic != null && itemDic.ContainsKey(itemID))
		{
			return itemDic[itemID].Name;
		}
		return $"ID{itemID}";
	}
}
