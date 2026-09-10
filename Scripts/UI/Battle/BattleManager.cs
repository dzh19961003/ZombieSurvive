using Godot;
using MyProject;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

public partial class BattleManager : Control
{
    public static BattleManager Instance { get; private set; }

    PlayerManager pm = PlayerManager.Instance;

    [Export] public NinePatchRect progressBar;
    [Export] public TextureRect playerHead;
    [Export] public TextureRect enemyHead;
    [Export] public TextureRect mask;
    [Export] public ColorRect armMask;
    [Export] public ColorRect bodyMask;
    [Export] public ColorRect headMask;
    [Export] public Label armCD;
    [Export] public Label bodyCD;
    [Export] public Label headCD;
    [Export] public Label armNum;
    [Export] public Label bodyNum;
    [Export] public Label headNum;
    [Export] public Button randomBtn;
    [Export] public Button handBtn;
    [Export] public Button bodyBtn;
    [Export] public Button headBtn;
    [Export] public TextureProgressBar playerHP;
    [Export] public Label playerHPLabel;
    [Export] public TextureProgressBar playerArmor;
    [Export] public Label playerArmorLabel;
    [Export] public Label handProp;
    [Export] public Label bodyProp;
    [Export] public Label headProp;
    [Export] public Label armDMG;
    [Export] public Label bodyDMG;
    [Export] public Label headDMG;
    [Export] public Label noArm;

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
    public ExploreUI exploreUI;
    public BattleEnemy battleEnemy;
    public BattleInfo battleInfo;
    public Dictionary<int, BattleEffectBase> playerEffectDic = new Dictionary<int, BattleEffectBase>();//玩家不可重复的效果实例
    public Dictionary<int, BattleEffectBase> enemyEffectDic = new Dictionary<int, BattleEffectBase>();//敌人不可重复的效果实例
    public List<BattleEffectBase> playerAllEffects = new List<BattleEffectBase>();//玩家所有效果实例
    public List<BattleEffectBase> enemyAllEffects = new List<BattleEffectBase>();//敌人所有效果实例
    private Dictionary<string, ColorRect> atkMaskDic = new Dictionary<string, ColorRect>();//攻击按钮的变暗遮罩

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
        //设置单例
        if (Instance != null)
        {
            GD.PrintErr("[BattleManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;
        //按钮绑定（最后一个参数是指定攻击部位，不传就按权重随机）
        randomBtn.Pressed += () => { Attack(battleInfo.character, ""); };
        handBtn.Pressed += () => { Attack(battleInfo.character, "arm");  };
        bodyBtn.Pressed += () => { Attack(battleInfo.character, "body"); };
        headBtn.Pressed += () => { Attack(battleInfo.character, "head"); };

        //进度条初始化
        positionBiasY = playerHead.Size.Y;
        positionBiasX = playerHead.Size.X / 2;
        NormalizedSpeed(4, 5);
    }
    //开战入口
    public void StartBattle()
    {
        BattleStart();
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
            playerPosition = 0;
        }
        if (enemyPosition >= 1)
        {
            battleState = BattleState.Enemy;
            enemyPosition = 0;
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
        //刷新玩家相关
        int weightSum = pm.Attack_limb_weight + pm.Attack_body_weight + pm.Attack_head_weight;
        handProp.Text = (int)Math.Round((double)pm.Attack_limb_weight / weightSum * 100) + "%";
        headProp.Text = (int)Math.Round((double)pm.Attack_head_weight / weightSum * 100) + "%";
        bodyProp.Text = (int)Math.Round((double)pm.Attack_body_weight / weightSum * 100) + "%";
        battleInfo.armProp = (int)Math.Round((double)pm.Attack_limb_weight / weightSum * 100);
        battleInfo.headProp = (int)Math.Round((double)pm.Attack_head_weight / weightSum * 100);
        battleInfo.bodyProp = (int)Math.Round((double)pm.Attack_body_weight / weightSum * 100);
        bodyDMG.Text = "伤害:" + battleInfo.bodyDamage;
        headDMG.Text = "伤害:" + battleInfo.headDamage;
        armDMG.Text = "伤害:" + battleInfo.armDamage;
        //血量和护甲保留一位小数显示，整数时不会多出 .0
        playerHP.Value = pm.Hp;
        playerHP.MaxValue = pm.MaxHp;
        playerHPLabel.Text = Math.Round(pm.Hp, 1) + "/" + Math.Round(pm.MaxHp, 1);
        playerArmor.Value = pm.Armor;
        playerArmor.MaxValue = pm.MaxArmor;
        playerArmorLabel.Text = Math.Round(pm.Armor, 1) + "/" + Math.Round(pm.MaxArmor, 1);
        //刷新战斗按钮相关
        RefreshAttackBtnUI();
        //刷新敌人相关      
        battleEnemy.RefreshUI();
    }
    private void RefreshAttackBtnUI() 
    {
        bool hasEnemy = battleEnemy != null;
        bool armAlive = hasEnemy && battleEnemy.HasAlivePart("arm");
        bool bodyAlive = hasEnemy && battleEnemy.HasAlivePart("body");
        bool headAlive = hasEnemy && battleEnemy.HasAlivePart("head");

        //按钮禁用：没有储存次数 或者 该部位的肢体全没了
        handBtn.Disabled = battleInfo.armNum == 0 || !armAlive;
        bodyBtn.Disabled = battleInfo.bodyNum == 0 || !bodyAlive;
        headBtn.Disabled = battleInfo.headNum == 0 || !headAlive;

        //masks 节点下的遮罩只表示"冷却中"：部位还在、只是没次数时才显示
        armMask.Visible = battleInfo.armNum == 0 && armAlive;
        bodyMask.Visible = battleInfo.bodyNum == 0 && bodyAlive;
        headMask.Visible = battleInfo.headNum == 0 && headAlive;

        //部位整体打没了：不显示冷却遮罩，改用代码生成的纯黑遮罩
        SetPartDeadMask("arm", hasEnemy && !armAlive);
        SetPartDeadMask("body", hasEnemy && !bodyAlive);
        SetPartDeadMask("head", hasEnemy && !headAlive);

        //手臂全没了时，额外显示"无可攻击手臂"提示
        noArm.Visible = hasEnemy && !armAlive;

        armCD.Text = battleInfo.armCD.ToString();
        bodyCD.Text = battleInfo.bodyCD.ToString(); 
        headCD.Text = battleInfo.headCD.ToString();
        armNum.Text = battleInfo.armNum.ToString();
        bodyNum.Text = battleInfo.bodyNum.ToString();
        headNum.Text = battleInfo.headNum.ToString();
    }

    #region 战斗逻辑


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
    private void PlayerTurn()
    {
        GD.Print("玩家行动");
        battleInfo.RefreshCD();
        TurnStart("player");
    }
    private void EnemyTurn()
    {
        GD.Print("敌人行动");
        TurnStart("enemy");
        Attack(battleInfo.character);
        battleState = BattleState.Moving;
    }
    private void TurnStart(string character)
    {
        OnTurnStart?.Invoke(character);
        battleInfo.character = character;
    }
    //3.进行攻击（targetPart 指定部位，不传就随机攻击）
    private void Attack(string attacker, string targetPart = "")
    {
        GD.Print("攻击");
        //敌人回合：自动攻击玩家，先扣护甲，护甲不够再扣血
        if (attacker == "enemy")
        {
            EnemyAttackPlayer();
            //敌人回合没有命中部位，清空避免把上一次的部位拿来挂状态
            battleInfo.bodyPart = "";
            OnDamageBuff?.Invoke();
            OnDamageDealed?.Invoke();
            OnStatusDealed?.Invoke();
            return;
        }
        if (attacker != "player")
        {
            return;
        }
        switch (targetPart)
        {
            case "head":
                battleInfo.headNum -= 1;
                break;
            case "body":
                battleInfo.bodyNum -= 1;
                break;
            case "arm":
                battleInfo.armNum -= 1;
                break;
            default:
                break;
        }
        if (targetPart == "")
        {
            //随机攻击：无视已经打没的部位，在剩下的部位里随机
            targetPart = GetRandomPart();
        }
        else if (!battleEnemy.HasAlivePart(targetPart))
        {
            //指定部位攻击：部位已经打没了就打不成（按钮此时应该已经被屏蔽）
            GD.Print(targetPart + "已经没有可攻击的肢体，本次攻击无效");
            return;
        }
        if (targetPart == "")
        {
            GD.Print("敌人已经没有可攻击的部位了");
            return;
        }
        //记录本次命中的部位
        battleInfo.bodyPart = targetPart;

        //随机选中该部位的一个存活肢体
        if (!battleEnemy.BeHit(targetPart))
        {
            return;
        }
        //结算伤害后的效果
        OnDamageDealed?.Invoke();
        //结算施加状态的效果
        OnStatusDealed?.Invoke();
        battleState = BattleState.Moving;
    }
    //4.回合结束
    //5.战斗结束
    public void BattleEnd()
    {
        OnBattleEnd?.Invoke();
    }

    //战斗相关方法
    //部位整体打没时，在按钮上盖一层代码生成的纯黑遮罩（show=true 显示）
    private void SetPartDeadMask(string part, bool show)
    {
        Button btn = GetAttackBtn(part);
        if (btn == null)
        {
            return;
        }
        //第一次用到时才生成，之后一直复用同一个
        if (!atkMaskDic.ContainsKey(part))
        {
            //按钮的父节点就是那张按钮背景卡片，遮罩盖在它上面正好只挡这一个按钮
            atkMaskDic[part] = CreateBlackMask(btn.GetParent<Control>());
        }
        atkMaskDic[part].Visible = show;
    }
    //按部位取对应的攻击按钮
    private Button GetAttackBtn(string part)
    {
        switch (part)
        {
            case "arm":
                return handBtn;
            case "body":
                return bodyBtn;
            case "head":
                return headBtn;
            default:
                return null;
        }
    }
    //在一个节点上生成一个黑色半透明遮罩，默认隐藏
    private ColorRect CreateBlackMask(Control parent)
    {
        ColorRect mask = new ColorRect();
        mask.Color = new Color(0, 0, 0, 0.8f);
        mask.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mask.MouseFilter = Control.MouseFilterEnum.Ignore;
        mask.Visible = false;
        parent.AddChild(mask);
        return mask;
    }
   
    //敌人攻击玩家：优先扣护甲，护甲不足的部分才扣血
    private void EnemyAttackPlayer()
    {
        //伤害保留一位小数，避免浮点尾巴
        double damage = Math.Round(battleInfo.enemyDamage, 1);
        double left = damage;
        //先扣护甲
        if (pm.Armor > 0)
        {
            double armorBefore = pm.Armor;
            pm.AddItem(10010, -Math.Round(Math.Min(pm.Armor, left), 1));
            //按实际扣掉的护甲算，避免护甲带状态加成时被多扣
            double realAbsorb = Math.Max(0, Math.Round(armorBefore - pm.Armor, 1));
            left = Math.Round(left - realAbsorb, 1);
            GD.Print("敌人攻击" + damage + "，护甲抵挡" + realAbsorb);
        }
        else
        {
            GD.Print("敌人攻击" + damage + "，没有护甲");
        }
        //护甲不够的部分扣血
        if (left > 0)
        {
            pm.AddItem(10001, -left);
            GD.Print("玩家扣血" + left + "，剩余生命" + pm.Hp);
        }
        RefreshUI();
    }
    //按权重随机一个还有可用肢体的部位
    private string GetRandomPart()
    {
        List<string> parts = new List<string>();
        List<int> weights = new List<int>();
        if (battleEnemy.HasAlivePart("head")) { parts.Add("head"); weights.Add(battleInfo.headWeight); }
        if (battleEnemy.HasAlivePart("body")) { parts.Add("body"); weights.Add(battleInfo.bodyWeight); }
        if (battleEnemy.HasAlivePart("arm")) { parts.Add("arm"); weights.Add(battleInfo.armWeight); }
        if (parts.Count == 0)
        {
            return "";
        }
        //拿到的是部位在列表里的下标
        int index = Tools.GetRandomNumber(new List<int>() { 0, 1, 2 }, weights);
        return parts[index];
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
                damageBonus.bodyPart = battleEffect.Part;
                damageBonus.amount = battleEffect.Amount;
                break;
            case "WeightBonus":
                WeightBonus weightBonus = new WeightBonus();
                battleEffectBase = weightBonus;
                weightBonus.bodyPart = battleEffect.Part;
                weightBonus.amount = battleEffect.Amount;
                break;
            case "ChargeBonus":
                break;
            case "ProgressBonus":
                break;
            case "ApplyMultipleStatus":
                ApplyMultipleStatus applyMultipleStatus = new ApplyMultipleStatus();
                battleEffectBase = applyMultipleStatus;
                applyMultipleStatus.bodyPart = battleEffect.Part;
                applyMultipleStatus.amount = battleEffect.Amount;
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

    #endregion 
}
