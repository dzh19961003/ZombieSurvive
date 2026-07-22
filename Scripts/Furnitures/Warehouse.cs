using Godot;
using System;

public partial class Warehouse : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.InputEvent += OnMousePressed;
	}

  
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    private void OnMousePressed(Node viewport, InputEvent @event, long shapeIdx)
    {
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left )
		{
			OpenWarehouse();
        }
    }


    private void OpenWarehouse() 
	{
		UIManager.Instance.ShowUI(Paths.WarehouseUI);
	}
}
