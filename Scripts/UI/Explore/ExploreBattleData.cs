// 探索流程里的战斗状态容器。
//
// 说明：旧探索流程（ExploreUI.tscn / RoomChooseBar / RoomChoose / RoomProgress /
// ExploreChooseBar / ExploreOptionCard / EventChooseBar / EventChooseBtn / ExploreIcon
// 及对应场景）已于 2026-11-13 整体删除，备份在 Backup/old_explore_20261113.zip。
//
// 这个类保留下来，是因为战斗系统 BattleInfo / BattleManager 仍按这个类型持有它，
// 用来跨战斗保留各部位攻击的 CD 与储存量（见 BattleInfo 构造函数与 RefreshCD）。
// 它不再是一个界面（ExploreUI.tscn 已删除），只是一个纯数据类。
// 改名为 ExploreBattleData（不再叫 ExploreUI、不继承 Control、没有 .uid），
// 这样 Godot 的 C# 源生成器不会为它生成 ScriptProperties 代码，避免残留缓存冲突。
//
// 新探索流程以后接战斗时：由新的战斗入口给 BattleManager.exploreUI 赋一个新实例，
// 就能复用这套 CD 保留逻辑；不赋值时 BattleInfo 走自己的默认值，不影响战斗。
public class ExploreBattleData
{
    public int bodyCD = 2;   // 身体冷却
    public int headCD = 4;   // 头部冷却
    public int armCD = 2;    // 手部冷却
    public int bodyNum = 2;  // 身体攻击当前储存量
    public int headNum = 2;  // 头部攻击当前储存量
    public int armNum = 2;   // 手部攻击当前储存量
}
