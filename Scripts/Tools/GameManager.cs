using Godot;
using Godot.Collections;
using MyProject;
using System;

public partial class GameManager : Node2D,ISaveable
{
    //当前游戏环节
    //写日记 = 1,
    //随机事件 = 2,
    //早晨 = 3,
    //探索 = 4,
    //夜晚 = 5
    public int gameState = 1;

    public static GameManager Instance { get; private set; }

    public string SaveKey => GetPath();

    public Dictionary GetSaveData()
    {
        return new Dictionary
        {
            { "gameState", gameState }
        };
    }

    public void LoadSaveData(Dictionary data)
    {
        
    }

    public override void _Ready()
	{
        if (Instance != null)
        {
            GD.PrintErr("[ConfigManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;

        this.AddToGroup("Save");
    }

}
