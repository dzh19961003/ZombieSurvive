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

    private ColorRect[] headMask;       //每个头部肢体的变暗遮罩
    private ColorRect[] bodyMask;       //每个身体肢体的变暗遮罩
    private ColorRect[] armMask;        //每个手臂肢体的变暗遮罩
    private bool armMasked = false;     //手臂整体是否已经屏蔽过（用于屏蔽攻击按钮）
    private bool isDead = false;        //头或身体被打没，敌人死亡

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

        //给每个肢体都生成一个遮罩，默认隐藏
        headMask = new ColorRect[headNum];
        bodyMask = new ColorRect[bodyNum];
        armMask = new ColorRect[armNum];

        for (int i = 0; i < headNum; i++)
        {
            headStatusDic[i] = new Dictionary<string, int>();
            headHP[i] = Math.Round(ConfigManager.Instance.enemyDic[enemyID].HeadHP[i], 1);
            headHPLabel[i].Text = headHP[i].ToString();
            headMask[i] = CreateBlackMask(head[i]);
        }
        for (int i = 0; i < bodyNum; i++)
        {
            bodyStatusDic[i] = new Dictionary<string, int>();
            bodyHP[i] = Math.Round(ConfigManager.Instance.enemyDic[enemyID].BodyHP[i], 1);
            bodyHPLabel[i].Text = bodyHP[i].ToString();
            bodyMask[i] = CreateBlackMask(body[i]);
        }
        for (int i = 0; i < armNum; i++)
        {
            armStatusDic[i] = new Dictionary<string, int>();
            armHP[i] = Math.Round(ConfigManager.Instance.enemyDic[enemyID].ArmHP[i], 1);
            handHPLabel[i].Text = armHP[i].ToString();
            armMask[i] = CreateBlackMask(arm[i]);
        }

        //加入敌人效果
        foreach (var item in ConfigManager.Instance.enemyDic[enemyID].BattleEffectID)
        {
            enemyEffects.Add(item);
        }
        enemyEffectBases = BattleManager.Instance.LoadBattleEffect(enemyEffects, "enemy");
    }
    //刷新敌人UI展示（只更新显示，不重置血量和状态）
    public void RefreshUI()
    {
        for (int i = 0; i < headNum; i++)
        {
            //血量统一保留一位小数再显示，避免出现一长串小数
            headHPLabel[i].Text = Math.Round(headHP[i], 1).ToString();
            //血量为0的肢体立刻盖上遮罩
            headMask[i].Visible = headHP[i] == 0;
        }
        for (int i = 0; i < bodyNum; i++)
        {
            bodyHPLabel[i].Text = Math.Round(bodyHP[i], 1).ToString();
            bodyMask[i].Visible = bodyHP[i] == 0;
        }
        for (int i = 0; i < armNum; i++)
        {
            handHPLabel[i].Text = Math.Round(armHP[i], 1).ToString();
            armMask[i].Visible = armHP[i] == 0;
        }
    }
    //判断某个部位是否还有血量为0以外的肢体
    public bool HasAlivePart(string part)
    {
        double[] hps = null;
        switch (part)
        {
            case "head":
                hps = headHP;
                break;
            case "body":
                hps = bodyHP;
                break;
            case "arm":
                hps = armHP;
                break;
        }
        if (hps == null)
        {
            return false;
        }
        foreach (var hp in hps)
        {
            if (hp != 0)
            {
                return true;
            }
        }
        return false;
    }
    //敌人挨打方法，返回是否真的打到了肢体
    public bool BeHit(string part)
    {
        //这个部位全被打没了，直接跳过
        if (!HasAlivePart(part))
        {
            return false;
        }
        int num = 0;
        switch (part)
        {
            case "head":
                //随机选择这个部位的其中一个，并判断是否血量为0，是就换一个
                num = Random.Shared.Next(0, headStatusDic.Length);
                num = GetExsistPart("head", num);
                headHP[num] = Math.Round(Math.Max(0, headHP[num] - BattleManager.Instance.battleInfo.headDamage), 1);
                GD.Print("head第" + num + "个肢体挨打，伤害" + BattleManager.Instance.battleInfo.headDamage);
                break;
            case "body":
                num = Random.Shared.Next(0, bodyStatusDic.Length);
                num = GetExsistPart("body", num);
                bodyHP[num] = Math.Round(Math.Max(0, bodyHP[num] - BattleManager.Instance.battleInfo.bodyDamage), 1);
                GD.Print("body第" + num + "个肢体挨打，伤害" + BattleManager.Instance.battleInfo.bodyDamage);
                break;
            case "arm":
                num = Random.Shared.Next(0, armStatusDic.Length);
                num = GetExsistPart("arm", num);
                armHP[num] = Math.Round(Math.Max(0, armHP[num] - BattleManager.Instance.battleInfo.armDamage), 1);
                GD.Print("arm第" + num + "个肢体挨打，伤害" + BattleManager.Instance.battleInfo.armDamage);
                break;
            default:
                return false;
        }
        CheckBodyPart();
        BattleManager.Instance.RefreshUI();
        return true;
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
        //每次都先重置，避免部位已经打没了还被当成可用
        headAble = false;
        bodyAble = false;
        armAble = false;

        //再看有哪些部位还可用
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

        //四肢整体打没了：不用在这里单独屏蔽按钮，
        //BeHit 末尾的 RefreshUI 会根据 HasAlivePart 自动禁用按钮并显示遮罩
        if (!armAble && !armMasked)
        {
            armMasked = true;
            GD.Print("手臂已被打断，无法再攻击手臂");
        }
        //头或身体打没了，按设定敌人死亡
        if ((!headAble || !bodyAble) && !isDead)
        {
            isDead = true;
            GD.Print("敌人死亡");
            BattleManager.Instance.BattleEnd();
        }
    }
    //在一个节点上生成一个黑色半透明遮罩，默认隐藏
    private ColorRect CreateBlackMask(Control parent)
    {
        ColorRect mask = new ColorRect();
        mask.Color = new Color(0, 0, 0, 0.6f);
        mask.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mask.MouseFilter = Control.MouseFilterEnum.Ignore;
        mask.Visible = false;
        parent.AddChild(mask);
        return mask;
    }
}
