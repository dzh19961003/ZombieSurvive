using Godot;
using System;

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
        if (character=="player")
        {
            bm.battleInfo.playerStatusNumDic?.Add(statusKind, bonus);
        }
        //敌人给对应肢体加
        else if (character == "enemy")
        {
            int num = 0;
            switch (bodyPart)
            {
                case "head":
                    //随机选择这个部位的其中一个，并判断是否血量为0，是就换一个
                    num = Random.Shared.Next(0, bm.battleEnemy.headStatusDic.Length);
                    num = bm.battleEnemy.GetExsistPart("head", num);
                    if (bonusBody == "head")
                    {
                        bm.battleEnemy.headStatusDic[num]?.Add(statusKind, bonus + bonusBodyAmount);
                    }
                    else 
                    {
                        bm.battleEnemy.headStatusDic[num]?.Add(statusKind, bonus);
                    }
                    break;
                case "body":
                    num = Random.Shared.Next(0, bm.battleEnemy.bodyStatusDic.Length);
                    num = bm.battleEnemy.GetExsistPart("body", num);
                    if (bonusBody == "body")
                    {
                        bm.battleEnemy.bodyStatusDic[num]?.Add(statusKind, bonus + bonusBodyAmount);
                    }
                    else 
                    {
                        bm.battleEnemy.bodyStatusDic[num]?.Add(statusKind, bonus);
                    }
                    break;
                case "arm":
                    num = Random.Shared.Next(0, bm.battleEnemy.armStatusDic.Length);
                    num = bm.battleEnemy.GetExsistPart("arm", num);
                    if (bonusBody == "arm")
                    {
                        bm.battleEnemy.armStatusDic[num]?.Add(statusKind, bonus + bonusBodyAmount);
                    }
                    else 
                    {
                        bm.battleEnemy.armStatusDic[num]?.Add(statusKind, bonus);
                    }        
                    break;
                default:
                    break;
            }
        }
    }
}
