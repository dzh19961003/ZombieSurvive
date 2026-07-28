using Godot;
using Godot.Collections;
using System;

public partial class ExploreMap : Control
{
	[Export] public Control[] buildings;
	public override void _Ready()
	{
		Array<int> buildArray = new Array<int>();
		for (int i = 5; i > 0; i--)
		{
            buildArray.Add(i);
        }
		InitialMap(buildArray);

    }
	public void InitialMap(Array<int> buildingArry) 
	{
		for (int i = 0; i < buildingArry.Count; i++)
		{
			Buildings building = buildings[i] as Buildings;
			building.InitialBuilding(buildingArry[i]);
		}
	}
}
