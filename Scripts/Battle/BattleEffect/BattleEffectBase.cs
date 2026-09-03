using Godot;
using System;

public partial class BattleEffectBase : Node
{
    public BattleManager bm;
    public PlayerManager pm = PlayerManager.Instance;
    public string character;
    public string body;
    public int bonus;

    private bool isSubscribed = false;
    

    public override void _Ready()
    {
        SubscribeToEvents();
    }

    public override void _ExitTree()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (isSubscribed) return;

        bm = BattleManager.Instance;
        bm.OnBattleStart += BattleStart;
        bm.OnTurnStart += TurnStart;
        bm.OnDamageBuff += DamageBuff;
        bm.OnDamageDealed += DamageDealed;
        bm.OnStatusDealed += StatusDealed;
        bm.OnTurnEnd += TurnEnd;
        bm.OnBattleEnd += BattleEnd;

        isSubscribed = true;
    }

    private void UnsubscribeFromEvents()
    {
        if (!isSubscribed || bm == null) return;

        bm.OnBattleStart -= BattleStart;
        bm.OnTurnStart -= TurnStart;
        bm.OnDamageBuff -= DamageBuff;
        bm.OnDamageDealed -= DamageDealed;
        bm.OnStatusDealed -= StatusDealed;
        bm.OnTurnEnd -= TurnEnd;
        bm.OnBattleEnd -= BattleEnd;

        isSubscribed = false;
    }

    public virtual void BattleStart() { }
    public virtual void TurnStart() { }
    public virtual void DamageBuff() {  }
    public virtual void DamageDealed() { }
    public virtual void StatusDealed() { }
    public virtual void TurnEnd() { }
    public virtual void BattleEnd() { }
    public virtual void MultyStatusAdd(){ }
}
