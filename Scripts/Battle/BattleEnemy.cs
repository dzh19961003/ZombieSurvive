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
    private double[] armsHp;

    public List<int> enemyEffects;
    private List<BattleEffectBase> enemyEffectBases = new List<BattleEffectBase>();

    public override void _Ready()
    {
        enemyEffects=new List<int>();
    }
    public void Initial(int enemyID) 
    {
        //初始化各部位生命值
        headNum = ConfigManager.Instance.enemyDic[enemyID].HeadNum;
        bodyNum = ConfigManager.Instance.enemyDic[enemyID].BodyNum;
        armNum = ConfigManager.Instance.enemyDic[enemyID].ArmNum;
       
        headHP = new double[headNum];
        bodyHP = new double[bodyNum];
        armsHp = new double[armNum];

        for (int i = 0; i < headNum; i++)
        {
            headHP[i] = ConfigManager.Instance.enemyDic[enemyID].HeadHP[i];
            headHPLabel[i].Text = headHP[i].ToString();
        }
        for (int i = 0; i < bodyNum; i++)
        {
            bodyHP[i] = ConfigManager.Instance.enemyDic[enemyID].BodyHP[i];
            bodyHPLabel[i].Text = bodyHP[i].ToString();
        }
        for (int i = 0; i < armNum; i++)
        {
            armsHp[i] = ConfigManager.Instance.enemyDic[enemyID].ArmHP[i];
            handHPLabel[i].Text = armsHp[i].ToString();
        }

        //加入敌人效果
        foreach (var item in ConfigManager.Instance.enemyDic[enemyID].BattleEffectID)
        {
            enemyEffects.Add(item);
        }
        enemyEffectBases = BattleManager.Instance.LoadBattleEffect(enemyEffects,"enemy");
    }
    public void BeHit(int part,double dmg) 
    {
        BattleManager.Instance.RefreshUI();
    }
   
}
