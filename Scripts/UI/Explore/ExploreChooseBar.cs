using Godot;
using MyProject;


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
            CommonTips tips = UIManager.Instance.ShowCommonTips("终止探索", "确认要离开当前位置并继续前进吗");
            tips.OnConfirm = () => exploreUI.LeaveRoom();
        };
        carefulExploreBtn.Pressed += () => explore(1); 
        quicklyExploreBtn.Pressed += () => explore(2);
        gameManager = GameManager.Instance;
    }

    public void Init(ExploreUI owner)
    {
        exploreUI = owner;
    }

    private int explore(int type) 
    {
        EventChooseBar eventChooseBar=(EventChooseBar)UIManager.Instance.CreateUI("res://UI/Explore/EventChooseBar.tscn");

        int eventID=1;
        if (type==1)
        {
            eventID=Tools.GetRandomNumber(gameManager.carefulEventArray);
        }
        else
        {
            eventID=Tools.GetRandomNumber(gameManager.quickEventArray);
        }
        return eventID;
    }
}
