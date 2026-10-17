using Godot;
using System;
using MyProject;

public partial class Plant : Area2D
{
	private const int MaxTextureLevel = 4;

	private Sprite2D _sprite;
	private Sprite2D _sprite2;
	//收获气泡展示
	[Export] private Sprite2D _tipBg;
	[Export] private Sprite2D _tipIcon;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_sprite = GetNodeOrNull<Sprite2D>("Plant");
		_sprite2 = GetNodeOrNull<Sprite2D>("Plant2");

		this.InputEvent += OnMousePressed;
		CallDeferred(nameof(DeferredInit));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnMousePressed(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (UIManager.Instance.IsSceneClickBlocked()) return;
			OpenPlantUI();
		}
	}

	private void DeferredInit()
	{
		if (PlayerManager.Instance == null)
		{
			GD.PrintErr("[Plant] PlayerManager 未就绪");
			return;
		}
		PlayerManager.Instance.GetItem += OnPlayerDataChanged;
		RefreshTexture();
	}

	private void OpenPlantUI()
	{
		UIManager.Instance.ShowUI(Paths.PlantUI);
	}

	private void OnPlayerDataChanged()
	{
		CallDeferred(nameof(RefreshTexture));
	}

	
	private const float TipIconDisplaySize = 85f;

	//收获时展示第一个可收回作物
	private void RefreshMatureTip()
	{
		if (_tipBg == null || _tipIcon == null) return;

		var dic = ConfigManager.Instance?.roofPlantsItemDic;
		if (dic != null && UIManager.Instance != null)
		{
			foreach (var kv in dic)
			{
				if (PlantManager.Instance.HasRipePlant(kv.Key))
				{
					var icon = UIManager.Instance.GetItemIcon(kv.Value.ItemID);
					if (icon != null)
					{
						_tipIcon.Texture = icon;
						//按目标显示尺寸计算缩放
						var size = icon.GetSize();
						float s = TipIconDisplaySize / Mathf.Max(size.X, size.Y);
						_tipIcon.Scale = new Vector2(s, s);
						_tipBg.Visible = true;
						return;
					}
				}
			}
		}
		_tipBg.Visible = false;
	}

	public void RefreshTexture()
	{
		if (PlantManager.Instance == null) return;

		if (_sprite != null)
		{
			int level = Mathf.Clamp(PlantManager.Instance.PlantLevel, 1, MaxTextureLevel);
			bool isNight = GameManager.Instance?.CurrentTimePeriod == 3;
			string prefix = isNight ? "night_" : "";
			var tex = GD.Load<Texture2D>($"res://Assets/Images/Base/{prefix}plant_{level}.png");
			if (tex != null)
			{
				_sprite.Texture = tex;
				if (_sprite2 != null)
				{
					_sprite2.Texture = tex;
				}
			}
		}

		RefreshMatureTip();
	}
}
