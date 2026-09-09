using Godot;
using MyProject;
using System;

public partial class DamageBonus : BattleEffectBase
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
                bm.battleInfo.bodyDamage = Math.Round(bm.battleInfo.bodyDamage * (1 + amount / 100.0), 1);
                break;
            case "head":
                bm.battleInfo.headDamage = Math.Round(bm.battleInfo.headDamage * (1 + amount / 100.0), 1);
                break;
            case "arm":
                bm.battleInfo.armDamage = Math.Round(bm.battleInfo.armDamage * (1 + amount / 100.0), 1);
                break;
            default:
                break;
        }
        bm.RefreshUI();
    }
}
