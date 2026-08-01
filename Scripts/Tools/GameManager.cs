using Godot;
using Godot.Collections;
using MyProject;
using System;

public partial class GameManager : Node2D,ISaveable
{
    /*当前游戏环节
    
    写日记 = 1,
    随机事件 = 2,
    早晨 = 3,
    探索 = 4,
    夜晚 = 5 */
    public int gameState = 1;

    /*当前探索环节
    
    选择房间 = 1,
    选择事件 = 2，
    探索完毕 = 3 */
    public int exploreState = 1;

    //当期建筑ID
    public int currentBuildingID = 0;

    //当前探索层级，与房间所在层级相同
    public int exploreLayer = 2;

    //已完成的支线
    public Array<int> finishedEvent = new Array<int>();

    //各建筑探索进度
    public Dictionary<int, int> exploreProgress = new Dictionary<int, int>();

    public static GameManager Instance { get; private set; }

    #region 存档相关
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
    #endregion

    public override void _Ready()
	{
        if (Instance != null)
        {
            GD.PrintErr("[GameManager] 单例已存在，重复创建！");
            QueueFree();
            return;
        }
        Instance = this;

        this.AddToGroup("Save");
    }

}
