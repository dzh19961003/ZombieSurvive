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
    public event Action<string> OnTurnStart;
    public event Action OnDamageBuff;
    public event Action OnDamageDealed;
    public event Action OnStatusDealed;
    public event Action OnTurnEnd;
    public event Action OnBattleEnd;

    //战斗所需
    public BattleEnemy battleEnemy;
    public BattleInfo battleInfo;
    public Dictionary<int, BattleEffectBase> playerEffectDic = new Dictionary<int, BattleEffectBase>();//玩家不可重复的效果实例
    public Dictionary<int, BattleEffectBase> enemyEffectDic = new Dictionary<int, BattleEffectBase>();//敌人不可重复的效果实例
    public List<BattleEffectBase> playerAllEffects = new List<BattleEffectBase>();//玩家所有效果实例
    public List<BattleEffectBase> enemyAllEffects = new List<BattleEffectBase>();//敌人所有效果实例

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
        TurnStart("player");
    }
    private void EnemyTurn()
    {
        GD.Print("敌人行动");
        TurnStart("enemy");
    }

    //战斗流程
    //1.战斗开始
    private void BattleStart()
    {
        battleInfo = new BattleInfo(GameManager.Instance.enemyID);
        //加载战斗、敌人和玩家相关数据       
        var enemy = GD.Load<PackedScene>("res://UI/Battle/enemy_" + ConfigManager.Instance.enemyDic[GameManager.Instance.enemyID].Type + ".tscn");
        battleEnemy = enemy.Instantiate<BattleEnemy>();
        AddChild(battleEnemy);
        battleEnemy.Initial(battleInfo.enemyID);
        OnBattleStart?.Invoke();
        RefreshUI();
    }
    //2.回合开始
    private void TurnStart(string character)
    {
        OnTurnStart?.Invoke(character);
    }
    //3.进行攻击
    private void Attack()
    {
        OnStatusDealed.Invoke();
    }


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
            if (character == "player") playerAllEffects.Add(item);
            else if (character == "enemy") enemyAllEffects.Add(item);
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
                weightBonus.bodyPart = battleEffect.Part;
                weightBonus.bonus = battleEffect.Amount;
                break;
            case "ChargeBonus":
                break;
            case "ProgressBonus":
                break;
            case "ApplyMultipleStatus":
                ApplyMultipleStatus applyMultipleStatus = new ApplyMultipleStatus();
                battleEffectBase = applyMultipleStatus;
                applyMultipleStatus.bodyPart = battleEffect.Part;
                applyMultipleStatus.bonus = battleEffect.Amount;
                applyMultipleStatus.statusKind = battleEffect.StatusKind;
                applyMultipleStatus.bonusBody = battleEffect.BonusPart;
                applyMultipleStatus.bonusBodyAmount = battleEffect.BonusAmount;
                break;
            default:
                break;
        }
        //IsMulty==1 可叠加，不入字典；IsMulty==0 只保留一个
        if (battleEffect.IsMulty == 1)
        {
            return battleEffectBase;
        }
        if (character == "enemy")
        {
            enemyEffectDic.TryAdd(item, battleEffectBase);
        }
        else if (character == "player")
        {
            playerEffectDic.TryAdd(item, battleEffectBase);
        }
        return battleEffectBase;
    }
    //战斗局内增加效果
    public void GetEffect(int effectID, string character)
    {
        BattleEffect battleEffect = ConfigManager.Instance.battleEffectDic[effectID];
        //IsMulty==0 且已存在则不再重复获得
        if (character == "enemy")
        {
            if (battleEffect.IsMulty == 0 && battleEnemy.enemyEffects.Contains(effectID))
                return;
            battleEnemy.enemyEffects.Add(effectID);
            var newEffect = LoadEffects(effectID, character);
            AddChild(newEffect);
            enemyAllEffects.Add(newEffect);
        }
        else if (character == "player")
        {
            if (battleEffect.IsMulty == 0 && battleInfo.playerEffects.Contains(effectID))
                return;
            battleInfo.playerEffects.Add(effectID);
            var newEffect = LoadEffects(effectID, character);
            AddChild(newEffect);
            playerAllEffects.Add(newEffect);
        }
    }
}
