using Godot;
using Godot.Collections;


public partial class ExploreChooseBar : NinePatchRect
{
    [Export] public TextureButton backBtn;
    [Export] public TextureButton carefulExploreBtn;
    [Export] public TextureButton quicklyExploreBtn;

    public ExploreUI exploreUI;
    private GameManager gameManager;
    public override void _Ready()
    {
        // 中途撤离按钮：弹确认框，确认后交给 ExploreUI 统一销毁所有探索界面
        backBtn.Pressed += () =>
        {
            CommonTips tips = UIManager.Instance.ShowCommonTips("直接离开", "确认要离开当前位置并继续前进吗（可使你跳过当前场景），当前离开风险高，很大概率惊动丧尸");
            tips.OnConfirm = () => exploreUI.LeaveRoom();
        };
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

    public void Init(ExploreUI owner)
    {
        exploreUI = owner;
    }

    private void explore(int type) 
    {
        EventChooseBar eventChooseBar=(EventChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/EventChooseBar.tscn");
        Dictionary<int, int> explorePogress = gameManager.exploreProgress;

        int eventID=1;
        if (type==1)
        {
            eventID=Tools.GetRandomNumber(gameManager.carefulEventArray);
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
        else
        {
            eventID=Tools.GetRandomNumber(gameManager.quickEventArray);
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
        //赋值当前事件ID
        GameManager.Instance.currentEventID = eventID;

        //处理噪音值和探索值达到上限的方法,后续补充
        if (gameManager.exploreNoise>100)
        {
            gameManager.exploreNoise -= 100;
        }

        eventChooseBar.exploreUI = exploreUI;       
        eventChooseBar.Initial(eventID);
        this.QueueFree();
    }
}
