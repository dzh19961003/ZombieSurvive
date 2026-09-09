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
		switch (bodyPart)
		{
			case "body":
                BattleManager.Instance.battleInfo.bodyWeight += amount;
                break;
            case "arm":
                BattleManager.Instance.battleInfo.armWeight += amount;
                break;
            case "head":
                BattleManager.Instance.battleInfo.headWeight += amount;
                break;
            default:
				break;
		}
	}
}
