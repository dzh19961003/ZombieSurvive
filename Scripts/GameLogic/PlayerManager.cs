// ============================================================
//  PlayerManager — 玩家数据管理（使用 SaveManager 的完整示例）
//
//  要参与存档只需要做三件事：
//   1. 实现 ISaveable 接口
//   2. 在 _Ready() 里 AddToGroup("Persist")
//   3. 写 GetSaveData() 和 LoadSaveData()
// ============================================================

using Godot;
using Godot.Collections;
using MyProject;
using System.Collections.Generic;

public partial class PlayerManager : Node, ISaveable
{
    public static PlayerManager Instance { get; private set; }
    public string SaveKey => GetPath();


    private int HP = 100;
    private int maxHP = 100;


    private Array<int> ItemArray=new Array<int>() { 2,3};

    public void GetItem(int id) 
    {
        
    }
    //获得天赋：ID
    //获得状态：ID

    #region 存档相关
    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("[PlayerManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;

        AddToGroup("Save");
        SaveManager.Instance.Save();
        SaveManager.Instance.Load();
    }
    public Dictionary GetSaveData()
    {
        return new Dictionary
        {
            { "hp", HP},
            { "maxHP",maxHP},
            { "ItemArray", ItemArray},
        };
    }
    public void LoadSaveData(Dictionary data)
    {
        HP = data.ContainsKey("hp")    ? (int)data["hp"]    : 100;
        maxHP = data.ContainsKey("maxHP")  ? (int)data["maxHP"]  : 100;
        ItemArray = data.ContainsKey("ItemArray") ? (Array<int>)data["ItemArray"] : new Array<int>{1,2};

        GD.Print($"[PlayerManager] 数据恢复完成：HP={HP}, maxHP={maxHP}, ItemArray={ItemArray}");
    }
    #endregion
}
