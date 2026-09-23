using Godot;
using System;

public partial class Train : Area2D
{
	private const int MaxTextureLevel = 5;

	private Sprite2D _sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_sprite = GetNodeOrNull<Sprite2D>("Train");
		
		this.InputEvent += OnMousePressed;
		CallDeferred(nameof(DeferredInit));
	}

  
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    private void OnMousePressed(Node viewport, InputEvent @event, long shapeIdx)
    {
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left )
		{
			// 有全屏面板（探索界面等）盖在上面时，忽略这次点击
			if (UIManager.Instance.IsSceneClickBlocked()) return;
			OpenTrainUI();
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

    private void OpenTrainUI() 
	{
		UIManager.Instance.ShowUI(Paths.TrainUI);
	}
	private void OnPlayerDataChanged()
	{
		CallDeferred(nameof(RefreshTexture));
	}
	public void RefreshTexture()
	{
		if (PlayerManager.Instance == null || _sprite == null) return;

		int level = Mathf.Clamp(PlayerManager.Instance.TrainLevel, 1, MaxTextureLevel);
		bool isNight = GameManager.Instance?.CurrentTimePeriod == 3;
		string prefix = isNight ? "night_" : "";
		var tex = GD.Load<Texture2D>($"res://Assets/Images/Base/{prefix}train_{level}.png");
		if (tex != null)
		{
			_sprite.Texture = tex;
		}
	}
}
