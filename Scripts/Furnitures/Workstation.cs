using Godot;
using System;

public partial class Workstation : Area2D
{
	
	private const int MaxTextureLevel = 5;

	private Sprite2D _sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_sprite = GetNodeOrNull<Sprite2D>("Workstation");
		this.InputEvent += OnMousePressed;

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
		if (PlayerManager.Instance == null)
		{
			GD.PrintErr("[Workstation] PlayerManager 未就绪");
			return;
		}
		PlayerManager.Instance.GetItem += OnPlayerDataChanged;
		RefreshTexture();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnMousePressed(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left )
		{
			OpenWorkstation();
        }
    }


    private void OpenWorkstation()
	{
		UIManager.Instance.ShowUI(Paths.WorkstationUI);
	}

	// 刷新纹理
	private void OnPlayerDataChanged()
	{
		CallDeferred(nameof(RefreshTexture));
	}

	
	public void RefreshTexture()
	{
		if (PlayerManager.Instance == null || _sprite == null) return;

		int level = Mathf.Clamp(PlayerManager.Instance.WorkStationLevel, 1, MaxTextureLevel);
		bool isNight = GameManager.Instance?.CurrentTimePeriod == 3;
		string prefix = isNight ? "night_" : "";
		var tex = GD.Load<Texture2D>($"res://Assets/Images/Base/{prefix}workspace_{level}.png");
		if (tex != null)
		{
			_sprite.Texture = tex;
		}
	}
}
