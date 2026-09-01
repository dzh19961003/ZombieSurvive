using Godot;
using MyProject;
using System;

public partial class DamageBonus : BattleEffect
{
    string body;
    int bonus;
    public override void _Ready()
    {
        base._Ready();

    }
    public override void DamageBuff()
    {
        if (body == bm.bodyPart)
        {
            bm.Damage = bm.Damage + bm.baseDamage * (1 + bonus / 100);
        }
    }
}
