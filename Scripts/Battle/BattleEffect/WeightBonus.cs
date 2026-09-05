using Godot;
using System;

public partial class WeightBonus : BattleEffectBase
{
	
	public override void _Ready()
	{
        base._Ready();
	}
    public override void BattleStart()
    {
		switch (body)
		{
			case "body":
				pm.AddItem(10014, bonus);
				break;
            case "arm":
                pm.AddItem(10012, bonus);
                break;
            case "head":
                pm.AddItem(10013, bonus);
                break;
            default:
				break;
		}
	}
}
