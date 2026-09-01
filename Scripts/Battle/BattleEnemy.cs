using Godot;
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
        headHP = new double[headNum];
        bodyHP = new double[bodyNum];
        armsHp = new double[armNum];
    }
    private void Initial(int enemyID) 
    {
        foreach (var item in headHP)
        {

        }
    }
}
