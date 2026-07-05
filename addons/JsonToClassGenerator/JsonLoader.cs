using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;

namespace MyProject.Tools;

/// <summary>
/// JSON 配置加载工具类。
/// 通过文件名（不含扩展名）在项目 res:// 下全局检索对应的 .json 文件，
/// 并将其反序列化为指定类型的 List 或 Dictionary 返回。
/// </summary>
public static class JsonLoader
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 根据文件名（不含扩展名）全局检索 JSON 文件，
    /// 并将内容反序列化为 List&lt;T&gt;。
    /// </summary>
    /// <typeparam name="T">目标数据类型</typeparam>
    /// <param name="jsonName">JSON 文件名，可带或不带 .json 扩展名</param>
    /// <returns>反序列化后的 List&lt;T&gt;，失败时返回 null</returns>
    public static List<T> LoadToList<T>(string jsonName)
    {
        if (string.IsNullOrWhiteSpace(jsonName))
        {
            GD.PrintErr("[JsonLoader] jsonName 为空");
            return null;
        }

        // 统一去掉扩展名，只保留文件名部分用于匹配
        string fileName = jsonName.Trim();
        if (fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            fileName = fileName[..^5];

        string resPath = FindJsonPath("res://", fileName);
        if (resPath == null)
        {
            GD.PrintErr($"[JsonLoader] 未找到名为 \"{jsonName}\" 的 JSON 文件");
            return null;
        }

        // 通过 Godot 资源系统读取文件内容，兼容导出版本
        using var file = Godot.FileAccess.Open(resPath, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"[JsonLoader] 无法打开文件: {resPath}");
            return null;
        }

        string jsonText = file.GetAsText();

        try
        {
            List<T> result = JsonSerializer.Deserialize<List<T>>(jsonText, s_options);
            if (result == null)
            {
                GD.PrintErr($"[JsonLoader] 反序列化结果为 null: {resPath}");
                return null;
            }
            return result;
        }
        catch (JsonException ex)
        {
            GD.PrintErr($"[JsonLoader] 反序列化失败 ({resPath}): {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 根据文件名（不含扩展名）全局检索 JSON 文件，
    /// 并将内容反序列化为 Dictionary&lt;int, T&gt;。
    /// 自动读取类 T 中名为 "ID"（大小写不敏感）的属性作为字典键。
    /// 若某条记录的 ID 属性无法转为 int，则跳过该条并在控制台发出警告。
    /// </summary>
    /// <typeparam name="T">目标数据类型，需包含名为 ID 的 int 类型属性</typeparam>
    /// <param name="jsonName">JSON 文件名，可带或不带 .json 扩展名</param>
    /// <returns>以 ID 为键、实例为值的字典，失败时返回 null</returns>
    public static Dictionary<int, T> LoadToDic<T>(string jsonName)
    {
        if (string.IsNullOrWhiteSpace(jsonName))
        {
            GD.PrintErr("[JsonLoader] jsonName 为空");
            return null;
        }

        // 统一去掉扩展名
        string fileName = jsonName.Trim();
        if (fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            fileName = fileName[..^5];

        string resPath = FindJsonPath("res://", fileName);
        if (resPath == null)
        {
            GD.PrintErr($"[JsonLoader] 未找到名为 \"{jsonName}\" 的 JSON 文件");
            return null;
        }

        using var file = Godot.FileAccess.Open(resPath, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"[JsonLoader] 无法打开文件: {resPath}");
            return null;
        }

        string jsonText = file.GetAsText();

        // 先反序列化为 List<T>
        List<T> list;
        try
        {
            list = JsonSerializer.Deserialize<List<T>>(jsonText, s_options);
            if (list == null)
            {
                GD.PrintErr($"[JsonLoader] 反序列化结果为 null: {resPath}");
                return null;
            }
        }
        catch (JsonException ex)
        {
            GD.PrintErr($"[JsonLoader] 反序列化失败 ({resPath}): {ex.Message}");
            return null;
        }

        // 查找类 T 中名为 "ID" 的属性（大小写不敏感）
        PropertyInfo idProp = typeof(T).GetProperty("ID",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (idProp == null)
        {
            GD.PrintErr($"[JsonLoader] 配置表 \"{typeof(T).Name}\" 未在第一列配置ID（类中无 ID 属性）");
            return null;
        }

        // 将 List 转为 Dictionary，以 ID 为键
        // ID 属性支持 int 及可隐式转为 int 的数值类型（double、long、short 等）
        var dict = new Dictionary<int, T>();
        foreach (T item in list)
        {
            object idValue = idProp.GetValue(item);
            if (idValue == null)
            {
                GD.PrintErr($"[JsonLoader] 配置表 \"{typeof(T).Name}\" 未在第一列配置ID（ID 值为 null）");
                continue;
            }

            int key;
            try
            {
                key = Convert.ToInt32(idValue);
            }
            catch (Exception)
            {
                GD.PrintErr($"[JsonLoader] 配置表 \"{typeof(T).Name}\" 未在第一列配置ID（ID 值 \"{idValue}\" 无法转为 int）");
                continue;
            }

            dict[key] = item;
        }

        return dict;
    }

    /// <summary>
    /// 将目录路径和子目录/文件名拼接为合法的 res:// 路径。
    /// 保证 res:// 中的双斜杠不被 TrimEnd 破坏。
    /// </summary>
    private static string CombinePath(string dirPath, string entry)
    {
        // res:// 是 Godot 的特殊协议前缀，需要保留双斜杠
        if (dirPath == "res://")
            return "res://" + entry;
        // 其他路径正常拼接，确保只有一个斜杠分隔
        return dirPath.TrimEnd('/') + "/" + entry;
    }

    /// <summary>
    /// 在指定目录下递归查找第一个文件名匹配的 .json 文件。
    /// 每层递归都重新创建独立的 DirAccess，避免嵌套 ListDirBegin 冲突。
    /// </summary>
    private static string FindJsonPath(string dirPath, string fileName)
    {
        // 先收集当前目录下的条目（关闭列举后再进入子目录，避免嵌套冲突）
        var fileEntries = new List<string>();
        var subDirs = new List<string>();

        using (var dir = DirAccess.Open(dirPath))
        {
            if (dir == null)
            {
                GD.PrintErr($"[JsonLoader] 无法打开目录: {dirPath}");
                return null;
            }

            dir.ListDirBegin();
            string entry = dir.GetNext();
            while (entry != string.Empty)
            {
                if (entry == "." || entry == "..")
                {
                    entry = dir.GetNext();
                    continue;
                }

                if (dir.CurrentIsDir())
                {
                    // 跳过 .godot 等隐藏目录，减少无意义扫描
                    if (!entry.StartsWith(".", StringComparison.Ordinal))
                        subDirs.Add(entry);
                }
                else
                {
                    fileEntries.Add(entry);
                }

                entry = dir.GetNext();
            }
            dir.ListDirEnd();
        }

        // 先检查当前目录的文件
        foreach (string entry in fileEntries)
        {
            if (entry.Equals(fileName + ".json", StringComparison.OrdinalIgnoreCase))
                return CombinePath(dirPath, entry);
        }

        // 再递归搜索子目录
        foreach (string sub in subDirs)
        {
            string subPath = CombinePath(dirPath, sub);
            string found = FindJsonPath(subPath, fileName);
            if (found != null)
                return found;
        }

        return null;
    }
}
