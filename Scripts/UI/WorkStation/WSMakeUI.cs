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
	// 不可用行灰色
	private static readonly Color DimColor = new(0.27752772f, 0.27752772f, 0.27752772f, 1f);
	private enum TabType { Make = 0, Cook = 1, Decompose = 2 }
	private TabType currentTab = TabType.Make;

	private VBoxContainer _recipeList;
	private TextureButton _rowTemplate;
	private Control _selectedArea;
	private Label _selectedTip;
	private Control _selectedInfo;
	private TextureRect _selectedIcon;
	private Label _selectedName;
	private Label _selectedDesc;
	private Button _actionBtn;

	// 当前选中的配置行
	private RoofWorkstationItem _selectedCfg;

	public override void _Ready()
	{
		_recipeList  = GetNode<VBoxContainer>("RecipeScroll/RecipeList");
		_selectedArea = GetNode<Control>("SelecedArea");
		_selectedTip  = GetNode<Label>("SelecedArea/Label");
		_selectedInfo = GetNode<Control>("SelecedArea/SelecedInfo");
		_selectedIcon = GetNode<TextureRect>("SelecedArea/SelecedInfo/Icon");
		_selectedName = GetNode<Label>("SelecedArea/SelecedInfo/InfoCol/NameLabel");
		_selectedDesc = GetNode<Label>("SelecedArea/SelecedInfo/InfoCol/DescLabel");
		_actionBtn    = GetNode<Button>("SelecedArea/ActionBtn");
		
		_rowTemplate = GetNode<TextureButton>("RecipeScroll/RecipeList/Row1");
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
		SwitchTab(currentTab);
	}

	private void OnPlayerDataChanged()
	{
		CallDeferred(nameof(RefreshRows));
		CallDeferred(nameof(RefreshActionBtn));
	}

	//切换页签
	private void SwitchTab(TabType tab)
	{
		currentTab = tab;
		if (make != null)      make.TextureNormal      = TabTexture(tab == TabType.Make);
		if (cook != null)      cook.TextureNormal      = TabTexture(tab == TabType.Cook);
		if (disassemb != null) disassemb.TextureNormal = TabTexture(tab == TabType.Decompose);
		RefreshRows();
		//切页签清空选中信息
		ClearSelection();
	}

	private Texture2D TabTexture(bool active)
	{
		return ResourceLoader.Load<Texture2D>(active
			? "res://Assets/Images/UI/tab_active.png"
			: "res://Assets/Images/UI/tab_inactive.png");
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
			child.QueueFree();
		}

		
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
		}
	}

	//填充
	private void FillRow(TextureButton row, RoofWorkstationItem cfg)
	{
		if (ConfigManager.Instance?.itemDic == null) return;

		//物品图标与名称
		var itemIcon = row.GetNode<TextureRect>("Row1Inner/Item/ItemIcon");
		var itemName = row.GetNode<Label>("Row1Inner/Item/ItemName");
		if (ConfigManager.Instance.itemDic.ContainsKey(cfg.ItemID))
		{
			var item = ConfigManager.Instance.itemDic[cfg.ItemID];
			if (UIManager.Instance != null)
			{
				var tex = UIManager.Instance.GetItemIcon(cfg.ItemID);
				if (tex != null) itemIcon.Texture = tex;
			}
			itemName.Text = item.Name;
		}

		//消耗列表
		List<int> costIDs, costNums;
		if (currentTab == TabType.Decompose)
		{
			costIDs = cfg.GetID ?? new List<int>();
			costNums = cfg.GetNum ?? new List<int>();
		}
		else
		{
			costIDs = cfg.MaterialID ?? new List<int>();
			costNums = cfg.MaterialNum ?? new List<int>();
		}
		FillCostSlot(row, "Row1Inner/CostList/Recipe1", costIDs, costNums, 0);
		FillCostSlot(row, "Row1Inner/CostList/Recipe2", costIDs, costNums, 1);

		//体力消耗
		var stamNum = row.GetNode<Label>("Row1Inner/Stamina/Num");
		stamNum.Text = $"×{cfg.Stamina}";

		//可用状态
		row.Modulate = IsRowAvailable(cfg) ? Colors.White : DimColor;
	}

	//填充一个消耗槽位（index 超出配置时隐藏该槽位）
	private void FillCostSlot(TextureButton row, string path, List<int> ids, List<int> nums, int index)
	{
		var slot = row.GetNode<NinePatchRect>(path);
		if (index >= ids.Count)
		{
			slot.Visible = false;
			return;
		}
		slot.Visible = true;
		var icon = slot.GetNode<TextureRect>("Icon");
		var num  = slot.GetNode<Label>("Num");
		if (UIManager.Instance != null)
		{
			var tex = UIManager.Instance.GetItemIcon(ids[index]);
			if (tex != null) icon.Texture = tex;
		}
		num.Text = $"×{(index < nums.Count ? nums[index] : 0)}";
	}

	//判断一行当前是否可执行
	private bool IsRowAvailable(RoofWorkstationItem cfg)
	{
		if (cfg == null || PlayerManager.Instance == null) return false;

		//等级校验
		if (cfg.Level > PlayerManager.Instance.WorkStationLevel) return false;

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

	//点击行：物品显示到selectedArea
	private void SelectRow(RoofWorkstationItem cfg)
	{
		_selectedCfg = cfg;
		if (ConfigManager.Instance?.itemDic == null) return;

		_selectedTip.Visible = false;
		_selectedInfo.Visible = true;

		if (ConfigManager.Instance.itemDic.ContainsKey(cfg.ItemID))
		{
			var item = ConfigManager.Instance.itemDic[cfg.ItemID];
			if (UIManager.Instance != null)
			{
				var tex = UIManager.Instance.GetItemIcon(cfg.ItemID);
				if (tex != null) _selectedIcon.Texture = tex;
			}
			_selectedName.Text = item.Name;
		}

		_selectedDesc.Text = BuildDesc(cfg);
		_actionBtn.Text = currentTab == TabType.Decompose ? "分解" : "制作";
		RefreshActionBtn();
	}

	//刷新动作按钮：条件不足时视觉变灰，保持可点击
	private void RefreshActionBtn()
	{
		if (_actionBtn == null) return;
		bool enabled = _selectedCfg != null && IsRowAvailable(_selectedCfg);
		_actionBtn.Modulate = enabled ? Colors.White : DimColor;
	}


	private void ClearSelection()
	{
		_selectedCfg = null;
		_selectedTip.Visible = true;
		_selectedInfo.Visible = false;
		if (_actionBtn != null) _actionBtn.Modulate = DimColor;
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

	//点击制作/分解按钮
	private void OnActionPressed()
	{
		if (_selectedCfg == null) return;
		if (PlayerManager.Instance == null || ConfigManager.Instance == null) return;
		var pm = PlayerManager.Instance;
		var cfg = _selectedCfg;

		//等级校验
		if (cfg.Level > pm.WorkStationLevel)
		{
			UIManager.Instance.ShowCommonTips2("工作台等级不足");
			return;
		}

		//材料/持有物校验
		if (currentTab == TabType.Decompose)
		{
			if (pm.GetItemCount(cfg.ItemID) <= 0)
			{
				UIManager.Instance.ShowCommonTips2($"未持有{GetItemName(cfg.ItemID)}，无法分解");
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
		if (_selectedCfg.Stamina > 0)
		{
			PlayerManager.Instance.AddItem(StaminaPropID, -_selectedCfg.Stamina);
		}

		if (decompose)
		{
			// 分解：消耗1个目标物品，产出 getID/getNum
			PlayerManager.Instance.RemoveItem(_selectedCfg.ItemID, 1);
			if (_selectedCfg.GetID != null)
			{
				for (int i = 0; i < _selectedCfg.GetID.Count; i++)
				{
					int n = (_selectedCfg.GetNum != null && i < _selectedCfg.GetNum.Count) ? _selectedCfg.GetNum[i] : 0;
					if (n > 0) PlayerManager.Instance.AddItem(_selectedCfg.GetID[i], n);
				}
			}
		}
		else
		{
			// 扣材料，产出物品
			if (_selectedCfg.MaterialID != null)
			{
				for (int i = 0; i < _selectedCfg.MaterialID.Count; i++)
				{
					int need = (_selectedCfg.MaterialNum != null && i < _selectedCfg.MaterialNum.Count) ? _selectedCfg.MaterialNum[i] : 0;
					if (need > 0) PlayerManager.Instance.RemoveItem(_selectedCfg.MaterialID[i], need);
				}
			}
			PlayerManager.Instance.AddItem(_selectedCfg.ItemID, 1);
		}

		string itemName = GetItemName(_selectedCfg.ItemID);
		GD.Print($"[WSMakeUI] {(decompose ? "分解" : "制作")}：{itemName} 完成");
	}

	//构建selectedArea的描述文本
	private string BuildDesc(RoofWorkstationItem cfg)
	{
		if (currentTab == TabType.Decompose)
		{
			string gets = JoinCost(cfg.GetID, cfg.GetNum);
			return $"拆解获得：{gets}\n消耗体力：×{cfg.Stamina}";
		}
		string mats = JoinCost(cfg.MaterialID, cfg.MaterialNum);
		return $"制作材料：{mats}\n消耗体力：×{cfg.Stamina}";
	}

	
	private string JoinCost(List<int> ids, List<int> nums)
	{
		if (ids == null || ids.Count == 0) return "无";
		var parts = new List<string>();
		for (int i = 0; i < ids.Count; i++)
		{
			int n = (nums != null && i < nums.Count) ? nums[i] : 0;
			parts.Add($"{GetItemName(ids[i])}×{n}");
		}
		return string.Join("　", parts);
	}
	private string GetItemName(int id)
	{
		return ConfigManager.Instance.itemDic.ContainsKey(id)
			? ConfigManager.Instance.itemDic[id].Name
			: $"ID{id}";
	}
}
