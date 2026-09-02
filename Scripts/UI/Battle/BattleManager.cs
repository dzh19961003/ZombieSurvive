using Godot;
using System;
using System.Diagnostics;

public partial class BattleManager : Control
{
    public static BattleManager Instance { get; private set; }

    PlayerManager pm = PlayerManager.Instance;

    [Export] public NinePatchRect progressBar;
    [Export] public TextureRect playerHead;
    [Export] public TextureRect enemyHead;
    [Export] public TextureRect mask;
    [Export] public TextureRect randomBtn;
    [Export] public TextureRect handBtn;
    [Export] public TextureRect bodyBtn;
    [Export] public TextureRect headBtn;
    [Export] public TextureProgressBar playerHP;
    [Export] public TextureProgressBar playerArmor;
    [Export] public Label handProp;
    [Export] public Label bodyProp;
    [Export] public Label headProp;

    //头像移动速度
    private double speed = 0.4;
    private double playerSpeed;
    private double enemySpeed;
    private double playerPosition;
    private double enemyPosition;
    private float positionBiasY;
    private float positionBiasX;

    //各状态事件
    public event Action OnBattleStart;
    public event Action OnTurnStart;
    public event Action OnDamageBuff;
    public event Action OnDamageDealed;
    public event Action OnStatusDealed;
    public event Action OnTurnEnd;
    public event Action OnBattleEnd;

    //各战斗所需
    public string bodyPart;        //攻击后取得的身体部位
    public double baseDamage;      //基础武器伤害
    public double Damage = 0;      //造成的最终伤害

    private BattleState battleState = BattleState.Moving;
    enum BattleState
    {
        Moving = 1,
        Player = 2,
        Enemy = 3,
        End = 4
    }
    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("[BattleManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;

        BattleStart();

        positionBiasY = playerHead.Size.Y;
        positionBiasX = playerHead.Size.X / 2;
        NormalizedSpeed(5, 5);
    }
    public override void _Process(double delta)
    {
        if (battleState != BattleState.Moving)
        {
            return;
        }
        float deltaF = (float)delta;
        playerPosition += playerSpeed * deltaF;
        enemyPosition += enemySpeed * deltaF;
        if (playerPosition >= 1)
        {
            battleState = BattleState.Player;
            PlayerTurn();
        }
        if (enemyPosition >= 1)
        {
            battleState = BattleState.Enemy;
            EnemyTurn();
        }
        UpdateUI();
    }
    private void UpdateUI()
    {
        playerHead.Position = progressBar.Position + new Vector2((float)(playerPosition / 1) * progressBar.Size.X - positionBiasX, -positionBiasY);
        enemyHead.Position = progressBar.Position + new Vector2((float)(enemyPosition / 1) * progressBar.Size.X - positionBiasX, positionBiasY / 2);
    }
    private void NormalizedSpeed(int playerSpeedProperty, int enemySpeedProperty)
    {
        if (playerSpeedProperty > enemySpeedProperty)
        {
            playerSpeed = speed;
            enemySpeed = Math.Round((double)enemySpeedProperty / playerSpeedProperty, 2) * speed;
        }
        else if (enemySpeedProperty > playerSpeedProperty)
        {
            enemySpeed = speed;
            playerSpeed = Math.Round((double)playerSpeedProperty / enemySpeedProperty, 2) * speed;
        }
        else if (playerSpeedProperty == enemySpeedProperty)
        {
            playerSpeed = speed;
            enemySpeed = 0.99 * speed;
        }
    }
    public void RefreshUI() 
    {
        int weightSum = pm.Attack_limb_weight + pm.Attack_body_weight + pm.Attack_head_weight;
        handProp.Text = (pm.Attack_limb_weight / weightSum).ToString();
        headProp.Text = (pm.Attack_head_weight / weightSum).ToString();
        bodyProp.Text = (pm.Attack_body_weight / weightSum).ToString();
    }
    private void PlayerTurn()
    {
        GD.Print("玩家行动");

    }
    private void EnemyTurn()
    {
        GD.Print("敌人行动");
    }

    //战斗流程
    //1.战斗开始
    private void BattleStart()
    {
        //加载敌人和玩家相关数据
        BattleEnemy battleEnemy = new BattleEnemy();
        battleEnemy.Initial(1);
        OnBattleStart.Invoke();
        RefreshUI();        
    }
    //2.回合开始
    private void TurnStart() 
    {
        OnTurnStart.Invoke();
    }
    //3.
}
