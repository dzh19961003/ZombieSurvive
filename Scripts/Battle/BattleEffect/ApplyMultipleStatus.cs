using Godot;
using System;
using System.Collections.Generic;

public partial class ApplyMultipleStatus : BattleEffectBase
{
    public override void _Ready()
    {
        base._Ready();
    }
    public override void TurnStart(string chara)
    {
        if (chara == "player") { }
        else if (chara == "enemy") { }
    }
    //给玩家或敌人增加状态
    public override void MultyStatusAdd()
    {
        //玩家直接在玩家上面加
        if (character == "enemy")
        {
            if (!bm.battleInfo.playerStatusNumDic.TryAdd(statusKind, bonus))
            {
                bm.battleInfo.playerStatusNumDic[statusKind] += bonus;
            }
        }
        //敌人给对应肢体加
        else if (character == "player")
        {
            int num = 0;
            switch (bm.battleInfo.bodyPart)
            {
                case "head":
                    //随机选择这个部位的其中一个，并判断是否血量为0，是就换一个
                    num = Random.Shared.Next(0, bm.battleEnemy.headStatusDic.Length);
                    num = bm.battleEnemy.GetExsistPart("head", num);
                    if (bonusBody == "head")
                    {
                        if (!bm.battleEnemy.headStatusDic[num].TryAdd(statusKind, bonus + bonusBodyAmount))
                        {
                            bm.battleEnemy.headStatusDic[num][statusKind] += bonus + bonusBodyAmount;
                        }
                    }
                    else
                    {
                        if (!bm.battleEnemy.headStatusDic[num].TryAdd(statusKind, bonus))
                        {
                            bm.battleEnemy.headStatusDic[num][statusKind] += bonus;
                        }
                    }
                    break;
                case "body":
                    num = Random.Shared.Next(0, bm.battleEnemy.bodyStatusDic.Length);
                    num = bm.battleEnemy.GetExsistPart("body", num);
                    if (bonusBody == "body")
                    {
                        if (!bm.battleEnemy.bodyStatusDic[num].TryAdd(statusKind, bonus + bonusBodyAmount))
                        {
                            bm.battleEnemy.bodyStatusDic[num][statusKind] += bonus + bonusBodyAmount;
                        }
                    }
                    else
                    {
                        if (!bm.battleEnemy.bodyStatusDic[num].TryAdd(statusKind, bonus))
                        {
                            bm.battleEnemy.bodyStatusDic[num][statusKind] += bonus;
                        }
                    }
                    break;
                case "arm":
                    num = Random.Shared.Next(0, bm.battleEnemy.armStatusDic.Length);
                    num = bm.battleEnemy.GetExsistPart("arm", num);
                    if (bonusBody == "arm")
                    {
                        if (!bm.battleEnemy.armStatusDic[num].TryAdd(statusKind, bonus + bonusBodyAmount))
                        {
                            bm.battleEnemy.armStatusDic[num][statusKind] += bonus + bonusBodyAmount;
                        }
                    }
                    else
                    {
                        if (!bm.battleEnemy.armStatusDic[num].TryAdd(statusKind, bonus))
                        {
                            bm.battleEnemy.armStatusDic[num][statusKind] += bonus;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
