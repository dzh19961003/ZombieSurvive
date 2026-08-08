using Godot;
using System;

public partial class ExploreIcon : Control
{
	[Export] public TextureRect icon;
    [Export] public Label addLabel;
    [Export] public Label minusLabel;
	[Export] public Label numLabel;

	public override void _Ready()
	{
	}

}
