using Godot;
using Godot.Collections;
using MyProject;
using System.Collections.Generic;


public partial class ExploreChooseBar : NinePatchRect
{
    [Export] public HBoxContainer optionContainer;   //两个选项卡片的容器
    [Export] public TextureRect detailPanel;         //下方详情面板
    [Export] public Label optionName;                //详情面板里的搜索方式名字
    [Export] public Label desLabel;                  //详情面板里的房间描述
    [Export] public Label qualityValue;              //品质等级
    [Export] public Label progressValue;             //进度等级
    [Export] public Label noiseValue;                //噪音等级
    [Export] public Button startBtn;                 //开始搜索
    [Export] public Label dangerLabel;               //离开风险等级
    [Export] public TextureButton leaveBtn;          //直接离开

    public ExploreUI exploreUI;

    private GameManager gameManager;
    private bool progress = true;
    private bool include = true;
    private int danger = 3;
    private int tempEvent;
    private int selectedType = 1;                    //当前选中的搜索方式
    private ExploreOptionCard selectedCard;
    private List<ExploreOptionCard> cards = new List<ExploreOptionCard>();

    public override void _Ready()
    {
        // 中途撤离按钮：弹确认框，确认后交给 ExploreUI 统一销毁所有探索界面
        leaveBtn.Pressed += OnBack;
        // 开始搜索：这时候才真正走探索逻辑
        startBtn.Pressed += () => Explore(selectedType);
        gameManager = GameManager.Instance;
    }

    public void Init(ExploreUI owner, bool finish)
    {
        exploreUI = owner;
        detailPanel.Visible = false;                 //没选之前不显示详情

        //根据权重取得风险
        danger = Tools.GetRandomNumber(Consts.leaveDanger, Consts.leaveDangerWeight);

        //探索度大于90时，固定为低风险
        if (gameManager.GetExploreProgress(gameManager.roomID) >= 90)
        {
            danger = 1;
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

        //详情面板里的描述直接复用当前房间的描述
        desLabel.Text = ConfigManager.Instance.roomDic[gameManager.roomID].Des;

        //生成两个搜索方式卡片
        CreateOption(1, "仔细搜索", "exploreImage_1");
        CreateOption(2, "高效搜索", "exploreImage_2");

        startBtn.Visible = !finish;
    }

    //生成一个搜索方式卡片
    private void CreateOption(int type, string optionText, string imageName)
    {
        var scene = GD.Load<PackedScene>("res://UI/Explore/exploreOptionCard.tscn");
        ExploreOptionCard card = scene.Instantiate<ExploreOptionCard>();
        optionContainer.AddChild(card);
        card.Initial(type, optionText, imageName);
        cards.Add(card);
    }

    //选中某个搜索方式：清掉上一个的边框和角标，保证同时只有一个被选中，然后刷新详情面板
    public void SelectOption(ExploreOptionCard card)
    {
        if (selectedCard == card)
        {
            return;
        }
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }
        selectedCard = card;
        selectedCard.SetSelected(true);
        selectedType = card.ExploreType;
        RefreshDetail();
    }

    //刷新详情面板里的名字和 品质/进度/噪音
    private void RefreshDetail()
    {
        int quality;
        int progressLevel;
        int noiseLevel;

        if (selectedType == 1)
        {
            optionName.Text = "仔细搜索";
            quality = Consts.carefulQualityLevel;
            progressLevel = Consts.carefulProgressLevel;
            noiseLevel = Consts.carefulNoiseLevel;
        }
        else
        {
            optionName.Text = "高效搜索";
            quality = Consts.quickQualityLevel;
            progressLevel = Consts.quickProgressLevel;
            noiseLevel = Consts.quickNoiseLevel;
        }

        qualityValue.Text = Consts.GetLevelText(quality);
        qualityValue.AddThemeColorOverride("font_color", GetLevelColor(quality, true));
        progressValue.Text = Consts.GetLevelText(progressLevel);
        progressValue.AddThemeColorOverride("font_color", GetLevelColor(progressLevel, true));
        noiseValue.Text = Consts.GetLevelText(noiseLevel);
        noiseValue.AddThemeColorOverride("font_color", GetLevelColor(noiseLevel, false));

        detailPanel.Visible = true;
    }

    //等级文字颜色：higherIsBetter=false 表示越低越好（噪音）
    private Color GetLevelColor(int level, bool higherIsBetter)
    {
        int good = higherIsBetter ? level : 4 - level;      //1=差 2=一般 3=好
        switch (good)
        {
            case 3:
                return Colors.Green;
            case 2:
                return Colors.Yellow;
            default:
                return Colors.Red;
        }
    }

    private void Explore(int type)
    {
        EventChooseBar eventChooseBar=(EventChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/EventChooseBar.tscn");
        int eventID=1;
        //仔细探索
        if (type==1)
        {
            eventID=Tools.GetRandomNumber(gameManager.carefulEventArray);
            //eventID=Tools.GetRandomNumber(eventID,)
        }
        //快速探索
        else
        {
            eventID=Tools.GetRandomNumber(gameManager.quickEventArray);
        }

        //根据难度判断进战斗还是普通事件
        int difficulty = ConfigManager.Instance.buildingDic[gameManager.currentBuildingID].Stars;
        int property = Consts.GetPropertyByDifficult(difficulty);
        Array<int> eventArray = new Array<int>() { eventID, exploreUI.BattleEventReady(1) };
        Array<int> weightArray = new Array<int>() { (100 - property), property };
        eventID = Tools.GetRandomNumber(eventArray, weightArray);

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
            if (gameManager.GetExploreProgress(gameManager.roomID) >= ConfigManager.Instance.roomDic[gameManager.roomID].TriggerProgress[0])
            {
                eventID = ConfigManager.Instance.roomDic[gameManager.roomID].SubTask[0];
            }
        }
        if (ConfigManager.Instance.roomDic[gameManager.roomID].SubTask.Count > 1 && !subTaskArray.Contains(ConfigManager.Instance.roomDic[gameManager.roomID].SubTask[1]))
        {
            if (gameManager.GetExploreProgress(gameManager.roomID) >= ConfigManager.Instance.roomDic[gameManager.roomID].TriggerProgress[1])
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
        //如果是重要支线，每次探索首次触发暂不标记，仅赋值给当前支线；多次触发直接跳过
        if (ConfigManager.Instance.exploreEventDic[eventID].EventType == 103)
        {
            if (tempEvent == eventID)
            {
                if (type == 1)
                {
                    eventID = Tools.GetRandomNumber(gameManager.carefulEventArray);
                }
                //快速探索
                else
                {
                    eventID = Tools.GetRandomNumber(gameManager.quickEventArray);
                }
            }
            else
            {
                progress = false;
                tempEvent = eventID;
            }
            gameManager.currentSubTask = eventID;
        }

        //不是支线再增加进度
        if (progress==true)
        {
            int noise = 0;
            int addProgress = 0;
            if (type == 1)
            {
                addProgress = Tools.GetRandomNumber(Consts.carefulExploreProgress);
                noise = Tools.GetRandomNumber(Consts.carefulNoiseProgress);
            }
            //快速探索
            else
            {
                addProgress = Tools.GetRandomNumber(Consts.quickExploreProgress);
                noise = Tools.GetRandomNumber(Consts.quickNoiseProgress);
            }
            gameManager.AddExploreProgress(gameManager.roomID, addProgress);
            gameManager.exploreNoise += noise;

            UIManager.Instance.ShowFloatTips(100001, "+" + noise.ToString() + "%");
            UIManager.Instance.ShowFloatTips(100002, "+" + addProgress.ToString() + "%");
            //处理噪音值和探索值达到上限的方法
            if (gameManager.exploreNoise >= 100)
            {
                gameManager.exploreNoise -= 100;
                eventID = exploreUI.BattleEventReady(2);
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
            //最后根据综合情况判断进度应该是多少：有未完成的重要支线时最多 90，否则最多 100
            int maxProgress = include ? 100 : 90;
            if (gameManager.GetExploreProgress(gameManager.roomID) > maxProgress)
            {
                gameManager.SetExploreProgress(gameManager.roomID, maxProgress);
            }            
        }      
        eventChooseBar.exploreUI = exploreUI;
        eventChooseBar.eventID = eventID;
        eventChooseBar.Initial();
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
