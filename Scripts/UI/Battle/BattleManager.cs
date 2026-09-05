using Godot;
using MyProject;
using System;
using System.Collections.Generic;

public partial class BattleManager : Control
{
    public static BattleManager Instance { get; private set; }

    PlayerManager pm = PlayerManager.Instance;

    [Export] public NinePatchRect progressBar;
    [Export] public TextureRect playerHead;
    [Export] public TextureRect enemyHead;
    [Export] public TextureRect mask;
    [Export] public NinePatchRect randomBtn;
    [Export] public NinePatchRect handBtn;
    [Export] public NinePatchRect bodyBtn;
    [Export] public NinePatchRect headBtn;
    [Export] public TextureProgressBar playerHP;
    [Export] public Label playerHPLabel;
    [Export] public TextureProgressBar playerArmor;
    [Export] public Label playerArmorLabel;
    [Export] public Label handProp;
    [Export] public Label bodyProp;
    [Export] public Label headProp;
    [Export] public Label handDMG;
    [Export] public Label bodyDMG;
    [Export] public Label headDMG;

    //头像移动相关
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

    //战斗所需
    public BattleEnemy battleEnemy;
    public BattleInfo battleInfo;
    public Dictionary<int, BattleEffectBase> playerEffectDic = new Dictionary<int, BattleEffectBase>();//玩家效果
    public Dictionary<int, BattleEffectBase> enemyEffectDic = new Dictionary<int, BattleEffectBase>();


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
        NormalizedSpeed(5, 2);
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
    //关闭UI时强行手动置空Instance
    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
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
        handProp.Text = (int)Math.Round((double)pm.Attack_limb_weight / weightSum * 100)  + "%";
        headProp.Text = (int)Math.Round((double)pm.Attack_head_weight / weightSum * 100)  + "%";
        bodyProp.Text = (int)Math.Round((double)pm.Attack_body_weight / weightSum * 100) + "%";
        playerHP.Value = pm.Hp;
        playerHP.MaxValue = pm.MaxHp;
        playerHPLabel.Text = pm.Hp + "/" + pm.MaxHp;
        playerArmor.Value = pm.Armor;
        playerArmor.MaxValue = pm.MaxArmor;
        playerArmorLabel.Text = pm.Armor + "/" + pm.MaxArmor;
    }
    private void PlayerTurn()
    {
        GD.Print("玩家行动");
        TurnStart();
    }
    private void EnemyTurn()
    {
        GD.Print("敌人行动");
        TurnStart();
    }

    //战斗流程
    //1.战斗开始
    private void BattleStart()
    {
        battleInfo = new BattleInfo(1);
        //加载战斗、敌人和玩家相关数据       
        var enemy = GD.Load<PackedScene>("res://UI/Battle/enemy_1.tscn");
        battleEnemy = enemy.Instantiate<BattleEnemy>();
        AddChild(battleEnemy);
        battleEnemy.Initial(battleInfo.enemyID);
        //加载敌人，这里先写死
        OnBattleStart?.Invoke();
        RefreshUI();
    }
    //2.回合开始
    private void TurnStart()
    {
        OnTurnStart?.Invoke();
    }
    //3.



    //战斗所有初始效果装填
    public List<BattleEffectBase> LoadBattleEffect(List<int> battleEffects, string character)
    {
        List<BattleEffectBase> battleEffectBases = new List<BattleEffectBase>();
        foreach (var item in battleEffects)
        {
            battleEffectBases.Add(LoadEffects(item, character));
            GD.Print("已加载" + character + "效果，ID:" + item);
        }
        foreach (var item in battleEffectBases)
        {
            item.character = character;
            AddChild(item);         
        }
        return battleEffectBases;
    }
    //战斗单个效果装填
    public BattleEffectBase LoadEffects(int item, string character)
    {
        BattleEffectBase battleEffectBase = null;
        BattleEffect battleEffect = ConfigManager.Instance.battleEffectDic[item];
        switch (battleEffect.Type)
        {
            case "DamageBonus":
                DamageBonus damageBonus = new DamageBonus();
                battleEffectBase = damageBonus;
                break;
            case "WeightBonus":
                WeightBonus weightBonus = new WeightBonus();
                battleEffectBase = weightBonus;
                weightBonus.body = battleEffect.Part;
                weightBonus.bonus = battleEffect.Amount;
                break;
            case "ChargeBonus":
                break;
            case "ProgressBonus":
                break;
            case "ApplyMultipleStatus":
                ApplyMultipleStatus applyMultipleStatus = new ApplyMultipleStatus();
                battleEffectBase = applyMultipleStatus;
                break;
            default:
                break;
        }
        if (character == "enemy")
        {
            enemyEffectDic?.Add(item, battleEffectBase);
        }
        else if (character == "player")
        {
            playerEffectDic?.Add(item, battleEffectBase);
        }
        return battleEffectBase;
    }
    //战斗局内增加效果
    public void GetEffect(int effectID, string character)
    {
        BattleEffect battleEffect = ConfigManager.Instance.battleEffectDic[effectID];
        if (character == "enemy")
        {
            if (!battleEnemy.enemyEffects.Contains(effectID))
            {
                battleEnemy.enemyEffects.Add(effectID);
                AddChild(LoadEffects(effectID, character));
            }
            else
            {
                if (battleEffect.IsMulty == 0)
                {
                    return;
                }
                else
                {
                    enemyEffectDic[effectID].MultyStatusAdd();
                }
            }
        }
        else if (character == "player")
        {
            if (!battleInfo.playerEffects.Contains(effectID))
            {
                battleInfo.playerEffects.Add(effectID);
                AddChild(LoadEffects(effectID, character));
            }
            else
            {
                if (battleEffect.IsMulty == 0)
                {
                    return;
                }
                else
                {
                    playerEffectDic[effectID].MultyStatusAdd();
                }
            }
        }

    }
}
