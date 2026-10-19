using Godot;
using MyProject;

/// <summary>
/// 开始菜单：新游戏 / 继续游戏。
/// 由 UIManager 在启动时显示（ShowUI 缓存，关闭后可直接 HideUI）。
/// </summary>
public partial class StartMenu : Control
{
    private Label titleLabel;
    private Button newGameBtn;
    private Button continueBtn;

    public override void _Ready()
    {
        // 不依赖 Export 解析，显式取节点，避免跨实例化时 Export 没生效导致 null
        titleLabel = GetNode<Label>("TitleLabel");
        newGameBtn = GetNode<Button>("BtnBox/NewGameBtn");
        continueBtn = GetNode<Button>("BtnBox/ContinueBtn");

        newGameBtn.Pressed += OnNewGamePressed;

        // 继续游戏暂时没做，先只给个提示
        continueBtn.Pressed += () =>
        {
            UIManager.Instance.ShowCommonTips("继续游戏", "读取存档功能还在做，先用新游戏吧。");
        };
    }

    private void OnNewGamePressed()
    {
        // 随机一个剧本，并把天数/进度重置
        GameManager.Instance.NewGameStart();

        // 关掉开始菜单，进入游戏主界面
        UIManager.Instance.HideUI(Paths.StartMenu);
        UIManager.Instance.ShowUI(Paths.MainUI);

        // 第一天早上也要弹日记
        OpenDiary();
    }

    /// <summary>
    /// 打开日记面板，打印当前天数 + 当前剧本对应的内容。
    /// 没有对应日记时仍会弹出（只是正文为空），避免"漏天"。
    /// </summary>
    public static void OpenDiary()
    {
        DiaryUI diary = (DiaryUI)UIManager.Instance.CreateUI(Paths.DiaryUI);
        if (diary == null) return;

        int day = GameManager.Instance.DayCount;
        string text = GameManager.Instance.GetDiaryTextForCurrentDay();
        diary.Show(day, text);
    }
}
