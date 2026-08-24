using Godot;
using System;
using MyProject;

public partial class StaHunUi : TextureButton
{
	[Export] public Button warehouseBtn;
	[Export] public Button propertyButton;
	private TextureRect _h1;
	private TextureRect _h2;
	private TextureRect _h3;

	// 绿色/灰色纹理
	private Texture2D _greenTex;
	private Texture2D _grayTex;
	//体力值
	private Label _staminaValueLabel;

	// 饱食状态文字（HungerState/Label）
	private Label _hungerStateLabel;
	
	public override void _Ready()
	{
		if (warehouseBtn != null) warehouseBtn.Pressed += EnterWarehouse;
		if (propertyButton != null) propertyButton.Pressed += EnterPropertyUI;

		// 读取三个小方块节点（sta_hunUI.tscn：H1 ~ H3）
		_h1 = GetNodeOrNull<TextureRect>("Hunger/H1");
		_h2 = GetNodeOrNull<TextureRect>("Hunger/H2");
		_h3 = GetNodeOrNull<TextureRect>("Hunger/H3");

		// 读取 Stamina 面板的文字
		_staminaValueLabel = GetNodeOrNull<Label>("Stamina/SValue");

		// 读取饱食状态文字
		_hungerStateLabel = GetNodeOrNull<Label>("HungerState/Label");

		// 预加载绿色/灰色方块纹理（与场景 H1/H3 纹理资源一致）
		_greenTex = GD.Load<Texture2D>("res://Assets/Images/UI/new/foodProgress.png");
		_grayTex  = GD.Load<Texture2D>("res://Assets/Images/UI/new/foodProgress_2.png");

		// 用 CallDeferred：保证 PlayerManager.Instance 已初始化（避免 _Ready 顺序不同步导致 null）
		CallDeferred(nameof(DeferredSetup));
	}

	/// <summary>当前帧所有节点 _Ready 完成后执行，此时 PlayerManager.Instance 必已存在</summary>
	private void DeferredSetup()
	{
		var pm = PlayerManager.Instance;
		if (pm == null)
		{
			GD.PrintErr("[StaHunUi] PlayerManager.Instance 为空，初始化失败！");
			return;
		}

		// 首次刷新
		RefreshHungerDisplay();
		RefreshStaminaDisplay();

		// 订阅属性变化：AddItem/GetState/OnDayEnd 都会触发 GetItem
		pm.GetItem += OnPlayerDataChanged;
	}

	public override void _ExitTree()
	{
		if (PlayerManager.Instance != null)
		{
			PlayerManager.Instance.GetItem -= OnPlayerDataChanged;
		}
	}

	/// <summary>玩家数据变化时刷新全部显示</summary>
	private void OnPlayerDataChanged()
	{
		RefreshHungerDisplay();
		RefreshStaminaDisplay();
	}

	
	/// <summary>
	/// 根据 Hunger 值把三个小方块染成绿色或灰色：
	///   下标 < Hunger 的方块 → 绿色（progressIn）
	///   下标 ≥ Hunger 的方块 → 灰色（progressOut）
	/// 例如 Hunger=2 → H1=绿、H2=绿、H3=灰（绿2灰1）。
	///
	/// 下方状态文字直接读取状态表中的对应 Name（饥饿/空腹/半饱/饱腹），
	/// 状态通过 GetHungerStateID(hunger) 索引到 state.json 的 ID 5~8。
	/// </summary>
	private void RefreshHungerDisplay()
	{
		var pm = PlayerManager.Instance;
		if (pm == null) return;

		int hunger = Mathf.Clamp(pm.Hunger, 0, PlayerManager.MaxHunger);
		int maxHunger = PlayerManager.MaxHunger;

		// 设置每个方块的纹理
		SetHungerSlotTexture(_h1, 0 < hunger);
		SetHungerSlotTexture(_h2, 1 < hunger);
		SetHungerSlotTexture(_h3, 2 < hunger);

		// 刷新饱食状态文字：读取状态表中对应状态的 Name
		if (_hungerStateLabel != null)
		{
			_hungerStateLabel.Text = GetHungerStateName(hunger);
		}

		GD.Print($"[StaHunUi] 饱食刷新：Hunger={hunger}/{maxHunger}，绿{hunger}灰{maxHunger - hunger}，状态文字={_hungerStateLabel?.Text ?? "(null)"}");
	}

	/// <summary>
	/// 根据 hunger 返回对应的状态 ID（与 PlayerManager.HungerStateIDs 一致）
	/// hunger=0→5(饥饿), 1→6(空腹), 2→7(半饱), 3→8(饱腹)
	/// </summary>
	private int GetHungerStateID(int hunger)
	{
		return hunger switch
		{
			0 => 5,
			1 => 6,
			2 => 7,
			3 => 8,
		};
	}

	/// <summary>
	/// 读取状态表中对应饥饿状态的 Name。
	/// 读表失败时回退到本地默认文字（饥饿/空腹/半饱/饱腹）。
	/// </summary>
	private string GetHungerStateName(int hunger)
	{
		int sid = GetHungerStateID(hunger);
		if (ConfigManager.Instance != null &&
		    ConfigManager.Instance.stateDic.TryGetValue(sid, out var state) &&
		    !string.IsNullOrEmpty(state.Name))
		{
			return state.Name;
		}
		// 回退
		return hunger switch
		{
			0 => "饥饿",
			1 => "空腹",
			2 => "半饱",
			3 => "饱腹",
		};
	}

	/// <summary>单个小方块设置纹理：green=true 用 progressIn，false 用 progressOut</summary>
	private void SetHungerSlotTexture(TextureRect slot, bool green)
	{
		if (slot == null) return;
		slot.Texture = green ? _greenTex : _grayTex;
	}

	/// <summary>刷新体力面板显示：显示 "X N" 格式（与场景初始 "X 15" 对齐）</summary>
	private void RefreshStaminaDisplay()
	{
		var pm = PlayerManager.Instance;
		if (pm == null || _staminaValueLabel == null) return;

		// 显示探索体力 / 探索最大体力
		_staminaValueLabel.Text = $"x{pm.BaseStamina}";
	}

	private void EnterWarehouse()
	{
		UIManager.Instance?.ShowUI(Paths.WarehouseUI);
	}
	private void EnterPropertyUI()
	{
		UIManager.Instance?.ShowUI(Paths.PropertyUI);
	}
}
