using Godot;
using System;

public partial class StaHunUi : TextureButton
{
	[Export] public Button warehouseBtn;
	[Export] public Button propertyButton;
	public override void _Ready()
	{
		warehouseBtn.Pressed += EnterWarehouse;
		propertyButton.Pressed += EnterPropertyUI;
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
