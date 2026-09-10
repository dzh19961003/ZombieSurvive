using Godot;
using MyProject;
using System.Collections.Generic;

public class BattleInfo
{
    private BattleManager bm = BattleManager.Instance;
    private PlayerManager pm = PlayerManager.Instance;
    private ConfigManager cm = ConfigManager.Instance;

    public ExploreUI exploreUI;         //探索脚本
    public int enemyID;                 //敌人ID
    public string bodyPart;             //攻击后取得的身体部位
    public double baseDamage;           //基础武器伤害
    public double damage = 0;           //造成的最终伤害
    public string character = "player"; //当前角色
    public List<int> playerEffects = new List<int>();//玩家所有效果
    public Dictionary<string, int> playerStatusNumDic = new Dictionary<string, int>();//拥有的效果层数
    public int bodyWeight = 0;          //身体权重
    public int headWeight = 0;          //头部权重          
    public int armWeight = 0;           //手臂权重
    public int armProp = 0;             //身体概率
    public int headProp = 0;            //头部概率 
    public int bodyProp = 0;            //手臂概率
    public double bodyDamage = 0;       //对身体伤害
    public double headDamage = 0;       //对头部伤害 
    public double armDamage = 0;        //对手臂伤害
    public int bodyCDMax = 2;           //身体冷却最大值
    public int headCDMax = 4;           //头部冷却最大值
    public int armCDMax = 2;            //手部冷却最大值
    public int bodyCD = 2;              //身体冷却当前值
    public int headCD = 4;              //头部冷却当前值
    public int armCD = 2;               //手部冷却当前值
    public int bodyMax = 2;             //身体攻击最大储存量
    public int headMax = 2;             //头部攻击最大储存量
    public int armMax = 2;              //手部攻击最大储存量
    public int bodyNum = 0;             //身体攻击当前储存量
    public int headNum = 0;             //头部攻击当前储存量
    public int armNum = 0;              //手部攻击当前储存量
    public double enemyDamage=8.2;      //敌人基础攻击

    public BattleInfo(int ID)
    {
        exploreUI = bm.exploreUI;
        LoadEquipEffect();
        bodyWeight = pm.Attack_body_weight;
        headWeight = pm.Attack_head_weight;
        armWeight = pm.Attack_limb_weight;
        bodyDamage = pm.Strength;
        headDamage = pm.Strength;
        armDamage = pm.Strength;
        enemyID = ID;
        //初始化各攻击CD状态（没有 exploreUI 时，沿用上面声明的默认值）
        if (exploreUI != null)
        {
            bodyCD = exploreUI.bodyCD;
            headCD = exploreUI.headCD;
            armCD = exploreUI.armCD;
            bodyNum = exploreUI.bodyNum;
            headNum = exploreUI.headNum;
            armNum = exploreUI.armNum;
        }
    }
    //加载玩家装备及天赋效果
    private void LoadEquipEffect()
    {
        int weaponID = cm.equipDic[cm.itemDic[pm.weaponID].EquipID].BattleType;
        int clothesID = cm.equipDic[cm.itemDic[pm.clothesID].EquipID].BattleType;
        int shoesID = cm.equipDic[cm.itemDic[pm.shoesID].EquipID].BattleType;
        int ringID = cm.equipDic[cm.itemDic[pm.ringID].EquipID].BattleType;

        if (weaponID != 0) playerEffects.Add(weaponID);
        if (clothesID != 0) playerEffects.Add(clothesID);
        if (shoesID != 0) playerEffects.Add(shoesID);
        if (ringID != 0) playerEffects.Add(ringID);

        foreach (var item in pm.GetTalentID())
        {
            if (cm.talentDic[item].BattleEffect!=0)
            {
                playerEffects.Add(cm.talentDic[item].BattleEffect);
            }
        }
        bm.LoadBattleEffect(playerEffects, "player");
    }
    public void RefreshCD()
    {
        //CD到了则增加一次使用次数
        if (bodyNum < bodyMax)
        {
            bodyCD -= 1;
            if (bodyCD==0)
            {
                bodyNum += 1;
                bodyCD = bodyCDMax;
            }
        }
        if (headNum < headMax)
        {
            headCD -= 1;
            if (headCD == 0)
            {
                headNum += 1;
                headCD = headCDMax;
            }
        }
        if (armNum < armMax)
        {
            armCD -= 1;
            if (armCD == 0)
            {
                armNum += 1;
                armCD = armCDMax;
            }
        }
        //把CD和储存量写回探索脚本，保证跨战斗保留（没有 exploreUI 时跳过）
        if (exploreUI != null)
        {
            exploreUI.bodyCD = bodyCD;
            exploreUI.headCD = headCD;
            exploreUI.armCD = armCD;
            exploreUI.bodyNum = bodyNum;
            exploreUI.headNum = headNum;
            exploreUI.armNum = armNum;
        }
        BattleManager.Instance.RefreshUI();
    }
}
