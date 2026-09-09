using Godot;
using MyProject;
using System;
using System.Collections.Generic;

public partial class BattleEnemy : Node
{
    [Export] public TextureRect[] head;
    [Export] public TextureRect[] body;
    [Export] public TextureRect[] arm;
    [Export] public Label[] headHPLabel;
    [Export] public Label[] bodyHPLabel;
    [Export] public Label[] handHPLabel;

    private int headNum;
    private int bodyNum;
    private int armNum;
    private double[] headHP;
    private double[] bodyHP;
    private double[] armHP;

    public Dictionary<string, int>[] headStatusDic;
    public Dictionary<string, int>[] bodyStatusDic;
    public Dictionary<string, int>[] armStatusDic;

    bool headAble = false;
    bool bodyAble = false;
    bool armAble = false;

    public List<int> enemyEffects;
    private List<BattleEffectBase> enemyEffectBases = new List<BattleEffectBase>();

    public override void _Ready()
    {
        enemyEffects = new List<int>();
    }
    public void Initial(int enemyID)
    {
        //初始化各部位生命值
        headNum = ConfigManager.Instance.enemyDic[enemyID].HeadNum;
        bodyNum = ConfigManager.Instance.enemyDic[enemyID].BodyNum;
        armNum = ConfigManager.Instance.enemyDic[enemyID].ArmNum;

        headHP = new double[headNum];
        bodyHP = new double[bodyNum];
        armHP = new double[armNum];

        //初始化各部位状态条
        headStatusDic = new Dictionary<string, int>[headNum];
        bodyStatusDic = new Dictionary<string, int>[bodyNum];
        armStatusDic = new Dictionary<string, int>[armNum];

        for (int i = 0; i < headNum; i++)
        {
            headStatusDic[i] = new Dictionary<string, int>();
            headHP[i] = ConfigManager.Instance.enemyDic[enemyID].HeadHP[i];
            headHPLabel[i].Text = headHP[i].ToString();
        }
        for (int i = 0; i < bodyNum; i++)
        {
            bodyStatusDic[i] = new Dictionary<string, int>();
            bodyHP[i] = ConfigManager.Instance.enemyDic[enemyID].BodyHP[i];
            bodyHPLabel[i].Text = bodyHP[i].ToString();
        }
        for (int i = 0; i < armNum; i++)
        {
            armStatusDic[i] = new Dictionary<string, int>();
            armHP[i] = ConfigManager.Instance.enemyDic[enemyID].ArmHP[i];
            handHPLabel[i].Text = armHP[i].ToString();
        }

        //加入敌人效果
        foreach (var item in ConfigManager.Instance.enemyDic[enemyID].BattleEffectID)
        {
            enemyEffects.Add(item);
        }
        enemyEffectBases = BattleManager.Instance.LoadBattleEffect(enemyEffects, "enemy");
    }
    //敌人挨打方法
    public void BeHit(string part, double dmg)
    {
        BattleManager.Instance.RefreshUI();
        int num = 0;
        switch (part)
        {
            case "head":
                //随机选择这个部位的其中一个，并判断是否血量为0，是就换一个
                num = Random.Shared.Next(0, headStatusDic.Length);
                num = GetExsistPart("head", num);
                headHP[num] -= dmg;
                break;
            case "body":
                num = Random.Shared.Next(0, bodyStatusDic.Length);
                num = GetExsistPart("body", num);
                bodyHP[num] -= dmg;
                break;
            case "arm":
                num = Random.Shared.Next(0, armStatusDic.Length);
                num = GetExsistPart("arm", num);
                armHP[num] -= dmg;
                break;
            default:
                break;
        }
        CheckBodyPart();
    }
    //当部位里的某一个肢体不可用时，选择另一个肢体
    public int GetExsistPart(string part, int num)
    {
        switch (part)
        {
            case "head":
                if (headHP[num] != 0)
                {
                    return num;
                }
                else
                {
                    for (int i = 0; i < headHP.Length; i++)
                    {
                        if (headHP[i] != 0)
                        {
                            return i;
                        }
                    }
                }
                break;
            case "body":
                if (bodyHP[num] != 0)
                {
                    return num;
                }
                else
                {
                    for (int i = 0; i < bodyHP.Length; i++)
                    {
                        if (bodyHP[i] != 0)
                        {
                            return i;
                        }
                    }
                }
                break;
            case "arm":
                if (armHP[num] != 0)
                {
                    return num;
                }
                else
                {
                    for (int i = 0; i < armHP.Length; i++)
                    {
                        if (armHP[i] != 0)
                        {
                            return i;
                        }
                    }
                }
                break;
        }
        return 0;
    }
    //当攻击后，排除不可用部位
    public void CheckBodyPart()
    {
        double hp = 0;

        //先看有哪些部位还可用
        foreach (var item in headHP)
        {
            if (item > hp)
            {
                hp = item;
            }
        }
        if (hp != 0) { headAble = true; hp = 0; }
        foreach (var item in bodyHP)
        {
            if (item > hp)
            {
                hp = item;
            }
        }
        if (hp != 0) { bodyAble = true; hp = 0; }
        foreach (var item in armHP)
        {
            if (item > hp)
            {
                hp = item;
            }
        }
        if (hp != 0) { armAble = true; hp = 0; }

        if (!bodyAble) { BattleManager.Instance.battleInfo.bodyWeight = 0; }
        if (!armAble) { BattleManager.Instance.battleInfo.armWeight = 0; }
        if (!headAble) { BattleManager.Instance.battleInfo.headWeight = 0; }
    }
}
