// ============================================================
//  PlayerManager — 玩家数据管理（使用 SaveManager 的完整示例）
//
//  要参与存档只需要做三件事：
//   1. 实现 ISaveable 接口
//   2. 在 _Ready() 里 AddToGroup("Persist")
//   3. 写 GetSaveData() 和 LoadSaveData()
//  不用 Register、不用 Unregister、不用手动起名。
// ============================================================

using Godot;
using Godot.Collections;

public partial class PlayerManager : Node, ISaveable
{
    // ─────────────────────────────────────────────────────────
    //  SaveKey：直接用节点路径作为唯一标识
    //  比如 "/root/Main/PlayerManager"，完全不用动脑
    // ─────────────────────────────────────────────────────────
    public string SaveKey => GetPath();

    // ─────────────────────────────────────────────────────────
    //  玩家数据
    // ─────────────────────────────────────────────────────────
    private int _hp = 100;
    private int _gold = 0;
    private int _level = 1;

    public override void _Ready()
    {
        // 加入分组就完事了，SaveManager 会自动发现你
        AddToGroup("Persist");
        SaveManager.Instance.Save();
        SaveManager.Instance.Load();
    }

    // ─────────────────────────────────────────────────────────
    //  GetSaveData：打包当前数据
    // ─────────────────────────────────────────────────────────
    public Dictionary GetSaveData()
    {
        return new Dictionary
        {
            { "hp",    _hp    },
            { "gold",  _gold  },
            { "level", _level },
        };
    }

    // ─────────────────────────────────────────────────────────
    //  LoadSaveData：从存档恢复数据
    //  用 ContainsKey 判断 + 默认值兜底，一行一个字段
    // ─────────────────────────────────────────────────────────
    public void LoadSaveData(Dictionary data)
    {
        _hp    = data.ContainsKey("hp")    ? (int)data["hp"]    : 100;
        _gold  = data.ContainsKey("gold")  ? (int)data["gold"]  : 0;
        _level = data.ContainsKey("level") ? (int)data["level"] : 1;

        GD.Print($"[PlayerManager] 数据恢复完成：HP={_hp}, Gold={_gold}, Level={_level}");
    }
}
