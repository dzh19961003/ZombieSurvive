using Godot;
using System;

public partial class RoomProgress : Control
{
	[Export] public TextureRect on;
    [Export] public TextureRect off;
    public override void _Ready()
	{
	}

	public void Initial() 
	{
		off.Visible = false;
		on.Visible = true;
	}

}
