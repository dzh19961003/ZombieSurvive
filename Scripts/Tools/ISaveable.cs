using Godot.Collections;

// ============================================================
//  ISaveable — 存档接口
//  任何需要参与存档/读档的脚本，实现这两个方法即可
//
//  使用方法（超简单）：
//   1. 让你的脚本实现 ISaveable 接口
//   2. 在 _Ready() 里 AddToGroup("Persist") —— 就这一行！
//   3. GetSaveData() 返回你要保存的数据（Dictionary）
//   4. LoadSaveData() 用 GetValueOrDefault 恢复数据，一行一个字段
//
//  完整示例（PlayerManager）：
//   public partial class PlayerManager : Node, ISaveable
//   {
//       public string SaveKey => GetPath();  // 自动用节点路径，不用起名
//
//       public override void _Ready()
//       {
//           AddToGroup("Persist");  // 就这一行
//       }
//
//       public Dictionary GetSaveData()
//       {
//           return new Dictionary { { "hp", _hp }, { "gold", _gold } };
//       }
//
//       public void LoadSaveData(Dictionary data)
//       {
//           _hp   = data.ContainsKey("hp")   ? (int)data["hp"]   : 100;
//           _gold = data.ContainsKey("gold") ? (int)data["gold"] : 0;
//       }
//   }
// ============================================================

public interface ISaveable
{
    // ─────────────────────────────────────────────────────────
    //  SaveKey：存档文件里区分不同数据块的唯一标识
    //  直接用 GetPath() 返回节点路径即可，不用手动起名
    //  比如 "/root/Main/PlayerManager"
    // ─────────────────────────────────────────────────────────
    string SaveKey { get; }

    // ─────────────────────────────────────────────────────────
    //  GetSaveData：返回当前需要保存的所有数据
    // ─────────────────────────────────────────────────────────
    Dictionary GetSaveData();

    // ─────────────────────────────────────────────────────────
    //  LoadSaveData：用存档数据恢复游戏状态
    //  建议用 ContainsKey + 三元表达式简化取值，如：
    //    _hp = data.ContainsKey("hp") ? (int)data["hp"] : 100;
    // ─────────────────────────────────────────────────────────
    void LoadSaveData(Dictionary data);
}
