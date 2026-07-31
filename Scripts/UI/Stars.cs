using Godot;
using System;
using System.Diagnostics;

public partial class Stars : Control
{
	[Export] public TextureRect[] stars;
	public override void _Ready()
	{
		
    }

	public void ShowStars(int starNum) 
	{
		switch (starNum)
		{
			case 1:
                StarVisible(0);
				break;
            case 2:
                StarVisible(1);
                break;
            case 3:
                StarVisible(2);
                break;
            case 4:
				StarVisible(3);
                break;
            case 5:
                StarVisible(4);
                break;
            default:
				break;
		}
	}
	private void StarVisible(int star) 
	{
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i].Visible = false;
		}
		stars[star].Visible = true;
	}
}
