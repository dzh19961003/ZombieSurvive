using Godot;
using Godot.Collections;

// ============================================================
//  SaveManager — 单存档管理器（单例）
//  挂在 Main.tscn 的某个节点上，负责整个游戏的存档与读档
//
//  核心流程（基于 Godot 分组自动发现）：
//   1. 需要存档的脚本在 _Ready() 里 AddToGroup("Persist")
//   2. 任何时候调用 Save() → 自动扫描 "Persist" 分组 → 合并数据 → 写 JSON
//   3. 任何时候调用 Load() → 读 JSON → 自动扫描分组 → 分发给各对象
//
//  你完全不需要手动 Register / Unregister / 起名。
// ============================================================

public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }

    // ─────────────────────────────────────────────────────────
    //  存档文件路径（user:// 是 Godot 的用户数据目录）
    // ─────────────────────────────────────────────────────────
    private const string SAVE_FILE_PATH = "user://save.json";

    // ─────────────────────────────────────────────────────────
    //  存档版本号。以后改数据结构时 +1，可以判断旧存档是否兼容
    // ─────────────────────────────────────────────────────────
    private const int SAVE_VERSION = 1;

    // ─────────────────────────────────────────────────────────
    //  用于扫描存档对象的分组名
    // ─────────────────────────────────────────────────────────
    private const string PERSIST_GROUP = "Persist";

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("[SaveManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;
        GD.Print("[SaveManager] 初始化完成");
    }

    // =========================================================
    //  存档 / 读档
    // =========================================================

    /// <summary>
    /// 保存游戏。自动扫描 "Persist" 分组的所有节点，
    /// 收集数据，合并写入一个 JSON 文件。
    /// 返回 true 表示成功，false 表示失败。
    /// </summary>
    public bool Save()
    {
        // ---------- 1. 自动扫描分组，找到所有存档节点 ----------
        var nodes = GetTree().GetNodesInGroup(PERSIST_GROUP);

        // ---------- 2. 构建存档根结构 ----------
        var root = new Dictionary();

        // 元信息
        var meta = new Dictionary
        {
            { "version", SAVE_VERSION },
            { "save_time", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };
        root["meta"] = meta;

        // ---------- 3. 收集各节点的数据 ----------
        var dataBlock = new Dictionary();
        foreach (var node in nodes)
        {
            if (node is ISaveable saveable)
            {
                dataBlock[saveable.SaveKey] = saveable.GetSaveData();
            }
        }
        root["data"] = dataBlock;

        // ---------- 4. 序列化并写入文件 ----------
        try
        {
            string jsonStr = Json.Stringify(root, "\t");

            using var file = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PrintErr("[SaveManager] 无法打开存档文件进行写入：" + SAVE_FILE_PATH);
                return false;
            }

            file.StoreString(jsonStr);
            GD.Print("[SaveManager] 存档保存成功：" + SAVE_FILE_PATH);
            return true;
        }
        catch (System.Exception e)
        {
            GD.PrintErr("[SaveManager] 保存存档时出错：" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 读取存档。先检查文件是否存在，然后自动扫描 "Persist" 分组，
    /// 把数据分发给各节点恢复状态。
    /// 返回 true 表示成功，false 表示失败（无存档或数据损坏）。
    /// </summary>
    public bool Load()
    {
        // ---------- 1. 检查文件是否存在 ----------
        if (!FileAccess.FileExists(SAVE_FILE_PATH))
        {
            GD.Print("[SaveManager] 没有找到存档文件，可能是新游戏");
            return false;
        }

        // ---------- 2. 读取并解析 JSON ----------
        string jsonStr;
        try
        {
            using var file = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Read);
            jsonStr = file.GetAsText();
        }
        catch (System.Exception e)
        {
            GD.PrintErr("[SaveManager] 读取存档文件失败：" + e.Message);
            return false;
        }

        var parsed = Json.ParseString(jsonStr);
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            GD.PrintErr("[SaveManager] 存档格式错误，不是有效的 JSON 对象");
            return false;
        }

        var root = parsed.AsGodotDictionary();

        // ---------- 3. 检查版本号 ----------
        if (root.ContainsKey("meta"))
        {
            var meta = root["meta"].AsGodotDictionary();
            if (meta.ContainsKey("version"))
            {
                int fileVersion = meta["version"].AsInt32();
                if (fileVersion != SAVE_VERSION)
                {
                    GD.Print($"[SaveManager] 注意：存档版本 {fileVersion} 与当前版本 {SAVE_VERSION} 不同");
                }
            }
        }

        // ---------- 4. 分发数据给各存档节点 ----------
        if (!root.ContainsKey("data"))
        {
            GD.PrintErr("[SaveManager] 存档文件中没有 data 字段");
            return false;
        }

        var dataBlock = root["data"].AsGodotDictionary();

        var nodes = GetTree().GetNodesInGroup(PERSIST_GROUP);
        foreach (var node in nodes)
        {
            if (node is ISaveable saveable)
            {
                if (dataBlock.ContainsKey(saveable.SaveKey))
                {
                    saveable.LoadSaveData(dataBlock[saveable.SaveKey].AsGodotDictionary());
                    GD.Print($"[SaveManager] 数据已恢复：{saveable.SaveKey}");
                }
                else
                {
                    GD.Print($"[SaveManager] 存档中没有 {saveable.SaveKey} 的数据，已跳过");
                }
            }
        }

        GD.Print("[SaveManager] 存档读取完成");
        return true;
    }

    // =========================================================
    //  工具方法
    // =========================================================

    /// <summary>
    /// 检查是否存在存档文件
    /// </summary>
    public bool HasSaveFile()
    {
        return FileAccess.FileExists(SAVE_FILE_PATH);
    }

    /// <summary>
    /// 删除存档文件（比如玩家选择"重新开始"）
    /// </summary>
    public void DeleteSaveFile()
    {
        if (FileAccess.FileExists(SAVE_FILE_PATH))
        {
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(SAVE_FILE_PATH));
            GD.Print("[SaveManager] 存档已删除");
        }
    }
}
