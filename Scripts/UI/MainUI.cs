using Godot;
using System;

public partial class MainUI : Control
{
	[Export] public Button propertyBtn;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		propertyBtn.Pressed += EnterPro;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void EnterPro() 
	{
		UIManager.Instance.ShowUI(Paths.PropertyUI);
	}
}
