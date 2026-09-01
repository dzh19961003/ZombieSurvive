using Godot;
using MyProject;
using System;

public partial class BattleEnemy : Node
{
    [Export] public TextureRect[] head;
    [Export] public TextureRect[] body;
    [Export] public TextureRect[] arm;

    private int headNum;
    private int bodyNum;
    private int armNum;
    private double[] headHP;
    private double[] bodyHP;
    private double[] armsHp;

    public override void _Ready()
    {
        
    }
    public void Initial(int enemyID) 
    {
        headNum = ConfigManager.Instance.enemyDic[enemyID].HeadNum;
        bodyNum = ConfigManager.Instance.enemyDic[enemyID].BodyNum;
        armNum = ConfigManager.Instance.enemyDic[enemyID].ArmNum;
       
        headHP = new double[headNum];
        bodyHP = new double[bodyNum];
        armsHp = new double[armNum];

        for (int i = 0; i < headHP.Length; i++)
        {
            headHP[i] = ConfigManager.Instance.enemyDic[enemyID].HeadHP[i];
        }
        for (int i = 0; i < bodyHP.Length; i++)
        {
            bodyHP[i] = ConfigManager.Instance.enemyDic[enemyID].BodyHP[i];
        }
        for (int i = 0; i < armsHp.Length; i++)
        {
            armsHp[i] = ConfigManager.Instance.enemyDic[enemyID].ArmHP[i];
        }
    }
    public void BeHit(int part,double dmg) 
    {
        BattleManager.Instance.RefreshUI();
    }
}
