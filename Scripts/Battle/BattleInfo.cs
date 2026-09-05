using Godot;
using MyProject;
using System;
using System.Collections.Generic;

public class BattleInfo
{
    private BattleManager bm = BattleManager.Instance;
    private PlayerManager pm = PlayerManager.Instance;
    private ConfigManager cm = ConfigManager.Instance;

    public int enemyID;                 //敌人ID
    public string bodyPart;             //攻击后取得的身体部位
    public double baseDamage;           //基础武器伤害
    public double Damage = 0;           //造成的最终伤害
    public string character = "player"; //当前角色
    public List<int> playerEffects = new List<int>();//玩家所有效果

    public BattleInfo(int ID)
    {
        LoadEquipEffect();
        enemyID = ID;
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
}
