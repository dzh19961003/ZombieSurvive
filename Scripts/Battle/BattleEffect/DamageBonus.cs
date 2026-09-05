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
        switch (body)
        {
            case "body":
                bm.bodyDMG.Text = "伤害:" + (bm.battleInfo.baseDamage) * (1 + bonus / 100.0);
                break;
            case "head":
                bm.headDMG.Text = "伤害:" + (bm.battleInfo.baseDamage) * (1 + bonus / 100.0);
                break;
            case "arm":
                bm.handDMG.Text = "伤害:" + (bm.battleInfo.baseDamage) * (1 + bonus / 100.0);
                break;
            default:
                break;
        }

    }
}
