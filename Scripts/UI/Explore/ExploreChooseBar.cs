using Godot;
using Godot.Collections;
using MyProject;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;


public partial class ExploreChooseBar : NinePatchRect
{
    [Export] public TextureButton backBtn;
    [Export] public TextureButton carefulExploreBtn;
    [Export] public TextureButton quicklyExploreBtn;
    [Export] public Label dangerLabel;

    public ExploreUI exploreUI;
    private GameManager gameManager;
    private bool progress=true;
    bool include = true;
    private int danger=3;
    public override void _Ready()
    {
        // 中途撤离按钮：弹确认框，确认后交给 ExploreUI 统一销毁所有探索界面
        backBtn.Pressed += OnBack;
        carefulExploreBtn.Pressed += () => 
        {
            explore(1); 
        };
        quicklyExploreBtn.Pressed += () => 
        {
            explore(2); 
        };
        gameManager = GameManager.Instance;
        
    }

    public void Init(ExploreUI owner,bool finish)
    {
        //根据权重取得风险
        danger = Tools.GetRandomNumber(Consts.leaveDanger, Consts.leaveDangerWeight);

        //探索度大于90时，固定为低风险
        if (gameManager.exploreProgress.TryGetValue(gameManager.roomID, out int d)) 
        {
            if (d >= 90)
            {
                danger = 1;
            }
        }

        switch (danger)
        {
            case 1:
                dangerLabel.Text = "低";
                dangerLabel.AddThemeColorOverride("font_color", Colors.Green);
                break;
            case 2:
                dangerLabel.Text = "中";
                dangerLabel.AddThemeColorOverride("font_color", Colors.Yellow);
                break;
            case 3:
                dangerLabel.Text = "高";
                dangerLabel.AddThemeColorOverride("font_color", Colors.Red);
                break;
            default:
                break;
        }
        exploreUI = owner;
        carefulExploreBtn.Visible = !finish;
        quicklyExploreBtn.Visible = !finish;
    }

    private void explore(int type) 
    {
        EventChooseBar eventChooseBar=(EventChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/EventChooseBar.tscn");
        Dictionary<int, int> explorePogress = gameManager.exploreProgress;

        int eventID=1;
        //仔细探索
        if (type==1)
        {
            eventID=Tools.GetRandomNumber(gameManager.carefulEventArray);
        }
        //快速探索
        else
        {
            eventID=Tools.GetRandomNumber(gameManager.quickEventArray);
        }

        //赋值当前事件ID
        GameManager.Instance.currentEventID = eventID;

        //如果该房间支线未触发且达到触发进度，则必定触发该支线

        Array<int> subTaskArray = new Array<int>();
        if (!gameManager.subTaskDic.ContainsKey(gameManager.roomID))
        {
            gameManager.subTaskDic.Add(gameManager.roomID, subTaskArray);
        }
        else
        {
            subTaskArray = gameManager.subTaskDic[gameManager.roomID];
        }
        //取出应该触发的支线事件ID
        if (ConfigManager.Instance.roomDic[gameManager.roomID].SubTask.Count > 0 && !subTaskArray.Contains(ConfigManager.Instance.roomDic[gameManager.roomID].SubTask[0]))
        {
            if (explorePogress[(gameManager.roomID)] >= ConfigManager.Instance.roomDic[gameManager.roomID].TriggerProgress[0])
            {
                eventID = ConfigManager.Instance.roomDic[gameManager.roomID].SubTask[0];
            }
        }
        if (ConfigManager.Instance.roomDic[gameManager.roomID].SubTask.Count > 1 && !subTaskArray.Contains(ConfigManager.Instance.roomDic[gameManager.roomID].SubTask[1]))
        {
            if (explorePogress[(gameManager.roomID)] >= ConfigManager.Instance.roomDic[gameManager.roomID].TriggerProgress[1])
            {
                eventID = ConfigManager.Instance.roomDic[gameManager.roomID].SubTask[1];
            }
        }
        //如果是普通支线，直接标记为完成
        if (ConfigManager.Instance.exploreEventDic[eventID].EventType == 102)
        {
            progress = false;
            subTaskArray.Add(eventID);
            gameManager.subTaskDic[gameManager.roomID] = subTaskArray;
        }
        //如果是重要支线，暂不标记，仅赋值给当前支线
        if (ConfigManager.Instance.exploreEventDic[eventID].EventType == 103)
        {
            progress = false;
            gameManager.currentSubTask = eventID;
        }

        //不是支线再增加进度
        if (progress==true)
        {
            if (type == 1)
            {               
                if (explorePogress.ContainsKey(gameManager.roomID))
                {
                    explorePogress[(gameManager.roomID)] += Tools.GetRandomNumber(Consts.carefulExploreProgress);
                }
                else
                {
                    explorePogress[(gameManager.roomID)] = Tools.GetRandomNumber(Consts.carefulExploreProgress);
                }
                gameManager.exploreNoise += Tools.GetRandomNumber(Consts.carefulNoiseProgress);
            }
            //快速探索
            else
            {
                if (explorePogress.ContainsKey(gameManager.roomID))
                {
                    explorePogress[(gameManager.roomID)] += Tools.GetRandomNumber(Consts.quickExploreProgress);
                }
                else
                {
                    explorePogress[(gameManager.roomID)] = Tools.GetRandomNumber(Consts.quickExploreProgress);
                }

                gameManager.exploreNoise += Tools.GetRandomNumber(Consts.quickNoiseProgress);
            }
            //处理噪音值和探索值达到上限的方法
            if (gameManager.exploreNoise > 100)
            {
                gameManager.exploreNoise -= 100;
            }
            //如果有重要支线未完成，进度不能超过90%
            //首先判断是否有已完成支线array，这个array是否包含未完成的重要支线
            if (gameManager.subTaskDic.TryGetValue(gameManager.roomID,out Array<int> array))
            {
                foreach (var item in ConfigManager.Instance.roomDic[gameManager.roomID].SubTask)
                {
                    if (!array.Contains(item))
                    {
                        include = false;
                    }
                }               
            }
            //如果找不到已完成支线，则判断该房间支线数量是否为0，如果不为0则直接限制进度
            else
            {
                if (ConfigManager.Instance.roomDic[gameManager.roomID].SubTask.Count!=0)
                {
                    include = false;
                }
            }
            //最后根据综合情况判断进度应该是多少
            if (include==false)
            {
                if (explorePogress[(gameManager.roomID)] > 90)
                {
                    explorePogress[(gameManager.roomID)] = 90;
                }
            }
            else
            {
                if (explorePogress[(gameManager.roomID)] > 100)
                {
                    explorePogress[(gameManager.roomID)] = 100;
                }
            }
            
        }
       

        eventChooseBar.exploreUI = exploreUI;       
        eventChooseBar.Initial(eventID);
        this.QueueFree();
    }

    private void OnBack()
    {
        switch (danger)
        {
            case 1:
                CommonTips tips1 = UIManager.Instance.ShowCommonTips("直接离开", "确认要离开当前位置并继续前进吗（可使你跳过当前场景）\n\n当前离开风险低，不会惊动丧尸，可放心撤离");
                tips1.OnConfirm = () => exploreUI.LeaveRoom();
                break;
            case 2:
                CommonTips tips2 = UIManager.Instance.ShowCommonTips("直接离开", "确认要离开当前位置并继续前进吗（可使你跳过当前场景）\n\n当前撤离风险适中，有一定概率惊动丧尸！");
                tips2.OnConfirm = () => exploreUI.LeaveRoom();
                break;
            case 3:
                CommonTips tips3 = UIManager.Instance.ShowCommonTips("直接离开", "确认要离开当前位置并继续前进吗（可使你跳过当前场景）\n\n当前撤离风险较高，很大概率会惊动丧尸！");
                tips3.OnConfirm = () => exploreUI.LeaveRoom();
                break;
            default:
                break;
        }              
    }


}
